using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using xTile;
using xTile.Layers;
using xTile.Tiles;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace MapEdit
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Game1), nameof(Game1.pressSwitchToolButton))]
        public class Game1_pressSwitchToolButton_Patch
        {
            private static bool Prefix()
            {
                if (!Config.ModEnabled || !Context.IsPlayerFree || Game1.input.GetMouseState().ScrollWheelValue == Game1.oldMouseState.ScrollWheelValue || !modActive.Value)
                    return true;

                if (MouseInMenu())
                    return false;
                if(currentTileDict.Value.Count > 0)
                {
                    SwitchTile(Game1.input.GetMouseState().ScrollWheelValue - Game1.oldMouseState.ScrollWheelValue > 0);
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Game1), nameof(Game1.drawMouseCursor))]
        public class Game1_drawMouseCursor_Patch
        {
            private static bool Prefix()
            {
                return !Config.ModEnabled || !modActive.Value || !MouseInMenu();
            }
        }
        [HarmonyPatch(typeof(Toolbar), nameof(Toolbar.performHoverAction))]
        public class Toolbar_performHoverAction_Patch
        {
            private static bool Prefix()
            {
                return !Config.ModEnabled || !modActive.Value || !MouseInMenu();
            }
        }
        [HarmonyPatch(typeof(Game1), nameof(Game1.SetWindowSize))]
        public class Game1_SetWindowSize_Patch
        {
            private static void Postfix()
            {
                if (!Config.ModEnabled || !modActive.Value || tileMenu.Value is null)
                    return;
                tileMenu.Value.RebuildElements();
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.MakeMapModifications))]
        public class GameLocation_MakeMapModifications_Patch
        {
            private static void Postfix(GameLocation __instance)
            {
                if (!Config.ModEnabled || __instance.mapPath.Value is null)
                    return;
                if (!mapCollectionData.mapDataDict.TryGetValue("Maps/" + __instance.NameOrUniqueName, out var dict) && !mapCollectionData.mapDataDict.TryGetValue(__instance.NameOrUniqueName, out dict))
                    return;
                EditMap(__instance.mapPath.Value, __instance.Map, dict);
            }
        }
    }
}