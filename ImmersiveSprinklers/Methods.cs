
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Sickhead.Engine.Util;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Object = StardewValley.Object;

namespace ImmersiveSprinklers
{
    public partial class ModEntry
    {

        private static Object GetSprinklerCached(TerrainFeature tf, int which, bool nozzle)
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
        private static Object GetSprinkler(TerrainFeature tf, int which, bool nozzle)
        {
            if(!tf.modData.TryGetValue(sprinklerKey + which, out string sprinklerString))
                return null;
            var bc = tf.modData.ContainsKey(bigCraftableKey + which);
            foreach (var kvp in bc ? Game1.bigCraftablesInformation :  Game1.objectInformation)
            {
                if (kvp.Value.Equals(sprinklerString) || kvp.Value.StartsWith(sprinklerString + "/"))
                {
                    var obj = bc ? new Object(Vector2.Zero, kvp.Key) : new Object(kvp.Key, 1);
                    if (nozzle)
                    {
                        obj.heldObject.Value = new Object(915, 1);
                    }
                    if(atApi is not null)
                    {
                        foreach (var kvp2 in tf.modData.Pairs)
                        {
                            if(kvp2.Key.EndsWith(which+"") && kvp2.Key.StartsWith(altTexturePrefix))
                            {
                                var key = kvp2.Key.Substring(prefixKey.Length, kvp2.Key.Length - prefixKey.Length - 1);
                                obj.modData[key] = kvp2.Value;
                            }
                        }
                    }
                    if(!tf.modData.TryGetValue(guidKey + which, out var guid))
                    {
                        guid = Guid.NewGuid().ToString();
                        tf.modData[guidKey + which] = guid;
                    }
                    sprinklerDict[guid] = obj;
                    return obj;
                }
            }
            return null;
        }
        private static string GetSprinklerString(Object instance)
        {
            if (instance.bigCraftable.Value && Game1.bigCraftablesInformation.TryGetValue(instance.ParentSheetIndex, out string str))
                return str;
            if (!instance.bigCraftable.Value && Game1.objectInformation.TryGetValue(instance.ParentSheetIndex, out str))
                return str;
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

        private static bool ReturnSprinkler(Farmer who, GameLocation location, Vector2 placementTile, int which)
        {
            if (location.terrainFeatures.TryGetValue(placementTile, out var tf) && tf is HoeDirt && TryReturnSprinkler(who, location, tf, placementTile, which))
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
                    var e = new Object(913, 1);
                    TryReturnObject(e, who);
                }
                if (tf.modData.ContainsKey(nozzleKey + which))
                {
                    tf.modData.Remove(nozzleKey + which);
                    var n = new Object(915, 1);
                    TryReturnObject(n, who);

                }
                if (tf.modData.TryGetValue(fertilizerKey + which, out var fertString))
                {
                    tf.modData.Remove(fertilizerKey + which);
                    Object f = GetFertilizer(fertString);
                    TryReturnObject(f, who);
                }
                SMonitor.Log($"Returning {sprinkler?.Name}");
                return true;
            }
            return false;
        }

        private static void TryReturnObject(Object obj, Farmer who)
        {
            if (obj is null)
                return;
            SMonitor.Log($"Trying to return {obj.Name}");
            if (!who.addItemToInventoryBool(obj))
            {
                who.currentLocation.debris.Add(new Debris(obj, who.Position));
            }
        }

