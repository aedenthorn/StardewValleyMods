
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.ConsoleAsync;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Object = StardewValley.Object;

namespace ImmersiveSprinklersAndScarecrows
{
    public partial class ModEntry
    {

        public static Object GetSprinklerCached(TerrainFeature tf, int which, bool nozzle)
        {
            if (!tf.modData.TryGetValue(guidKey + which, out var guid))
            {
                guid = Guid.NewGuid().ToString();
                tf.modData[guidKey + which] = guid;
            }
            if (!sprinklerDict.TryGetValue(guid, out var obj))
            {
                obj = GetSprinkler(tf, which, nozzle);
                if(obj is not null) 
                    sprinklerDict[guid] = obj;
            }
            return obj;
        }
        public static Object GetSprinkler(TerrainFeature tf, int which, bool nozzle)
        {
            if(tf?.modData.TryGetValue(sprinklerKey + which, out string sprinklerName) != true)
                return null;
            Object obj = null;
            if(tf.modData.ContainsKey(bigCraftableKey + which))
            {
                if(Game1.bigCraftableData.TryGetValue(sprinklerName, out var data))
                {
                    obj = new Object(Vector2.Zero, sprinklerName);
                }
            }
            else if (Game1.objectData.TryGetValue(sprinklerName, out var data))
            {
                obj = new Object(sprinklerName, 1);
            }
            if(obj != null)
            {
                if (nozzle)
                {
                    obj.heldObject.Value = new Object("915", 1);
                }
                if (atApi is not null)
                {
                    foreach (var kvp2 in tf.modData.Pairs)
                    {
                        if (kvp2.Key.EndsWith(which + "") && kvp2.Key.StartsWith(altTexturePrefix))
                        {
                            var key = kvp2.Key.Substring(prefixKey.Length, kvp2.Key.Length - prefixKey.Length - 1);
                            obj.modData[key] = kvp2.Value;
                        }
                    }
                }
                if (!tf.modData.TryGetValue(guidKey + which, out var guid))
                {
                    guid = Guid.NewGuid().ToString();
                    tf.modData[guidKey + which] = guid;
                }
                sprinklerDict[guid] = obj;
            }
            return obj;
        }
        public static Vector2 GetSprinklerCorner(int i)
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

        public static int GetMouseCorner()
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
        public static Object GetSprinklerAtMouse()
        {
            if (Game1.currentLocation == null)
                return null;

            var tile = Game1.currentCursorTile;
            int corner = GetMouseCorner();

            if (!GetSprinklerTileBool(Game1.currentLocation, ref tile, ref corner, out string sprinklerString))
                return null;

            Game1.currentLocation.terrainFeatures.TryGetValue(tile, out var tf);
            return GetSprinkler(tf, corner, false);
        }
        public static bool GetSprinklerTileBool(GameLocation location, ref Vector2 tile, ref int which, out string sprinklerString)
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

        public static string TileSprinklerString(GameLocation location, Vector2 tile, int which)
        {
            return (location.terrainFeatures.TryGetValue(tile, out var tf) && tf.modData.TryGetValue(sprinklerKey + which, out var sprinklerString)) ? sprinklerString : null;
        }

