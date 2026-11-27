using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.BigCraftables;
using StardewValley.Network;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace CustomChestTypes
{
    public class ModEntry : Mod
    {
        public static ModEntry context;
        private static ModConfig Config;
        private static IMonitor SMonitor;
        private static IModHelper SHelper;
        private static Dictionary<string, CustomChestType> customChestTypesDict = new Dictionary<string, CustomChestType>();
        public static string dictPath = "aedenthorn.CustomChestTypes/dict";

        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;

            SMonitor = Monitor;
            SHelper = Helper;


            helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(this.ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.isCollidingPosition), new Type[] { typeof(Rectangle), typeof(xTile.Dimensions.Rectangle), typeof(bool), typeof(int), typeof(bool), typeof(Character)}),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(GameLocation_isCollidingPosition_Prefix))
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

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/BigCraftables") && customChestTypesDict.Count > 0)
            {
                e.Edit((IAssetData data) =>
                {
                    var dict = data.AsDictionary<string, BigCraftableData>();
                    SMonitor.Log($"Patching BigCraftables");
                    foreach (KeyValuePair<string, CustomChestType> kvp in customChestTypesDict)
                    {
                        dict.Data[kvp.Key] = new BigCraftableData()
                        {
                            Name = kvp.Key,
                            DisplayName = kvp.Value.name,
                            Description = kvp.Value.description,
                            Price = kvp.Value.price,
                            Fragility = 0,
                            CanBePlacedIndoors = true,
                            CanBePlacedOutdoors = true,
                            IsLamp = false,
                            Texture = null, 
                            SpriteIndex = 0,
                            ContextTags = null,
                            CustomFields = null
                        }; 
                    }
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, CustomChestType>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
        }



        private static bool GameLocation_isCollidingPosition_Prefix(GameLocation __instance, Rectangle position, ref bool __result)
        {
            foreach (KeyValuePair<Vector2, Object> kvp in __instance.objects.Pairs)
            {
                if (customChestTypesDict.ContainsKey(kvp.Value.Name) && kvp.Value.boundingBox.Value.Intersects(position))
                {
                    __result = true;
                    return false;
                }
            }
            
            return true;
        }    

        private static bool Chest_draw_Prefix(Chest __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
        {
            if (!customChestTypesDict.TryGetValue(__instance.ParentSheetIndex, out var chestInfo))
                return true;

            float base_sort_order = Math.Max(0f, ((y + 1f) * 64f - 24f) / 10000f) + y * 1E-05f;
            int currentFrame = alpha < 1 ? 1 : (int) MathHelper.Clamp(SHelper.Reflection.GetField<int>(__instance, "currentLidFrame").GetValue(), 1, chestInfo.frames);
            Texture2D texture = SHelper.GameContent.Load<Texture2D>(chestInfo.texture.ElementAt(currentFrame - 1));
            spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64f + (float)((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (y - texture.Height / 16 + 1) * 64f)), new Rectangle(0,0, texture.Width, texture.Height), __instance.Tint * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order);

            return false;
        }        
        
        private static bool Object_drawInMenu_Prefix(Object __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            if (!customChestTypesDict.TryGetValue(__instance.ParentSheetIndex, out var chestInfo))
                return true;

            bool shouldDrawStackNumber = ((drawStackNumber == StackDrawType.Draw && __instance.maximumStackSize() > 1 && __instance.Stack > 1) || drawStackNumber == StackDrawType.Draw_OneInclusive) && scaleSize > 0.3 && __instance.Stack != int.MaxValue;
            Texture2D texture = SHelper.GameContent.Load<Texture2D>(chestInfo.texture.First());
            float extraSize = Math.Max(texture.Height, texture.Width) / 32f;
            Rectangle sourceRect = new Rectangle(0, 0, texture.Width, texture.Height);
            spriteBatch.Draw(texture, location + new Vector2(32f / extraSize, 32f / extraSize), sourceRect, color * transparency, 0f, new Vector2(8f, 16f), 4f * (((double)scaleSize < 0.2) ? scaleSize : (scaleSize / 2f)) / extraSize, SpriteEffects.None, layerDepth);
            if (shouldDrawStackNumber)
            {
                Utility.drawTinyDigits(__instance.Stack, spriteBatch, location + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString(__instance.Stack, 3f * scaleSize)) + 3f * scaleSize, 64f - 18f * scaleSize + 2f), 3f * scaleSize, 1f, color);
            }
            return false;
        }

        private static bool Object_draw_Prefix(Object __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
        {
            if (!customChestTypesDict.TryGetValue(__instance.ParentSheetIndex, out var chestInfo))
                return true;
            
            float base_sort_order = Math.Max(0f, ((y + 1f) * 64f - 24f) / 10000f) + y * 1E-05f;
            int currentFrame = alpha < 1 ? 1 : (int) MathHelper.Clamp(SHelper.Reflection.GetField<int>(__instance, "currentLidFrame").GetValue(), 1, chestInfo.frames);
            Texture2D texture = SHelper.GameContent.Load<Texture2D>(chestInfo.texture.ElementAt(currentFrame - 1));
            spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64f + (float)((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (y - texture.Height / 16 + 1) * 64f)), new Rectangle(0,0, texture.Width, texture.Height), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order);

            return false;
        }
        
        private static bool Object_drawWhenHeld_Prefix(Object __instance, SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
        {
            if (!customChestTypesDict.TryGetValue(__instance.ParentSheetIndex, out var chestInfo))
                return true;
            Texture2D texture = SHelper.GameContent.Load<Texture2D>(chestInfo.texture.First());
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
            foreach(var kvp in customChestTypesDict)
            {
                Chest chest = new Chest(kvp.Value.id, Vector2.Zero, 217, 2);
                chest.Price = kvp.Value.price;
                chest.name = kvp.Value.name;
                __result.Add(chest, new int[] { kvp.Value.price, int.MaxValue});
            }
        }
    }
}
