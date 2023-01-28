using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace TramplableCrops
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(HoeDirt), nameof(HoeDirt.doCollisionAction))]
        public class HoeDirt_doCollisionAction_Patch
        {
            public static bool Prefix(HoeDirt __instance, Rectangle positionOfCollider, int speedOfCollision, Vector2 tileLocation, Character who, GameLocation location)
            {
                if (!Config.EnableMod)
                    return true;
                var bb = __instance.getBoundingBox(tileLocation);
                if (__instance.crop != null && __instance.crop.currentPhase.Value != 0 && speedOfCollision > 0 && positionOfCollider.Intersects(new Rectangle(bb.X + Config.Border, bb.Y + Config.Border, bb.Width - Config.Border * 2, bb.Height - Config.Border * 2)))
                {
                    __instance.destroyCrop(tileLocation, true, location);
                    return false;
                }
                return true;
            }
        }
    }
}