        public static bool ReturnSprinkler(Farmer who, GameLocation location, Vector2 placementTile, int which)
        {
            if (location.terrainFeatures.TryGetValue(placementTile, out var tf) && tf is HoeDirt && TryReturnSprinkler(who, location, tf, which))
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
                    if (TryReturnSprinkler(who, location, otf, kvp.Key))
                        return true;
                }
            }
            return false;
        }

        public static bool DropSprinkler(TerrainFeature tf, int which)
        {
            Object sprinkler = null;
            if (tf.modData.ContainsKey(sprinklerKey + which))
            {
                sprinkler = GetSprinkler(tf, which, false);
                tf.Location.debris.Add(new Debris(sprinkler, tf.Tile * 64));
                tf.modData.Remove(sprinklerKey + which);
                tf.modData.Remove(guidKey + which);
                tf.modData.Remove(bigCraftableKey + which);
                if (tf.modData.ContainsKey(enricherKey + which))
                {
                    tf.modData.Remove(enricherKey + which);
                    var e = new Object("913", 1);
                    tf.Location.debris.Add(new Debris(e, tf.Tile * 64));
                }
                if (tf.modData.ContainsKey(nozzleKey + which))
                {
                    tf.modData.Remove(nozzleKey + which);
                    var n = new Object("915", 1);
                    tf.Location.debris.Add(new Debris(n, tf.Tile * 64));

                }
                if (tf.modData.TryGetValue(fertilizerKey + which, out var fertString))
                {
                    tf.modData.Remove(fertilizerKey + which);
                    Object f = GetFertilizer(fertString);
                    tf.Location.debris.Add(new Debris(f, tf.Tile * 64));
                }
                SMonitor.Log($"Dropped {sprinkler?.Name}");
                return true;
            }
            return false;
        }
        public static bool TryReturnSprinkler(Farmer who, GameLocation location, TerrainFeature tf, int which)
        {
            Object sprinkler = null;
            if (tf.modData.ContainsKey(sprinklerKey + which))
            {
                sprinkler = GetSprinkler(tf, which, false);
                TryReturnObject(sprinkler, who);
                tf.modData.Remove(sprinklerKey + which);
                tf.modData.Remove(guidKey + which);
                tf.modData.Remove(bigCraftableKey + which);
                if (tf.modData.ContainsKey(enricherKey + which))
                {
                    tf.modData.Remove(enricherKey + which);
                    var e = new Object("913", 1);
                    TryReturnObject(e, who);
                }
                if (tf.modData.ContainsKey(nozzleKey + which))
                {
                    tf.modData.Remove(nozzleKey + which);
                    var n = new Object("915", 1);
                    TryReturnObject(n, who);

                }
                if (tf.modData.TryGetValue(fertilizerKey + which, out var fertString))
                {
                    tf.modData.Remove(fertilizerKey + which);
                    Object f = GetFertilizer(fertString);
                    TryReturnObject(f, who);
                }
                SMonitor.Log($"Returned {sprinkler?.Name}");
                return true;
            }
            return false;
        }

        public static void TryReturnObject(Object obj, Farmer who)
        {
            if (obj is null)
                return;
            SMonitor.Log($"Trying to return {obj.Name}");
            if (!who.addItemToInventoryBool(obj))
            {
                who.currentLocation.debris.Add(new Debris(obj, who.Position));
            }
        }

        public static Object GetFertilizer(string fertString)
        {
            var fertData = fertString.Split(',');
            return new Object(fertData[0], int.Parse(fertData[1]));
        }
        public static int GetSprinklerRadius(Object obj)
        {
            if (!Config.SprinklerRadii.TryGetValue(obj.ItemId, out int radius) && !Config.SprinklerRadii.TryGetValue(obj.Name, out radius))
                return obj.GetModifiedRadiusForSprinkler();
            if (obj.heldObject.Value != null && Utility.IsNormalObjectAtParentSheetIndex((Object)obj.heldObject.Value, "915"))
            {
                radius++;
            }
            return radius;
        }

        public static List<Vector2> GetSprinklerTiles(Vector2 tileLocation, int which, int radius)
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

        public static void ActivateSprinkler(GameLocation environment, Vector2 tileLocation, Object obj, int which, bool delay)
        {
            if (Game1.player.team.SpecialOrderRuleActive("NO_SPRINKLER", null))
            {
                return;
            }
            int radius = GetSprinklerRadius(obj);
            if (radius < 0)
                return;

            obj.Location = environment;

            foreach (Vector2 tile in GetSprinklerTiles(tileLocation, which, radius))
            {
                obj.ApplySprinkler(tile);
                if(environment.objects.TryGetValue(tile, out var o) && o is IndoorPot) 
                {
                    (o as IndoorPot).hoeDirt.Value.state.Value = 1;
                    o.showNextIndex.Value = true;
                }
            }
            ApplySprinklerAnimation(tileLocation, which, radius, environment, delay ? Game1.random.Next(1000) : 0);
        }
        public static void ApplySprinklerAnimation(Vector2 tileLocation, int which, int radius, GameLocation location, int delay)
        {
            int id = (int)(tileLocation.X * 4000f + tileLocation.Y * 10 + which);
            if (radius < 0 || location.getTemporarySpriteByID(id) is not null)
            {
                return;
            }
            var corner = GetSprinklerCorner(which);
            var position = tileLocation * 64 + corner * 32;
            float layerDepth = 1;
            if (radius == 0)
            {
                float rotation = 60 * MathHelper.Pi / 180;
                int a = 24;
                int b = 40;
                location.temporarySprites.Add(new TemporaryAnimatedSprite(29, position + new Vector2(a, -b), Color.White * 0.5f, 4, false, 60f, 100, -1, layerDepth, -1, 0)
                {
                    rotation = rotation,
                    delayBeforeAnimationStart = delay,
                    id = id
                });
                location.temporarySprites.Add(new TemporaryAnimatedSprite(29, position + new Vector2(b, a), Color.White * 0.5f, 4, false, 60f, 100, -1, layerDepth, -1, 0)
                {
                    rotation = 1.57079637f + rotation,
                    delayBeforeAnimationStart = delay,
                    id = id
                });
                location.temporarySprites.Add(new TemporaryAnimatedSprite(29, position + new Vector2(-a, b), Color.White * 0.5f, 4, false, 60f, 100, -1, layerDepth, -1, 0)
                {
                    rotation = 3.14159274f + rotation,
                    delayBeforeAnimationStart = delay,
                    id = id
                });
                location.temporarySprites.Add(new TemporaryAnimatedSprite(29, position + new Vector2(-b, -a), Color.White * 0.5f, 4, false, 60f, 100, -1, layerDepth, -1, 0)
                {
                    rotation = 4.712389f + rotation,
                    delayBeforeAnimationStart = delay,
                    id = id
                });
                return;
            }
            if (radius == 1)
            {
                location.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 1984, 192, 192), 60f, 3, 100, position + new Vector2(-96f, -96f), false, false)
                {
                    color = Color.White * 0.4f,
                    delayBeforeAnimationStart = delay,
                    id = id,
                    layerDepth = layerDepth,
                    scale = 1.3f
                });
                return;
            }
            float scale = radius / 1.6f;
            location.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 2176, 320, 320), 60f, 4, 100, position + new Vector2(32f, 32f) + new Vector2(-160f, -160f) * scale, false, false)
            {
                color = Color.White * 0.4f,
                delayBeforeAnimationStart = delay,
                id = id,
                layerDepth = layerDepth,
                scale = scale
            });
        }

        public static void SetAltTextureForObject(Object obj)
        {
            if (atApi is null)
                return;
            try
            {
                AccessTools.Method(atApi.GetType(), "SetTextureForObject").Invoke(atApi, new object[] { obj });
            }
            catch
            {
            }
        }

        public static Texture2D GetAltTextureForObject(Object obj, out Rectangle sourceRect)
        {
            sourceRect = new Rectangle();
            var inputParams = new object[] { obj, sourceRect };
            Texture2D result = (Texture2D)AccessTools.Method(atApi.GetType(), "GetTextureForObject").Invoke(atApi, inputParams);
            sourceRect = (Rectangle)inputParams[1];
            return result;
        }

        public static bool RemoveWhere(KeyValuePair<Vector2, TerrainFeature> pair)
        {
            HoeDirt dirt = pair.Value as HoeDirt;
            var match = dirt != null && dirt.crop == null && Game1.random.NextDouble() < 0.8;
            if (!Config.EnableMod || !match)
                return match;

            for (int i = 0; i < 4; i++)
            {
                if (dirt.modData.TryGetValue(sprinklerKey + i, out var sprinklerString))
                {
                    SMonitor.Log($"Preventing hoedirt removal");
                    return false;
                }
            }
            return match;
        }
    }
}