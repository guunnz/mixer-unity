# Graphics Swap Plan (New IP) — Prefab + Animator + Sprite Parts

This document describes the **quickest practical path** to replace Axie/Spine visuals with a **Unity prefab + Animator** while keeping gameplay logic intact.

The goal: **don’t rewrite the game**, only replace the rendering/animation implementation behind a small “visual wrapper”.

---

## Target architecture (what you described)

### 1) Create a character prefab with an Animator

You will create a prefab, e.g. `Assets/NewIP/Prefabs/Character.prefab`, that contains:

- **Root**: `Character`
  - `Animator` (drives animation states)
  - `NewIpCharacterView` (script that connects parts + animator)
- **Child transforms** (one per body part):
  - `Body` (SpriteRenderer)
  - `Eyes` (SpriteRenderer)
  - `Ears` (SpriteRenderer)
  - `Mouth` (SpriteRenderer)
  - `Horn` (SpriteRenderer)
  - `Back` (SpriteRenderer)
  - `Tail` (SpriteRenderer)
  - Optional: `Shadow`, `FX`, etc.

Animation clips in the Animator should cover the same gameplay “verbs” the code currently expects:

- **Idle** (loop)
- **Run** (loop)
- **Attack** (or multiple attacks, optional)
- **Hit / Hurt** (optional)
- **Death** (optional)
- **Victory** (optional)

> Tip: If your existing code assumes specific animation names (Spine strings), we’ll map those to Animator state names.

---

### 2) Add a script that references Animator + SpriteRenderers for parts

Create a script (example name):

- `NewIpCharacterView`:
  - Holds references to the `Animator`
  - Holds references to each body-part `SpriteRenderer`
  - Provides methods to:
    - Set sprites by “part id”
    - Play animations (idle/run/attack/etc.)
    - Flip left/right

This script is purely “view-layer”.

---

### 3) Create a wrapper interface so gameplay doesn’t care if it’s Spine or Animator

Create a small interface, e.g.:

- `ICharacterVisual`
  - `SetParts(PartIds ids)` (or equivalent)
  - `PlayIdle()`
  - `PlayRun()`
  - `PlayAttack(AttackType type)` (or `PlayAttack(string key)`)
  - `PlayDeath()`
  - `SetFacing(bool faceRight)`
  - Optional: `SetSortingOrder(int order)` / `SetMaterial(...)`

Then implement it twice:

- **Spine implementation (existing)**: `SpineCharacterVisual`
  - Wraps `SkeletonAnimation` / `SkeletonGraphic`
  - Calls `AnimationState.SetAnimation(...)`
- **Animator implementation (new)**: `AnimatorCharacterVisual`
  - Wraps `NewIpCharacterView`
  - Calls `Animator.Play(...)` or uses parameters/triggers

With this, your gameplay code changes from:

> “set `SkeletonAnim.AnimationName = ...`”

to:

> “`visual.PlayRun()` / `visual.PlayIdle()`”

---

## Where to plug this into THIS project (high leverage files)

### Battle spawns (highest leverage)

- `Assets/AxieSpawner.cs`
  - Today: creates `SkeletonAnimation` from AxieMixer builder result
  - Target: instantiate your character prefab, attach `AnimatorCharacterVisual`, and assign it to the controller

### Runtime animation switching

- `Assets/LawlessGames/.../AxieController.cs`
  - Today: sets `SkeletonAnim.AnimationName = "action/run"` etc.
  - Target: call the wrapper (`visual.PlayRun()`, `visual.PlayIdle()`)

- `Assets/LawlessGames/.../AxieBehavior.cs`
  - Today: selects attack animation strings based on class and calls Spine
  - Target: decide an `AttackType` and call `visual.PlayAttack(type)`

### UI previews (menu/team selection)

You have UI code that currently uses `SkeletonGraphic`:

- `Assets/AxiesManager.cs` (menu axies list)
- `Assets/Scripts/AbilitiesManager.cs` / `Assets/Scripts/FakeAxieComboManager.cs` (combo/abilities UI)

You have two options:

- **Option 1 (fastest)**: keep UI as simple sprites (thumbnail) and don’t render full animated characters in UI.
- **Option 2 (still reasonable)**: render prefab into a RenderTexture (camera) for UI.

---

## How “part ids” should work (so swapping is easy)

The new IP should have its own part identifier system. Example:

- `BodyId`, `EyesId`, `EarsId`, `MouthId`, `HornId`, `BackId`, `TailId`

The important thing is: the gameplay should only store these IDs, not sprite references.

### Sprite selection

Create a `ScriptableObject` database, e.g.:

- `NewIpPartDatabase`
  - For each body part type: map `string partId` → `Sprite`

Then `NewIpCharacterView.SetParts(...)` simply looks up sprites and assigns them to each `SpriteRenderer`.

---

## Minimal step-by-step implementation plan

1. **Build one working prefab**
   - Make `Character.prefab` with Animator and 2–3 clips (Idle/Run/Attack)
2. **Add `NewIpCharacterView`**
   - Hardwire sprites first just to prove animation works
3. **Add `ICharacterVisual` + `AnimatorCharacterVisual`**
4. **Modify only the spawn path**
   - In `AxieSpawner.CreateAxie(...)`, instantiate prefab and set `controller.visual = ...`
5. **Replace direct Spine animation calls**
   - Update `AxieController` and `AxieBehavior` to call `visual.PlayX()`
6. **Part selection**
   - Add `NewIpPartDatabase` and implement `SetParts(...)`
7. **Optional UI**
   - Decide if you want full animated UI previews or simple thumbnails first

---

## What you do NOT need to rewrite (initially)

Keep these stable while swapping visuals:

- Teams: `TeamManager`, saving/loading teams
- Stats & skills: `GetAxiesExample.Stats`, `SkillName` (temporary)
- Combat logic: `AxieController`, `Team`, targeting, pathing
- Offline mode: offline DB, offline opponent generator

Later, once visuals are swapped, you can replace:

- Genes and AxieMixer dependencies
- Axie-specific naming (`AxieClass`, `SkillName`) with your new IP equivalents

---

## Key principle

**One adapter layer buys you the whole swap.**

If you keep gameplay calling `ICharacterVisual`, you can change animation systems (Spine → Animator → anything) without touching gameplay again.

