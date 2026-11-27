# Projectiles

Spells without projectiles will trigger their effects immediately upon casting. Spells with projectiles will not trigger until the projectile hits an object or reaches the target tile (except in the case of "Line" spells which will cause their effects in a line as the projectile travels).

Projectile properties are too numerous to list here, but two simple types are explained below. For a full list of projectile properties, see the [Modding:Weapons#Advanced](https://wiki.stardewvalley.net/Modding:Weapons#Advanced) page on the Stardew Valley wiki. This mod uses the same data format, with the addition of "Texture" and "SourceRect" fields as described below.

## Simple Projectile

To create a simple projectile, just specify the sprite index, e.g.:
```
{
    "SpriteIndex": 10
}
```

Sprite indexes correspond to the `TileSheets/projectiles` file in the game content.

## Custom Texture Projectile

You can also specify a custom texture and optional source rect for a projectile. For example:
```
{
    "Texture": "Path/To/My/projectile.png",
    "SourceRect": {
        "X": 0,
        "Y": 0,
        "Width": 16,
        "Height": 16
    }
}
```