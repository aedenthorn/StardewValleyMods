# AOE Spells
## Creating Spells
Spells are added to the mod using Content Patcher, e.g.:

```
{
    "Action": "EditData",
    "Target": "aedenthorn.AreaOfEffect/spells",
    "Entries": {
        "Fireball": {
            "DisplayName": "Fireball",
            "Sequence": [
                "Up",
                "DownRight",
                "UpRight",
                "Down"
            ],
            "SetSound": "fireball",
            "SpellLevels": [
                {
                    "CastSound": "fireball",
                    "TriggerSound": "fireball",
                    "Charges": 1,
                    "AreaType": "Circle",
                    "Radius": 3,
                    "Buffs": [
                        "12"
                    ],
                    "Projectiles": [
                        {
                            "SpriteIndex": 10
                        }
                    ],
                    "Sprites": [
                        {
                            "Type": "Fire"
                        }
                    ],
                    "Effects": [
                        {
                            "Affected": [
                                "Monster",
                                "Farmer"
                            ],
                            "EffectType": "Damage"
                            "Value": 20
                        },
                        {
                            "Affected": [
                                "Twig",
                                "Weed",
                                "Grass",
                                "Tree"
                            ],
                            "EffectType": "Burn"
                        }
                    ]
                }
            ]
        }
    }
}
```


## Field Explanation

| Field | Type | Description |
|:-----:|:----:|:------------|
| DisplayName | string | The name of the spell as it will appear in the game. |
| Sequence | string[] | The sequence of directions the player must input to cast the spell. Valid directions are "Up", "Down", "Left", "Right", "UpLeft", "UpRight", "DownLeft", and "DownRight". |
| SetSound | string | The sound that plays when the spell is set (i.e., when the player inputs the sequence). |
| SpellLevels | SpellLevel[] | An array of spell levels, each defining the properties of the spell at the casting tool's current charge level (see spell properties below). |
| AddTutorial | bool | If true (or omitted), instructions for casting the spell will be added to the tutorial list (requires the [Tutorials mod](https://www.nexusmods.com/stardewvalley/mods/48555) to be installed to have any effect). |


## Spell Properties

| Field | Type | Description |
|:-----:|:----:|:------------|
| CastSound | string | The sound that plays when the spell is cast. |
| TriggerSound | string | The sound that plays when the spell is triggered (for spells with projectiles, when it hits an object or reaches the target tile). |
| TriggerAction | string | A [trigger action](https://stardewvalleywiki.com/Modding:Trigger_actions) to trigger on casting this spell. |
| Charges | int | The number of charges consumed when casting the spell at this level. |
| AreaType | string | The type of area affected by the spell. Valid values are "Circle", "Square", and "Line". "Line" requires a projectile and causes its effect as the projectile travels. |
| Radius | int | The tile radius of the area affected by the spell, besides the target tile for "Circle" and "Square" area types. A radius of 0 only affects the target tile. |
| Buffs | string[] | An array of buff IDs that are applied to the player when the spell is cast. |
| Projectiles | Projectile[] | An array of projectiles that are spawned when the spell is cast. See [Projectiles](projectiles.md) for details. |
| Sprites | Sprite[] | An array of visual sprites that are displayed when the spell is cast. See [Sprites](sprites.md) for details. |
| Effects | Effect[] | An array of effects that are applied to targets when the spell is cast. See [Effects](effects.md) for details. |
