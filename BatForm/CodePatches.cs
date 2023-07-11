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
                if (!Config.ModEnabled || Game1.currentLocation == null || BatFormStatus(Game1.player) == BatForm.Inactive)
                    return;

                float nextZoomLevel = __result - height.Value / 100f;
                int nextViewportWidth = (int) Math.Ceiling((double) Game1.game1.localMultiplayerWindow.Width * (1.0 / (double) nextZoomLevel));
                int nextViewportHeight = (int) Math.Ceiling((double) Game1.game1.localMultiplayerWindow.Height * (1.0 / (double) nextZoomLevel));
                bool viewportWidthHasOrWillOverflowMapWidth = Game1.viewport.Size.Width > Game1.currentLocation.Map.DisplayWidth || nextViewportWidth > Game1.currentLocation.Map.DisplayWidth;
                bool viewportHeightHasOrWillOverflowMapHeight = Game1.viewport.Size.Height > Game1.currentLocation.Map.DisplayHeight || nextViewportHeight > Game1.currentLocation.Map.DisplayHeight;

                if (viewportWidthHasOrWillOverflowMapWidth || viewportHeightHasOrWillOverflowMapHeight || (BatFormStatus(Game1.player) == BatForm.SwitchingFrom && height.Value >= heightViewportLimit.Value))
                {
                    float zoomLevelBasedOnMaxViewportWidth = (float) Game1.game1.localMultiplayerWindow.Width / Game1.currentLocation.Map.DisplayWidth;
                    float zoomLevelBasedOnMaxViewportHeight = (float) Game1.game1.localMultiplayerWindow.Height / Game1.currentLocation.Map.DisplayHeight;

                    if (viewportWidthHasOrWillOverflowMapWidth && viewportHeightHasOrWillOverflowMapHeight)
                    {
                        __result = Math.Min(__result, Math.Max(zoomLevelBasedOnMaxViewportWidth, zoomLevelBasedOnMaxViewportHeight));
                    }
                    else
                    {
                        if (viewportWidthHasOrWillOverflowMapWidth)
                            __result = Math.Min(__result, zoomLevelBasedOnMaxViewportWidth);
                        else
                            __result = Math.Min(__result, zoomLevelBasedOnMaxViewportHeight);
                    }
                    if (heightViewportLimit.Value == maxHeight)
                        heightViewportLimit.Value = height.Value;
                }
                else
                {
                    __result = nextZoomLevel;
                }
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
        [HarmonyPatch(typeof(Game1), nameof(Game1.pressActionButton))]
        public class Game1_pressActionButton_Patch
        {
            public static bool Prefix()
            {
                if (!Config.ModEnabled || Config.ActionsEnabled || BatFormStatus(Game1.player) == BatForm.Inactive)
                    return true;
                return false;
            }
        }
        [HarmonyPatch(typeof(Game1), nameof(Game1.pressUseToolButton))]
        public class Game1_pressUseToolButton_Patch
        {
            public static bool Prefix()
            {
                if (!Config.ModEnabled || Config.ActionsEnabled || BatFormStatus(Game1.player) == BatForm.Inactive)
                    return true;
                return false;
            }
        }
    }
}