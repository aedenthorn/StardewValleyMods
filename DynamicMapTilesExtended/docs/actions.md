Actions are (mostly) things that happen when a trigger is triggered.

When adding map properties directly (as opposed to adding a field to the DMT dictionary), the format of the map property key should be:

DMT/{ActionName}{_Once/Swap}{TriggerName}

Once and Swap are optional.

DMT currently recognizes the following action keys (triggers are listed [here](https://github.com/aedenthorn/StardewValleyMods/wiki/Dynamic-Map-Tiles-Extended-Triggers)):

## Map-Related Actions

- [DMT/action](#DMTaction)
- [DMT/addLayer](#DMTaddLayer)
- [DMT/addTilesheet](#DMTaddTilesheet)
- [DMT/barrier](#DMTbarrier)
- [DMT/changeIndex](#DMTchangeIndex)
- [DMT/changeMultipleIndex](#DMTchangeMultipleIndex)
- [DMT/changeMultipleProperties](#DMTchangeMultipleProperties)
- [DMT/changeProperties](#DMTchangeProperties)
- [DMT/push](#DMTpush)
- [DMT/pushable](#DMTpushable)
- [DMT/pushAlso](#DMTpushAlso)
- [DMT/pushOthers](#DMTpushOthers)

## Player-Related Actions

- [DMT/addQuest](#DMTaddQuest)
- [DMT/appearance](#DMTappearance)
- [DMT/buff](#DMTbuff)
- [DMT/emote](#DMTemote)
- [DMT/friends](#DMTfriends)
- [DMT/health](#DMThealth)
- [DMT/healthPerSecond](#DMThealthPerSecond)
- [DMT/healthPerSecondContinuous](#DMThealthPerSecondContinuous)
- [DMT/give](#DMTgive)
- [DMT/mailbox](#DMTmailbox)
- [DMT/mail](#DMTmail)
- [DMT/mailRemove](#DMTmailRemove)
- [DMT/makeover](#DMTmakeover)
- [DMT/makeoverGendered](#DMTmakeoverGendered)
- [DMT/move](#DMTmove)
- [DMT/removeQuest](#DMTremoveQuest)
- [DMT/slippery](#DMTslippery)
- [DMT/speed](#DMTspeed)
- [DMT/stamina](#DMTstamina)
- [DMT/staminaPerSecond](#DMTstaminaPerSecond)
- [DMT/staminaPerSecondContinuous](#DMTstaminaPerSecondContinuous)
- [DMT/teleport](#DMTteleport)
- [DMT/teleportTile](#DMTteleportTile)
- [DMT/take](#DMTtake)
- [DMT/transmog](#DMTtransmog)
- [DMT/transmogGendered](#DMTtransmogGendered)
- [DMT/warp](#DMTwarp)
- [DMT/wardrobe](#DMTwardrobe)
- [DMT/wardrobeGendered](#DMTwardrobeGendered)

## Visual and Audio Actions

- [DMT/animation](#DMTanimation)
- [DMT/animationOff](#DMTanimationOff)
- [DMT/letter](#DMTletter)
- [DMT/message](#DMTmessage)
- [DMT/music](#DMTmusic)
- [DMT/sound](#DMTsound)

## Gameplay Actions

- [DMT/chest](#DMTchest)
- [DMT/event](#DMTevent)
- [DMT/eventString](#DMTeventString)
- [DMT/explode](#DMTexplode)
- [DMT/explosion](#DMTexplosion)
- [DMT/modData](#DMTmodData)
- [DMT/monster](#DMTmonster)
- [DMT/object](#DMTobject)
- [DMT/terrainFeature](#DMTterrainFeature)
- [DMT/triggerAction](#DMTtriggerAction)

## Crop Actions

- [DMT/fertilize](#DMTfertilize)
- [DMT/growCrop](#DMTgrowCrop)
- [DMT/killCrop](#DMTkillCrop)
- [DMT/setCrop](#DMTsetCrop)

## Misc Actions

- [DMT/invalidate](#DMTinvalidate)



## DMT/action
Trigger an existing tile action.

### Accepted Value
The x,y coordinates of the tile to trigger, separated by a comma (',').

### Example
"DMT/action_On": "69,20"



## DMT/addLayer
Add a layer to the current map.

### Accepted Value
The id of the layer as a string, must be unique.

### Example
"DMT/addLayer_Enter": "DMT/TestLayer"



## DMT/addQuest
Give a player a quest.

### Accepted Value
The quest ID you wish to add.

### Example
"DMT/addQuest_Once_MonsterSlain(GoopAlienBrood)": "MindMeltMax.WeirdEvents/FriendOfTheGoop"



## DMT/addTilesheet
Add a tilesheet to the current map.

### Accepted Value
A comma (',') delimited string of the id of the tilesheet (must be unique), and the path to the tilesheet image (tilesheet image can be loaded). 

{  
"LogName": "My Custom Tilesheet Load",  
"Action": "Load",  
"Target": "Maps/z_MyUniqueTilesheet",  
"FromFile": "assets/z_MyUniqueTilesheet.png"  
},

### Example
"DMT/addTilesheet_Enter": "{{ModId}}_MyTilesheetId1,TileSheets/furniture",  
"DMT/addTilesheet_Load": "{{ModId}}_MyTilesheetId2,Maps/z_MyUniqueTilesheet",
 "DMT/changeMultipleIndex_Item((O)390)": "Front 59 24={{ModId}}_MyTilesheetId2/0"



## DMT/barrier
Prevent certain character types from entering this tile.

### Accepted Value
A list of character types separated by pipes ('|'). Possibe character types include:

- Character
- FarmAnimal
- NPC
- Pet
- Child
- Horse
- Junimo
- JunimoHarvester
- TrashBear
- Raccoon
- Farmer
- Monster

and all specific monster types. Note that character types will also check subtypes, so Character will check every character type, NPC will check NPCs and any NPC subtypes (like Pet, Monster, etc).

Note that this doesn't work on flying monsters.

### Example
"DMT/barrier": "GreenSlime|Farmer|Pet"



## DMT/animation
Animations are such complex objects (even with the new format), that they deserve their own file, which you can find [here](Dynamic-Map-Tiles-Extended-Animations).



## DMT/animationOff
Animations are such complex objects (even with the new format), that they deserve their own file, which you can find [here](Dynamic-Map-Tiles-Extended-Animations).


## DMT/appearance
Changes various aspects of the triggering farmer's appearance. 

### Accepted Value
Changes take a comma-separated pair: {type},{value}. Types can include:

- hairstyle (number value - each hairstyle has an associated number (see `Data/HairData` for vanilla hairstyles)
- haircolor (#FFFFFF color value)
- eyecolor  (#FFFFFF color value)
- accessory (number value -1 to 29)
- skincolor (number value 0 to 23)
- gender ("female" or "male" value)

Multiple changes can be triggered using pipes to separate ('|').

### Example
"DMT/appearance_On": "hairstyle,1"  
"DMT/appearance_Off": "hairstyle,2|haircolor,#000000"



## DMT/buff
Add a buff or debuff to the player.

### Accepted Value
A string with multiple id's of items in ``Data\\Buffs`` separated by bars ('|'). Each id can also have a custom display source specified when followed by a comma (',').

### Example
"DMT/buff_Once_On": "22|27"  
"DMT/buff_On": "MindMeltMax.WeirdEvents/Gooped,Sludge Tile"



## DMT/changeIndex
Change the index of the tile currently under the player.

### Accepted Values
* An empty string, this will remove the tile from the layer.
* A number representing a different index in the same tilesheet as the current tile.
* A combination of an existing tilesheet id (string) and an index in said tilesheet (number) split by a forward slash ('/').
* A collection of option 2 or 3 to create an animated tile split by a space (' ') followed by comma (',') and the interval between frames in milliseconds

### Examples
* "DMT/changeIndex_On": ""  
* "DMT/changeIndex_On": "137"  
* "DMT/changeIndex_On": "indoors/57"  
* "DMT/changeIndex_On": "indoors/1741 indoors/1773 indoors/1805 indoors/1773 indoors/1741,250"



## DMT/changeMultipleIndex
Change multiple tile indexes on the current map.

### Accepted Values
Every accepted value for this works the same as [Change Index](#change-index), however they should be prefixed with the layer and tile coordinates.

### Examples
This example mixes some available values from [Change Index](#change-index), you can mix and match the variants from there as you need them here, as long as they are prefixed by '\{layer id\} \{x position of tile\} \{y position of tile\}=...', each one must bu split by a pipe ('|').

"DMT/changeMultipleIndex_Enter": "Buildings 12 27=indoors/472|Buildings 12 28=indoors/473|Buildings 12 29=indoors/474|Buildings 13 27=indoors/504|Buildings 13 28=indoors/505|Buildings 13 29=indoors/506|Buildings 14 27=indoors/536|Buildings 14 28=indoors/537|Buildings 14 29=indoors/538"



## DMT/changeMultipleProperties
Change the properties of multiple tiles.

### Key
"DMT/changeMultipleProperties"

### Accepted Values
A pipe ('|') delimited string of key=value pairs.  
Unlike with [Change Properties](#change-properties), here the key is split up into multiple parts separated by comma's (',').  
These parts are formatted like:
* (optional) The tile layer id to select the tile from
* The tile x position
* The tile y position
* The actual tile property

**Note** if the "=value" is ommited, the property will be removed instead.

### Example
"DMT/changeMultipleProperties_Once_On": "Buildings,17,8,Action=MindMeltMax.YouWishThisExisted/CouponMachine|Buildings,17,8,Passable|17,8,NoFurniture=T"  
"DMT/changeMultipleProperties_Enter": "NoFurniture=T|TouchAction=MindMeltMax.WeirdEvents/BossFight7"



## DMT/changeProperties
Change the properties of a tile.

### Accepted Value
A pipe ('|') delimited string of key=value pairs.

**Note** if the "=value" is ommited, the property will be removed instead.

### Example
"DMT/changeProperties_On": "TouchAction|Action=kitchen"



## DMT/chest
Spawn a chest with item's (and/or money).

### Accepted Value
A string with multiple parts, defined as follows:
* The tile position as a space (' ') delimited string, followed by an equals sign ('=').
* (optional) The non-qualified item id of the chest followed by a pipe ('|').
* A space (' ') delimited string of either items or money as defined in the formats for [give](#give-item) and [take](#take-item).

**Note** that since the 1.6 update, a chest can not directly contain money. The workaround for this is that DMT will add the new Gold coin object with the stack size being the amount of money, each gold coin is worth 250 money.

### Example
"DMT/chest_Once_MonsterSlain": "7 12=BigChest|Money/10 (O)336,5 (O)384,17 (O)125"  
"DMT/chest_Once_On": "15 15=(O)349,3 (O)773,3 (O)772" 



## DMT/emote
Make the player perform an emote.

### Accepted Value
The numerical id of the emote in string form. This can be found by opening ``Tilesheets\\emotes`` and picking the emote you want. The id will be: (4 x The row of the emote) + 4; the + 4 is added because the first row in the texture is skipped.

### Example
"DMT/emote_Talk(Abigail)": "52"



## DMT/event
Start an event from an asset.

### Accepted Values
Fields split by pipes ('|'):
* The asset name.
* The entire event key in the asset, including requirements (e.g. "3910674/f Shane 1000").
* The event id to add to the player's seen events.

### Example
"DMT/event_Once_Enter": "MindMeltMax.WeirdEvents|GoopAliens|goopAlienLanding"


## DMT/eventString
Start an event from an event string.

### Accepted Values
Fields split by pipes ('|'):
* The full event script.
* The event id to add to the player's seen events.

### Example
"DMT/eventString_Once_Enter": "jaunty/13 17/farmer 13 19 0/end|goopAlienLanding"



## DMT/explode
Makes a tile vulnerable to explosions.

### Accepted value
An optional mail flag to set when the tile explodes.

**Note** Tiles with this property will be removed when an explosion hits them. The key should simply be "DMT/explode" with no modifiers.

### Example
"DMT/explode": ""  
"DMT/explode": "myTileExplodedMailFlag"  



## DMT/explosion
Triggers an explosion.

### Accepted value
A comma (',') delimited string with multiple options to use when triggering the explosion. These options are as follows in order:
* The tile x position (must set y too) - defaults to this tile
* The tile y position (must set x too) - defaults to this tile
* The radius of the explosion
* Whether to damage the farmer
* A custom amount of damage
* Whether to destroy objects
* The sound of the explosion - defaults to "explosion"

**Note** all values are technically optional, and any one can be ommited by just leaving the value blank (see examples below).

### Example
"DMT/explosion_MonsterSlain(Mummy)": "7,15,3,,,,thunder"  
"DMT/explosion_On": ",,5,120,false,true"



## DMT/fertilize
Fertilize one or more tiles with a given fertilizer.

### Accepted Value
A string of the format x,y=fertilizerName. If fertilizerName is omitted, the fertilizer will be removed from that tile. To fertilize multiple tiles at once repeat this pattern separated by bars ('|').

### Example
"DMT/fertilize": "64,19,368"
"DMT/fertilize": "64,19,"



## DMT/friends
Increase or decrease friendship with an npc or animal type

### Accepted Value
A string with pairs of the NPC's internal name or the animal type, and the amount by which to change friendship, separated by a comma (','). To change multiple values at once repeat this pattern separated by bars ('|')

### Example
"DMT/friends": "Marnie,50|Cow,15|Chicken,15"



## DMT/give
Give the player a specified item (or money)

### Accepted Values
* A string beginning with "Money/" followed by the amount of money to be added.
* A single qualified item id.
* A comma (',') delimited string with: the qualified item id, the stack size, (optional) the quality; In that order.

### Example
"DMT/give_Once_On": "(O)76"
"DMT/give_Once_MonsterSlain": "Money/2500"  
"DMT/give_Talk(Abigail)": "(O)66"  
"DMT/give_Talk(Willy)": "(O)131,1,4"



## DMT/growCrop
Grow one or more crops completely when triggered.

### Accepted Values
A string a pair of x and y tile coordinates separated by a comma (','). To change multiple values at once repeat this pattern separated by bars ('|')


### Example
"DMT/growCrop_On": "64,19|64,20"



## DMT/health
Add or remove health from the player.

### Accepted Value
A number value in string form. Can be either positive to add health, or negative to remove health.

### Example
"DMT/health_On" "-12"  
"DMT/health_MonsterSlain": "2"



## DMT/healthPerSecond
Add or remove health from the player every second for a duration.

### Accepted Value
2 number values in string form split by a comma (','). The first value specifies the number of seconds to update health for, the second the amount of health restored/taken.

### Example
"DMT/healthPerSecond_On": "10,2"  
"DMT/healthPerSecond_Off": "15,-1"



## DMT/healthPerSecondContinuous
Add or remove health from the player every second until they move from the tile they were on when this was triggered.

### Accepted Value
a number value in string form specifying the amount of health restored/taken.

### Example
"DMT/healthPerSecondContinuous_On": "2"  
"DMT/healthPerSecondContinuous_On": "-1"



## DMT/invalidate
Invalidate an asset from the game's asset cache, forcing it to be reloaded when next requested.

### Accepted Value
The name of the asset you want to invalidate, may be split by bars ('|') to invalidate multiple assets

### Example
"DMT/invalidate_Enter": "Data/Locations"  
"DMT/invalidate_On": "Data/Objects|Strings/Objects|Strings/BigCraftables|Data/BigCraftables"



## DMT/killCrop
Kill one or more crops when triggered.

### Accepted Values
A string a pair of x and y tile coordinates separated by a comma (','). To change multiple values at once repeat this pattern separated by bars ('|')


### Example
"DMT/killCrop_On": "64,19|64,20"




## DMT/letter
Show a mail letter from Data/mail.

### Accepted Values
Two fields separated by pipes ('|'):

Fields are:

- the mail key
- whether the letter should behave as if it was opened from the collections menu (true or false)

### Example
"DMT/letter_MonsterSlain": "MindMeltMax.EndlessDungeon/TotallyTheLastOne|true"




## DMT/mail
Add a mail flag from the player if they don't have it yet.

### Accepted Value
The mail flag you wish to set, if not added already.

### Example
"DMT/mail_Once_Talk(Haley)": "MindMeltMax.BestGirl/MetTheQueen"



## DMT/mailbox
Add mail for tomorrow to the players mailbox.

### Accepted Value
The id of the item in ``Data\\Mail`` you wish to add to the mailbox, if it's not already added.

### Example
"DMT/mailbox_Once_MonsterSlain(GoopAlienBrood)": "MindMeltMax.WeirdEvents/MYSONHOWCOULDYOU"



## DMT/mailRemove
Remove a mail flag from the player if they have it.

### Accepted Value
The mail flag you wish to remove, if it's known by the player.

### Example
"DMT/mailRemove_Once_MonsterSlain(GoopAlienBrood)": "MindMeltMax.WeirdEvents/FriendOfTheGoop"



## DMT/makeover
Give a farmer a makeover outfit from "Data/MakeoverOutfits". Currently worn clothing will be returned to the player inventory or lost & found.

### Accepted Value
The id of the makeover outfit. Multiple ids can be separated with a pipe '|' to choose one randomly.

### Example
"DMT/makeover_Once_On": "JojaCap_Shirt105_FarmerPants"



## DMT/makeoverGendered
Give a farmer a makeover outfit from "Data/MakeoverOutfits" based on their gender. Currently worn clothing will be returned to the player inventory or lost & found.

### Accepted Value
Two ids of makeover outfits for each gender, separated with a pipe '|'.

### Example
"DMT/makeoverGendered_Once_On": "Beanie_SkullShirt_FarmerPants|Beanie_ClassyTop_Dress"



## DMT/message
Show a message box.

### Accepted Values
This action accepts a plain string (which can be split by a hashtag ('#') to separate into multiple consecutive dialogues).  
However another accepted value starts with a boolean value ("True" or "False") followed by a pipe ('|') and then either a regular dialogue string (if false) or a string with mail formatting (if true).  

**Note** that regular dialogue works also when true, however mail formatting won't be applied when false.

**Note** also that some triggers which make menus appear (like Talk), will not work well with this action, use of it is not advised.

### Example
"DMT/message_Item((O)289)": "T-This is huge!#What kind of chickens do you have on that farm!"  
"DMT/message_MonsterSlain": "True|It is done...^The ritual is complete...^Now all that rests you is to claim the loot in the last dungeon.^I promise it's the last this time...^I think...  %item quest MindMeltMax.EndlessDungeon/TotallyTheLastOne true %% [textcolor red][#]Found Letter #237"



## DMT/monster
Spawn one or more monsters.

### Accepted Values
Each monster to spawn should be a comma-separated list of values. Multiple mosters can be spawned using a pipe ('|'). The list of values will depend on the specific C# constructor being called, but the first three values should be the C# monster class name, the x coordinate, and the y coordinate in that order. Subsequent values should correspond to constructor parameters besides the spawn position. Possible parameters include:

- integer, e.g. for mineLevel
- boolean, e.g. for hardMode
- color, e.g. for GreenSlimes

**Note** To use this key you will need to know the constructor you want to call. There is a list [here](https://github.com/aedenthorn/StardewValleyMods/blob/master/CustomMonsters/doc/ctor.txt).

**Note** Color parameters should be in the format #FFFFFF.

**Note** This action supports the Custom Monsters mod, in which case you should only pass the first three values.

### Example
"DMT/monster_On": "GreenSlime,640,1280,#00FF00|GreenSlime,704,1280,#0000FF"  
"DMT/monster_Enter": "MyCustomMonster,640,1280" 



## DMT/modData
Change mod data for object or furniture at given tiles.

### Accepted Values
Each mod data to set should be a comma-separated list of fields. Multiple mod data keys can be set using a pipe ('|').

Fields are:

- X coordinate of the tile to set mod data on
- Y coordinate of the tile to set mod data on
- key to set
- value to set the key to

### Example
"DMT/modData_Load": "5,6,AlternativeTexture,MyTextureName"  



## DMT/object
Spawn objects or furniture at given tiles.

### Accepted Values
Each object to be spawned should be a comma-separated list of fields. Multiple objects can be spawn using a pipe ('|').

Fields are:

- X coordinate of the tile to spawn the object on
- Y coordinate of the tile to spawn the object on
- Qualified item id of the object to spawn (e.g. "(BC)55" or "(F)UprightPiano")
- (optional) true or false value of whether the object or furniture can be moved by players
- (optional) integer 0-4 for furniture rotation, only applicable if spawning furniture

### Example
"DMT/object_Load": "5,6,(F)UprightPiano,false,1|10,6,(BC)55,false"



## DMT/move
Move the player in a direction.

### Accepted Value
2 decimal number values in string form split by a comma (','). These numbers represent the direction the player should move in, where **1** is 1 pixel moved.  
The first number is the x position, setting this to negative makes the player move left, while a positive value makes the player move right.  
The second number is the y position, same logic applies here: negative = down, positive = up.

**Note** that the general rule is 64 pixels = 1 tile, so to make the player move one tile right, the first number needs to be 64.

### Example
"DMT/move_On": "256,77"  
"DMT/move_Off": "-122,320"



## DMT/music
Play an existing music track.

### Accepted Value
The name of a known music track. This does accept mod added songs in the same way. Multiple tracks can be separated with a pipe ('|') to choose one at random.

### Example
"DMT/music_Enter": "winter1"



## "DMT/push"
Push a tile when triggered.

### Keys

### Accepted Value
A string of tile positions in the form of '{x} {y}' representing the allowed destination tiles, separated by commas (',').

**Note** this property is only applicable to the "Buildings" layer.

### Example
"DMT/push_On": "7 15, 7 17"  


## "DMT/pushable"
Allow a tile to be pushed by farmers.

### Accepted Value
A string of tile positions in the form of '{x} {y}' representing the allowed destination tiles, separated by commas (',').

**Note** this key should not take any modifers.

**Note** this property is only applicable to the "Buildings" layer.

### Example
"DMT/pushable": "3 3, 3 5, 2 4, 4 4"



## "DMT/pushAlso"
Allow a tile to be pushed when a neighbor tile is pushed.

### Accepted Value
A string of tile offsets with the layer id to push the tile from in the form of '{layer Id} {x offset} {y offset}' representing the tile offset on the specified layer, relative to the original pushed tile. Each part must be separated by comma's (',').

**Note** this property can push tiles on layers other than "Buildings".  
**Note** also, this property does not accept triggers and can only be actived when a neighboring tile is pushed.

### Example
"DMT/pushAlso": "Front 0 -1, Front 0 -2, Buildings 0 -3"



## "DMT/pushOthers"
Specify other tiles to be pushed.

### Accepted Value
A string of tile positions with the layer id to push the tile from in the form of '{layer Id} {x} {y}' representing the tile position on the specified layer. Each part must be separated by comma's (',').

**Note** this property can push tiles on layers other than "Buildings".
**Note** also, tiles pushed by this property will activate tiles with the [Push Also](#push-also-experimental) property.

### Example
"DMT/pushOthers_On": "Buildings 5 15, Front 4 16, Front 5 16, Front 6 16"



## DMT/removeQuest
Remove a quest from a player.

### Accepted Value
The quest ID you wish to remove.

### Example
"DMT/removeQuest_Once_MonsterSlain(GoopAlienBrood)": "MindMeltMax.WeirdEvents/FriendOfTheGoop"



## DMT/setCrop
Set crop for one or more hoe dirt when triggered.

### Accepted Values
A string in the format x,y,SeedId,GrowthPhase. To change multiple values at once repeat this pattern separated by bars ('|')

**Note** If a hoe dirt is not found at the coordinates, 

**Note** GrowthPhase can be omitted to just plant a seed, or set to -1 to fully grow the crop.

**Note** Existing crops will be replaced.

### Example
"DMT/setCrop_On": "64,19,433,1"



## DMT/sound
Play one or more sounds.

### Accepted Value
A pipe ('|') delimited string of sound names with optional delays (split by commas (',')).

### Example
"DMT/sound_Item((O)335)": "hammer"  
"DMT/sound_Item((O)335)": "hammer,150"  
"DMT/sound_Item((O)335)": "pickupItem|money,300|money,450"



## DMT/slippery
Make the player slide across tiles.

### Accepted value
A decimal number in string form representing the speed at which to slide the player.

**Note** this value stacks when surrounding tiles also have this property.
**Note** also, this property does not accept triggers and will only activate when stepped on.

### Example
"DMT/slippery": "1.2"



## DMT/speed
Set a speed multiplier for the player.

### Accepted Value
A decimal number value in string form.

### Example
"DMT/speed_Enter": "1.2"



## DMT/stamina
Add or remove stamina from the player.

### Accepted Value
A number value in string form. Can be either positive to add health, or negative to remove health.

**Note** that the value accepts whole and decimal values.

### Example
"DMT/stamina_Item((O)306)": "25"  
"DMT/stamina_Talk(Piere)": "-5"



## DMT/staminaPerSecond
Add or remove stamina from the player every second for a duration.

### Accepted Value
2 number values in string form split by a comma (','). The first value specifies the number of seconds to update stamina for, the second the amount of stamina restored/taken.

**Note** that the second number (the actual stamina value) accepts whole and decimal values.

### Example
"DMT/staminaPerSecond_Once_Enter": "60,1.5"  
"DMT/staminaPerSecond_Explode": "5,-2"



## DMT/staminaPerSecondContinuous
Add or remove stamina from the player every second until they move from the tile they were on when this was triggered.

### Accepted Value
a number value in string form specifying the amount of stamina restored/taken.

### Example
"DMT/staminaPerSecondContinuous": "2"  
"DMT/staminaPerSecondContinuous": "-1"



## DMT/take
Take a specified item (or money) from the player.

### Accepted Values
* A string beginning with "Money/" followed by the amount of money to be added.
* A single qualified item id.
* A comma (',') delimited string with: the qualified item id, the stack size, (optional) the quality; In that order.

### Example
"DMT/take_Once_Enter": "Money/500"  
"DMT/take_Talk(Piere)": "(O)MindMeltMax.YouWishThisExisted/DiscountCoupon"  
"DMT/take_Talk(Abigail)": "(O)66,5"



## DMT/teleport
Teleport the player to a specified pixel coordinate.

### Accepted Value
A comma (',') delimited string with the X and Y pixel coordinates.

### Example
"DMT/teleport_On": "516,1272"



## DMT/teleportTile
Teleport the player to a specified tile coordinate.

### Accepted Value
A comma (',') delimited string with the X and Y tile coordinates.

### Example
"DMT/teleportTile_Off": "6,13"

## DMT/terrainFeature
Spawn terrain features at given tiles.

### Accepted Values
Each terrain feature to be spawned should be a comma-separated list of fields. Multiple terrain features can be spawn using a pipe ('|').

Fields are:

- X coordinate of the tile to spawn the terrain feature on
- Y coordinate of the tile to spawn the terrain feature on
- type of terrain feature to spawn (one of Tree, FruitTree, Grass, HoeDirt)
- (optional) type of Grass, Tree, or Fruit Tree
- (optional) number of grass weeds, Tree or Fruit Tree growth stage

### Example
"DMT/terrainFeature_Load": "5,6,HoeDirt|10,6,Tree,1,5"



## DMT/transmog
Give a farmer a makeover outfit from "Data/MakeoverOutfits". Currently worn clothing will be irrevocably replaced.

### Accepted Value
The id of the makeover outfit. Multiple ids can be separated with a pipe '|' to choose one randomly.

### Example
"DMT/transmog_On": "JojaCap_Shirt105_FarmerPants"



## DMT/transmogGendered
Give a farmer a makeover outfit from "Data/MakeoverOutfits" based on their gender. Currently worn clothing will be irrevocably replaced.

### Accepted Value
Two ids of makeover outfits for each gender, separated with a pipe '|'.

### Example
"DMT/transmogGendered_Once_On": "Beanie_SkullShirt_FarmerPants|Beanie_ClassyTop_Dress"



## DMT/triggerAction
Trigger a [Trigger Action](https://stardewvalleywiki.com/Modding:Trigger_actions).

### Accepted Value
The trigger action ID, and optionally text parameters separated with a pipe '|'.

### Example
"DMT/triggerAction_Once_Enter": "SomeTriggerAction"  
"DMT/triggerAction_Once_Enter": "SomeTriggerAction|Parameter1|Parameter2"



## DMT/wardrobe
Give a farmer a makeover outfit from "Data/MakeoverOutfits" if they have the required items in their inventory. Currently worn clothing will be returned to the player inventory or lost & found.

### Accepted Value
The id of the makeover outfit. Multiple ids can be separated with a pipe '|' to choose one randomly.

### Example
"DMT/wardrobe_On": "JojaCap_Shirt105_FarmerPants"



## DMT/wardrobeGendered
Give a farmer a makeover outfit from "Data/MakeoverOutfits" based on their gender if they have the required items in their inventory. Currently worn clothing will be returned to the player inventory or lost & found.

### Accepted Value
Two ids of makeover outfits for each gender, separated with a pipe '|'.

### Example
"DMT/wardrobeGendered_Swap_On": "Beanie_SkullShirt_FarmerPants|Beanie_ClassyTop_Dress"  
"SwapDMT/wardrobeGendered_Swap_On": "None_GraySuit_FarmerPants|LogoCap_SugarShirt_Skirt"



## DMT/warp
Warp a farmer to a given location and coordinates.

### Accepted Value
A string of the format LocationName,x,y.

### Example
"DMT/teleport": "Farm,69,20"

