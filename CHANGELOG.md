# Changelog

All notable changes to this project will be documented in this file.

## 1.0.0 (2026-08-21)


### Features

* first tagged release ([d0428e3](https://github.com/Paubox/paubox-csharp/commit/d0428e314933f066f0bf0d962773d8838fd2a88b))

## [Unreleased]

This SDK has never been published to NuGet and has never carried a git tag.
`1.0.0` will be the first tagged release. The notes below describe the state of
the source at that release, not a diff against a previously shipped artifact —
there isn't one.

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
