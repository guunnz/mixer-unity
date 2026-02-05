# Mixer-Unity (Game) — Project Guide (LLM Quickstart)

This document is optimized so **any LLM (or new dev)** can understand the project quickly: what the game does, where the key flows are, how offline mode works, and which files to edit for a new IP/graphics swap.

---

## High-level concept

The original project is an Axie-based game. The core pipeline is:

**Axie genes (hex) → AxieMixer → Spine SkeletonDataAsset/material → `Axie` runtime object → UI + gameplay**

To support development without API keys, the project has been extended with an **offline mode** that loads Axies/Lands from local assets and stubs network calls.

---

## Tech stack

- **Unity**: 2022.3.x
- **Render pipeline**: **URP** (see `Packages/manifest.json`)
- **Animation**: **Spine** runtime
- **Axie visuals**: **SkyMavis AxieMixer** (gene-based)
- **JSON**: Unity `JsonUtility` + `Newtonsoft.Json` (varies by file)

---

## “Where do I start?” (main entry points)

- **Main scenes**: `Assets/Scenes/Play.unity`, `Assets/Scenes/Play2.unity`
- **Login/bootstrap**: `Assets/Scripts/SkyMavisLogin.cs`
  - Offline mode short-circuits the online login flow.
  - Ensures `SkyMavis.AxieMixer.Unity.Mixer.Init()` is called before building Axie visuals.
- **Core game state**: `Assets/Scripts/AccountManager.cs`, `Assets/RunManagerSingleton.cs`

---

## Data model used across the game

### Axies (runtime DTOs)

- **`Assets/GetAxiesExample.cs`**
  - `GetAxiesExample.Axie`: the “Axie object” used across UI and gameplay.
  - `GetAxiesExample.Part`: part/skill metadata used by combo + ability UI.
    - Important: If `name` is missing, it falls back to `PartFinder` to set a display name + parse `SkillName`.

### Teams

- **`Assets/Scripts/TeamManager.cs`**
  - `AxieTeam`: a saved team with `AxieIds`, `position`, `landTokenId`, etc.
  - Teams are stored in `Application.persistentDataPath` as JSON (`*axieTeams3.json`).

---

## Genes → graphics (AxieMixer)

### The mixer itself

- **`Assets/AxieInfinity/AxieMixerUnity/Components/Builder/Mixer.cs`**
  - `Mixer.Init()` loads resources and sets up the builder.
  - `Mixer.Builder.BuildSpineFromGene(...)` returns:
    - `SkeletonDataAsset`
    - `adultCombo`
    - shared material

### The project’s “spawn/render” orchestrator

- **`Assets/AxieSpawner.cs`**
  - `ProcessMixer(...)`: main entry to build/spawn visuals.
  - `CreateAxie(...)`: creates an `AxieController`, Spine object, skill controller, etc.
  - **Note**: Axie IDs are not assumed numeric anymore (offline IDs like `offline-1`), see `Assets/Scripts/AxieIdUtil.cs`.

### Part + ability lookup

- **`Assets/Scripts/PartFinder.cs`**
  - Loads `Resources/part_states.json` and maps part ids → names.
- **`Assets/Scripts/AxieGeneUtils.cs`**
  - Parses genes into part ids/classes.
  - Computes stats from `AxieClass` + class combo.

---

## Offline mode (how it works)

### Toggle

- **`Assets/Scripts/Offline/OfflineModeSettings.cs`** (ScriptableObject)
- **`Assets/Scripts/Offline/OfflineMode.cs`** (static accessor)
- Assets must be inside **`Assets/Resources/`** so they can be loaded at runtime.

### Offline database

- **`Assets/Scripts/Offline/OfflineAxieDatabase.cs`**
  - Stores offline axies + lands.

### Creating offline assets in-editor

- **`Assets/Editor/Offline/OfflineAssetsMenu.cs`**
  - Menu: `Tools/Offline/Create Offline Mode Assets`
  - Creates:
    - `Assets/Resources/OfflineModeSettings.asset`
    - `Assets/Resources/OfflineAxieDatabase.asset`

### Generating offline axies (no gene typing)

- **`Assets/Editor/Offline/OfflineAxieGeneratorWindow.cs`**
  - Menu: `Tools/Offline/Axie Generator`
  - “FromParts” mode:
    - Dropdowns are sourced from `Assets/Resources/part_states.json`
    - Genes are generated internally via `Genes.FakeAxie512.FakeAxie(...)`

### Loading offline assets at runtime

