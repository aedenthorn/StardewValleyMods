using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Tools;
using xTile.Dimensions;

namespace FishSpotBait
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.checkAction))]
        public class GameLocation_checkAction_Patch
        {
            public static bool Prefix(GameLocation __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled)
                    return true;
                if (who.ActiveObject is null)
                    return true;
                if(!__instance.isWaterTile(tileLocation.X, tileLocation.Y)) 
                    return true;
                if(__instance.Objects.ContainsKey(new Vector2(tileLocation.X, tileLocation.Y))) 
                    return true;
                if(int.TryParse(Config.BaitItem, out int index))
                {
                    if (who.ActiveObject.ParentSheetIndex != index)
                        return true;
                }
                else if(who.ActiveObject.Name != Config.BaitItem)
                    return true;
                Point p = new Point(tileLocation.X, tileLocation.Y);
                Point op = p;
                Point diff = new Point(0, -1);
                switch (who.FacingDirection)
                {
                    case 1:
                        diff = new Point(1, 0);
                        break;
                    case 2:
                        diff = new Point(0, 1);
                        break;
                    case 3:
                        diff = new Point(-1, 0);
                        break;
                }
                int tries = 0;
                while(!__instance.isOpenWater(p.X, p.Y) || __instance.doesTileHaveProperty(p.X, p.Y, "NoFishing", "Back") is not null)
                {
                    p = op + new Point(diff.X * Game1.random.Next(5), diff.Y * Game1.random.Next(5));
                    tries++;
                    if (tries >= 5)
                        return true;
                }
                    
                who.reduceActiveItemByOne();
                __instance.playSound("dropItemInWater");
                __instance.fishSplashPoint.Value = p;
                SMonitor.Log($"Set fish splash point to {p}");
                __result = true;
                return false;
            }
        }
    }
}