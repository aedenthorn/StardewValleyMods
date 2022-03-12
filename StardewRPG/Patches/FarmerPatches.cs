using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;
using System;
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
			else if (__instance.CurrentTool is Axe)
			{
				need = (int)Math.Ceiling(__instance.CurrentTool.UpgradeLevel * Config.ToolLevelReqMult);
				have = __instance.ForagingLevel;
				skill = Game1.content.LoadString("Strings\\StringsFromCSFiles:SkillsPage.cs.11606");
			}
			else if (__instance.CurrentTool is FishingRod)
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
		private static bool Farmer_gainExperience_Prefix(Farmer __instance, int howMuch, int which)
		{
			if (!Config.EnableMod || which > 4)
				return true;
			if (howMuch > 0)
			{
				GainExperience(ref __instance, howMuch);
				//SMonitor.Log($"Gained {howMuch} exp, new total {GetModData(__instance, "exp")}");
			}
			return !Config.ManualSkillUpgrades;
		}

		private static void Farmer_CanBeDamaged_Postfix(Farmer __instance, ref bool __result)
		{
			if (!Config.EnableMod || !Config.DexRollForMiss || !__result || !Environment.StackTrace.Contains("takeDamage"))
				return;
			if (Game1.random.Next(20) < GetStatValue(__instance, "dex", Config.BaseStatValue) - 10)
			{
				__instance.currentLocation.debris.Add(new Debris(SHelper.Translation.Get("miss"), 1, __instance.GetBoundingBox().Center.ToVector2(), Color.LightGray, 1f, 0f));
				__instance.temporarilyInvincible = true;
				__instance.temporaryInvincibilityTimer = 0;
				__instance.currentTemporaryInvincibilityDuration = 1200 + __instance.GetEffectsOfRingMultiplier(861) * 400;
				__result = false;
				SMonitor.Log($"successful dex roll, missed!");
			}
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
			if (__instance.IsLocalPlayer && consumed.ParentSheetIndex == 434)
			{
				SetModData(__instance, "points", Math.Max(0, GetStatValue(__instance, "points")) + Config.StatPointsPerStardrop);
				SMonitor.Log($"ate stardrop, gained {Config.StatPointsPerStardrop}, total {GetStatValue(__instance, "points")}");
				SetStats(ref __instance);
			}
		}

		private static void Farmer_Postfix(Farmer __instance)
		{
			if (!Config.EnableMod)
				return;
			SetStats(ref __instance, true);
		}
		private static void Farmer_takeDamage_Prefix(Farmer __instance, ref int __state)
		{
			if (!Config.EnableMod)
				return;
			__state = __instance.resilience;
			__instance.resilience += GetStatMod(GetStatValue(__instance, "con", Config.BaseStatValue)) * Config.ConDefenseBonus;
			SMonitor.Log($"Modifying resilience {__state} => {__instance.resilience }");
		}
		private static void Farmer_takeDamage_Prefix(Farmer __instance, int __state)
		{
			if (!Config.EnableMod)
				return;
			__instance.resilience = __state;

		}
		private static void Farmer_changeFriendship_Prefix(Farmer __instance, ref int amount)
		{
			if (!Config.EnableMod)
				return;
			int add = (int)Math.Round(Math.Abs(amount) * GetStatMod(GetStatValue(Game1.player, "cha", Config.BaseStatValue)) * Config.ChaFriendshipBonus);
			SMonitor.Log($"Modifying friendship change {amount} + {add}");
			amount += add;
		}
		private static void Farmer_performTenMinuteUpdate_Postfix(Farmer __instance)
		{
			if (!Config.EnableMod || !Context.CanPlayerMove)
				return;
			SetStats(ref __instance);
			
			int mod = Math.Max(0, GetStatMod(GetStatValue(__instance, "con")));
			if (__instance.health < __instance.maxHealth)
				__instance.health = Math.Min(__instance.maxHealth, __instance.health + Config.HealthRegen + mod * Config.ConHealthRegenBonus);
			if (__instance.Stamina < __instance.MaxStamina)
				__instance.Stamina = Math.Min(__instance.MaxStamina, __instance.Stamina + Config.StaminaRegen + mod * Config.ConStaminaRegenBonus);
			return;
		}
	}
}