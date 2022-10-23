using HarmonyLib;
using StardewValley;
using System;
using System.Collections.Generic;

namespace StardewRPG
{
    public partial class ModEntry 
    {
		public static int[] skillLevels = new int[]
		{
			100, // 100
			380, // 280
			770, // 390		
			1300, // 530
			2150, // 850
			3300, // 1150
			4800, // 1500
			6900, // 2100
			10000, // 3100
			15000, // 5000
			23000 // 8000
		};

		public static string[] skillNames = new string[]
		{
			"str", "con", "dex", "int","wis", "cha"
		};

		public static void GainExperience(ref Farmer instance, int howMuch)
		{
			int currentXP = GetStatValue(instance, "exp");
			int newXP = currentXP + (int)Math.Round(howMuch * (1 + GetStatMod(GetStatValue(Game1.player, "wis", Config.BaseStatValue)) * Config.WisExpBonus));
			SetModData(instance, "exp", newXP);
			bool levelUp = false;
			foreach (int level in GetExperienceLevels())
			{
				if (currentXP < level)
                {
					if(newXP >= level)
						levelUp = true;
					break;
                }
            }
			if (levelUp)
            {
				SetStats(ref instance);
				instance.health = instance.maxHealth;
				instance.stamina = instance.MaxStamina;
				AlertLevelUp(instance.displayName, GetExperienceLevel(instance));
			}
		}

        private static void AlertLevelUp(string name, int level)
		{
            if (Config.NotifyOnLevelUp)
				Game1.addHUDMessage(new HUDMessage(string.Format(SHelper.Translation.Get("farmer-level-up-#"), level)));
		}

		private static void SetStats(ref Farmer instance, bool newFarmer = false)
		{
			Dictionary<string, int> skillSet = new Dictionary<string, int>();
			foreach(string name in skillNames)
            {
				if (!instance.modData.TryGetValue(modDataKey + name, out string skillString) || !int.TryParse(skillString, out int skill))
                {
					skill = Config.BaseStatValue;
					SetModData(instance, name, skill);
				}
				skillSet[name] = skill;
			}
            if (newFarmer || GetStatValue(instance, "exp") < 0)
            {
				SetModData(instance, "exp", 0);
            }
			int level = GetExperienceLevel(instance);
			instance.maxHealth = (int)Math.Max(1, level * Config.BaseHealthPerLevel * (1 + Config.ConHealthBonus * GetStatMod(skillSet["con"])));
			instance.MaxStamina = (int)Math.Max(1, level * Config.BaseStaminaPerLevel * (1 + Config.ConStaminaBonus * GetStatMod(skillSet["con"])));
			if (newFarmer)
            {
				instance.health = instance.maxHealth;
				instance.stamina = instance.MaxStamina;
            }
			SMonitor.Log($"Farmer health {instance.health}/{instance.maxHealth}, stamina {instance.stamina}/{instance.MaxStamina}");
		}

        public static int GetStatValue(Farmer instance, string key, int defaultValue = -1)
        {
			string value = GetModData(instance, key);
			if (value == null || !int.TryParse(value, out int output))
				return defaultValue;
			return output;
		}

		private static string GetModData(Farmer instance, string key)
        {
			return instance.modData.TryGetValue(modDataKey + key, out string output) ? output : null;

		}

		private static void SetModData(Farmer instance, string key, object value)
		{
			instance.modData[modDataKey + key] = value.ToString();
			SetStats(ref instance);
		}

		public static int GetStatMod(int v)
        {
			int mod = 0;
			foreach(string bonus in Config.StatBonusLevels.Split(','))
            {
				if (!int.TryParse(bonus, out int level) || v < level)
					break;
				mod++;
            }
			foreach(string penalty in Config.StatPenaltyLevels.Split(','))
            {
				if (!int.TryParse(penalty, out int level) || v > level)
					break;
				mod--;
            }
			return mod;
        }

		private static int[] GetExperienceLevels()
        {
			List<int> levels = new List<int>();
			for (int i = 0; i < skillLevels.Length - 1; i++)
			{
				int toNextLevel = skillLevels[i + 1] - skillLevels[i];
				for (int j = 0; j < 6; j++)
                {
					levels.Add((int)Math.Round((levels.Count > 0 ? levels[levels.Count - 1] : 0) + (skillLevels[i] + toNextLevel * j / 6) * Config.LevelIncrementExpMult));
                }
			}
			return levels.ToArray();
        }

		private static int GetExperienceLevel(Farmer instance)
        {
			int exp = GetStatValue(instance, "exp");
			int l = 1;
			foreach(int level in GetExperienceLevels())
            {
				if (exp < level)
					return l;
				l++;
            }
			return l;
		}

		private static int GetTotalSkillLevels(Farmer player)
		{
			return player.FarmingLevel + player.MiningLevel + player.ForagingLevel + player.FishingLevel + player.CombatLevel;
		}


		private static void LevelUp(int num)
		{
			var levels = GetExperienceLevels();
			var exp = GetStatValue(Game1.player, "exp");
			for(int j = 0; j < num; j++)
            {
				for (int i = 0; i < levels.Length; i++)
				{
					if (exp < levels[i])
					{
						Farmer f = (Farmer)AccessTools.Field(typeof(Game1), "_player").GetValue(null);
						GainExperience(ref f, levels[i] - exp);
						SMonitor.Log($"Added {levels[i] - exp} exp");
						break;
					}
				}
			}
		}
		private static void Respec()
		{
			Game1.player.FarmingLevel = 0;
			Game1.player.FishingLevel = 0;
			Game1.player.ForagingLevel = 0;
			Game1.player.MiningLevel = 0;
			Game1.player.CombatLevel = 0;
			Game1.player.experiencePoints[0] = 0;
			Game1.player.experiencePoints[1] = 0;
			Game1.player.experiencePoints[2] = 0;
			Game1.player.experiencePoints[3] = 0;
			Game1.player.experiencePoints[4] = 0;
			var totalPoints = 0;
			foreach(var name in skillNames)
            {
				totalPoints += GetStatValue(Game1.player, name, Config.BaseStatValue) - Config.BaseStatValue;
				SetModData(Game1.player, name, Config.BaseStatValue);
            }
			SetModData(Game1.player, "points", totalPoints);
			Farmer f = Game1.player;
			SetStats(ref f, true);
			Game1.player = f;
		}
	}
}