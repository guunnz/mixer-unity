# VanillaMons Sprite Overrides

Runtime sprite lookup starts from this `Resources` folder.

## Body Construction

Per-class mon pieces live in:

`Parts/<Class>/<part>.png`

Supported classes:

`Beast`, `Bug`, `Bird`, `Reptile`, `Plant`, `Aquatic`, `Mech`, `Dawn`, `Dusk`

Supported body construction sprites:

`body`, `belly`, `highlight`, `face`, `eye`, `eye-shine`, `shadow`, `front-foot`, `back-foot`, `attack-flash`, `eyes`, `ears`, `horn`, `mouth`, `back`, `tail`

## Ability Icons

Per-ability body-part sprites live in:

`Abilities/<Class>/<SkillName>.png`

Example:

`Abilities/Plant/Pumpkin.png`

`MonsterBodyPart.bodyPartSprite` can also be assigned directly in the inspector for a single ability override. If no ability sprite is assigned or found, the game falls back to the class/body-part sprite.
