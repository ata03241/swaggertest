var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

var app = builder.Build();
app.MapOpenApi();

var users = new List<User>
{
    new(1, "ada@example.com", "Ada", "$2a$11$demo-not-a-real-hash-one"),
    new(2, "linus@example.com", "Linus", "$2a$11$demo-not-a-real-hash-two"),
};

app.MapGet("/health", () => Results.Ok(new HealthStatus("ok")))
   .Produces<HealthStatus>();

// BUG: returns the full User, including PasswordHash.
app.MapGet("/users", () => Results.Ok(users))
   .Produces<List<User>>();

// BUG: same leak on a single user.
app.MapGet("/users/{id:int}", (int id) =>
        users.FirstOrDefault(u => u.Id == id) is { } user
            ? Results.Ok(user)
            : Results.NotFound())
   .Produces<User>()
   .Produces(404);

// BUG: exposes secrets that should never leave the server.
app.MapGet("/settings/integration", () =>
        Results.Ok(new IntegrationSettings(
            "https://payments.example.com",
            "demo-not-a-real-key",
            "demo-not-a-real-secret")))
   .Produces<IntegrationSettings>();

app.Run();

record User(int Id, string Email, string Name, string PasswordHash);
record HealthStatus(string Status);
record IntegrationSettings(string Endpoint, string ApiKey, string ClientSecret);