---
name: api-scan
description: Scan an OpenAPI/Swagger JSON spec with swaggertest and explain findings in plain language. Use whenever the user wants to scan or audit an API spec for leaked passwords, secrets, or sensitive fields.
---

# api-scan

Runs `swaggertest scan` on a spec file and explains the result to the user.
The tool does the detection. Your job is to run it correctly and explain the
output clearly, not to look for problems yourself.

## Before you start

1. Check that the tool is installed by running `swaggertest scan --help`.
   If it is missing, tell the user and point them to the install steps in the
   repository README. Do not try to reimplement the checks.
2. You need a spec **file** in JSON. If the user only has a running API on
   localhost, ask for its spec URL (ASP.NET apps usually serve it at
   `/openapi/v1.json`, others at `/swagger/v1/swagger.json`), save it to a
   file, and scan the file. Only fetch from localhost or an API the user owns.
   YAML specs are not supported yet.

## Run

```
swaggertest scan <path-to-spec.json>
```

The JSON report is on stdout. Error messages are on stderr.

## Exit codes

- `0`: no high-severity findings.
- `1`: high-severity findings. This is a normal result, not a failure of the
  tool, so do not report it as an error.
- `2`: the tool could not run (missing file, unreadable spec, bad arguments).
  Read stderr, explain what went wrong, and help fix the input.

## Read the report

Each entry in `findings` has `ruleId`, `severity`, `endpoint`, `message`, and
`fixHint`. For `SENSITIVE_FIELD`, the path in the message shows where the field
sits: `$.apiKey` is on the response object, and `$[].passwordHash` is on each
item of a list response.

## Explain to the user

- Start with the number of findings and the worst severity.
- Group findings by endpoint. For each one, say what is exposed, why it matters
  (anyone who can call the endpoint can read it), and what to change. Make the
  `fixHint` concrete for the user's code if you can see it.
- Keep it short and plain. No lecture.

## Say these limits when relevant

- This reads the spec only. It does not call the API, so real responses could
  differ from the spec.
- A clean report means this one check found nothing. It does not mean the API
  is secure.
- Never claim a problem the report does not list.

## After a fix

If the user changes the code, regenerate the spec, run the scan again, and
confirm the finding is gone before saying it is fixed.
