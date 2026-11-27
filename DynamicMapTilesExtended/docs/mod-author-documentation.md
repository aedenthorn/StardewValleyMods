# Dynamic Map Tiles Extended
Dynamic Map Tiles Extended (DMTE for short) allows content pack authors to add advanced tile interactions which aren't achievable without c#.

This version has been changed somewhat substantially from the original (created by [aedenthorn]()), but it was written with backwards compatibility in mind.  
While most content packs won't work directly without edits, these changes should be minor. It is advised however to migrate to the new formats.

* [Getting Started](#getting-started)
* [How to add tile actions](#how-to-add-tile-actions)
	* [Edit the map](#edit-the-map)
	* [Add to custom data](#add-to-custom-data)
    * [New property format](#new-property-format)
* [Triggers](#triggers)

## Getting Started

To get started with DMTE, start by installing the mod like you would with any other mod (installation instructions on nexus page).

Once it is installed, create a folder somewhere on your computer where you can easily access it and give it the name of your mod.

Open the folder and create the following files:

* ``manifest.json`` - The mod manifest required for any stardew valley mod (details can be found [here](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Manifest))
* ``content.json`` - The entry point for a content patcher mod (details can be found [here](https://github.com/Pathoschild/StardewMods/blob/develop/ContentPatcher/docs/author-guide.md#get-started))

The content patcher documentation explains in great detail how these files should be filled out, so I do recommend you read through them at any point you become stuck.

The mod does not need to be added as a dependency and any edit can be added [conditionally](https://github.com/Pathoschild/StardewMods/blob/develop/ContentPatcher/docs/author-guide/tokens.md#conditions) with content patcher's [HasMod](https://github.com/Pathoschild/StardewMods/blob/develop/ContentPatcher/docs/author-guide/tokens.md#HasMod) condition.

## How to add tile actions
Tile actions can be added to the map in one of two ways.

### Edit the map
Any tile action keys can be added directly to a tile's properties by editing the map tiles, like so:
```json
{
    "Format": "2.3.0",
    "Changes": 
    [
        {
            "Action": "EditMap",
            "Target": "Maps/Farm",
            "MapTiles": 
            [
                {
                    "Position": 
                    { 
                        "X": 72, 
                        "Y": 15 
                    },
                    "Layer": "Back",
                    "SetProperties": 
                    {
                        "DMT/changeIndex_On": "69"
                    }
                }
            ]
        },
    ]
}
```
The above code edit's the farm map and sets the property "DMT/changeIndex" on the tile at position X:72,Y:15 on the "Back" layer. 
This property has the trigger "On", meaning the property will activate when the player steps on the tile.
Once activated, the tile's texture will change the specified index in it's tilesheet (in this case: "69").

### Add to custom data
The mod also provides a custom data dictionary which you can add all your stuff to which will automatically be applied to applicable locations when a player enters them.

The data has the following fields you can set:
* **Locations**: (optional) A list of names of the locations to apply the properties to, for example: "Locations: ["Farm", "Beach"]" or "locations: ["Town", "Mountain", "Forest"]". Leaving this out or empty will check every location.
* **Layers**: (optional) A list of ids of layers to apply the properties to, for example: "Layers: ["Back"]" or "layers: ["Buildings"]". Leaving this out or empty will check every layer.
* **TileSheets**: (optional) A list of ids of tilesheets to check against before applying any properties, if a tile does not have the specified tilesheet id, the property will not be applied. For example: "TileSheets: ["indoors"]" or "tileSheets: ["outdoors"]".
* **TileSheetPaths**: (optional) A list of ids of tilesheet paths to check against before applying any properties, if a tile does not have a tilesheet with the specified path, the property will not be applied.
* **Indexes**: (optional) A list of numerical ids of tile indexes to check against before applying any properties, if a tile's index in the tilesheet does not appear in this list, the property will not be applied.
* **Rectangles**: (optional) A list of specified areas to search within, for example: "Rectangles: [{"X": 5, "Y": 7, "Width": 3, "Height": 3}, {"X": 12, "Y": 23, "Width": 2, "Height": 6}]"". Rectangles are searched on a tile basis, instead of a size basis.
* **Tiles**: (optional) A list of specified tiles on which to apply the actions, for example: "Tiles: [{"X": 7, "Y": 18}, {"X": 24, "Y": 20}]". Recommended use alongside **Layers**.
* **Properties**: (optional) A dictionary of properties to apply when the conditions are met, this is the old format, use **Actions** instead.
* **Actions**: A list of objects which specify details for properties to apply to a map, more details on the format can be found [here](#new-property-format).

Following this format, your content patcher file should look something like this:
```json
{
    "Format": "2.3.0",
    "Changes":
    [
        {
            "Action": "EditData",
            "Target": "DMT/Tiles",
            "Entries": 
            {
                "ExampleMod.DMTEdits":
                {
                    "Locations": ["Farm"],
                    "Layers": ["Back"],
                    "Properties": 
                    {
                        "DMT/changeProperties_Once_Enter": "NoSprinklers"
                    },
                    "Actions": 
                    [
                        {
                            "LogName": "RemoveNoSprinklerProperty",
                            "Key": "DMT/changeProperties",
                            "Value": "NoSprinklers",
                            "Trigger": "Enter",
                            "Once": True
                        }
                    ]
                }
            }
        }
    ]
}
```

In this example the NoSprinklers property is removed from the farm map's "Back" layer. In this case, **Properties** and **Actions** do the same thing, except:
* **Properties** is shorter which can be useful for small changes like this
* **Actions** is more detailed in what everything means which makes it a bit more beginner friendly.

### New property format
With this re-write comes a new way to add properties through content patcher's EditData action.  
This section will explain the properties included in the new format.

* **Key**: The key of the tile property like "DMT/give" or "DMT/animate". A full list of keys can be found [here](./actions.md).
* **Value**: The value of the tile property following the format described by the action you want to use.
* **LogName**: (optional) A unique name which will be included whenever anything related to this property is logged to the console, useful for debugging.
* **Trigger**: The trigger to use for activating this property. A full list of triggers can be found [here](#triggers).
* **Once**: (optional) Whether to only run this property once and then remove it. Default false.

For a basic overview of how these properties are used, you can see the example code [here](#add-to-custom-data).

## Triggers
The mod provides multiple ways of triggering an action.

Here's a list of the available triggers:

* **On**: A trigger for when the player steps on a tile.
* **Off**: A trigger for when the player steps off a tile.
* **Enter**: A trigger for when the player enters a location.
* **Push**: A trigger for when a tile is [pushed]().
* **Pushed**: A trigger for when a tile is done being [pushed]().
* **Explode**: A trigger for when the tile is within the blast radius of an explosion. (see also: [explode action]())
* **Tool(\{tool type name\})**: A trigger for when a certain tool is used on a tile, for example: "DMT/changeIndex_Tool(WateringCan)". For a full list of available tool names see [the section on tool triggers]().
* **Item(\{qualified item id or name\}-\{optional stack requirement\}-\{optional quality requirement\})**: A trigger for when the player is holding a certain item when they interact with the tile, for example: "DMT/changeIndex_Item((O)276-10-3)" or "DMT/sound_Item((O)74)". If you want to use the quality option, the stack option must also be provided. |To be added later| The quality option can also have an optional plus ('+') or minus ('-') modifier to make the quality accept higher and lower qualities respectfully on top of the requested value.
* **Talk(\{optional target npc's internal name\})**: A trigger for when the player talks to an NPC, for example "DMT/animate_Talk" or "DMT/animate_Talk(Demetrius)". The name must match the unique internal name of the npc to avoid conflicts.
* **MonsterSlain(\{optional target monster's internal name\})**: A trigger for when the player slays a monster, for example "DMT/explosion_MonsterSlain" or "DMT/explosion_MonsterSlain(Ghost)". The name must match the unique internal name of the monster to avoid conflicts.
* **Mount(\{optional name of the mount\})**: A trigger for when the player mounts a rideable (Not Implemented!)
* **Dismount(\{optional name of the mount\})**: A trigger for when the player dismounts a rideable (Not Implemented!)