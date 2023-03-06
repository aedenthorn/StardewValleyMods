using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Linq;
using xTile.Dimensions;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace DungeonMerchants
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(MineShaft), nameof(MineShaft.GetMine))]
        public class MineShaft_GetMine_Patch
        {
            public static void Postfix(ref MineShaft __result)
            {
                if (!Config.ModEnabled || __result.mineLevel == 77377)
                    return;
                if (!__result.modData.ContainsKey(dwarfKey) 
                    && __result.mineLevel >= Config.DwarfFloorMin 
                    && (Config.DwarfFloorMax < 0 || __result.mineLevel <= Config.DwarfFloorMax) 
                    && (!Config.DwarfFloors.Any() || Config.DwarfFloors.Contains(__result.mineLevel)) 
                    && (Config.DwarfFloorMult < 0 || __result.mineLevel % Config.DwarfFloorMult == 0)
                    && (Config.DwarfFloors.Contains(__result.mineLevel) || (Config.DwarfFloorMult >= 0 && __result.mineLevel % Config.DwarfFloorMult == 0) || Game1.random.NextDouble() < Config.DwarfFloorChancePercent / 100f)
                )
                {
                    SpawnDwarf(__result);
                }
                if(!__result.modData.ContainsKey(merchantKey)
                    && __result.mineLevel >= Config.MerchantFloorMin 
                    && (Config.MerchantFloorMax < 0 || __result.mineLevel <= Config.MerchantFloorMax) 
                    && (!Config.MerchantFloors.Any() || Config.MerchantFloors.Contains(__result.mineLevel)) 
                    && (Config.MerchantFloorMult < 0 || __result.mineLevel % Config.MerchantFloorMult == 0)
                    && (Config.MerchantFloors.Contains(__result.mineLevel) || (Config.MerchantFloorMult >= 0 && __result.mineLevel % Config.MerchantFloorMult == 0) || Game1.random.NextDouble() < Config.MerchantFloorChancePercent / 100f)
                )
                {
                    SpawnMerchant(__result);
                }
            }
        }
        [HarmonyPatch(typeof(NPC), nameof(NPC.checkAction))]
        public class NPC_checkAction_Patch
        {
            public static bool Prefix(NPC __instance, Farmer who, GameLocation l, ref bool __result)
            {
                if (!Config.ModEnabled || !__instance.Name.Equals("Dwarf") || !who.canUnderstandDwarves || l is not MineShaft)
                    return true;
                __result = true;
                Game1.activeClickableMenu = new ShopMenu(Utility.getDwarfShopStock(), 0, "Dwarf", null, null, null);
                return false;
            }
        }
        [HarmonyPatch(typeof(MineShaft), nameof(MineShaft.checkAction))]
        public class MineShaft_checkAction_Patch
        {
            public static bool Prefix(MineShaft __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled || __instance.getTemporarySpriteByID(merchantSpriteID) is null)
                    return true;
                var pos = __instance.getTemporarySpriteByID(merchantSpriteID).position;

                Rectangle tileRect = new Rectangle(tileLocation.X * 64, tileLocation.Y * 64, 64, 64);

                if (tileRect.Intersects(new Rectangle((int)pos.X, (int)pos.Y + 64, 20, 26)))
                {
                    Game1.activeClickableMenu = new ShopMenu(Desert.getDesertMerchantTradeStock(Game1.player), 0, "DesertTrade", new Func<ISalable, Farmer, int, bool>(boughtTraderItem), null, null);
                    __result = true;
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(MineShaft), nameof(MineShaft.performTenMinuteUpdate))]
        public class MineShaft_performTenMinuteUpdate_Patch
        {
            public static void Postfix(MineShaft __instance)
            {
                if (!Config.ModEnabled || __instance.getTemporarySpriteByID(merchantSpriteID) is null)
                    return;
                var pos = __instance.getTemporarySpriteByID(merchantSpriteID).position;
                if (Game1.random.NextDouble() < 0.33)
                {
                    __instance.temporarySprites.Add(new TemporaryAnimatedSprite
                    {
                        texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"),
                        sourceRect = new Microsoft.Xna.Framework.Rectangle(40, 614, 20, 26),
                        sourceRectStartingPos = new Vector2(40f, 614f),
                        animationLength = 6,
                        totalNumberOfLoops = 1,
                        interval = 100f,
                        scale = 3f,
                        position = pos,
                        layerDepth = (pos.Y + 1) / 10000f,
                        id = merchantSpriteID + 1,
                        pingPong = true
                    });
                }
                else
                {
                    __instance.temporarySprites.Add(new TemporaryAnimatedSprite
                    {
                        texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"),
                        sourceRect = new Microsoft.Xna.Framework.Rectangle(20, 614, 20, 26),
                        sourceRectStartingPos = new Vector2(20f, 614f),
                        animationLength = 1,
                        totalNumberOfLoops = 1,
                        interval = (float)Game1.random.Next(100, 800),
                        scale = 3f,
                        position = pos,
                        layerDepth = (pos.Y + 1) / 10000f,
                        id = merchantSpriteID + 2
                    });
                }
            }
        }
    }
}