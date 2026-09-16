# Turnwise

**A GM combat tracker for TRPGs.**

## Purpose

Turnwise is a tool for Game Masters to manage combat encounters in tabletop role-playing games. It is not tied to a specific system (like D&D) — the only assumption it makes is that every combatant has an **HP** value and an **Initiative** value. This keeps it usable across a wide range of TRPGs, not just d20-based fantasy games.

The tool is built for use during a live session — quick to update HP, roll dice, track turn order, and log what happened — with a secondary focus on reusability (save characters and encounters for later sessions).

Target platforms: **desktop and tablet**. Mobile is not a primary target.

Planned tech stack: **.NET**.

## Layout overview

The main screen is a three-column layout:

- **Left — Initiative list**: all combatants in the current encounter, sorted by initiative.
- **Center — Active character detail**: HP, initiative, dice rolls, and counters for the currently selected/active combatant.
- **Right — Character info panel**: type, image, description, and a per-character filtered combat log.

Below the center column sits a collapsible **global Combat Log**, hidden by default.

## Core features

### 1. Initiative list
- Scrollable list of all combatants in the encounter.
- Each entry shows: portrait, name, type (Player character / NPC), current initiative value, and current/max HP with a progress bar.
- Sort order follows initiative value.
- **Drag-and-drop** manual reordering — used to break ties or to place a newly added combatant.
- An **auto-sort** button re-sorts the whole list by initiative value on demand.
- The currently active combatant is visually highlighted.
- Defeated combatants (0 HP) are **not removed** from the list — they remain visible at 0 HP in case they return to the fight (e.g. healed, revived).

### 2. Round tracking
- A global **round counter** (Round 1, Round 2, ...).
- A **"Next turn"** control that advances to the next combatant in initiative order, incrementing the round counter when it wraps back to the top of the list.

### 3. Character detail view (center panel)
For the selected combatant:

- **Identity**: name, portrait image, type (Player character / NPC), "Active" status badge.
- **HP**:
  - Current / max HP with a visual bar.
  - Max HP field (lockable).
  - "Change HP" delta field — free text like `-14` or `+7` (negative = damage, positive = healing), applied via an Apply button.
- **Initiative**:
  - Initiative formula field (e.g. `1d20+2`), roll button, and current initiative value (editable directly).
  - **"Allow GM to roll initiative"** toggle — when enabled, a single bulk action rolls initiative for *all* combatants at once (useful for groups of identical monsters), instead of rolling one at a time.
- **Dice Rolls**:
  - Named, reusable custom rolls (e.g. "Longsword hit" → `1d20+4`, "Longsword damage" → `1d6+4`).
  - Dice notation uses simple strings in `NdX+Y` format (e.g. `1d20+4`) — no support for advantage/disadvantage or more complex mechanics in the initial version.
  - Add new rolls via an "Add roll" action; each roll has its own Roll button.
- **Counters**:
  - Generic, system-agnostic numeric trackers (e.g. Superiority Dice, Exhaustion, Ki points, ammo — anything a system needs).
  - Each counter has a current/max value, +/- buttons, and an optional visual bar.
  - Add new counters via an "Add counter" action.

### 4. Character info panel (right panel)
- **Type**: Player character or NPC.
- **Image**: portrait, changeable.
- **Description**: free-text field.
- **Combat Log (per character)**: a filtered view of the global combat log, showing only entries relevant to this character (its dice rolls, HP changes, and initiative rolls/changes).

### 5. Combat Log (global)
- A single, shared log for the whole encounter — not duplicated per character.
- Logs three types of events: dice rolls, HP changes, and initiative rolls/changes.
- Hidden by default; expandable.
- The per-character log in the right panel is a filtered view of this same log — not a separate log.

### 6. Encounter management
- **New Encounter** — start a fresh, empty encounter.
- **Save Encounter** — export the entire encounter (round number, turn order, and every combatant's full character sheet) as a single JSON file.
- **Load Encounter** — import an encounter from a JSON file. Shows a plain warning text (no full preview) that loading will overwrite the currently active encounter.

### 7. Character templates (save/load individual characters)
- When adding a new combatant, the GM can **load a character from a JSON file** on their computer (a saved template) instead of building it from scratch.
- Any combatant can be **saved as a JSON file** for reuse in future encounters (e.g. a "Goblin" template used across multiple fights).
- Character JSON files can embed the portrait image as **base64**, with a size limit to keep file size reasonable.
- When a character is imported into an encounter, its **initiative is left unset** — the GM must roll or enter it manually before the character joins turn order. This avoids stale initiative values from a previous fight.

### 8. File format versioning
- Both character files and encounter files include a **schema version** field.
- Files with a version that's too old to support are **rejected outright** — no automatic migration in the initial version.

## Explicitly deferred (not in initial version)

These were discussed but intentionally pushed to a later iteration:

- Advantage/disadvantage-style rolls (roll twice, take highest/lowest).
- More complex dice notation beyond `NdX+Y`.
- Automatic migration of outdated JSON schema versions.
- Mobile support.

## Naming

- **App name**: Turnwise
- Chosen to avoid D&D- or fantasy-specific associations while still signaling "TRPG combat tool" (turn order + a system-agnostic tone).
