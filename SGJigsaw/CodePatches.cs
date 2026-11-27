using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.GameData.Locations;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.Quests;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SGJigsaw
{
	public partial class ModEntry
    {

        [HarmonyPatch(typeof(Furniture), nameof(Furniture.checkForAction))]
        public class Furniture_checkForAction_Patch
        {

            public static bool Prefix(Furniture __instance, ref bool __result)
            {
                if (!Config.ModEnabled || __instance.heldObject.Value?.ItemId != boxId)
                    return true;
                Game1.playSound("dwoop");
                Game1.activeClickableMenu = new JigsawGameMenu(__instance.heldObject.Value);
                __result = true;
                return false;
            }
        }
    }
}