- **`Assets/Scripts/AccountManager.cs`**
  - `LoadOfflineAssets(...)` fills `AccountManager.userAxies` and `AccountManager.userLands`,
    then calls `LoadGraphicAssets()` per axie so UI works without API.

---

## Networking / “endpoints” (current behavior)

### Opponent fetching / runs

- **`Assets/Scripts/AxieLandBattleTarget.cs`**
  - Online: uses `UnityWebRequest` to hit `run.api.axielandbattles.com`
  - Offline: `GetScoreAsync(...)` returns an **offline-generated opponent** (no network)
    - Opponent is built from your current team (`TeamManager.instance.currentTeam`)
    - Land is randomized offline
    - Enemy positions are generated in “backend space” (then converted by `Team.AddCharacter(...)`)

- **`Assets/Scripts/GetAxiesEnemies.cs`**
  - Consumes the opponent JSON (`Opponent`) and spawns enemy axies.

### Leaderboard

- **`Assets/Scripts/LeaderboardManager.cs`**
  - Offline mode skips network calls to avoid infinite loading.

---

## Gameplay spawning + grid (important nuance)

- **`Assets/LawlessGames/.../Scripts/Team.cs`**
  - `Team.AddCharacter(...)` treats good vs enemy teams differently.
  - Enemy team uses a transform from backend `startingRow/startingCol` into map coordinates:
    - `localX = Abs(startingRow - 7)`
    - `localY = Abs(startingCol - 4)`
  - If you change opponent generation, keep this conversion in mind.

---

## Graphics / shaders (Mac vs Windows)

### Grass shader

- **`Assets/Shaders/BotW Grass/BotWGrass.shader`**
  - Windows path uses tessellation + geometry (procedural blades).
  - Metal (macOS) **cannot** run geometry shaders → a Metal-only fallback exists.
  - The fallback is intentionally “pretty tiles” (animated tint + noise), not real blades.

---

## If you’re changing the IP (graphics + animation): quickest “safe” approach

The fastest path is **not** to delete systems, but to **introduce a thin abstraction layer** so gameplay stops depending on Axie-specific rendering.

### What to avoid

- Don’t refactor everything at once.
- Don’t make gameplay depend directly on genes or AxieMixer APIs.

### Practical plan (lowest risk)

1. **Introduce a neutral unit model**
   - Example: `UnitDefinition` (id, class/type, stats, skills, cosmetic key)
2. **Add a renderer adapter**
   - `IAxieRenderer` (or `IUnitRenderer`) interface:
     - `BuildMenuVisual(UnitDefinition)`
     - `BuildBattleVisual(UnitDefinition)`
   - Implementations:
     - `AxieMixerRenderer` (current: genes → mixer → spine)
     - `NewIPRenderer` (prefab/Animator/Spine/sprites)
3. **Route spawns through the adapter**
   - Primary touchpoints:
     - `Assets/AxieSpawner.cs`
     - `Assets/AxiesManager.cs`
     - Any UI that directly reads `axie.skeletonDataAsset`
4. **Swap data source later**
   - Provider interface:
     - `IUnitDataProvider` (offline DB now, server later)
   - Today you can keep using offline DB and just map it to `UnitDefinition`.

### “Which files will I edit first to change visuals?”

If you want the quickest visible change:

- **Menu axie rendering**: `Assets/AxiesManager.cs` (`InitializeUIAxies`, `ShowMenuAxies`)
- **Battle axie spawning**: `Assets/AxieSpawner.cs` (`ProcessMixer`, `CreateAxie`, enemy spawn paths)
- **Ability/Combo UI dependencies**:
  - `Assets/Scripts/AbilitiesManager.cs`
  - `Assets/Scripts/FakeAxieComboManager.cs`
  - These expect `Axie.parts[]` with valid `SkillName` + display `name`.

If your new IP doesn’t use genes at all, your goal is to make these systems consume:

- `stats`
- `skills`
- `cosmetic renderer key`

…and stop caring about genes/mixer.

---

## Quick checklist (for a future LLM)

- Need offline toggle? → `OfflineModeSettings` + `SkyMavisLogin`
- Need axies offline? → `OfflineAxieDatabase` + `AccountManager.LoadOfflineAssets`
- Need create axies without genes? → `OfflineAxieGeneratorWindow` (FromParts)
- Need fix “axie id must be numeric” bugs? → use `AxieIdUtil.ToStableInt(...)`
- Need opponent offline? → `AxieLandBattleTarget.GetScoreAsync` offline generator
- Need grass on Mac? → Metal fallback in `BotWGrass.shader`

