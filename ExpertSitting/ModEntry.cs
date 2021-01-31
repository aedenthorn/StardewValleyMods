using Harmony;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
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
            var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);

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
    }
}
 