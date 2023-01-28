
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;

namespace ImmersiveSprinklers
{
    public partial class ModEntry
    {

        private static Object GetSprinkler(string sprinklerString)
        {

            foreach (var kvp in Game1.objectInformation)
            {
                if (kvp.Value.StartsWith(sprinklerString + "/"))
                {
                    var obj = new Object(kvp.Key, 1);
                    sprinklerDict[sprinklerString] = obj;
                    return obj;
                }
            }
            return null;
        }
        private static string GetSprinklerString(Object instance)
        {
            return instance.Name;
        }
        private static Vector2 GetSprinklerCorner(int i)
        {
            switch (i)
            {
                case 0:
                    return new Vector2(-1, -1);
                case 1:
                    return new Vector2(1, -1);
                case 2:
                    return new Vector2(-1, 1);
                default:
                    return new Vector2(1, 1);
            }
        }

        private static int GetMouseCorner()
        {
            var x = Game1.getMouseX() + Game1.viewport.X;
            var y = Game1.getMouseY() + Game1.viewport.Y;
            if (x % 64 < 32)
            {
                if (y % 64 < 32)
                {
                    return 0;
                }
                else
                {
                    return 2;
                }
            }
            else
            {
                if (y % 64 < 32)
                {
                    return 1;
                }
                else
                {
                    return 3;
                }
            }
        }

        private static bool GetSprinklerTileBool(GameLocation location, ref Vector2 tile, ref int which, out string sprinklerString)
        {
            if ((sprinklerString = TileSprinklerString(location, tile, which)) is not null)
            { 
                return true; 
            }
            else
            {
                Dictionary<int, Vector2> dict = new();
                switch (which)
                {
                    case 0:
                        dict.Add(3, new Vector2(-1, -1));
                        dict.Add(2, new Vector2(0, -1));
                        dict.Add(1, new Vector2(-1, 0));
                        break;
                    case 1:
                        dict.Add(3, new Vector2(0, -1));
                        dict.Add(2, new Vector2(1, 1));
                        dict.Add(0, new Vector2(1, 0));
                        break;
                    case 2:
                        dict.Add(3, new Vector2(-1, 0));
                        dict.Add(1, new Vector2(-1, 1));
                        dict.Add(0, new Vector2(0, 1));
                        break;
                    case 3:
                        dict.Add(2, new Vector2(1, 0));
                        dict.Add(1, new Vector2(0, 1));
                        dict.Add(0, new Vector2(1, 1));
                        break;
                }
                foreach (var kvp in dict)
                {
                    var newTile = tile + kvp.Value;
                    if ((sprinklerString = TileSprinklerString(location, newTile, kvp.Key)) is not null)
                    {
                        tile = newTile;
                        which = kvp.Key;
                        return true;
                    }
                }
            }
            return false;
        }

        private static string TileSprinklerString(GameLocation location, Vector2 tile, int which)
        {
            return (location.terrainFeatures.TryGetValue(tile, out var tf) && tf.modData.TryGetValue(sprinklerKey + which, out var sprinklerString)) ? sprinklerString : null;
        }

        private static bool ReturnSprinkler(Farmer who, GameLocation location, TerrainFeature tf, Vector2 placementTile, int which)
        {
            if (TryReturnSprinkler(who, location, tf, placementTile, which))
            { 
                return true; 
            }
            else
            {
                Dictionary<int, Vector2> dict = new();
                switch (which)
                {
                    case 0:
                        dict.Add(3, new Vector2(-1, -1));
                        dict.Add(2, new Vector2(0, -1));
                        dict.Add(1, new Vector2(-1, 0));
                        break;
                    case 1:
                        dict.Add(3, new Vector2(0, -1));
                        dict.Add(2, new Vector2(1, 1));
                        dict.Add(0, new Vector2(1, 0));
                        break;
                    case 2:
                        dict.Add(3, new Vector2(-1, 0));
                        dict.Add(1, new Vector2(-1, 1));
                        dict.Add(0, new Vector2(0, 1));
                        break;
                    case 3:
                        dict.Add(2, new Vector2(1, 0));
                        dict.Add(1, new Vector2(0, 1));
                        dict.Add(0, new Vector2(1, 1));
                        break;
                }
                foreach (var kvp in dict)
                {
                    if (!location.terrainFeatures.TryGetValue(placementTile + kvp.Value, out var otf))
                        continue;
                    if (TryReturnSprinkler(who, location, otf, placementTile + kvp.Value, kvp.Key))
                        return true;
                }
            }
            return false;
        }

