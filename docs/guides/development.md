# Development

All cross-project workflows go through `justfile`. `just` with no argument prints the full recipe list.

## Prerequisites

- .NET SDK `10.0.x` (`global.json` pins `rollForward=latestMinor`, `allowPrerelease=true`)
- `just` — `brew install just`
- `jq` — used by `just update` to read the tool manifest
- `openssl` — used by the `gen-*-keys` recipes
- Access to the NuGet feeds configured in `.xs.credentials` (provisioned by the umbrella repo: `cd ~/Projects/annium && just copy-keys`)

## First-time setup

```bash
cd projects/base
just setup            # dotnet tool restore: CSharpier, xs.cli, doclint, docfx, versioning
```

## Daily loop

```bash
just format           # CSharpier + `xs format -sc -ic`
just build            # Release build of Annium.Base.sln
just test             # Every test project via xunit.v3 + TRX report
```

`just format-full` additionally runs `dotnet format style` and `dotnet format analyzers` — useful before a release but slow enough that it's not part of the default loop.

## Running a single test

```bash
# One project
dotnet test --project base/Core/tests/Annium.Core.Mediator.Tests/Annium.Core.Mediator.Tests.csproj

# One test by FQN fragment
dotnet test --filter "FullyQualifiedName~Mediator"

# One class and method
dotnet test --filter "ClassName=Annium.Core.Mediator.Tests.SomeClass&MethodName=SomeMethod"
```

## Packing and publishing

```bash
just pack                     # build, compute version via `xx versioning get-version`, produce *.nupkg + *.snupkg
just publish "$NUGET_API_KEY" # push to nuget.org and delete local .nupkg files
```

The version lives in `./version` and is fed into MSBuild as `PackageVersion`. To bump it:

```bash
# Set in CI (via ci-set-package-version) or manually:
echo "0.42.0" > version
git add version && git commit -m "chore: bump version"
```

## Adding a new package

1. Create `base/<Group>/src/<Package>/<Package>.csproj`.
2. Create `base/<Group>/tests/<Package>.Tests/<Package>.Tests.csproj`.
3. Register both in `Annium.Base.sln` (`dotnet sln add …`).
4. If the group is new, add `base/<NewGroup>/Directory.Build.props` copying the pattern from a sibling group.
5. If the package should appear in generated API docs, add an entry in `docfx.json` (`metadata.src.files`) and a link in `toc.yml` and `index.md`.
6. Run `just format` and `just build`.

## Dependency management

- **Third-party versions** — edit `Directory.Packages.props`. Never add `Version="..."` to a `PackageReference`.
- **Update all packages** — `just update` reinstalls the dotnet tool manifest and then runs `xs update all -sc -ic`.
- **Inside the umbrella repo** — `just link-base` / `just unlink-base` wire this sub-project into sibling repos for local dev.

## Clean slate

```bash
just clean            # xs clean + remove stray *.nupkg
just docs-clean       # remove _site/ and api/
```

`bin/`, `obj/`, `_site/`, and `api/` are gitignored.

## CI entry points

`justfile` defines three CI bundles:

| Recipe | Use |
|--------|-----|
| `just ci-merge-request-short` | PR gate without tests: setup → format → ensure-no-changes → clean → build |
| `just ci-merge-request-full` | Full PR gate: short + test |
| `just ci-release <apiKey> <repo> <ghToken>` | Release pipeline: set version → pack → publish → tag |

`ensure-no-changes` is the one that surprises people — it runs `git status --porcelain` after `just format` and fails the build if anything changed, forcing formatting to live in a preceding commit.

## Troubleshooting

- **`dotnet tool run` fails** — run `just setup`. Tools are managed via the local manifest, not globally.
- **`dotnet tool run xs ...` missing feed** — ensure `.xs.credentials` exists. The umbrella repo generates it via `just copy-keys`.
- **Warnings break the build** — `WarningsAsErrors=true` is global. Either fix the warning or add a targeted suppression via pragma/attribute.
- **Analyzer complaints on exception names** — `Annium.Analyzers` enforces Annium-specific exception naming; rename the exception to match the convention rather than suppressing.
- **Doc build fails on XML comments** — run `just docs-lint` to pinpoint missing XML doc. See [Documentation guide](documentation.md).
