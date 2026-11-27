# Field Descriptions

This document describes all possible fields that can be used for Custom Monsters dictionary entries at `aedenthorn.CustomMonsters/dict`. For an example of an actual Content Patcher content.json file, [click here](https://github.com/aedenthorn/StardewValleyMods/blob/master/CustomMonsters/doc/content.json).


## General

### Type
The vanilla monster type to base the custom monster type on. Possible values:

	AngryRoger
	Bat
	BigSlime
	BlueSquid
	Bug
	DinoMonster
	Duggy
	DustSpirit
	DwarvishSentry
	Fly
	Ghost
	GreenSlime
	Grub
	HotHead
	LavaLurk
	Leaper
	MetalHead
	Mummy
	RockCrab
	RockGolem
	Serpent
	ShadowBrute
	ShadowGirl
	ShadowGuy
	ShadowShaman
	Shooter
	Skeleton
	Spiker
	SquidKid

### Parameters
comma-separated list of the parameters that describe the C# constructor used to instantiate the custom monster type (default "position"). Possible parameter types:

- position - refers to the position passed to the API by your spawning mod (e.g. Dynamic Map Tiles)
- level - taken from the Level field
- facing - taken from the Facing field
- name - taken from the Name field
- color - taken from the Color field
- switch - taken from the Switch field

Some of these will work differently for different monster types. See each corresponding field for more info.

[A list of constructors is here.](https://github.com/aedenthorn/StardewValleyMods/blob/master/CustomMonsters/doc/ctor.txt)


### Level
The level of the monster, sometimes corresponding to a mine level, sometimes to a range of mine levels. See [the constructor data](https://github.com/aedenthorn/StardewValleyMods/blob/master/CustomMonsters/doc/ctor.txt) for hints. Will be passed to constructors with "level" parameter. E.g. MetalHead: "position,level".
### Facing
The facing direction (0 = up, 1 = right, 2 = down, 3 = left). For Spikers it's the direction they will sense players and move to hit them. Will be passed to constructors with "facing" parameter.
### Name
The custom subtype used for certain monsters, e.g. Serpent has "Royal Serpent". Will be passed to constructors with "name" parameter.
### Color
The color used for certain monsters, e.g. slimes. Will be passed to constructors with "color" parameter but will affect GreenSlimes, BigSlimes, and MetalHeads no matter the constructor used. Must be in the hex-based format "#RRGGBB". E.g. "#FF0000" for red.
### Colors
Alternatively, a list of colors to randomly choose from for each monster instance. Will be passed to constructors with "color" parameter but will affect GreenSlimes, BigSlimes, and MetalHeads no matter the constructor used. Each entry has a Chance field for comparative weight of the entry, and can have either a "Color" field with a hex-based format above or R, G, and B fields with Min and Max values. E.g.:
	"Colors":[
		{
			"Color":"#FF0000",
			"Chance":10
		},
		{
			"R":
			{
				"Min":200,
				"Max":255,
			},
			"G":
			{
				"Min":0,
				"Max":0,
			},
			"B":
			{
				"Min":0,
				"Max":0,
			},
			"Chance":10
		}
	]
R, G, and B are the red, green, and blue values for the color, and Min and Max can be between 0 and 255. 
### Switch
A true or false switch that does very different things for different monster types. See [the constructor data](https://github.com/aedenthorn/StardewValleyMods/blob/master/CustomMonsters/doc/ctor.txt) for more info. Will be passed to constructors with "switch" parameter.

### Sprite
a path to a custom or vanilla sprite. Custom paths must be implemented using Content Patcher.
### Scale
scale multiplier (causes problems for non-flying monsters if > 1).
### Damage
damage to farmer.
### HardMode
used to change the nature of some monster types, mimicking their changes for hard mode dungeons.
### Health
monster max health.
### Speed
monster speed.
### Experience
experience gained for killing the monster.
### Resilience
monster resilience - reduces damage taken.
### MissChance
percent chance to miss monster when attacking.

### Jitteriness
monster jitteriness - causes them to change direction.
### Slipperiness
monster slipperiness - affects velocity.
### LightType
an index for the vanilla light type used by certain monsters (not really implemented yet).

### HideShadow
whether to hide the monster's shadow.
### Despawn
optional field to force this monster to despawn. Can be "Day" to despawn at the end of the day or a number representing the time in 24 hour format (e.g. 1300 for 1:00 PM) to despawn at that time of day. Without this field monsters outside of the mines will not despawn unless they are killed.

### PlayerThreshold
max tile distance from the player for the monster to begin acting (actions vary per monster).

### Drops
a list of items to replace the base type's item drops. Each item takes the following fields:

- ItemId - the qualified item id to drop.
- MinQuantity - minimum amount to drop.
- MaxQuantity - maximum amount to drop.
- Quality - dropped item quality.
- Chance - percent chance to drop (default 100).

e.g.:

		  "Drops":[
			  {
				  "ItemId":"(O)74",
				  "MinQuantity": 1,
				  "MaxQuantity":4,
				  "Chance":10
			  }
		  ]
		  
This causes the monster to have a 10% chance to drop 1-4 prismatic shards.
### ClearDrops
whether to remove the base type's item drops (default: true).


### MoveSound
sound when certain monsters move.
### MoveSound2
secondary move sound for a few monsters (e.g. when Spikers hit something).
### SpawnSound
sound to play when the monster spawns.
### DamageSound
sound when the monster gets hit.
### DamageSound2
secondary sound on hit in combination with the first for a few monsters.
### ArmorSound
sound made when hitting the armor of some monster types (e.g. bugs)
### DeathSound
sound to play when the monster dies.
### DeathSound2
secondary sound on death in combination with the first for a few monsters.
### ContactSound
sound to play when contacting farmer (green slime).

### ProjectileSound
sound when firing a projectile.
### ProjectileSound2
secondary projectile sound for some monsters (e.g. on bounce).
### ProjectileIndex
the vanilla index for certain monster projectiles (see the game's TileSheets/Projectiles file).
### ProjectileDebuff
custom debuff for existing debuff projectiles.
### ProjectileSprite
a custom sprite path for a monster's projectile.
### ProjectileSource
a custom source rectangle for a projectile sprite.
### ProjectileScale
custom scale for certain projectiles (e.g. DinoMonster).
### ProjectileDamage
damage value for some projectiles.
### ProjectileRange
range of some projectiles.

### MineSpawns
List of entries for causing this monster type to spawn on specific mine floors. Each entry takes the following fields:

- MinLevel - the minimum mine level for the monster to spawn.
- MaxLevel - the maximum mine level for the monster to spawn.
- MinDifficulty - the minimum mine floor difficulty for the monster to spawn (ordinary mine floors are 0 difficulty).
- MaxDifficulty - the maximum mine floor difficulty for the monster to spawn (ordinary mine floors are 0 difficulty).
- Chance - percent chance for the monster to spawn instead of a given vanilla monster.
- Types - optional list of vanilla monster types to replace with this custom monster type (see [Type](https://github.com/aedenthorn/StardewValleyMods/blob/master/CustomMonsters/doc/doc.md#type) above). If omitted, can spawn instead of any vanilla monster. 

### VolcanoSpawns
List of entries for causing this monster type to spawn on specific mine floors. Each entry takes the following fields:

- MinLevel - the minimum volcano level for the monster to spawn.
- MaxLevel - the maximum volcano level for the monster to spawn.
- Chance - percent chance for the monster to spawn instead of a given vanilla monster.
- Types - optional list of vanilla monster types to replace with this custom monster type (see [Type](https://github.com/aedenthorn/StardewValleyMods/blob/master/CustomMonsters/doc/doc.md#type) above). If omitted, can spawn instead of any vanilla monster. 

### LocationSpawns
List of entries for causing this monster type to spawn in specific locations. Each entry takes the following fields:

- Location - the name of the location for the monster to spawn in (e.g. "Forest" or "Mountain").
- MinSpawn - the minimum number of this monster type to spawn at once.
- MaxSpawn - the maximum number of this monster type to spawn at once.
- MaxAmount - the max number of this monster type that can be present in the location at once (if omitted, no limit).
- MinTile - the minimum x,y tile coordinates for the monster to spawn at.
- MaxTile - the maximum x,y tile coordinates for the monster to spawn at.
- Respawn - the period after which to trigger a respawn. Can be one of "Daily", "Hourly", or "TenMinutes".

Example:

    "LocationSpawns": [
        {
            "Location": "Forest",
            "MinSpawn": 1,
            "MaxSpawn": 3,
            "MaxAmount": 10,
            "MinTile": {
                "X": 0,
                "Y": 0
            },
            "MaxTile": {
                "X": 100,
                "Y": 100
            },
            "Respawn": "Daily"
        }
    ]

## AngryRoger specific

### SprinkleColor
color of sprinkles.


## Bat specific

### CanLunge
lunges
### CursedDoll
is a cursed doll.
### HauntedSkull
is a haunted skull (requires "CursedDoll": true to work).
### SpeedMin
min value for max speed.
### SpeedMax
max value for max speed.
### ExtraVelocity
extra speed.


## BigSlime specific

### HeldItem
the item to display inside the monster.


## Bug specific

### Armored
whether the bug is armored.


## GreenSlime specific

### Stacks
a list of possible stack sizes for the slime to spawn with. Each entry takes the following fields:
- Chance - weight for the slime to spawn with this stack size (relative to the other entries in the list).
- MinStacks - min number of stacks for the slime to spawn with.
- MaxStacks - max number of stacks for the slime to spawn with.

e.g.
	"Stacks":[
		{
			"Chance":4,
			"MinStacks":1,
			"MaxStacks":1
		},
		{
			"Chance":3,
			"MinStacks":2,
			"MaxStacks":3
		},
		{
			"Chance":1,
			"MinStacks":3,
			"MaxStacks":5
		}
	]

This makes it so the slime has a 4/8 chance to spawn with 1 stack, a 3/8 chance to spawn with 2 - 3 stacks, and a 1/8 chance to spawn with 3 - 5 stacks (total of all chances is 8).
### ChildhoodLength
how long until a baby grows up (this value is halved every day until it reaches 0).
### Prismatic
this will override the monster's color, making it prismatic instead. Also affects vanilla drops.
### ContactDebuff
the buff id applied to farmers hit by this monster.


## Mummy specific

### CrumbleSound
sound when the monster crumbles.
### UncrumbleSound
sound when the monster gets back up.
### ReviveTimer
time in ms to revive. Vanilla is 10000.


## RockCrab specific

### WaiterChance
chance to wait.
### ShellSound
sound of hitting the shell with a pickaxe.
### BreakSound
sound to make when the shell breaks.
### StickBug
true or false, is a stick bug.


## Serpent specific

### MinSegment
minimum number of segments for a royal serpent.
### MaxSegment
maximum number of segments for a royal serpent.


## Shooter specific

### ProjectileSpeed
speed of projectiles.
### ShotsPerFire
shots per fire.
### AimTime
time to aim when a player moves.
### BurstTime
time between shots.
### AimEndTime
time to aim after firing finishes.
### DesiredDistance
wants to be this far from players.
### FireRange
max firing range.

## Spiker specific

### Vulnerable
whether monster can be damaged.


## SquidKid specific

### ProjectileCount
how many projectiles to fire in one period.
### ProjectileTimer
how long in ms to wait between fire periods.






