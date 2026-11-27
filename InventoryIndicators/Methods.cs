using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;
using Object = StardewValley.Object;

namespace InventoryIndicators
{
	public partial class ModEntry : Mod
	{
        public static IEnumerable<FieldInfo> fieldInfos;
        public static string[] universalLoves;
        public static Dictionary<string, IndicatorData> dataDict = new Dictionary<string, IndicatorData>();
        public static IEnumerable<FieldInfo> GetFieldInfos()
		{
            if(fieldInfos is null)
            {
                fieldInfos = typeof(Object).GetFields().Where(f => f.FieldType == typeof(int) && f.IsLiteral && (f.Name.EndsWith("Category") || f.Name == "metalResources" || f.Name == "buildingResources" || f.Name == "sellAtPierresAndMarnies"));
            }
            return fieldInfos;
        }
        public static string GetCategory(int category)
        {
            foreach (var f in GetFieldInfos())
            {
                if((int)f.GetValue(new Object()) == category)
                    return f.Name;
            }
            return null;
        }
        public static int GetCategory(string category)
        {
            foreach (var f in GetFieldInfos())
            {
                if(f.Name == category)
                    return (int)f.GetValue(new Object());
            }
            return 0;
        }
        public static void DrawBefore(Item __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth)
        {
            Color? color = null;
            if (dataDict.TryGetValue(__instance.QualifiedItemId, out var data))
            {
                color = data.color;
            }
            else
            {
                data = new IndicatorData();
                var cat = GetCategory(__instance.Category);
                if (cat != null && Config.Colors.TryGetValue(cat, out var c) && c.A > 0)
                {
                    color = c;
                    data.color = c;
                }
                if (__instance is Object && !__instance.HasTypeBigCraftable())
                {
                    string loveText = null;

                    if (universalLoves is null)
                        universalLoves = ArgUtility.SplitBySpace(Game1.NPCGiftTastes["Universal_Love"]);

                    if (favoriteThings is null)
                    {
                        favoriteThings = new Dictionary<string, HashSet<string>>();
                        foreach (var kvp in Game1.NPCGiftTastes)
                        {
                            try
                            {
                                var favs = ArgUtility.SplitBySpace(kvp.Value.Split('/', StringSplitOptions.None)[1]);
                                foreach (var fav in favs)
                                {
                                    if (!favoriteThings.TryGetValue(fav, out var l))
                                    {
                                        l = new HashSet<string>();
                                        favoriteThings[fav] = l;
                                    }
                                    l.Add(kvp.Key);
                                }
                            }
                            catch
                            {

                            }
                        }

                    }

                    data.universalLove = Array.Exists(universalLoves, s => s.Equals(__instance.ItemId));
                    if (data.universalLove)
                    {
                        loveText = SHelper.Translation.Get("universal_love");
                    }
                    else if (__instance is Object obj && !obj.bigCraftable.Value && favoriteThings.TryGetValue(__instance.ItemId, out var list))
                    {
                        List<string> names = new List<string>();
                        foreach (var npc in list)
                        {
                            if (Config.ShowUngiftedFavorites || (Game1.player.giftedItems.TryGetValue(npc, out var giftData) && giftData.TryGetValue(__instance.ItemId, out var value) && value > 0))
                            {
                                var portrait = Game1.getCharacterFromName(npc)?.Portrait;
                                if (portrait != null)
                                {
                                    if (data.lovePortraits == null)
                                        data.lovePortraits = new();
                                    data.lovePortraits.Add(portrait);
                                }
                                names.Add(Game1.getCharacterFromName(npc, false, false)?.displayName ?? npc);
                            }
                        }
                        if (names.Count == 1)
                        {
                            loveText = string.Format(SHelper.Translation.Get("x_love"), names[0]);
                        }
                        else if (names.Count == 2)
                        {
                            loveText = string.Format(SHelper.Translation.Get("x_love_duo"), names[0], names[1]);
                        }
                        else if (names.Count > 2)
                        {
                            string mult = string.Join(SHelper.Translation.Get("x_love_mult_separator"), names.GetRange(0, names.Count - 1));
                            loveText = string.Format(SHelper.Translation.Get("x_love_mult"), mult, names[names.Count - 1]);
                        }
                    }
                    if (__instance is Object && Game1.RequireLocation<CommunityCenter>("CommunityCenter", false).couldThisIngredienteBeUsedInABundle(__instance as Object))
                    {
                        data.bundle = true;
                    }
                    if (__instance is Object && __instance.Category == Object.SeedsCategory)
                    {
                        if (__instance.Name.Contains("Mixed") || Crop.TryGetData(Crop.ResolveSeedId(__instance.ItemId, Game1.currentLocation), out var cropData) && cropData.Seasons.Contains(Game1.currentLocation.GetSeason()))
                        {
                            data.seed = true;
                        }
                    }
                    string text = null;
                    if (loveText != null)
                        text += loveText + " ";
                    if (data.seed)
                        text += SHelper.Translation.Get("can_plant") + " ";
                    if (data.bundle)
                        text += SHelper.Translation.Get("can_bundle");
                    data.hoverText = text?.Trim();
                }
                dataDict[__instance.QualifiedItemId] = data;
            }
            if (color != null && (Game1.activeClickableMenu is not null || !Config.ShowOnlyInMenu))
            {
                if (Config.OutlineWidth < 1)
                {
                    spriteBatch.Draw(Game1.staminaRect, new Rectangle((int)location.X + 4, (int)location.Y + 4, 56, 56), null, color.Value, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
                }
                else
                {
                    spriteBatch.Draw(Game1.staminaRect, new Rectangle((int)location.X + 4, (int)location.Y + 4, 56, Config.OutlineWidth), null, color.Value, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
                    spriteBatch.Draw(Game1.staminaRect, new Rectangle((int)location.X + 4, (int)location.Y + 4 + Config.OutlineWidth, Config.OutlineWidth, 56 - Config.OutlineWidth), null, color.Value, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
                    spriteBatch.Draw(Game1.staminaRect, new Rectangle((int)location.X + 60 - Config.OutlineWidth, (int)location.Y + 4 + Config.OutlineWidth, Config.OutlineWidth, 56 - Config.OutlineWidth), null, color.Value, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
                    spriteBatch.Draw(Game1.staminaRect, new Rectangle((int)location.X + 4 + Config.OutlineWidth, (int)location.Y + 60 - Config.OutlineWidth, 56 - Config.OutlineWidth * 2, Config.OutlineWidth), null, color.Value, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
                }
            }
        }
        public static void DrawAfter(Item __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth)
        {
            if ((Game1.activeClickableMenu is null && Config.ShowOnlyInMenu) || !dataDict.TryGetValue(__instance.QualifiedItemId, out var data))
                return;
            bool hover = new Rectangle(location.ToPoint(), new Point(64, 64)).Contains(Game1.getMousePosition());

            int offset = -2;
            if (Config.ShowFavorites)
            {
                if (Config.ShowUniversalFavorites && data.universalLove)
                {
                    spriteBatch.Draw(Game1.mouseCursors, location + new Vector2(offset, offset), new Rectangle(172, 514, 9, 10), Color.White, 0, Vector2.Zero, 2f, SpriteEffects.None, layerDepth);
                }
                else if (data.lovePortraits != null)
                {
                    for (int i = 0; i < data.lovePortraits.Count; i++)
                    {
                        var portrait = data.lovePortraits[i];

                        if (portrait != null)
                        {
                            spriteBatch.Draw(portrait, location + new Vector2(i * 8 + offset, offset), new Rectangle(0, 0, 64, 64), Color.White, 0, Vector2.Zero, 0.5f, SpriteEffects.None, layerDepth);
                        }
                    }
                }
            }
            if (Config.ShowBundleItems && data.bundle)
            {
                spriteBatch.Draw(SHelper.GameContent.Load<Texture2D>("Characters/Junimo"), location + new Vector2(32 - offset, offset), new Rectangle(0, 1, 16, 15), Config.JunimoColor, 0, Vector2.Zero, 2f, SpriteEffects.None, layerDepth);
            }
            if (Config.ShowPlantableSeeds && data.seed)
            {
                spriteBatch.Draw(Game1.mouseCursors, location + new Vector2(offset, 32 - offset), new Rectangle(18, 625, 13, 15), new Color(1f, 1f, 1f, Config.PlantableOpacity), 0, Vector2.Zero, 2f, SpriteEffects.None, layerDepth);
            }
        }
    }
}
