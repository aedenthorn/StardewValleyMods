# Spell Effects

Spell effects are applied to a variety of objects including the tiles themselves and any entity on a tile in the AOE.

Different effect types will require different data. Generic effect fields are:

| Field | Type | Description |
|:-----|:----:|:------------|
| TriggerAction | string | A [trigger action](https://stardewvalleywiki.com/Modding:Trigger_actions) to trigger for each tile affected, or the center tile if PerTile is false below. Passes the X and Y coordinates as arguments. |
| Affected | string[] | Types of objects affected by the effect. |
| Unaffected | string[] | Types of objects unaffected by the effect (used for sub-types, e.g. Trees vs TerrainFeatures). |
| EffectType | string | The type of effect to apply (see below). |
| Sprites | SpriteData[] | A list of sprites to add directly to each affected object that move with the affected object. See [Sprites](sprites.md). |
| AsFarmer | bool | Whether to act as though the caster themselves was performing the action. This means, for example, that tool spells will consume stamina, gain XP, etc. Default is false. |
| First | bool | Whether to stop after the first instance of each affected type is found. Default is false. |
| PerTile | bool | Whether to apply the effect to each tile in the AOE. Default is true. Only used for explosions. |
| Seconds | int | Set this to a positive integer if you want the effect to reapply every second for the number of seconds specified. |
| ReapplyToTile | bool | Whether reapplied effects over time should be reapplied to the tile (otherwise they will be reapplied to each originally affected object, even if those objects leave the triggering tile). |
| Value | varies | A generic variable used by various effect types for different purposes (see below). |


## Affected Types

You can specify multiple affected types for each effect, and optionally specify unaffected types to exclude sub-types. For example, you could cause the effect to affect all TerrainFeatures except Trees by specifying "TerrainFeature" in Affected and "Tree" in Unaffected. 

Here is a list of implemented affected types:

| Type | Description |
|:----|:------------|
| Animal | farm animals. |
| Building |  |
| Crop |  |
| Farmer | player characters (yes, friendly-fire is possible) |
| FruitTree |  |
| Grass |  |
| HoeDirt | tilled soil |
| Horse |  |
| Monster |  |
| NPC | human NPCs |
| Object | placed objects like furnaces, etc. |
| Pet |  |
| ResourceClump | large four-tile objects like stumps and boulders |
| Stone | breakable rocks  |
| Tile | the tile itself |
| Tree |  |
| Twig |  |
| Weed |  |

Different objects are affected differently by different effect types.


## Effect Types

Here is a list of implemented spell effects:

| Effect | Can Affect | Description |
|:------|:-------:|:------------|
| Buff | Farmer, Monster | Applies a buff to the target (monster buffing is limited atm to those that effect their speed or cause them to freeze). |
| Burn | Tile, TerrainFeature, Crop, Object, ResourceClump | Causes the target to catch fire, being destroyed after a time. |
| Custom | all except Tile | Change a custom C# field or property for the target. |
| Damage | Monster, Farmer | Reduce health as though hit. |
| Destroy | ResourceClump, Crop, Object, TerrainFeature | Remove the target completely. |
| Explode | Tile | Cause an explosion at the given tile. |
| Fertilize | HoeDirt | Apply fertilizer. |
| Freeze | Monster, Farmer | Cause the target to halt for a duration. |
| Grow | Grass, Tree, FruitTree, Crop | Causes the target to grow fully. |
| Heal | Farmer | Increases health. |
| Harvest | Crop, Animal, Grass, FruitTree | Harvest as though done by the farmer. |
| Invincible | Farmer | Make a player temporarily invincible. |
| Index |  Tile | Not yet implemented. |
| Light | Farmer, Tile, Monster | Applies a temporary lightsource attached to the target. |
| ModData | all except Tile | Sets a key value pair in the target's mod data dictionary. |
| Pet | Pet, Animal | Pets the target.  |
| Plant | HoeDirt | Plant a crop at the target. |
| Property | Tile | Not yet implemented. |
| TileSheet | Tile | Not yet implemented. |
| Tool | Tile, TerrainFeature, ResourceClump, Object, Crop | Perform a tool action on the target. |
| Transform |  | Not yet implemented. |
| Water | Tile, HoeDirt, Building | Water the target. |


### Buff

Buff uses the Value field to specify the buff ID to apply. Buffs are defined in the Buffs.json file, and can be added by mods. E.g.:

```
"Effects": [
    {
        "EffectType": "Buff",
        "Affected": ["Monster"],
        "Value": "19"
    }
]
```


### Burn

Burn doesn't use any special fields at the moment.


### Custom

Custom uses the Name field to specify the name of the field or property to change, and the Value field to specify the new value. Works for fields or properties of types int, long, byte, float, double, bool, string, Color, Vector2, Point, and Rectangle and all NetField variants.

E.g.:

```
"Effects": [
    {
        "EffectType": "Custom",
        "Affected": ["Monster"],
        "Name": "stunTime",
        "Value": 10000,
    }
]
```

You can optionally specify "ChangeType", which can be one of the following:

| ChangeType | Value Type | Description | Notes |
|:-----------|:----------:|:------------|:-----|
| Set | same | Sets the field or property to the given value. | default type |
| Toggle | N/A | Toggles the field or property to the opposite of its current value. | boolean only |
| Subtract | same | Subtracts for numbers, vectors, points; removes the substring from strings. | 
| Add | same | | |
| Prefix | string | Adds the value to the beginning of a string. | strings only |
| Multiply | number  | Multiplies the current value by the given value. | used for numbers, vectors, points, rectangles |
| Divide | number | Divides the current value by the given value. | used for numbers, vectors, points, rectangles |
| Replace | string | Replaces the current value with the given value. | strings only |


### Damage

Damage uses the Value field to specify the amount of damage to apply. E.g.:

```
"Effects": [
    {
        "EffectType": "Damage",
        "Affected": ["Monster"],
        "Value": 10
    }
]
```


### Destroy

Destroy doesn't use any special fields.


### Explode

Explode uses the Radius field to specify the radius of the explosion, and optionally the Value field to specify the damage of the explosion (otherwise damage depends on radius). It also uses the Affected field to specify whether monsters or farmers are damaged by the explosion.

E.g.:

```
"Effects": [
    {
        "EffectType": "Explode",
        "PerTile": false,
        "Radius": 5,
        "Value": 10,
        "Affected": ["Farmer", "Monster"]
    }
]
```


### Fertilize

Fertilize uses the Value field to specify the type of fertilizer to apply. E.g.:

```
"Effects": [
    {
        "EffectType": "Fertilize",
        "Affected": ["HoeDirt"],
        "Value": "465"
    }
]
```


### Grow

Grow uses the Value field to specify growth target or grows the object completely if Value is omitted. E.g.:

```
"Effects": [
    {
        "EffectType": "Grow",
        "Affected": ["Tree"],
        "Value": 4
    }
]
```


### Heal

Heal uses the Value field to specify a heal amount. E.g.:

```
"Effects": [
    {
        "EffectType": "Heal",
        "Affected": ["Farmer"],
        "Value": 20
    }
]
```


### Harvest

Harvest doesn't use any special fields.


### Invincible

Invincible uses the Value field to specify the duration of invincibility in milliseconds. E.g.:

```
"Effects": [
    {
        "EffectType": "Invincible",
        "Affected": ["Farmer"],
        "Value": 10000
    }
]
```


### Light

Light uses the following fields:

- Value: the time in milliseconds the light will last
- Radius: the tile radius of the light
- Color: the color of the light.

E.g.:

```
"Effects": [
    {
        "EffectType": "Light",
        "Affected": ["Farmer"],
        "Value": 10000,
        "Radius": 5,
        "Color": {
            "R": 255,
            "G": 0,
            "B": 0,
            "A": 100
        }
    }
]
```


### ModData

ModData uses the Name field to specify the key to set, and the Value field to specify the value to set. E.g.:

```
"Effects": [
    {
        "EffectType": "ModData",
        "Affected": ["Farmer"],
        "Name": "myModKey",
        "Value": "myValue"
    }
]
```


### Pet

Pet doesn't use any special fields.


### Plant

Plant uses the Value field to specify the type of fertilizer to apply. E.g.:

```
"Effects": [
    {
        "EffectType": "Plant",
        "Affected": ["HoeDirt"],
        "Value": "472"
    }
]
```


### Tool

Tool uses the Value field to specify the tool ID to use. E.g.:

```
"Effects": [
    {
        "EffectType": "Tool",
        "Affected": ["Stone"],
        "Value": "(T)IridiumPickaxe",
    }
]
```


### Water

Water doesn't use any special fields.