        private static bool TryReturnSprinkler(Farmer who, GameLocation location, TerrainFeature tf, Vector2 placementTile, int which)
        {
            Object sprinkler = null;
            if (tf.modData.TryGetValue(sprinklerKey + which, out var sprinklerString))
            {
                tf.modData.Remove(sprinklerKey + which);
                sprinkler = GetSprinkler(sprinklerString);
                if (sprinkler is not null && !who.addItemToInventoryBool(sprinkler))
                {
                    who.currentLocation.debris.Add(new Debris(sprinkler, who.Position));
                }
                return true;
            }
            return false;
        }

        private static List<Vector2> GetSprinklerTiles(Vector2 tileLocation, int which, int radius)
        {
            Vector2 start = tileLocation + new Vector2(-1, -1) * radius;
            List<Vector2> list = new();
            switch (which)
            {
                case 0:
                    start += new Vector2(-1, -1);
                    break;
                case 1:
                    start += new Vector2(0, -1);
                    break;
                case 2:
                    start += new Vector2(-1, 0);
                    break;
            }
            var diameter = (radius + 1) * 2;
            for (int x = 0; x < diameter; x++)
            {
                for (int y = 0; y < diameter; y++)
                {
                    list.Add(start + new Vector2(x, y));
                }
            }
            return list;

        }

        private static void ActivateSprinkler(GameLocation environment, Vector2 tileLocation, Object obj, int which, bool delay)
        {
            var radius = obj.GetModifiedRadiusForSprinkler();
            if (Game1.player.team.SpecialOrderRuleActive("NO_SPRINKLER", null))
            {
                return;
            }
            foreach (Vector2 v2 in GetSprinklerTiles(tileLocation, which, radius))
            {
                obj.ApplySprinkler(environment, v2);
            }
            ApplySprinklerAnimation(tileLocation, which, radius, environment, delay ? Game1.random.Next(1000) : 0);
        }
        private static void ApplySprinklerAnimation(Vector2 tileLocation, int which, int radius, GameLocation location, int delay)
        {

            if (radius < 0)
            {
                return;
            }
            var corner = GetSprinklerCorner(which);
            var position = tileLocation * 64 + corner * 32;
            if (radius == 0)
            {
                float rotation = 60 * MathHelper.Pi / 180;
                int a = 24;
                int b = 40;
                location.temporarySprites.Add(new TemporaryAnimatedSprite(29, position + new Vector2(a, -b), Color.White * 0.5f, 4, false, 60f, 100, -1, -1f, -1, 0)
                {
                    rotation = rotation,
                    delayBeforeAnimationStart = delay,
                    id = tileLocation.X * 4000f + tileLocation.Y
                });
                location.temporarySprites.Add(new TemporaryAnimatedSprite(29, position + new Vector2(b, a), Color.White * 0.5f, 4, false, 60f, 100, -1, -1f, -1, 0)
                {
                    rotation = 1.57079637f + rotation,
                    delayBeforeAnimationStart = delay,
                    id = tileLocation.X * 4000f + tileLocation.Y
                });
                location.temporarySprites.Add(new TemporaryAnimatedSprite(29, position + new Vector2(-a, b), Color.White * 0.5f, 4, false, 60f, 100, -1, -1f, -1, 0)
                {
                    rotation = 3.14159274f + rotation,
                    delayBeforeAnimationStart = delay,
                    id = tileLocation.X * 4000f + tileLocation.Y
                });
                location.temporarySprites.Add(new TemporaryAnimatedSprite(29, position + new Vector2(-b, -a), Color.White * 0.5f, 4, false, 60f, 100, -1, -1f, -1, 0)
                {
                    rotation = 4.712389f + rotation,
                    delayBeforeAnimationStart = delay,
                    id = tileLocation.X * 4000f + tileLocation.Y
                });
                return;
            }
            if (radius == 1)
            {
                location.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 1984, 192, 192), 60f, 3, 100, position + new Vector2(-96f, -96f), false, false)
                {
                    color = Color.White * 0.4f,
                    delayBeforeAnimationStart = delay,
                    id = tileLocation.X * 4000f + tileLocation.Y,
                    scale = 1.3f
                });
                return;
            }
            float scale = radius / 1.6f;
            location.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 2176, 320, 320), 60f, 4, 100, position + new Vector2(32f, 32f) + new Vector2(-160f, -160f) * scale, false, false)
            {
                color = Color.White * 0.4f,
                delayBeforeAnimationStart = delay,
                id = tileLocation.X * 4000f + tileLocation.Y,
                scale = scale
            });
        }
    }
}