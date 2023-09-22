using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System;
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
                if (!Config.EnableMod || !Context.IsPlayerFree || Game1.input.GetMouseState().ScrollWheelValue == Game1.oldMouseState.ScrollWheelValue || !modActive.Value)
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
                return !Config.EnableMod || !modActive.Value || !MouseInMenu();
            }
        }
    }
}