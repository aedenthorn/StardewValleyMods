# Dialogue Display Framework API

## Usage

Dialogue Display Framework uses Content Patcher to load a dictionary from a fake path. Your content pack should be a pack for Content Patcher and target the following path:

"**aedenthorn.DialogueDisplayFramework/dictionary**"

Dictionary keys should be either "default" for a global dialogue setup or the name ID of the NPC (e.g. "Emily").

So, an example CP shell would look like:

    {
        "Format": "1.23.0",
        "Changes": [
            {
                "Action": "EditData",
                "Target": "aedenthorn.DialogueDisplayFramework/dictionary",
                "Entries": {
                    "Emily": {
                        (your data goes here)
                    }
                }
            }
        ]
    }

When testing out your pack, you can press F5 in game to reload all registered entries, so you can make edits and see them reflected in-game in real time.


## Dictionary Objects

Dictionary values are objects with the following keys:

- "packName" - string, Manifest ID of the content pack containing this entry, used for reloading the data in-game.
- "xOffset" - integer, custom x offset of the dialogue box relative to its normal position on the screen.
- "yOffset" - integer, custom y offset of the dialogue box relative to its normal position on the screen.
- "width" - integer, custom width of the dialogue box (omit to use normal width, 1200)
- "height" - integer, custom height of the dialogue box (omit to use normal height)
- "dialogue" - object for customizing dialogue display (see below)
- "portrait" - object for customizing portrait display (see below)
- "name" - object for customizing name display (see below)
- "jewel" - object for customizing friendship jewel display (see below)
- "button" - object for customizing action button display (see below)
- "sprite" - object relating to custom character sprite (see below)
- "gifts" - object relating to custom gift display (see below)
- "hearts" - object relating to custom hearts display (see below)
- "images" - array of objects relating to custom images (see below)
- "texts" - array of objects relating to custom texts (see below)
- "dividers" - array of objects relating to custom dividers (see below)
- "disabled" - boolean, whether to disable this entry and use the game's default dialogue box setup

If any field is missing in an NPC entry, the field from "default" entry will be used instead.

## Base Data

For all of the above entries that are objects (or arrays of objects), the objects have the following common keys available (though they may not all use them):

- "xOffset" - integer, x offset relative to the box, default 0
- "yOffset" - integer, y offset relative to the box, default 0
- "right" - boolean, whether the x offset should be calculated from the right side of the box, default false
- "bottom" - boolean, whether the y offset should be calculated from the bottom of the box, default false
- "width" - integer, width of elements that need it
- "height" - integer, height of elements that need it
- "alpha" - decimal, opacity, default 1 (full opacity)
- "scale" - decimal, size scale, default 4 (most things in the game are displayed at 4x)
- "layerDepth" - decimal, z-index of the element, default 0.88
- "variable" - boolean, whether the size of the element is variable, tells the mod to calculate size based on the center of the element, default false
- "disabled" - boolean, whether to disable this element, i.e. if this is for an NPC for which you don't want a default element added, default false


## Name Data

Name data has the following additional keys available:

- "color" - integer, the color code of the text (these are built into the game, test yourself), default -1 (default text color)
- "scroll" - boolean, whether to draw a scroll behind the text
- "placeholderText" - if using scroll background, this affects the size of the scroll
- "centered" - boolean, whether to center the text on the scroll
- "scrollType" - integer, idek
- "junimo" - whether the name should be displayed in Junimo characters, because why not


## Dialogue Data

Dialogue data has the following additional keys available:

- "color" - integer, the color code of the text (these are built into the game, test yourself), default -1 (default text color)
- "alignment" - enum, text alignment: 0 = left, 1 = center, 2 = right


## Portrait Data

Portrait data has the following additional keys available:

- "texturePath" - string, the fake or real game path relative to the Content folder of the texture file used to draw (if omitted, use the character's default portrait sheet)
- "x" - integer, x position in the source texture file (if tileSheet is false)
- "y" - integer, y position in the source texture file (if tileSheet is false)
- "w" - integer, width in the source texture file, default 64
- "h" - integer, height in the source texture file, default 64
- "tileSheet" - boolean, whether the source texture  default true


## Sprite Data

Sprite data has the following additional keys available:

- "background" - boolean, whether to show the day / night background behind the sprite
- "frame" - integer, which frame on the character sprite sheet to show. Set to -1 to animate the sprite instead


## Jewel Data

Jewel data has no additional keys.


## Button Data

Button data has no additional keys.


## Hearts Data

Hearts data has the following additional keys available:

- "heartsPerRow" - integer, number of hearts per row, default 14
- "showEmptyHearts" - boolean, include empty hearts, default true
- "centered" - boolean, if true, xOffset will point to the center of the row of hearts


## Gift Data

Gift data has the following additional keys available:

- "showGiftIcon" - boolean, show the gift icon, default true
- "inline" - boolean, show the check boxes to the right of the icon, default false


## Image Data

The images field is an array of Image Data objects. Image data has the following additional keys available:

- "texturePath" - string, the fake or real game path relative to the Content folder of the texture file used to draw 
- "x" - integer, x position in the source texture file
- "y" - integer, y position in the source texture file
- "w" - integer, width in the source texture file
- "h" - integer, height in the source texture file


## Text Data

The texts field is an array of Text Data objects. Text data has the following additional keys available:

- "color" - integer, the color code of the text (these are built into the game, test yourself), default -1 (default text color)
- "text" - string, the text
- "scroll" - boolean, whether to draw a scroll behind the text
- "placeholderText" - if using scroll background, this affects the size of the scroll
- "centered" - boolean, whether to center the text on the scroll
- "scrollType" - integer, idek
- "junimo" - whether the name should be displayed in Junimo characters, because why not


## Divider Data

The dividers field is an array of Divider Data objects. Divider data has the following additional keys available:

- "horizontal" - boolean, horizontal, default false (i.e. vertical)
- "small" - boolean, show teeny divider, default false
- "red" - byte, red tint of divider, default -1
- "green" - byte, blue tint of divider, default -1
- "blue" - byte, green tint of divider, default -1