        private static Object GetFertilizer(string fertString)
        {
            var fertData = fertString.Split(',');
            return new Object(int.Parse(fertData[0]), int.Parse(fertData[1]));
        }
        private static int GetSprinklerRadius(Object obj)
        {
            if (!Config.SprinklerRadii.TryGetValue(obj.Name, out int radius))
                return obj.GetModifiedRadiusForSprinkler();
            if (obj.heldObject.Value != null && Utility.IsNormalObjectAtParentSheetIndex((Object)obj.heldObject.Value, 915))
            {
                radius++;
            }
            return radius;
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
            if (Game1.player.team.SpecialOrderRuleActive("NO_SPRINKLER", null))
            {
                return;
            }
            int radius = GetSprinklerRadius(obj);
            if (radius < 0)
                return;
            foreach (Vector2 tile in GetSprinklerTiles(tileLocation, which, radius))
            {
                obj.ApplySprinkler(environment, tile);
                if(environment.objects.TryGetValue(tile, out var o) && o is IndoorPot) 
                {
                    (o as IndoorPot).hoeDirt.Value.state.Value = 1;
                    o.showNextIndex.Value = true;
                }
            }
            ApplySprinklerAnimation(tileLocation, which, radius, environment, delay ? Game1.random.Next(1000) : 0);
        }
        private static void ApplySprinklerAnimation(Vector2 tileLocation, int which, int radius, GameLocation location, int delay)
        {

            if (radius < 0 || location.getTemporarySpriteByID((int)(tileLocation.X * 4000f + tileLocation.Y)) is not null)
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
                    id = tileLocation.X * 4000f + tileLocation.Y
                });
                location.temporarySprites.Add(new TemporaryAnimatedSprite(29, position + new Vector2(b, a), Color.White * 0.5f, 4, false, 60f, 100, -1, layerDepth, -1, 0)
                {
                    rotation = 1.57079637f + rotation,
                    delayBeforeAnimationStart = delay,
                    id = tileLocation.X * 4000f + tileLocation.Y
                });
                location.temporarySprites.Add(new TemporaryAnimatedSprite(29, position + new Vector2(-a, b), Color.White * 0.5f, 4, false, 60f, 100, -1, layerDepth, -1, 0)
                {
                    rotation = 3.14159274f + rotation,
                    delayBeforeAnimationStart = delay,
                    id = tileLocation.X * 4000f + tileLocation.Y
                });
                location.temporarySprites.Add(new TemporaryAnimatedSprite(29, position + new Vector2(-b, -a), Color.White * 0.5f, 4, false, 60f, 100, -1, layerDepth, -1, 0)
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
                id = tileLocation.X * 4000f + tileLocation.Y,
                layerDepth = layerDepth,
                scale = scale
            });
        }


        private static void SetAltTextureForObject(Object obj)
        {
            if (atApi is null)
                return;

            var textureMgr = AccessTools.Field(atApi.GetType().Assembly.GetType("AlternativeTextures.AlternativeTextures"), "textureManager").GetValue(null);

            var modelType = "Craftable";
            var baseName = AccessTools.Method(atApi.GetType().Assembly.GetType("AlternativeTextures.Framework.Patches.PatchTemplate"), "GetObjectName").Invoke(null, new object[] { obj });
            var instanceName = $"{modelType}_{baseName}";
            var instanceSeasonName = $"{instanceName}_{Game1.currentSeason}";

            bool hasAlt = (bool)AccessTools.Method(textureMgr.GetType(), "DoesObjectHaveAlternativeTexture", new System.Type[] { typeof(string) }).Invoke(textureMgr, new object[] { instanceName });
            bool hasAltSeason = (bool)AccessTools.Method(textureMgr.GetType(), "DoesObjectHaveAlternativeTexture", new System.Type[] { typeof(string) }).Invoke(textureMgr, new object[] { instanceSeasonName });
            MethodInfo assignModData = AccessTools.Method(atApi.GetType().Assembly.GetType("AlternativeTextures.Framework.Patches.PatchTemplate"), "AssignModData").MakeGenericMethod(typeof(Object));
            if ((bool)AccessTools.Method(atApi.GetType().Assembly.GetType("AlternativeTextures.Framework.Patches.PatchTemplate"), "HasCachedTextureName").MakeGenericMethod(typeof(Object)).Invoke(null, new object[] { obj, false }))
            {
                return;
            }
            else if (hasAlt && hasAltSeason)
            {
                var result = Game1.random.Next(2) > 0 ? assignModData.Invoke(null, new object[] { obj, instanceSeasonName, true, obj.bigCraftable.Value }) : assignModData.Invoke(null, new object[] { obj, instanceName, false, obj.bigCraftable.Value });
                return;
            }
            else
            {
                if (hasAlt)
                {
                    assignModData.Invoke(null, new object[] { obj, instanceName, false, obj.bigCraftable.Value });
                    return;
                }

                if (hasAltSeason)
                {
                    assignModData.Invoke(null, new object[] { obj, instanceSeasonName, true, obj.bigCraftable.Value });
                    return;
                }
            }

            AccessTools.Method(atApi.GetType().Assembly.GetType("AlternativeTextures.Framework.Patches.PatchTemplate"), "AssignDefaultModData").MakeGenericMethod(typeof(Object)).Invoke(null, new object[] { obj, instanceSeasonName, true, obj.bigCraftable.Value });
        }


        private static Texture2D GetAltTextureForObject(Object obj, out Rectangle sourceRect)
        {
            sourceRect = new Rectangle();
            if (!obj.modData.TryGetValue("AlternativeTextureName", out var str))
                return null;
            var textureMgr = AccessTools.Field(atApi.GetType().Assembly.GetType("AlternativeTextures.AlternativeTextures"), "textureManager").GetValue(null);
            var textureModel = AccessTools.Method(textureMgr.GetType(),"GetSpecificTextureModel").Invoke(textureMgr, new object[] { str } );
            if (textureModel is null)
            {
                return null;
            }
            var textureVariation = int.Parse(obj.modData["AlternativeTextureVariation"]);
            var modConfig = AccessTools.Field(atApi.GetType().Assembly.GetType("AlternativeTextures.AlternativeTextures"), "modConfig").GetValue(null);
            if (textureVariation == -1 || (bool)AccessTools.Method(modConfig.GetType(), "IsTextureVariationDisabled").Invoke(modConfig, new object[] { AccessTools.Method(textureModel.GetType(), "GetId").Invoke(textureModel, new object[] { }), textureVariation } ))
            {
                return null;
            }
            var textureOffset = (int)AccessTools.Method(textureModel.GetType(), "GetTextureOffset").Invoke(textureModel, new object[] { textureVariation });

            // Get the current X index for the source tile
            var xTileOffset = obj.modData.ContainsKey("AlternativeTextureSheetId") ? obj.ParentSheetIndex - int.Parse(obj.modData["AlternativeTextureSheetId"]) : 0;
            if (obj.showNextIndex.Value)
            {
                xTileOffset += 1;
            }

            // Override xTileOffset if AlternativeTextureModel has an animation
            if ((bool)AccessTools.Method(textureModel.GetType(), "HasAnimation").Invoke(textureModel, new object[] { textureVariation }))
            {
                if (!obj.modData.ContainsKey("AlternativeTextureCurrentFrame") || !obj.modData.ContainsKey("AlternativeTextureFrameIndex") || !obj.modData.ContainsKey("AlternativeTextureFrameDuration") || !obj.modData.ContainsKey("AlternativeTextureElapsedDuration"))
                {
                    var animationData = AccessTools.Method(textureModel.GetType(), "GetAnimationDataAtIndex").Invoke(textureModel, new object[] { textureVariation, 0 });
                    obj.modData["AlternativeTextureCurrentFrame"] = "0";
                    obj.modData["AlternativeTextureFrameIndex"] = "0";
                    obj.modData["AlternativeTextureFrameDuration"] = AccessTools.Property(animationData.GetType(), "Duration").GetValue(animationData).ToString();// Animation.ElementAt(0).Duration.ToString();
                    obj.modData["AlternativeTextureElapsedDuration"] = "0";
                }

                var currentFrame = int.Parse(obj.modData["AlternativeTextureCurrentFrame"]);
                var frameIndex = int.Parse(obj.modData["AlternativeTextureFrameIndex"]);
                var frameDuration = int.Parse(obj.modData["AlternativeTextureFrameDuration"]);
                var elapsedDuration = int.Parse(obj.modData["AlternativeTextureElapsedDuration"]);

                if (elapsedDuration >= frameDuration)
                {
                    var animationDataList = (IEnumerable<object>)AccessTools.Method(textureModel.GetType(), "GetAnimationData").Invoke(textureModel, new object[] { textureVariation });
                    frameIndex = frameIndex + 1 >= animationDataList.Count() ? 0 : frameIndex + 1;

                    var animationData = AccessTools.Method(textureModel.GetType(), "GetAnimationDataAtIndex").Invoke(textureModel, new object[] { textureVariation, frameIndex });
                    currentFrame = (int)AccessTools.Property(animationData.GetType(), "Frame").GetValue(animationData);

                    obj.modData["AlternativeTextureCurrentFrame"] = currentFrame.ToString();
                    obj.modData["AlternativeTextureFrameIndex"] = frameIndex.ToString();
                    obj.modData["AlternativeTextureFrameDuration"] = AccessTools.Property(animationData.GetType(), "Duration").GetValue(animationData).ToString();
                    obj.modData["AlternativeTextureElapsedDuration"] = "0";
                }
                else
                {
                    obj.modData["AlternativeTextureElapsedDuration"] = (elapsedDuration + Game1.currentGameTime.ElapsedGameTime.Milliseconds).ToString();
                }

                xTileOffset = currentFrame;
            }
            var w = (int)AccessTools.Property(textureModel.GetType(), "TextureWidth").GetValue(textureModel);
            var h = (int)AccessTools.Property(textureModel.GetType(), "TextureHeight").GetValue(textureModel);
            sourceRect = new Rectangle(xTileOffset * w, textureOffset, w, h);
            return (Texture2D)AccessTools.Method(textureModel.GetType(), "GetTexture").Invoke(textureModel, new object[] { textureVariation });
        }
    }
}