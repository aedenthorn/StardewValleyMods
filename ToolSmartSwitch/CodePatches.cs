using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace ToolSmartSwitch
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(Game1), nameof(Game1.pressUseToolButton))]
        public class Farmer_pressUseToolButton_Patch
        {
            public static void Prefix()
            {
                if (!Config.ModEnabled ||!Config.SwitchEnabled || Game1.fadeToBlack || !Context.CanPlayerMove || (Game1.player.CurrentTool is null && Config.HoldingTool))
                    return;
                SmartSwitch(Game1.player);
            }
        }

        [HarmonyPatch(typeof(HoeDirt), nameof(HoeDirt.performUseAction))]
        public class HoeDirt_performUseAction_Patch
        {
            public static void Prefix(HoeDirt __instance)
            {
                if (!Config.ModEnabled || !Config.SwitchEnabled || !Config.SwitchForCrops || (Game1.player.CurrentTool is not Tool && Config.HoldingTool))
                    return;
                SwitchForTerrainFeature(Game1.player, __instance, GetTools(Game1.player));
            }
        }
    }
}