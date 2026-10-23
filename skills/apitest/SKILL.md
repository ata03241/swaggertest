---
name: apitest
description: Entry point for swaggertest. Use whenever the user wants to check, audit, review, or test a REST API or OpenAPI/Swagger spec, or asks if an AI-generated API leaks passwords, keys, or data.
---

# apitest (router)

swaggertest is a command-line tool that reads an OpenAPI spec file and reports
common API problems. This skill picks the right workflow for the request and
keeps the answer honest about what the tool can and cannot do.

## Pick a workflow

- The user wants an API or spec checked for problems (leaks, exposed secrets,
  "is my AI-generated API safe?") -> use the `api-scan` skill.
- The user wants a running API called, authentication tested, or real
  responses compared with the spec -> this version cannot do that yet. It only
  reads spec files. Say so plainly, then offer to run `api-scan` on the spec.
- The user wants a guarantee that the API is secure -> explain that no scan can
  give one. This tool checks for specific common problems only.

## Ground rules

- Only work on APIs and specs the user owns or is allowed to test.
- Report only what the tool prints. A finding the tool did not report is not a
  finding.
