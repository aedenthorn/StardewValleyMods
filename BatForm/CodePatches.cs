using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Object = StardewValley.Object;

namespace BatForm
{
    public partial class ModEntry
    {
        public static int buffId = 4277377;

        [HarmonyPatch(typeof(Farmer), nameof(Farmer.draw))]
        public class Farmer_draw_Patch
        {
            public static bool Prefix(Farmer __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled || __instance != Game1.player || BatFormStatus(Game1.player) == BatForm.Inactive)
                    return true;
                Buff? buff = Game1.buffsDisplay.otherBuffs.FirstOrDefault(p => p.which == buffId);
                if (buff == null)
                {
                    Game1.buffsDisplay.addOtherBuff(
                        buff = new Buff(0, 0, 0, 0, 0, 0, 0, 0, 0, speed: Config.MoveSpeed, 0, 0, minutesDuration: 1, source: "Bat Form Mod", displaySource: "Bat Form") { which = buffId }
                    );
                }
                buff.millisecondsDuration = 50;
                return false;
            }
        }
        [HarmonyPatch(typeof(Options), nameof(Options.zoomLevel))]
        [HarmonyPatch(MethodType.Getter)]
        public class Options_zoomLevel_Patch
        {
            public static void Postfix(Options __instance, ref float __result)
            {
                if (!Config.ModEnabled || BatFormStatus(Game1.player) == BatForm.Inactive)
                    return;
                __result -= height.Value / 100f;
            }
        }
        [HarmonyPatch(typeof(FarmerSprite), "checkForFootstep")]
        public class FarmerSprite_checkForFootstep_Patch
        {
            public static bool Prefix(FarmerSprite __instance, Farmer ___owner)
            {
                if (!Config.ModEnabled || BatFormStatus(Game1.player) == BatForm.Inactive || ___owner != Game1.player)
                    return true;
                return false;
            }
        }
        [HarmonyPatch(typeof(Grass), nameof(Grass.doCollisionAction))]
        public class Grass_doCollisionAction_Patch
        {
            public static bool Prefix(Character who)
            {
                if (!Config.ModEnabled || BatFormStatus(Game1.player) == BatForm.Inactive || who != Game1.player)
                    return true;
                return false;
            }
        }
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.takeDamage))]
        public class Farmer_takeDamage_Patch
        {
            public static bool Prefix(Farmer __instance)
            {
                if (!Config.ModEnabled || BatFormStatus(Game1.player) == BatForm.Inactive || __instance != Game1.player)
                    return true;
                return false;
            }
        }
    }
}