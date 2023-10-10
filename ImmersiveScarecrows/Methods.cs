
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using xTile.Tiles;
using Object = StardewValley.Object;

namespace ImmersiveScarecrows
{
    public partial class ModEntry
    {
        private static Object GetScarecrow(TerrainFeature tf, int which)
        {
            if (!tf.modData.TryGetValue(scarecrowKey + which, out string scarecrowString))
                return null;

            Object obj = null;
            foreach (var kvp in Game1.bigCraftablesInformation)
            {
                if (kvp.Value.Equals(scarecrowString))
                {
                    obj = new Object(Vector2.Zero, kvp.Key);
                    break;
                }
            }
            if(obj is null)
            {
                scarecrowString = scarecrowString.Split('/')[0];
                foreach (var kvp in Game1.bigCraftablesInformation)
                {
                    if (kvp.Value.StartsWith(scarecrowString + "/"))
                    {
                        obj = new Object(Vector2.Zero, kvp.Key);
                        break;
                    }
                }
            }
            if (obj is null)
                return null;

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
            scarecrowDict[guid] = obj;

            return obj;
        }
        private static string GetScarecrowString(Object instance)
        {
            return Game1.bigCraftablesInformation.TryGetValue(instance.ParentSheetIndex, out var str) ? str : instance.Name;
        }
        private static Vector2 GetScarecrowCorner(int i)
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

        private static bool GetScarecrowTileBool(GameLocation location, ref Vector2 tile, ref int which, out string scarecrowString)
        {
            if ((scarecrowString = TileScarecrowString(location, tile, which)) is not null)
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
                    if ((scarecrowString = TileScarecrowString(location, newTile, kvp.Key)) is not null)
                    {
                        tile = newTile;
                        which = kvp.Key;
                        return true;
                    }
                }
            }
            return false;
        }

        private static string TileScarecrowString(GameLocation location, Vector2 tile, int which)
        {
            return (location.terrainFeatures.TryGetValue(tile, out var tf) && tf.modData.TryGetValue(scarecrowKey + which, out var scarecrowString)) ? scarecrowString : null;
        }

        private static bool ReturnScarecrow(Farmer who, GameLocation location, Vector2 placementTile, int which)
        {
            if (location.terrainFeatures.TryGetValue(placementTile, out var tf) && tf is HoeDirt && TryReturnScarecrow(who, location, tf, placementTile, which))
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
                    if (TryReturnScarecrow(who, location, otf, placementTile + kvp.Value, kvp.Key))
                        return true;
                }
            }
            return false;
        }

        private static bool TryReturnScarecrow(Farmer who, GameLocation location, TerrainFeature tf, Vector2 placementTile, int which)
        {
            Object scarecrow = null;
            if (tf.modData.TryGetValue(scarecrowKey + which, out var scarecrowString))
            {
                scarecrow = GetScarecrow(tf, which);
                tf.modData.Remove(scarecrowKey + which);
                tf.modData.Remove(scaredKey + which);
                tf.modData.Remove(guidKey + which);
                if (scarecrow is not null && !who.addItemToInventoryBool(scarecrow))
                {
                    who.currentLocation.debris.Add(new Debris(scarecrow, who.Position));
                }
                SMonitor.Log($"Returning {scarecrow.Name}");
                return true;
            }
            return false;
        }

        private static List<Vector2> GetScarecrowTiles(Vector2 tileLocation, int which, int radius)
        {
            Vector2 start = tileLocation + new Vector2(-1, -1) * (radius - 2);
            Vector2 position = tileLocation + GetScarecrowCorner(which) * 0.5f;
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
        private static bool IsNoScarecrowInRange(Farm f, Vector2 v)
        {
            //SMonitor.Log("Checking for scarecrows near crop");
            foreach (var kvp in f.terrainFeatures.Pairs)
            {
                if (kvp.Value is HoeDirt)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (kvp.Value.modData.TryGetValue(scarecrowKey + i, out var scarecrowString))
                        {
                            var obj = GetScarecrow(kvp.Value, i);
                            if (obj is not null)
                            {
                                var tiles = GetScarecrowTiles(kvp.Key, i, obj.GetRadiusForScarecrow());
                                if(tiles.Contains(v))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            return true;
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
            var textureModel = AccessTools.Method(textureMgr.GetType(), "GetSpecificTextureModel").Invoke(textureMgr, new object[] { str });
            if (textureModel is null)
            {
                return null;
            }
            var textureVariation = int.Parse(obj.modData["AlternativeTextureVariation"]);
            var modConfig = AccessTools.Field(atApi.GetType().Assembly.GetType("AlternativeTextures.AlternativeTextures"), "modConfig").GetValue(null);
            if (textureVariation == -1 || (bool)AccessTools.Method(modConfig.GetType(), "IsTextureVariationDisabled").Invoke(modConfig, new object[] { AccessTools.Method(textureModel.GetType(), "GetId").Invoke(textureModel, new object[] { }), textureVariation }))
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