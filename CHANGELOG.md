# Changelog

All notable changes to this project will be documented in this file.

## [1.0.1](https://github.com/Paubox/paubox-csharp/compare/v1.0.0...v1.0.1) (2026-08-25)


### Bug Fixes

* target the documented /v1/email and /v1/forms base URLs ([#28](https://github.com/Paubox/paubox-csharp/issues/28)) ([a8bd23d](https://github.com/Paubox/paubox-csharp/commit/a8bd23d286b6de8719649d7a1f1bc84042c906d9))

## 1.0.0 (2026-08-21)

First tagged release. This SDK had never been published to NuGet and had never
carried a git tag, so the notes below describe the state of the source at
`1.0.0` rather than a diff against a previously shipped artifact — there isn't
one.

### 🚀 Features

- **Transactional Email** via `EmailLibrary`: `SendMessage`, `SendBulkMessages`, `SendTemplatedMessage`, and `GetEmailDisposition`
- **Dynamic templates**: `ListDynamicTemplates`, `GetDynamicTemplate`, `CreateDynamicTemplate`, `UpdateDynamicTemplate`, `DeleteDynamicTemplate`
- **Paubox Forms** via `FormsLibrary`
  - Public endpoints, no credential attached: `GetForm`, `SubmitForm`
  - Form management with a scoped API key (`forms` scope, sent as a Bearer token): `ListForms`, `CreateForm`, `GetFormById`, `UpdateForm`, `ArchiveForm`, `UnarchiveForm`, `CopyForm`, `GetFormStats`
  - Submissions: `ListFormSubmissions`, `ExportSubmissionsCsv`, `ExportSubmissionCsv`, `ExportSubmissionPdf`
- Authentication no longer requires a username — an API key alone authenticates, and base URLs point at `api.paubox.com`

### ⚠️ Notes for consumers

- The `Paubox.Email.API.dll` committed under `lib/` was last rebuilt in **July 2019** and is 36 source commits behind. It predates the Forms API, dynamic templates, and the authentication change. Releases from `1.0.0` onward attach a freshly built DLL to the GitHub release; prefer that over the committed file
- The SDK is not on NuGet. Publishing is deliberately out of scope for now
