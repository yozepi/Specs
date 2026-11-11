---
name: nuget-readme
description: 'Generate or refresh a README.md for the NuGet package project (.csproj) in the current folder. Use when: creating a package readme, NuGet readme, project README, PackageReadmeFile, documenting a library for consumers. Draws on memory-bank files, the GitHub wiki (documents/wiki), and the project code; focuses on installation and usage, links to the GitHub wiki for depth, and omits architecture.'
argument-hint: 'Optional: path to the .csproj or project folder'
---

# NuGet Package README

Produces a consumer-facing `README.md` next to a packable `.csproj` and wires it into the package so nuget.org displays it.

## Scope Rules

- **Audience**: developers installing and using the package. Not contributors.
- **Include**: what the package is, when to pick it, install, quick start, key usage, configuration, compatibility, related packages, links.
- **Exclude**: architecture, internal types, design patterns, component diagrams, build/CI internals, contributor setup. If a concept needs internals to explain, summarize the user-visible behaviour in one or two sentences and link the wiki.
- **Link, don't duplicate**: each summarized concept ends with a link to the GitHub wiki page/section that covers it in depth.
- **No invented APIs**: every code sample must compile against the project's public API or match existing samples/tests/templates in the repo.

## Procedure

### 1. Identify the target project

1. Use the folder from the argument, else the folder of the active editor file, else ask.
2. Find exactly one `*.csproj` (or `*.vbproj`/`*.fsproj`) in that folder. If none or several, ask which one.
3. Resolve effective MSBuild properties from the project file and every `Directory.Build.props` / `Directory.Packages.props` up the tree: `PackageId` (fallback: project name), `Description`, `PackageTags`, `TargetFramework(s)`, `IsPackable`, `PackageType`, `RepositoryUrl`, `PackageProjectUrl`, `PackageLicenseExpression`, `PackageReadmeFile`.
4. If `IsPackable` is `false`, stop and ask whether to continue (the project is likely embedded in another package).

### 2. Gather sources (read, don't skim)

| Source | What to extract |
|---|---|
| `memory-bank/projectbrief.md`, `productContext.md` | Purpose, audience, value proposition, package family |
| `memory-bank/techContext.md` | Supported frameworks/SDKs, package IDs, required settings, dependencies |
| `memory-bank/activeContext.md`, `progress.md` | Current status, known limitations worth stating to consumers |
| `memory-bank/glossary.md` (if present) | Terminology to use consistently |
| `documents/wiki/*.md` | Page names and `##`/`###` headings for deep links; usage samples; troubleshooting items |
| `documents/wiki/wiki-preferences.md` (if present) | Tone, language (e.g. C# + VB), audience rules — apply them to the README too |
| Project code | Public types/members a consumer touches, attributes, settings keys, `build/*.props`/`*.targets` shipped in the package, `dotnet new` templates |
| Tests / samples / templates | Compilable examples to base snippets on |

Skip `architecture.md`, `systemPatterns.md`, and similar design docs except to confirm names. If the memory bank or wiki is missing, say so and rely on code.

### 3. Build wiki links

- Base URL: `<RepositoryUrl>/wiki/` (strip trailing `.git`). Example: `https://github.com/owner/repo/wiki/Usage-Guide`.
- Page slug = wiki file name without `.md` (e.g. `Usage-Guide.md` → `Usage-Guide`). Skip pages prefixed with `_`.
- Section anchor = heading text lowercased, spaces → `-`, punctuation except `-` removed (e.g. `### Filtering on VSTest (\`--filter\`)` → `#filtering-on-vstest---filter`).
- Only link to pages and headings that actually exist in `documents/wiki/`.
- Use absolute URLs everywhere — relative links break on nuget.org.

### 4. Write the README

Follow [the template](./assets/README.template.md). Drop sections that don't apply; don't pad. Keep it short: a reader should reach a working example within the first screen.

Content rules:
- Lead with one or two sentences on what the package does and who it's for; if part of a package family, include a "which package do I need?" table.
- Install via `dotnet add package <PackageId>` plus any required extra setup (e.g. `global.json` entries, props, template install).
- Quick start = the smallest end-to-end example that works, then the command to run it.
- Usage sections: 2–5 of the most important tasks, each = short explanation + snippet + "More: [wiki link]".
- Configuration: only settings consumers set (keys, attributes, env vars, runsettings), as a table, linking the wiki for the full list.
- Compatibility: target frameworks and required SDK/tooling versions from the project file and `global.json`.
- Respect `wiki-preferences.md` language/tone rules (e.g. paired C#/VB samples).

nuget.org rendering constraints:
- No Mermaid, no raw HTML beyond simple tags, no relative links or relative image paths.
- Badges/images only from trusted hosts (e.g. `img.shields.io`, `github.com`, `raw.githubusercontent.com`).

### 5. Wire into the package

If `PackageReadmeFile` isn't already set for this project, add to the `.csproj`:

```xml
<PropertyGroup>
  <PackageReadmeFile>README.md</PackageReadmeFile>
</PropertyGroup>
<ItemGroup>
  <None Include="README.md" Pack="true" PackagePath="\" />
</ItemGroup>
```

If an existing `README.md` is present, update it in place and preserve any hand-written sections that are still accurate; ask before removing content.

### 6. Validate

- [ ] Every code sample uses real public API (grep the project for each type/member used).
- [ ] Every wiki link resolves to an existing file and heading in `documents/wiki/`.
- [ ] No architecture/internal type names beyond the public API.
- [ ] No relative links, Mermaid, or untrusted images.
- [ ] `PackageId`, TFMs, and versions match the project files.
- [ ] The `.csproj` has `PackageReadmeFile` and a matching `None Include="README.md" Pack="true"` item.

Report the file path, sections included, and any gaps (e.g. a concept with no wiki page to link).
