using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Network;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace CustomChestTypes
{
    public class ModEntry : Mod, IAssetEditor
    {
        public static ModEntry context;
        private static ModConfig Config;
        private static IMonitor SMonitor;
        private static IModHelper SHelper;
        private static Dictionary<int, CustomChestType> customChestTypesDict = new Dictionary<int, CustomChestType>();

        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;

            SMonitor = Monitor;
            SHelper = Helper;


            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

            var harmony = new Harmony(this.ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.isCollidingPosition), new Type[] { typeof(Rectangle), typeof(xTile.Dimensions.Rectangle), typeof(bool), typeof(int), typeof(bool), typeof(Character)}),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(GameLocation_isCollidingPosition_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Debris), nameof(Debris.collect)),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(Debris_collect_Prefix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(Object), "loadDisplayName"),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(Object_loadDisplayName_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Object), nameof(Object.placementAction)),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(Object_placementAction_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Object), nameof(Object.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(Object_draw_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Object), nameof(Object.drawInMenu), new Type[] { typeof(SpriteBatch), typeof(Vector2),  typeof(float),  typeof(float),  typeof(float),  typeof(StackDrawType),  typeof(Color),  typeof(bool)}),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(Object_drawInMenu_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Object), nameof(Object.drawWhenHeld)),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(Object_drawWhenHeld_Prefix))
            );
            
            harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(Chest_draw_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.checkForAction)),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(Chest_checkForAction_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.GetActualCapacity)),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(Chest_GetActualCapacity_Prefix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.getCarpenterStock)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(Utility_getCarpenterStock_Postfix))
            );
        }

        /// <summary>Get whether this instance can edit the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanEdit<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals("Data/BigCraftablesInformation"))
            {
                return true;
            }

            return false;
        }

        /// <summary>Edit a matched asset.</summary>
        /// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it.</param>
        public void Edit<T>(IAssetData asset)
        {
            if (asset.AssetNameEquals("Data/BigCraftablesInformation") && customChestTypesDict.Count > 0)
            {

                var editor = asset.AsDictionary<int, string>();
                SMonitor.Log($"Patching BigCraftablesInformation");
                foreach (KeyValuePair<int, CustomChestType> kvp in customChestTypesDict)
                {
                    editor.Data[kvp.Key] = $"{kvp.Value.name}/0/-300/Crafting -9/{kvp.Value.description}/true/true/0/Chest";
                }
            }
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            CustomChestTypeData data;
            CustomChestTypeConfig conf = new CustomChestTypeConfig();
            int id = 424000;
            Dictionary<int, string> existingPSIs = new Dictionary<int, string>();
            foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned())
            {
                conf = contentPack.ReadJsonFile<CustomChestTypeConfig>("config.json") ?? new CustomChestTypeConfig();
                foreach (KeyValuePair<string, int> psi in conf.parentSheetIndexes)
                {
                    existingPSIs.Add(psi.Value, psi.Key);
                }

            }
            foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned())
            {
                try
                {
                    int add = 0;
                    Monitor.Log($"Reading content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} from {contentPack.DirectoryPath}");
                    data = contentPack.ReadJsonFile<CustomChestTypeData>("content.json");
                    conf = contentPack.ReadJsonFile<CustomChestTypeConfig>("config.json") ?? new CustomChestTypeConfig();
                    foreach (CustomChestType chestInfo in data.chestTypes)
                    {
                        try
                        {
                            if (conf.parentSheetIndexes.ContainsKey(chestInfo.name))
                            {
                                chestInfo.id = conf.parentSheetIndexes[chestInfo.name];
                                Monitor.Log($"Got existing parentSheetIndex {chestInfo.id} for chest type {chestInfo.name} from content pack {contentPack.Manifest.Name}", LogLevel.Debug);
                            }
                            else
                            {
                                while (existingPSIs.ContainsKey(id))
                                    id++;
                                chestInfo.id = id++;
                                Monitor.Log($"Set new parentSheetIndex {chestInfo.id} for chest type {chestInfo.name} from content pack {contentPack.Manifest.Name}", LogLevel.Debug);
                            }
                            for (int frame = 1; frame <= chestInfo.frames; frame++)
                            {
                                var texturePath = chestInfo.texturePath.Replace("{frame}", frame.ToString());
                                chestInfo.texture.Add(contentPack.LoadAsset<Texture2D>(texturePath));
                            }
                            customChestTypesDict.Add(chestInfo.id, chestInfo);
                            conf.parentSheetIndexes[chestInfo.name] = chestInfo.id;
                            add++;
                        }
                        catch (Exception ex)
                        {
                            Monitor.Log($"Error parsing chest {chestInfo.name}: {ex}", LogLevel.Error);
                        }
                    }
                    Monitor.Log($"Got {add} chest types from content pack {contentPack.Manifest.Name}", LogLevel.Debug);
                    contentPack.WriteJsonFile("config.json", conf);
                    Monitor.Log($"Wrote ids to config for content pack {contentPack.Manifest.Name}", LogLevel.Debug);
                }
                catch (Exception ex)
                {
                    SMonitor.Log($"Error processing content.json in content pack {contentPack.Manifest.Name} {ex}", LogLevel.Error);
                }
            }
            Monitor.Log($"Got {customChestTypesDict.Count} chest types total", LogLevel.Debug);
        }

        private static bool GameLocation_isCollidingPosition_Prefix(GameLocation __instance, Rectangle position, ref bool __result)
        {
            foreach (KeyValuePair<Vector2, Object> obj in __instance.objects.Pairs)
            {
                if (customChestTypesDict.ContainsKey(obj.Value.ParentSheetIndex) &&  obj.Value.boundingBox.Value.Intersects(position))
                {
                    __result = true;
                    return false;
                }
            }
            
            return true;
        }    

        private static bool Debris_collect_Prefix(Debris __instance, ref bool __result, NetObjectShrinkList<Chunk> ___chunks, Farmer farmer, Chunk chunk)
        {
            if (chunk == null)
            {
                if (___chunks.Count <= 0)
                {
                    return false;
                }
                chunk = ___chunks[0];
            }

            if (!customChestTypesDict.ContainsKey(chunk.debrisType) && !customChestTypesDict.ContainsKey(-chunk.debrisType))
                return true;

            SMonitor.Log($"collecting {chunk.debrisType}");

            if (farmer.addItemToInventoryBool(new Object(Vector2.Zero, -chunk.debrisType, false), false))
            {
                __result = true;
                return false;
            }
            return true;
        }        

        private static bool Object_loadDisplayName_Prefix(Object __instance, ref string __result)
        {
            if (!customChestTypesDict.ContainsKey(__instance.ParentSheetIndex))
                return true;
            __result = customChestTypesDict[__instance.ParentSheetIndex].name;
            return false;
        }
        private static bool Object_placementAction_Prefix(Object __instance, ref bool __result, GameLocation location, int x, int y, Farmer who)
        {
            if (!customChestTypesDict.TryGetValue(__instance.ParentSheetIndex, out var chestInfo))
                return true;
            SMonitor.Log($"placing chest {__instance.name}");

            Vector2 placementTile = new Vector2((float)(x / 64), (float)(y / 64));

            Chest chest = new Chest(__instance.ParentSheetIndex, placementTile, 1, chestInfo.frames)
            {
                shakeTimer = 50,
            };
            chest.modData["aedenthorn.CustomChestTypes/IsCustomChest"] = "true";
            chest.resetLidFrame();
            Texture2D texture = chestInfo.texture.First();
            Rectangle bb = chestInfo.boundingBox;
            chest.boundingBox.Value = new Rectangle((int)chest.tileLocation.X * 64 + bb.X * 4, (int)chest.tileLocation.Y * 64 + 64 - texture.Height * 4 + bb.Y * 4, bb.Width * 4, bb.Height * 4);
            SMonitor.Log($"bounding box: {chest.boundingBox}");
            location.objects.Add(placementTile, chest);
            location.playSound("hammer", NetAudio.SoundContext.Default);
            __result = true;
            return false;
        }        
        private static bool Chest_draw_Prefix(Chest __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
        {
            if (!customChestTypesDict.TryGetValue(__instance.ParentSheetIndex, out var chestInfo))
                return true;

            float base_sort_order = Math.Max(0f, ((y + 1f) * 64f - 24f) / 10000f) + y * 1E-05f;
            int currentFrame = alpha < 1 ? 1 : (int) MathHelper.Clamp(SHelper.Reflection.GetField<int>(__instance, "currentLidFrame").GetValue(), 1, chestInfo.frames);
            Texture2D texture = chestInfo.texture.ElementAt(currentFrame-1);
            spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64f + (float)((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (y - texture.Height / 16 + 1) * 64f)), new Rectangle(0,0, texture.Width, texture.Height), __instance.Tint * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order);

            return false;
        }        
        
        private static bool Object_drawInMenu_Prefix(Object __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            if (!customChestTypesDict.TryGetValue(__instance.ParentSheetIndex, out var chestInfo))
                return true;

            bool shouldDrawStackNumber = ((drawStackNumber == StackDrawType.Draw && __instance.maximumStackSize() > 1 && __instance.Stack > 1) || drawStackNumber == StackDrawType.Draw_OneInclusive) && scaleSize > 0.3 && __instance.Stack != int.MaxValue;
            Texture2D texture = chestInfo.texture.First();
            float extraSize = Math.Max(texture.Height, texture.Width) / 32f;
            Rectangle sourceRect = new Rectangle(0, 0, texture.Width, texture.Height);
            spriteBatch.Draw(texture, location + new Vector2(32f / extraSize, 32f / extraSize), sourceRect, color * transparency, 0f, new Vector2(8f, 16f), 4f * (((double)scaleSize < 0.2) ? scaleSize : (scaleSize / 2f)) / extraSize, SpriteEffects.None, layerDepth);
            if (shouldDrawStackNumber)
            {
                Utility.drawTinyDigits(__instance.stack, spriteBatch, location + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString(__instance.stack, 3f * scaleSize)) + 3f * scaleSize, 64f - 18f * scaleSize + 2f), 3f * scaleSize, 1f, color);
            }
            return false;
        }

        private static bool Object_draw_Prefix(Object __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
        {
            if (!customChestTypesDict.TryGetValue(__instance.ParentSheetIndex, out var chestInfo))
                return true;
            
            float base_sort_order = Math.Max(0f, ((y + 1f) * 64f - 24f) / 10000f) + y * 1E-05f;
            int currentFrame = alpha < 1 ? 1 : (int) MathHelper.Clamp(SHelper.Reflection.GetField<int>(__instance, "currentLidFrame").GetValue(), 1, chestInfo.frames);
            Texture2D texture = chestInfo.texture.ElementAt(currentFrame-1);
            spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64f + (float)((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (y - texture.Height / 16 + 1) * 64f)), new Rectangle(0,0, texture.Width, texture.Height), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order);

            return false;
        }
        
        private static bool Object_drawWhenHeld_Prefix(Object __instance, SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
        {
            if (!customChestTypesDict.TryGetValue(__instance.ParentSheetIndex, out var chestInfo))
                return true;
            Texture2D texture = chestInfo.texture.First();
            objectPosition.X -= texture.Width * 2f - 32;
            objectPosition.Y -= (texture.Height - chestInfo.boundingBox.Height) * 4f - 64;
            var tint = __instance is Chest chest ? chest.Tint : Color.White;
            spriteBatch.Draw(texture, objectPosition, new Rectangle(0,0, texture.Width, texture.Height), tint, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
            return false;
        }
        
        private static bool Chest_checkForAction_Prefix(Chest __instance, ref bool __result, Farmer who, bool justCheckingForActivity)
        {
            if (justCheckingForActivity || !customChestTypesDict.TryGetValue(__instance.ParentSheetIndex, out CustomChestType chestInfo))
                return true;

            SMonitor.Log($"clicked on chest {__instance.name}");
            __instance.GetMutex().RequestLock(delegate
            {
                __instance.frameCounter.Value = chestInfo.frames;
                Game1.playSound(chestInfo.openSound);
                Game1.player.Halt();
                Game1.player.freezePause = 1000;

            });
            __result = true; 
            return false;
        }        
        private static bool Chest_GetActualCapacity_Prefix(Chest __instance, ref int __result)
        {
            if (!customChestTypesDict.TryGetValue(__instance.ParentSheetIndex, out CustomChestType chestInfo))
                return true;

            __result = chestInfo.capacity; 
            return false;
        }        
        
        private static void Chest_Postfix(Chest __instance, int parent_sheet_index)
        {
            if (!customChestTypesDict.ContainsKey(parent_sheet_index))
                return;
            __instance.Name = $"{customChestTypesDict[parent_sheet_index].name}";
            SMonitor.Log($"Created chest {__instance.Name}"); 
        }

        private static void Utility_getCarpenterStock_Postfix(ref Dictionary<ISalable, int[]> __result)
        {
            foreach(KeyValuePair<int, CustomChestType> kvp in customChestTypesDict)
            {
                Chest chest = new Chest(kvp.Value.id, Vector2.Zero, 217, 2);
                chest.Price = kvp.Value.price;
                chest.name = kvp.Value.name;
                __result.Add(chest, new int[] { kvp.Value.price, int.MaxValue});
            }
        }
    }
}
