using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Input;
using xTile.Dimensions;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace ToolSmartSwitch
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(Game1), nameof(Game1.pressUseToolButton))]
        public class Farmer_pressUseToolButton_Patch
        {
            public static void Prefix()
            {
                if (!Config.EnableMod || Game1.fadeToBlack || !Context.CanPlayerMove || (Game1.player.CurrentTool is null && Config.HoldingTool))
                    return;
                SmartSwitch(Game1.player);
            }
        }

        [HarmonyPatch(typeof(HoeDirt), nameof(HoeDirt.performUseAction))]
        public class HoeDirt_performUseAction_Patch
        {
            public static void Prefix(HoeDirt __instance)
            {
                if (!Config.EnableMod || !Config.SwitchForCrops || (Game1.player.CurrentTool is not Tool && Config.HoldingTool))
                    return;
                SwitchForTerrainFeature(Game1.player, __instance, GetTools(Game1.player));
            }
        }
    }
}