# Turnwise

A GM combat tracker for TRPGs. See [docs/turnwise-spec.md](docs/turnwise-spec.md) for the full product spec.

This repository currently contains the **initial project scaffold** - a working Clean Architecture
foundation with the core domain model, application use cases, JSON file persistence, and a
Blazor WebAssembly UI. It is not a finished product: most panels are functional end-to-end
(add/select/reorder combatants, HP changes, dice rolls, counters, save/load encounters and
character templates), but polish, validation, and some spec features (e.g. richer drag-and-drop
feedback) are still to come.

## Architecture

The solution follows Clean Architecture - dependencies only point inward, toward the domain:

```
Turnwise.Web.Client  ──depends on──>  Turnwise.Infrastructure  ──depends on──>  Turnwise.Application  ──depends on──>  Turnwise.Domain
     │                                                                                ^
     └────────────────────────────────────────────────────────────────────────────────┘
```

- **Turnwise.Domain** - entities (`Encounter`, `Combatant`, `NamedRoll`, `Counter`, `CombatLogEntry`)
  and value objects (`DiceFormula`). Pure C#, no dependencies, no framework references.
- **Turnwise.Application** - use cases (`EncounterService`, `DiceRollingService`,
  `CharacterTemplateService`, `EncounterFileService`) and the ports they need (`IRandomSource`,
  `ICharacterTemplateStore`, `IEncounterStore`). Depends only on Domain.
- **Turnwise.Infrastructure** - implementations of those ports: JSON file persistence
  (`JsonEncounterStore`, `JsonCharacterTemplateStore`) with schema-version checks, and a
  `System.Random`-backed `IRandomSource`.
- **Turnwise.Web.Client** - standalone Blazor WebAssembly app: the whole UI runs in the browser,
  no server process. Composition root (`Program.cs`) wires up DI for all layers. UI components
  live under `Components/Combat/` and talk to Application services; `EncounterSessionState` holds
  the current in-memory encounter for the browser tab.

There is no database - per the spec, persistence is entirely file-based (JSON export/import for
encounters and character templates), so the Infrastructure layer is intentionally lightweight.

### Tests

- `Turnwise.Domain.Tests` - entity/value-object behavior (dice parsing, HP clamping, turn order).
- `Turnwise.Application.Tests` - use-case orchestration, with a fake `IRandomSource` for
  deterministic dice rolls.
- `Turnwise.Infrastructure.Tests` - JSON round-trip and schema-version rejection.

## Running

Requires the .NET 10 SDK.

```
dotnet run --project src/Turnwise.Web.Client
```

Then open the URL printed in the console (e.g. `http://localhost:5300`).

To produce a deployable static build (a `wwwroot` folder - `index.html`, `app.css`, `js/`,
`_framework/` - that any static host can serve, no .NET process required):

```
dotnet publish src/Turnwise.Web.Client -c Release -o <output-folder>
```

The deployable output ends up in `<output-folder>/wwwroot`. For a smaller/faster build, install
the `wasm-tools` workload (`dotnet workload install wasm-tools`) first - without it, publish still
works but skips IL trimming/AOT optimization.

WebAssembly is the app's only hosting model: no persistent server process, no per-connection
server memory - deploy is just static files. Note that the app is not yet usable offline: there
is no service worker (PWA), and fonts/icons load from a CDN, so a reload without a connection
will not start it. Adding a service worker is the missing piece for true offline use.

### Deploying to GitHub Pages

`.github/workflows/deploy-pages.yml` builds, tests and publishes the app on every push to `main`
(and on manual dispatch). One-time setup: in the repository settings, go to *Pages* and set the
source to **GitHub Actions**. Free GitHub Pages requires a public repository.

The site is served from `https://<user>.github.io/<repo>/`, so the workflow rewrites the published
`index.html`'s `<base href>` to `/<repo>/` (local `dotnet run` keeps `/`). It also adds
`.nojekyll` (Jekyll would otherwise ignore `_framework/`) and a `404.html` copy of `index.html`
so deep links still boot the app.

## Tests

```
dotnet test
```
