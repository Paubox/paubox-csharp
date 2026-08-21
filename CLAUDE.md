# CLAUDE.md — Paubox C# SDK

This file provides context for AI-assisted development on this repository.

## Project Overview

This is the official Paubox C# SDK. It wraps two Paubox APIs:

| Library | Class | Base URL | Auth |
|---|---|---|---|
| Email API | `EmailLibrary` | `https://api.paubox.com/v1/` | `Token token={apiKey}` header |
| Forms API | `FormsLibrary` | `https://api.paubox.com/forms/` | Scoped API key (`Bearer {apiKey}`, `forms` scope) for management endpoints; public endpoints unauthenticated |

## Repository Structure

```
paubox-csharp/
├── README.md                  # User-facing setup and usage guide
├── api.md                     # API reference for both libraries
├── CLAUDE.md                  # This file
├── lib/                       # Pre-compiled DLL (Paubox.Email.API.dll)
└── Paubox_Csharp/
    ├── Paubox_Csharp.sln
    ├── EmailLib/              # Core library project
    │   ├── APIHelper.cs       # HTTP layer (HttpClient wrapper)
    │   ├── IAPIHelper.cs      # Interface for APIHelper (injectable for tests)
    │   ├── EmailLibrary.cs    # Email API client
    │   ├── IEmailLibrary.cs   # Email API interface
    │   ├── FormsLibrary.cs    # Forms API client
    │   ├── IFormsLibrary.cs   # Forms API interface
    │   ├── CommonClasses.cs   # Email-related models and response types
    │   ├── FormsClasses.cs    # Forms-related models (Form, FormAttachment)
    │   ├── Message.cs         # Email message model
    │   ├── BaseMessage.cs     # Abstract base for email messages
    │   └── TemplatedMessage.cs
    └── UnitTestProject/       # NUnit test suite
        ├── GetFormTest.cs
        ├── SubmitFormTest.cs
        ├── SendMessageTest.cs
        └── ... (other test files)
```

## Running Tests

```bash
dotnet test Paubox_Csharp/UnitTestProject/UnitTestProject.csproj
```

Build only:

```bash
dotnet build Paubox_Csharp/Paubox_Csharp.sln
```

Target framework: **.NET 8.0**

## Key Patterns

### HTTP Layer

`APIHelper` wraps `HttpClient` and is injected via `IAPIHelper`. All library constructors accept
an `IAPIHelper` parameter for testing:

```csharp
var lib = new FormsLibrary(_mockApiHelper.Object);   // tests
var lib = new FormsLibrary();                         // production
```

`CallToAPI` signature:
```csharp
string CallToAPI(string baseUrl, string requestUri, string authHeader, string apiVerb, string requestBody = "")
```

Passing `null` for `authHeader` is safe — the header is only added when non-null and non-empty.

### JSON Serialization

All JSON handling uses **Newtonsoft.Json**. Response models use `[JsonProperty("snake_case_name")]`
attributes to map API field names to PascalCase C# properties.

### Error Handling

Both libraries throw `SystemException` with the raw API response body as the message when the API
returns an error. Guard conditions throw `ArgumentException` / `ArgumentNullException` for invalid
inputs before making any HTTP call.

### Test Structure

Each public method has a corresponding `*Test.cs` file in `UnitTestProject/`. Tests use:
- **NUnit 3** as the test framework
- **Moq** to mock `IAPIHelper`
- Moq `.Callback<>()` to capture request arguments for payload verification

## Development Branch

Active feature work: `claude/paubox-forms-api-support-hAB2b`

## Adding New Endpoints

1. Add model classes to `CommonClasses.cs` (Email API) or `FormsClasses.cs` (Forms API)
2. Add the method signature to the relevant interface (`IEmailLibrary` or `IFormsLibrary`)
3. Implement in `EmailLibrary.cs` or `FormsLibrary.cs`
4. Add a `*Test.cs` file in `UnitTestProject/` covering happy path, error cases, and payload shape
5. Update `README.md` and `api.md`

## Releases

Releases are automated with [release-please](https://github.com/googleapis/release-please). Merging to `master` refreshes a standing release PR; merging *that* PR bumps `version.txt` and the assembly version attributes, writes `CHANGELOG.md`, creates a bare `vX.Y.Z` tag and a GitHub release, and **attaches a freshly built DLL to that release**.

Do **not** hand-edit the assembly version, `version.txt`, or `CHANGELOG.md` — release-please owns all three.

The next version comes from PR titles: `feat:` gives a minor bump, `fix:` a patch, and a `!` suffix or a `BREAKING CHANGE:` footer gives a major. `.github/workflows/pr-title.yml` rejects titles release-please cannot parse.

### Where the version actually lives

`EmailLib.csproj` sets `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>`, so the version comes from `Properties/AssemblyInfo.cs`, **not** from any MSBuild property. Adding a `<Version>` to the csproj would have no effect on the built assembly.

Those two attribute lines carry `// x-release-please-version` annotations. They are load-bearing: delete them and the assembly version silently stops tracking releases while everything still appears to work.

### Not published to a registry

This SDK is **not on NuGet**, and publishing is deliberately out of scope. Until that changes the GitHub release is the distribution channel, and the DLL committed under `lib/` was last rebuilt in **July 2019** — 36 source commits behind, predating the Forms API, dynamic templates and the auth change. Prefer the DLL attached to the latest release.

`CHANGELOG.md` must keep a heading matching `/\n###? v?[0-9[]/`, which is why the staging heading is the bracketed `## [Unreleased]`. Without a match release-please prepends a second `# Changelog` title and demotes every existing heading.
