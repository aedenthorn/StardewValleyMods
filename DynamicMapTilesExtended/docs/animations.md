# Animations
This document goes over everything you need to know to create animations using DMT.

^You still have a chance to run.^

* [Basics]()
* [Format]()
	* [Fields]()
* [Examples]()

## Basics

Like all DMT actions, animations actions can be used by specifying their key, which in this case is **"DMT/animate"**.

This action supports the same triggers and modifiers regular actions like give and sound do.

## Format
Because of the complexity of animations, their format was changed from the regular space or bar delimited string, to a more readable json object.

This did however come with the issue: DMT actions only support string values. Which is why this object must still be in string form.

This means that your data would look like this:
```json
{
	"Name": "SomeName"
	...
}
```

And the value inside DMT should look like this: ``"{\"Name\": \"SomeName\" ...}"``

You might notice three glaring differences:
1. It's on one line instead of formatted.
2. All of the quotes in the json have slashes in front of them.
3. The json has quotes surrounding it.

Let's stand still and look at it for a second...

Feeling disgusted yet? Well I certainly was when I first saw!  
So while this is certainly an option if you're crazy enough to try, there's a better way.

*Drumroll...* The new **"DMT/Animations"** Asset!

This asset has a dictionary with a key (I recommend your mod's unique id) and a list of animation objects in the new format.  
This method does require each animation to have a unique name, however it shortens the value given for each tile action down to:
``"DMT/animate": "{The unique id in the dictionary},{the unique name of the animation}"``

For those who still wish to use the other method though, I recommend making the object in a separate json file and using the following free online tools to format the value:
1. https://www.text-utils.com/json-formatter/ to format the object down to a single line
2. https://onlinestringtools.com/escape-string to add the slashes in front of the text (yes they're required, I know, ok)

### Fields
The fields for animations can be divided up into three categories:

1. Shorthand animations using ``Tilesheets\\animations`` as the texture.
2. Custom animations using another asset as the texture.
3. Common fields, used by both.

**1: Short animations**

| Name | Type | Description | Required? | Default Value |
| ---- | ---- | ----------- | --------- | ------------- |
| TextureRowIndex | Number | The index of the row this animation starts from in the texture | Yes | -1 |
| SourceRectWidth | Number | The width of the source rectangle of the texture | No | 64 |
| SourceRectHeight | Number | The height of the source rectangle of the texture | No | 64 |

---

**2: Custom animations**

| Name | Type | Description | Required? | Default Value |
| ---- | ---- | ----------- | --------- | ------------- |
| Texture | String | The name of the texture to use for the animation, this is loaded by the game's content loader | Yes | NULL |
| SourceRect | Rectangle | The source rectangle of the texture | Yes | 0,0,0,0 |
| Flicker | Boolean | Whether or not the texture of the animation should flicker | No | false |
| AlphaFade | Decimal Number | The strenth at which the texture should fade out | No | 0 |
| Scale | Decimal Number | The scale of the texture | Yes | 0 |
| ScaleChange | Decimal Number | By how much should the scale change when the animation updates | No | 0 |
| Rotation | Decimal Number | The rotation of the texture | No | 0 |
| RotationChange | Decimal Number | By how much should the rotation change when the animation updates | No | 0 |
| Local | Boolean | Whether or not to use the local position instead of the global position (local = tile position, global = pixel position) | No | false |
| Motion | Vector | The way in which the animation should move around the map | No | 0,0 |
| Acceleration | Vector | The speed at which the animation moves around the map | No | 0,0 |

---

**3: Common fields**

| Name | Type | Description | Required? | Default Value |
| ---- | ---- | ----------- | --------- | ------------- |
| Name | String | A unique name to use this animation for tile actions | Yes | NULL |
| Position | Vector | The position in the map at which the animation should start | Yes | 0,0 |
| Color | Color | The color of the animated texture | Yes | 255,255,255,255 |
| Length | Number | The amount of frames this animation consists of | No | 8 |
| Flipped | Boolean | Whether or not the texture of the animation should be flipped | No | false |
| Interval | Decimal Number | The interval between animation frames in milliseconds | No | 100 |
| Loops | Number | The amount of times the animation should loop | No | 0 |
| LayerDepth | Decimal Number | Controls the order of drawn textures on the same layer (higher: draw over, lower: draw under) | No | -1 |
| Delay | Number | The delay before the animation should start | No | 0 |
| Id | Number | The internal game id for this animation | No | -87965 |


Some examples of how these fields are applied are:
```json
{
  "Format": "2.8.0",
  "Changes": [
    {
      "LogName": "MyAnimations Tilesheet Load",
      "Action": "Load",
      "Target": "{{ModId}}/MyAnimations",
      "FromFile": "assets/MyAnimations.png"
    },
    {
      "LogName": "List of Animations",
      "Action": "EditData",
      "Target": "DMT/Animations",
      "Entries": {
        "{{ModId}}.DMTAnimations": [
          {
            "Name": "Sparkle",
            "TextureRowIndex": 10,
            "Position": {
              "X": 27,
              "Y": 12
            },
            "Color": {
              "R": 255,
              "G": 255,
              "B": 255,
              "A": 255
            },
            "Interval": 150,
            "LayerDepth": 0.79,
            "Delay": 250,
            "Loops": 3
          },
          {
            "Name": "Custom",
            "Texture": "{{ModId}}/MyAnimations",
            "SourceRect": {
              "X": 0,
              "Y": 0,
              "Width": 16,
              "Height": 16
            },
            "Position": {
              "X": 8,
              "Y": 34
            },
            "Scale": 4,
            "Id": 196748,
            "Delay": 500,
            "Length": 4,
            "Loops": 4,
            "Local": true,
            "Flicker": true
          }
        ]
      }
    },
    {
      "LogName": "Play Animation When In My Town",
      "Action": "EditData",
      "Target": "DMT/Tiles",
      "Entries": {
        "{{ModId}}.DMT_AnimatedTiles": {
          "Locations": [
            "{{ModId}}.MyTown"
          ],
          "Layers": [
            "Back"
          ],
          "Actions": [
            {
              "LogName": "PlayAnimation_1",
              "Key": "DMT/animation",
              "Value": "{{ModId}}.DMTAnimations,Sparkle|Custom",
              "Trigger": "On"
            }
          ]
        }
      }
    }
  ]
}
```