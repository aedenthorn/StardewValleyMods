using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace PersisitentGrangeDisplay
{
    public class ModEntry : Mod 
    {
        public static ModEntry context;

        public static ModConfig Config;
        
        public static bool isGrangeMenu = false;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;

            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.GameLoop.Saving += GameLoop_Saving;

            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.draw), new Type[]{typeof(SpriteBatch),typeof(int),typeof(int),typeof(float),typeof(float)}),
               postfix: new HarmonyMethod(typeof(GrangePatches), nameof(GrangePatches.Object_draw_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
               postfix: new HarmonyMethod(typeof(GrangePatches), nameof(GrangePatches.Object_draw_Postfix2))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.isPassable)),
               prefix: new HarmonyMethod(typeof(GrangePatches), nameof(GrangePatches.Object_isPassable_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.checkForAction)),
               postfix: new HarmonyMethod(typeof(GrangePatches), nameof(GrangePatches.Object_checkForAction_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(StorageContainer), nameof(StorageContainer.draw), new Type[] { typeof(SpriteBatch) } ),
               postfix: new HarmonyMethod(typeof(GrangePatches), nameof(GrangePatches.StorageContainer_draw_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Game1), nameof(Game1.exitActiveMenu)),
               postfix: new HarmonyMethod(typeof(GrangePatches), nameof(GrangePatches.Game1_exitActiveMenu_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(FarmerTeam), nameof(FarmerTeam.NewDay)),
               prefix: new HarmonyMethod(typeof(GrangePatches), nameof(GrangePatches.FarmerTeam_NewDay_Prefix)),
               postfix: new HarmonyMethod(typeof(GrangePatches), nameof(GrangePatches.FarmerTeam_NewDay_Postfix))
            );

        }

        private void GameLoop_Saving(object sender, StardewModdingAPI.Events.SavingEventArgs e)
        {
            Monitor.Log($"saving Grange display items: {Game1.player.team.grangeDisplay.Count}");

            List<PersistentItem> grangeItems = new List<PersistentItem>();

            foreach(Item item in Game1.player.team.grangeDisplay)
            {
                if (item is Object)
                    grangeItems.Add(new PersistentItem(item));
                else
                    grangeItems.Add(null);
            }

            PersistentGrangeDisplayData data = new PersistentGrangeDisplayData();
            data.PersistentGrangeDisplay = grangeItems;

            Helper.Data.WriteSaveData("PersistentGrangeDisplay", data);
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            var GrangeDisplay = Helper.Data.ReadSaveData<PersistentGrangeDisplayData>("PersistentGrangeDisplay") ?? new PersistentGrangeDisplayData();
            Monitor.Log($"got saved Grange display items: {GrangeDisplay.PersistentGrangeDisplay.Count}");

            while (Game1.player.team.grangeDisplay.Count < 9)
            {
                Game1.player.team.grangeDisplay.Add(null);
            }
            int i = 0;
            foreach(PersistentItem item in GrangeDisplay.PersistentGrangeDisplay)
            {
                if(item == null)
                    Game1.player.team.grangeDisplay[i++] = null;
                else
                    Game1.player.team.grangeDisplay[i++] = new Object(item.id, 1, false, -1, item.quality);
            }
        }

        public static int GetGrangeScore()
        {
            int pointsEarned = 14;
            Dictionary<int, bool> categoriesRepresented = new Dictionary<int, bool>();
            int nullsCount = 0;
            foreach (Item i in Game1.player.team.grangeDisplay)
            {
                if (i != null && i is Object)
                {
                    if (Event.IsItemMayorShorts(i as Object))
                    {
                        return -666;
                    }
                    pointsEarned += (i as Object).Quality + 1;
                    int num = (i as Object).sellToStorePrice(-1L);
                    if (num >= 20)
                    {
                        pointsEarned++;
                    }
                    if (num >= 90)
                    {
                        pointsEarned++;
                    }
                    if (num >= 200)
                    {
                        pointsEarned++;
                    }
                    if (num >= 300 && (i as Object).Quality < 2)
                    {
                        pointsEarned++;
                    }
                    if (num >= 400 && (i as Object).Quality < 1)
                    {
                        pointsEarned++;
                    }
                    int category = (i as Object).Category;
                    if (category <= -27)
                    {
                        switch (category)
                        {
                            case -81:
                            case -80:
                                break;
                            case -79:
                                categoriesRepresented[-79] = true;
                                continue;
                            case -78:
                            case -77:
                            case -76:
                                continue;
                            case -75:
                                categoriesRepresented[-75] = true;
                                continue;
                            default:
                                if (category != -27)
                                {
                                    continue;
                                }
                                break;
                        }
                        categoriesRepresented[-81] = true;
                    }
                    else if (category != -26)
                    {
                        if (category != -18)
                        {
                            switch (category)
                            {
                                case -14:
                                case -6:
                                case -5:
                                    break;
                                case -13:
                                case -11:
                                case -10:
                                case -9:
                                case -8:
                                case -3:
                                    continue;
                                case -12:
                                case -2:
                                    categoriesRepresented[-12] = true;
                                    continue;
                                case -7:
                                    categoriesRepresented[-7] = true;
                                    continue;
                                case -4:
                                    categoriesRepresented[-4] = true;
                                    continue;
                                default:
                                    continue;
                            }
                        }
                        categoriesRepresented[-5] = true;
                    }
                    else
                    {
                        categoriesRepresented[-26] = true;
                    }
                }
                else if (i == null)
                {
                    nullsCount++;
                }
            }
            pointsEarned += Math.Min(30, categoriesRepresented.Count * 5);
            int displayFilledPoints = 9 - 2 * nullsCount;
            pointsEarned += displayFilledPoints;
            return pointsEarned;
        }

        public static Color GetPointsColor(int score)
        {
            if (score >= 90)
                return new Color(120, 255, 120);
            if (score >= 75)
                return Color.Yellow;
            if (score >= 60)
                return new Color(255,200,0);
            if (score < 0)
                return Color.MediumPurple;
            return Color.Red;
        }
    }
}
