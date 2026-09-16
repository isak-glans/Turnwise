# Turnwise

A GM combat tracker for TRPGs. See [docs/turnwise-spec.md](docs/turnwise-spec.md) for the full product spec.

This repository currently contains the **initial project scaffold** - a working Clean Architecture
foundation with the core domain model, application use cases, JSON file persistence, and a
Blazor Server UI shell. It is not a finished product: most panels are functional end-to-end
(add/select/reorder combatants, HP changes, dice rolls, counters, save/load encounters and
character templates), but polish, validation, and some spec features (e.g. richer drag-and-drop
feedback) are still to come.

## Architecture

The solution follows Clean Architecture - dependencies only point inward, toward the domain:

```
Turnwise.Web  ──depends on──>  Turnwise.Infrastructure  ──depends on──>  Turnwise.Application  ──depends on──>  Turnwise.Domain
     │                                                                          ^
     └──────────────────────────────────────────────────────────────────────────┘
```

- **Turnwise.Domain** - entities (`Encounter`, `Combatant`, `NamedRoll`, `Counter`, `CombatLogEntry`)
  and value objects (`DiceFormula`). Pure C#, no dependencies, no framework references.
- **Turnwise.Application** - use cases (`EncounterService`, `DiceRollingService`,
  `CharacterTemplateService`, `EncounterFileService`) and the ports they need (`IRandomSource`,
  `ICharacterTemplateStore`, `IEncounterStore`). Depends only on Domain.
- **Turnwise.Infrastructure** - implementations of those ports: JSON file persistence
  (`JsonEncounterStore`, `JsonCharacterTemplateStore`) with schema-version checks, and a
  `System.Random`-backed `IRandomSource`.
- **Turnwise.Web** - Blazor Server app (interactive server render mode). Composition root
  (`Program.cs`) wires up DI for all layers. UI components live under `Components/Combat/` and
  talk to Application services; `EncounterSessionState` holds the current in-memory encounter for
  the browser session.

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
dotnet run --project src/Turnwise.Web
```

Then open the URL printed in the console (e.g. `https://localhost:5001`).

## Tests

```
dotnet test
```
