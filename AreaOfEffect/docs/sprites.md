# Sprites

Sprites are visual effects that appear when the spell is cast. This mod has multiple built-in sprite types (see below) taken from various sources of the game code, but you can also create highly customized sprites if you want.

By default, sprites appear on every tile affected by the spell. You can make sprites appear at the target tile only by adding `"PerTile": false`. This is required for the "Fountain" and "Lightning" sprite types to appear.

## Built-in Sprite Types

The mod has the following built-in sprite types:
| Type | Description |
| Balloon | A balloon like when the community center is completed. |
| Butterfly | a random|colored butterfly. |
| EvilRabbit | ...look at the bones! |
| Explosion | An explosion effect like from a bomb exploding. |
| Fire | A dancing flame like the campfire. |
| Ice | An ice sprite like the Ice Rod. |
| Lightning | A lightning bolt like a lightning strike on the farm. |
| Fountain | A water fountain effect like from the prismatic sprinkler. |
| Heart | A floating heart. |
| Object | A sprite that uses a game object texture. | 
| Poof | A poof effect. |
| Smoking | A single puff of smoke like from a chimney. |
| Sparkle | A sparkle effect. |
| Texture | A sprite that uses a custom texture. |

To trigger most of these, all you need is the "Type" field, e.g.:
```
{
    "Type": "Fire"
}
```

## Custom Sprites

Custom sprites are complicated; there are a ton of properties you can set, and not all have yet been implemented by this mod. Besides adding variables to the above types, there are two purely custom sprite types: "Texture" and "Animation". Texture sprites use a custom texture. Animation sprites use an animation index. I will add some documentation for this when I can.

Here's the current list of current possible sprite properties and their default values if any:

- SpriteType Type
- bool PerTile = true
- string Texture
- Rectangle SourceRect
- float Alpha = 1
- float AlphaFade
- Color Color = Color.White
- int Index = -1
- float Interval = 100
- int Length
- int Loops
- Vector2 Offset
- Vector2 Acceleration
- int YStop
- bool Flicker
- bool? Flipped
- bool FlippedRandom
- bool FlippedVertical
- Vector2 Motion
- float Rotation
- float RotationChange
- int SourceWidth = -1
- int SourceHeight = -1
- float LayerDepth = -1
- float? Scale
- float ScaleChange
- float ScaleChangeChange
- int Delay
- int Number
- bool DrawAbove
- bool Bounce