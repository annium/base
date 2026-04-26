# Documentation

Two layers of documentation live in this repo:

1. **Narrative docs** — this `docs/` tree plus `CLAUDE.md`. Hand-written.
2. **API reference** — generated from source XML comments by [DocFX](https://dotnet.github.io/docfx/). Output lives under `api/` and is composed into the landing page (`index.md`, `toc.yml`) during `just docs-build`.

## Commands

| Command | Description |
|---------|-------------|
| `just docs-lint` | `doclint lint -w . -i '**/*.cs' -e '**/obj/**/*.cs'` — enforces XML doc presence on public APIs |
| `just docs-metadata` | `docfx metadata docfx.json` — regenerates `api/*.yml` from assemblies |
| `just docs-build` | Full DocFX build — writes the static site to `_site/` |
| `just docs-serve` | Serve `_site/` locally (http://localhost:8080 by default) |
| `just docs-watch` | Combined rebuild + serve with file watching |
| `just docs-clean` | Remove `_site/` and `api/` |

## Writing XML doc comments

`Directory.Build.props` sets `GenerateDocumentationFile=true` and `WarningsAsErrors=true`, so any public member without an XML `<summary>` (plus `<param>`/`<returns>` where applicable) fails the build.

```csharp
/// <summary>
/// Parses the expression and returns the compiled delegate.
/// </summary>
/// <param name="source">Source expression text.</param>
/// <returns>Compiled expression delegate.</returns>
/// <exception cref="FormatException">Thrown when <paramref name="source"/> is malformed.</exception>
public Func<T, T> Parse(string source) { ... }
```

`doclint` checks a broader set of rules than the compiler (e.g., missing `<param>` entries, undocumented type parameters). Run it locally before pushing:

```bash
just docs-lint
```

## DocFX configuration

- `docfx.json` declares one `metadata` block per `src/*/*.csproj`. If you add a new package, add a matching block so its API reference is generated.
- `toc.yml` is the navigation tree. Update it when adding a new API group.
- `index.md` is the landing page — one bullet per module, grouped by tier.

## Regenerating after code changes

```bash
just docs-clean
just docs-metadata        # only needed if public surface changed
just docs-build
just docs-serve           # localhost:8080
```

## Updating narrative docs

The `/document-repository` skill is the maintained way to refresh `CLAUDE.md` and `docs/`. Run it after:

- Adding or removing modules under `base/` or `integrations/`
- Changing build commands in `justfile`
- Restructuring `Directory.Build.props` / `Directory.Packages.props`
- Adjusting the target framework or SDK pin in `global.json`

For scoped updates pass `--scope=<area>` (e.g., `--scope=testing`) to only rewrite relevant pages.

## Auxiliary files

| File | Purpose |
|------|---------|
| `AGENTS.md` | Mirror of `CLAUDE.md` for non-Claude agent runners — keep the two in sync when editing. |
| `README.md` | Placeholder; the useful content lives in `CLAUDE.md` + `docs/`. |
| `index.md` | DocFX landing page used by `just docs-build`, not browsed directly from the repo root. |
| `lint.log` | Output of the most recent `docs-lint` run (gitignored via `*.log`). |
