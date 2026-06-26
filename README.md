# Monster Land Battles

This project now uses the vanilla monster visual layer.

The imported mixer, generated gene art, and skeletal rendering stack have been removed. Gameplay data still flows through the existing monster domain models: class, stats, body parts, skills, targeting, battle state, and UI screens.

## Visuals

- `VanillaMonsterVisual` renders world actors with class-colored sprites, anchors, sorting, visibility, and debug state motion.
- `VanillaMonsterGraphic` renders UGUI previews for selection, captain, leaderboard, tooltip, combo, and results screens.
- `MonsterVisualState` is the shared visual-state enum used by skills and battle actors.
- `MonsterClassPalette` keeps class colors in one place.

These visuals are intentionally simple placeholders so final art or a different runtime can be plugged in later without changing battle logic.
