using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace StardewRPG
{
    public partial class ModEntry
    {
        public static IEnumerable<CodeInstruction> CraftingRecipe_consumeIngredients_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling CraftingRecipe.consumeIngredients");

            var codes = new List<CodeInstruction>(instructions);
            if (codes[15].opcode == OpCodes.Stloc_1)
            {
                SMonitor.Log("Overriding required amount");
                codes.Insert(15, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetRecipeRequiredAmount))));
            }
            else
            {
                SMonitor.Log("Couldn't override required amount", StardewModdingAPI.LogLevel.Error);
            }
            return codes.AsEnumerable();
        }
        
        public static IEnumerable<CodeInstruction> CraftingRecipe_doesFarmerHaveIngredientsInInventory_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling CraftingRecipe.doesFarmerHaveIngredientsInInventory");

            var codes = new List<CodeInstruction>(instructions);
            if (codes[10].opcode == OpCodes.Stloc_2)
            {
                SMonitor.Log("Overriding required amount");
                codes.Insert(10, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetRecipeRequiredAmount))));
            }
            else
            {
                SMonitor.Log("Couldn't override required amount", StardewModdingAPI.LogLevel.Error);
            }
            return codes.AsEnumerable();
        }

        private static int GetRecipeRequiredAmount(int amount)
        {
            if (!Config.EnableMod)
                return amount;
            var sub = GetStatMod(GetStatValue(Game1.player, "wis", Config.BaseStatValue)) * Config.WisCraftResourceReqBonus;
            //aSMonitor.Log($"Modifying craft resource amount {amount} => {amount * (1 - sub)}");
            return (int)Math.Max(1,Math.Round(amount * (1 - sub)));

        }
        private static bool CraftingPage_clickCraftingRecipe_Prefix(CraftingPage __instance, ClickableTextureComponent c, int ___currentCraftingPage)
        {
            if (!Config.EnableMod || !Config.IntRollCraftingChance)
                return true;
            if (Game1.random.Next(20) >= GetStatValue(Game1.player, "int"))
            {
                __instance.pagesOfCraftingRecipes[___currentCraftingPage][c].createItem();
                SMonitor.Log("Int check failed on craft");
                Game1.addHUDMessage(new HUDMessage(SHelper.Translation.Get("int-check-failed"), 3));
                Game1.playSound("cancel");
                return false;
            }
            return true;
        }
    }
}