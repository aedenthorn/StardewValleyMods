using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace StardewRPG
{
    public partial class ModEntry
    {
        private static bool Farmer_performBeginUsingTool_Prefix(Farmer __instance)
        {
            if (!Config.EnableMod || __instance.CurrentTool?.UpgradeLevel <= 0)
                return true;
			bool can = true;
			int need = 0;
			int have = 0;
			string skill = "";
			if (__instance.CurrentTool is Pickaxe)
			{
				need = (int)Math.Ceiling(__instance.CurrentTool.UpgradeLevel * Config.ToolLevelReqMult);
				have = __instance.MiningLevel;
				skill = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11605");
			}
			else if(__instance.CurrentTool is Axe)
			{
				need = (int)Math.Ceiling(__instance.CurrentTool.UpgradeLevel * Config.ToolLevelReqMult);
				have = __instance.ForagingLevel;
				skill = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11606");
			}
			else if(__instance.CurrentTool is FishingRod)
			{
				need = (int)Math.Ceiling(__instance.CurrentTool.UpgradeLevel * Config.ToolLevelReqMult);
				have = __instance.FishingLevel;
				skill = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11607");
			}
			else if (__instance.CurrentTool is WateringCan || __instance.CurrentTool is Hoe)
			{
				need = (int)Math.Ceiling(__instance.CurrentTool.UpgradeLevel * Config.ToolLevelReqMult);
				have = __instance.FarmingLevel;
				skill = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11604");
			}
			else if (__instance.CurrentTool is MeleeWeapon)
            {
				have = __instance.CombatLevel;
				need = (int)Math.Ceiling((__instance.CurrentTool as MeleeWeapon).getItemLevel() * Config.WeaponLevelReqMult);
				skill = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11608");
			}
            if (have < need)
			{
				Game1.addHUDMessage(new HUDMessage(string.Format(SHelper.Translation.Get("need-#-level-x-skill"), need, skill), 3));
				Game1.playSound("cancel");
				can = false;
			}
			return can;
        }
        private static bool Farmer_gainExperience_Prefix(Farmer __instance, int howMuch)
        {
            if (!Config.EnableMod)
                return true;
            GainExperience(__instance, howMuch);
            return !Config.ManualSkillUpgrades;
        }
		
        private static bool Farmer_Level_Prefix(Farmer __instance, ref int __result)
        {
            if (!Config.EnableMod)
                return true;

			__result = GetExperienceLevel(__instance);
			return false;
		}
		
        private static void Farmer_doneEating_Postfix(Farmer __instance)
        {
            if (!Config.EnableMod)
                return;

			Object consumed = __instance.itemToEat as Object;
			if(__instance.IsLocalPlayer && consumed.ParentSheetIndex == 434)
            {
				SetModData(__instance, "points", Math.Max(0, GetStatValue(__instance, "points")) + Config.StatPointsPerStardrop);
				SetStats(ref __instance);
            }
		}

        private static void Farmer_Postfix(Farmer __instance)
        {
            if (!Config.EnableMod)
                return;
            SetStats(ref __instance, true);
        }
   }
}