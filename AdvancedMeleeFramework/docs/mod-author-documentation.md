# Advanced Melee Framework

This documentation outlines the things you need to know to start making content packs for Advanced Melee Framework

This document is mostly based off of the original article outlining how to make content packs, which can be found [here](https://www.nexusmods.com/stardewvalley/articles/629)

A full example pack using most available fields can be found [here](../examples/example%20pack%201/)

* [Getting Started](#getting-started)
* [Weapons](#weapons)
* [Enchantments](#enchantments)
    * [Vanila Enchantment Types](#vanila-enchantment-types)
    * [Custom Enchantment Types](#custom-enchantment-types)
    * [Parameters](#parameters)
* [Frames](#frames)
* [Special Effects](#special-effects)
    * [Effect Names](#effect-names)
    * [Parameters](#parameters-1)
* [Projectiles](#projectiles)
* [Configuration](#configuration)
* [C# Api](#c-api)
* [Notes](#notes)

## Getting Started

To get started with AMF, start by installing the mod like you would with any other.

Once it is installed, create a folder somewhere on your computer with the name of your mod.

Open this folder and create the following files:

* ``manifest.json``
* ``content.json``

If at any point you add configurable values (will be explained further down), the mod will generate a ``config.json`` file when it is loaded.

Start by opening the ``manifest.json`` file. This file tells SMAPI some important information about your mod. For details about all of the required fields, you should read [this page](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Manifest).

Since you're making a content pack, add the following to the manifest:

```
"ContentPackFor": {
    "UniqueID": "aedenthorn.AdvancedMeleeFramework",
    "MinimumVersion": "0.8.2-unofficial.4-mindmeltmax"
}
```

This will tell SMAPI to treat this mod as a content pack for this AMF. The minimum version doesn't need to be up-to-date with the mods latest version, only with the version you're using to create the content pack.

The ``content.json`` file should look like this before any content is added:

```
{
    "weapons": [
        
    ]
}
```

Any content will be added inside the ``[ ]``.

When this documentation speaks of an object, it means it should be surrounded by ``{ }`` in the JSON.
When this documentation speaks of a list, it means it should be surrounded by ``[ ]`` in  the JSON.


(**Hint**: as of version 0.8.2-unofficial.4, the initial ``content.json`` can be shortened to just ``[ ]`` and still work the same)

## Weapons

Weapon skills are objects added directly to the ``weapons`` field.

It has the following fields:
* **"id"**: The ItemId of a weapon, or the name (for JSON Assets weapons). This is ignored when the **type** field is set.
* **"type"**: There are 3 valid values for this field: **1** (Daggers), **2** (Clubs), **3** (Swords). Setting this field will make the other fields of this weapon skill apply to every weapon in the given category.
* **"enchantments"**: A list of [Enchantments](#enchantments) attached to this weapon skill.
* **"skillLevel"**: The minimum combat skill level required to use this weapon skill. Skills requiring higher levels supersede those requiring lower levels.
* **"cooldown"**: How long it will take before this weapon skill can be used again.
* **"frames"**: A list of [Frames](#frames) attached to this weapon skill.
* **"config"**: An object with fields which point to user configurable values. (see [the configuration section below](#configuration))

## Enchantments

Enchantments are objects added to weapon skills inside of the ``enchantments`` field.

It has the following fields:
* **"name"**: A unique name for the enchantment (Hint: use ``Mod Name/enchantment name``).
* **"type"**: The type of the enchantment, available enchantments can be found below.
* **"parameters"**: An object with fields passed along for the enchantment. The available values for this can be found [here](#parameters).
* **"config"**: An object with fields which point to user configurable values. (see [the configuration section below](#configuration))

### Vanila Enchantment Types

The following are ids for vanila enchantment types. These do not accept parameter values.

* **"jade"**
* **"aquamarine"**
* **"topaz"**
* **"amethyst"**
* **"ruby"**
* **"emerald"**
* **"vampiric"**
* **"haymaker"**
* **"bugkiller"**
* **"crusader"**
* **"magic"**

For more information on these enchantments, read [the wiki page](https://stardewvalleywiki.com/Forge#Enchantments)

### Custom Enchantment Types

The following are ids for custom enchantment types. These accept parameter values.

* **"heal"**: Heal the user based on given parameters.
* **"hurt"**: Hurt the user based on given parameters.
* **"loot"**: Drop extra loot or a specific item when a monster is slain, based on given parameters.
* **"coins"**: Give coins to the user based when hitting or slaying a monster, based on given parameters.

### Parameters

An object which contains fields used by custom enchantment types to determine things like when to trigger them, the chance, sound, and more.

It has the following fields:
* **"trigger"**: When the enchantment should be triggered. Available values for this are: **"slay"** (when a monster is slain), **"damage"** (when damage is dealt), **"crit"** (when a critical strike is dealt). Applicable to all custom enchants.
* **"chance"**: The chance percentage of an enchantment to trigger. Applicable to all custom enchants.
* **"amountMult"**: How much should be healed / dropped based on monster max hp (for trigger **"slay"**) or damage dealt (for trigger **"damage"** and **"crit"**). Applicable to **"heal"**, **"hurt"**, and **"coins"**.
* **"sound"**: The name of the sound cue to play on trigger. Applicable to all custom enchants.
* **"extraDropChecks"**: How many extra times to check for loot when a monster is slain (use either this **or** **"extraDropItems"**). Applicable to **"loot"**.
* **"extraDropItems"**: Add specific items to monster loot drops when a monster is slain (use either this **or** **"extraDropChecks"**). Applicable to **"loot"**.

For **"extraDropItems"** provide a comma seperated string in one of the following patterns:
* Unqualified (e.g. without the item type definition id like ``(O)`` or ``(BC)``) object item ids like: ``"378,380,384"``.
* Item ids as shown above with a percentage drop chance, seperated by an underscore (``_``) like: ``"378_100,380_50,384_20"``
* Item ids as shown above with a minimum amount, maximum amount, and percentage drop chance, seperated by an underscore (``_``) like: ``"378_3_5_100,380_2_4_50,384_1_3_20"``

Currently **"extraDropItems"** only accepts item ids for object types and bigcraftable types. These can be found in either ``Data\\Objects`` or ``Data\\BigCraftables``. Modded items are accepted, providing they are of this type (check if the item ids are prefixed with ``(O)`` or ``(BC)``). This is a known limitation that will be fixed in an upcoming update.

## Frames

Frames are objects added to weapon skills inside of the ``frames`` field.

It has the following fields:
* **"frameTicks"**: How many ticks to wait before starting the next frame.
* **"invincible"**: When true, makes the user invincible for a few seconds. When false, this is skipped (default false).
* **"action"**: Allowed values are: 0 (None), 1 (Regular Attack), 2 (Special attack (i.e. block for sword, slam for hammer, and stab for dagger)).
* **"special"**: A [Special Effect](#special-effects) attached to this frame.
* **"relativeFacingDirection"**: How may 90 degree clockwise turns to make from the original facing direction for this frame.
* **"trajectoryX"**: Set the users horizontal trajectory relative to the users facing direction, as if the user were facing down. (More than 0: Move to the left; Less than 0: Move to the right).
* **"trajectoryY"**: Set the users vertical trajectory relative to the users facing direction, as if the user were facing down. (More than 0: Move down; Less than 0: Move up).
* **"sound"**: The name of the sound cue to play for this frame. (Remember: Attacks have their own sound already).
* **"projectiles"**: A list of [Projectiles](#projectiles) attached to this frame.
* **"config"**: An object with fields which point to user configurable values. (see [the configuration section below](#configuration))

## Special Effects

Special Effects are objects added to a frame's ``special`` field.

It has the following fields:
* **"name"**: The name of the special effect to trigger. Available values can be found below.
* **"parameters"**: An object with fields passed along for the Special Effect. Available values can be found below.
* **"config"**: An object with fields which point to user configurable values. (see [the configuration section below](#configuration))

### Effect Names

The following are names of recognized special effect types.

* **"lightning"**: Creates a lighining strike based on the given parameters.
* **"explosion"**: Creates an explosion based on the given parameters.

### Parameters

An object with fields used by special effects.

It has the following fields:
* **"damageMult"**: The amount of damage to inflict based on the weapon's min and max damage. (If you want to set Min/Max damage manually, use **"minDamage"** and **"maxDamage"** instead).
* **"minDamage"**: The minimum amount of damage to inflict when a monster is hit.
* **"maxDamage"**: The maximum amount of damage to inflict when a monster is hit.
* **"radius"**: How big the area of the effect will be.
* **"sound"**: The name of the sound cue to play for this effect.

## Projectiles

Projectiles are objects added inside of a frame's ``projectiles`` field.

It has the following fields:
* **"damage"**: When added to a specific weapon: The damage to inflict on contact; When added to a weapon type: The multiplier of the weapons damage to inflict on contact.
* **"parentSheetIndex"**: Which graphic to use for the sprite (taken from ``TileSheets\\Projectiles``).
* **"bouncesTillDestruct"**: The amount of times a projectile will bounce of a surface before being destroyed. (Default: **0**, don't ricochet).
* **"tailLength"**: Extend the projectile sprite by smaller variations of the sprite.
* **"rotationVelocity"**: How fast a projectile sprite will spin.
* **"xVelocity"**: The horizontal travel speed relative to the users facing direction, as if the user were facing down. (More than 0: Move to the left; Less than 0: Move to the right).
* **"yVelocity"**: The vertical travel speed relative to the users facing direction, as if the user were facing down. (More than 0: Move down; Less than 0: Move up).
* **"startingPositionX"**: The horizontal starting position from the center of the user, relative to the users facing direction, as if the user were facing down. (More than 0: Move to the left; Less than 0: Move to the right).
* **"startingPositionY"**: The vertical starting position from the center of the user, relative to the users facing direction, as if the user were facing down. (More than 0: Move down; Less than 0: Move up).
* **"collisionSound"**: The name of the sound cue to play when this projectile collides with something.
* **"bounceSound"**: The name of the sound cue to play when this projectile bounces of something.
* **"firingSound"**: The name of the sound cue to play when this projectile is fired.
* **"explode"**: Whether or not the projectile should explode on impact.
* **"damagesMonsters"**: Whether or not the projectile does damage to monsters.
* **"shotItemId"**: A qualified item id (meaning, an item id prefixed with a type definition id. i.e. ``(O)174``) which when set, will use the sprite for that item instead of the standard projectiles spritesheet. Setting this will mean **"parentSheetIndex"** will be ignored.
* **"config"**: An object with fields which point to user configurable values. (see [the configuration section below](#configuration))

## Configuration

A lot of the fields mentioned above can be made configurable. To do this, simply add the field name you want to configure to the config and set it's value to the name of the field in the ``config.json``.

A full example can be found [in the provided example content pack](../examples/example%20pack%201/).

Remember to not include your ``config.json`` file in the mod zip you upload to avoid overriding user settings.

## C# Api
As of version ``0.8.2-unofficial.4-mindmeltmax`` the C# api has been expanded to allow for custom enchantments (like heal, hurt, etc.) and custom special effects (like lightning and explosion).

The api and it's documentation can be found [here](../AdvancedMeleeFrameworkApi.cs). The mod implements the default enchantments and effects in the same way so for examples, see [the modentry](../ModEntry.cs)

## Notes

For a full list of available sound cues, read [this page](https://stardewvalleywiki.com/Modding:Audio#Sound). (Not fully up-to-date with 1.6 sounds).

For a comprehensive list of qualified item ids, I recommend [this page](https://mateusaquino.github.io/stardewids/) which has sections for each item category with qualified ids (object ids are not qualified, these should be prefixed with ``(O)``).

If at any point you want to reload all weapon packs, the mod provides a reload button which can be found in the config (default is Numpad 0).

Full credit to [aedenthorn](https://www.nexusmods.com/stardewvalley/users/18901754) for creating the mod, and the original documentation.