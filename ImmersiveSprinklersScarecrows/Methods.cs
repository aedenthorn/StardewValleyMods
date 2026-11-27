
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace ImmersiveSprinklersScarecrows
{
    public partial class ModEntry
    {

        public static bool IsImmersive(Object obj)
        {
            return obj.modData.ContainsKey(sprinklerKey) || obj.modData.ContainsKey(scarecrowKey); 
        }

        public static bool TryGetSprinkler(GameLocation location, Vector2 tile, out Object sprinkler)
        {
            sprinkler = null;
            return location?.Objects?.TryGetValue(tile, out sprinkler) == true && sprinkler.modData.ContainsKey(sprinklerKey);
        }

        public static bool TryGetScarecrow(GameLocation location, Vector2 tile, out Object scarecrow)
        {
            scarecrow = null;
            return location?.Objects?.TryGetValue(tile, out scarecrow) == true && scarecrow.modData.ContainsKey(scarecrowKey);
        }
        public static Object GetSprinklerAtMouse()
        {
            if (Game1.currentLocation == null)
                return null;

            var tile = Game1.currentCursorTile;

            if (!TryGetSprinkler(Game1.currentLocation, tile, out var sprinkler))
                return null;
            return sprinkler;
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

        public static List<Vector2> GetSprinklerTiles(Vector2 tileLocation, int radius)
        {
            
            Vector2 start = tileLocation + new Vector2(-1, -1) * radius;
            List<Vector2> list = new();
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

        public static void ActivateSprinkler(GameLocation environment, Vector2 tileLocation, Object obj, bool delay)
        {
            if (Game1.player.team.SpecialOrderRuleActive("NO_SPRINKLER", null))
            {
                return;
            }
            int radius = GetSprinklerRadius(obj);
            if (radius < 0)
                return;

            obj.Location = environment;

            foreach (Vector2 tile in GetSprinklerTiles(tileLocation, radius))
            {
                obj.ApplySprinkler(tile);
                if(environment.objects.TryGetValue(tile, out var o) && o is IndoorPot) 
                {
                    (o as IndoorPot).hoeDirt.Value.state.Value = 1;
                    o.showNextIndex.Value = true;
                }
            }
            ApplySprinklerAnimation(tileLocation, radius, environment, delay ? Game1.random.Next(1000) : 0);
        }
        public static void ApplySprinklerAnimation(Vector2 tileLocation, int radius, GameLocation location, int delay)
        {
            int id = (int)(tileLocation.X * 4000f + tileLocation.Y * 10);
            if (radius < 0 || location.getTemporarySpriteByID(id) is not null)
            {
                return;
            }
            var position = tileLocation * 64 + new Vector2(32, 32);
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

        public static List<Vector2> GetScarecrowTiles(Vector2 tileLocation, int radius)
        {
            Vector2 start = tileLocation + new Vector2(-1, -1) * (radius - 2);
            Vector2 position = tileLocation + new Vector2(0.5f,0.5f);
            List<Vector2> list = new();
            var diameter = (radius - 1) * 2;
            for (int x = 0; x < diameter; x++)
            {
                for (int y = 0; y < diameter; y++)
                {
                    Vector2 tile = start + new Vector2(x, y);
                    if ((int)Math.Ceiling(Vector2.Distance(position, tile)) <= radius)
                        list.Add(tile);
                }
            }
            return list;

        }
        public static bool IsScarecrowInRange(bool scarecrow, Farm f, Vector2 v)
        {
            if (!Config.EnableMod || scarecrow)
                return scarecrow;
            foreach(var kvp in f.Objects.Pairs)
            {
                if (kvp.Value.modData.ContainsKey(scarecrowKey))
                {
                    var tiles = GetScarecrowTiles(kvp.Key, kvp.Value.GetRadiusForScarecrow());
                    if (tiles.Contains(v))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static Texture2D GetAltTextureForObject(Object obj, out Rectangle sourceRect)
        {
            sourceRect = new Rectangle();
            var inputParams = new object[] { obj, sourceRect };
            Texture2D result = (Texture2D)AccessTools.Method(atApi.GetType(), "GetTextureForObject").Invoke(atApi, inputParams);
            sourceRect = (Rectangle)inputParams[1];
            return result;
        }
        public static Vector2 GetMouseTile()
        {
            var m = Game1.getMousePosition();
            var v = Game1.viewport.Location;
            return new Vector2((m.X + v.X - 32) / 64, (m.Y + v.Y - 32) / 64);
        }
    }
}