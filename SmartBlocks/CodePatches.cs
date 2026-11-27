using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using static StardewValley.Menus.CharacterCustomization;
using Object = StardewValley.Object;

namespace SmartBlocks
{
    public partial class ModEntry
    {
        //[HarmonyPatch(typeof(Furniture), nameof(Furniture.checkForAction))]
        public static class Furniture_checkForAction_Patch
        {
            public static bool Prefix(Furniture __instance, bool justCheckingForActivity, ref bool __result)
            {
                if (!Config.ModEnabled || !BlockTypes.TryGetValue(__instance.ItemId, out var data))
                {
                    return true;
                }
                return false;
            }

        }
        
        //[HarmonyPatch(typeof(Furniture), nameof(Furniture.draw), new Type[] { typeof(SpriteBatch),typeof(int),typeof(int),typeof(float), })]
        public static class Furniture_draw_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) // reduce height for fewer weeds
            {
                SMonitor.Log($"Transpiling Furniture.draw");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count - 4; i++)
                {
                }
                return codes.AsEnumerable();
            }

        }
    }
}