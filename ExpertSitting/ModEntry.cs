using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Globalization;
using System.Linq;
using xTile.Dimensions;
using Object = StardewValley.Object;

namespace ExpertSitting
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {

        public static ModEntry context;

        internal static ModConfig Config;
        private static IMonitor SMonitor;


        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = this.Helper.ReadConfig<ModConfig>();
            SMonitor = this.Monitor;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            
            Harmony harmony = new(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.checkAction)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.checkAction_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(MapSeat), nameof(MapSeat.RemoveSittingFarmer)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.RemoveSittingFarmer_Postfix))
            );

        }

        private static void RemoveSittingFarmer_Postfix(MapSeat __instance, Farmer farmer)
        {
            if(__instance.seatType.Value.EndsWith("expert sitting mod"))
            {
                farmer.currentLocation.mapSeats.Remove(__instance);
            }
        }

        private static void checkAction_Postfix(GameLocation __instance, ref bool __result, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
        {
            if (!Config.EnableMod || __result)
                return;

            //SMonitor.Log($"Checking for seat");

            foreach(Object obj in __instance.objects.Values)
            {
                if(obj.TileLocation == new Vector2(tileLocation.X, tileLocation.Y) && Config.SeatTypes.Contains(obj.name))
                {
                    SMonitor.Log($"Got object seat");
                    MapSeat ms = new MapSeat();
                    ms.tilePosition.Value = new Vector2(tileLocation.X, tileLocation.Y);
                    ms.seatType.Value = "stool expert sitting mod";
                    ms.size.Value = new Vector2(1,1);
                    __instance.mapSeats.Add(ms);
                    who.BeginSitting(ms);
                    __result = true;
                    break;
                }
            }
            if (__result)
                return;

            foreach(ResourceClump rc in __instance.resourceClumps)
            {
                if (rc.occupiesTile(tileLocation.X, tileLocation.Y - 1))
                {
                    SMonitor.Log($"Got clump seat");
                    MapSeat ms = new MapSeat();
                    ms.tilePosition.Value = new Vector2(tileLocation.X, tileLocation.Y);
                    ms.seatType.Value = "clump expert sitting mod";
                    switch (who.FacingDirection)
                    {
                        case 0:
                        case 2:
                            ms.direction.Value = 2;
                            break;
                        default:
                            if (rc.occupiesTile(tileLocation.X - 1, tileLocation.Y - 1))
                                ms.direction.Value = 1;
                            else
                                ms.direction.Value = 3;
                            break;
                    }
                    ms.size.Value = new Vector2(1, 1);
                    __instance.mapSeats.Add(ms);
                    who.BeginSitting(ms);
                    __result = true;
                    break;
                }
            }

            if (__result)
                return;

            if (Config.AllowMapSit && (Config.MapSitModKey == SButton.None || context.Helper.Input.IsDown(Config.MapSitModKey)) && __instance.map.GetLayer("Buildings")?.PickTile(tileLocation * Game1.tileSize, Game1.viewport.Size) != null && __instance.map.GetLayer("Building")?.PickTile(new Location(tileLocation.X, tileLocation.Y + 1) * Game1.tileSize, Game1.viewport.Size) == null)
            {
                SMonitor.Log($"Got map seat");
                MapSeat ms = new MapSeat();
                ms.tilePosition.Value = new Vector2(tileLocation.X, tileLocation.Y);
                ms.size.Value = new Vector2(1, 1);
                ms.seatType.Value = "map expert sitting mod";
                __instance.mapSeats.Add(ms);
                ms.direction.Value = 2;
                who.BeginSitting(ms);
                __result = true;
            }
        }
        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            //SMonitor.Log(string.Join(",", Game1.objectData.Where(kvp => kvp.Value.Type == "Arch").Select(kvp => kvp.Key)));
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is not null)
            {
                configMenu.Register(
                    mod: ModManifest,
                    reset: () => Config = new ModConfig(),
                    save: () => Helper.WriteConfig(Config)
                );

                var props = typeof(ModConfig).GetProperties().ToArray();
                var configMenuExt = Helper.ModRegistry.GetApi<IGMCMOptionsAPI>("jltaylor-us.GMCMOptions");

                foreach (var p in props)
                {
                    if (p.Name == "Debug")
                        continue;
                    if (p.PropertyType == typeof(bool))
                    {
                        configMenu.AddBoolOption(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (bool)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(int))
                    {
                        configMenu.AddNumberOption(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (int)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(float))
                    {
                        configMenu.AddNumberOption(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (float)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(double))
                    {
                        configMenu.AddTextOption(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => p.GetValue(Config).ToString(),
                            setValue: value => { if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) { p.SetValue(Config, d); } }
                        );
                    }
                    else if (p.PropertyType == typeof(string))
                    {
                        configMenu.AddTextOption(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (string)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(KeybindList))
                    {
                        configMenu.AddKeybindList(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (KeybindList)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(SButton))
                    {
                        configMenu.AddKeybind(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (SButton)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(Color) && configMenuExt is not null)
                    {
                        configMenuExt.AddColorOption(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (Color)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                }
            }
        }

        public static string AddSpaces(string str)
        {
            string newStr = "";
            foreach (var c in str)
            {
                if (c >= 'A' && c <= 'Z' && newStr.Length > 0)
                {
                    newStr += " ";
                }
                newStr += c;
            }
            return newStr;
        }
    }
}
 