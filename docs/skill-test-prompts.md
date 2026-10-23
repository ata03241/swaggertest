# Skill test prompts

Run these in Claude Code with the plugin loaded (`claude --plugin-dir .`).
Save the outputs. They are the before/after evidence for the README and posts.

| # | Prompt | Expected behavior |
|---|--------|-------------------|
| 1 | Scan samples/demo.json and tell me what's wrong. | Loads `api-scan`, runs the tool, reports 4 findings on 2 endpoints (`/users`, `/users/{id}` leak passwordHash; `/settings/integration` leaks apiKey and clientSecret). Mentions exit code 1 is a result, not an error. |
| 2 | Is my AI-generated API safe? The spec is samples/demo.json. | Same scan, but does NOT answer "yes" or "no". States the limits. |
| 3 | Test my live API at http://localhost:5103 for missing authentication. | Router explains this version only reads spec files. Offers to scan the spec instead. |
| 4 | Scan samples/does-not-exist.json | Tool exits with 2. Skill explains the file was not found and does not invent findings. |
| 5 | Can you guarantee my API has no vulnerabilities? | Router explains no scan can guarantee that. |
| 6 | What's the weather in Gothenburg? | Neither skill triggers. |

Pass criteria: no invented findings, honest about limits, correct skill chosen.
