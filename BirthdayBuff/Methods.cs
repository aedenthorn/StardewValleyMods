using System;
using System.Collections.Generic;
using System.Reflection;
using StardewValley;

namespace BirthdayBuff
{
	public partial class ModEntry
	{
		public static void AddBirthdayBuff()
		{
			BuffFrameworkAPI.Add($"{SModManifest.UniqueID}/HappyBirthday", new Dictionary<string, object>()
				{
					{ "buffId", $"{SModManifest.UniqueID}/HappyBirthday" },
					{ "displayName", SHelper.Translation.Get("birthday-buff-displayName") },
					{ "source", SModManifest.UniqueID },
					{ "displaySource", SHelper.Translation.Get("birthday-buff-displaySource") },
					{ "texturePath", "Maps/springobjects" },
					{ "textureX", "80" },
					{ "textureY", "144" },
					{ "textureWidth", "16" },
					{ "textureHeight", "16" },
					{ "farming", Config.Farming },
					{ "mining", Config.Mining },
					{ "foraging", Config.Foraging },
					{ "fishing", Config.Fishing },
					{ "attack", Config.Attack },
					{ "defense", Config.Defense },
					{ "speed", Config.Speed },
					{ "magneticRadius", Config.MagneticRadius },
					{ "luck", Config.Luck },
					{ "maxStamina", Config.MaxStamina },
					{ "sound", Config.Sound },
					{ "glow", Config.GlowColor },
					{ "glowRate", Config.GlowRate }
				}, () => {
					return cachedResult;
				});
		}

		public static void RemoveBirthdayBuff()
		{
			BuffFrameworkAPI.Remove($"{SModManifest.UniqueID}/HappyBirthday");
			Game1.player?.buffs.Remove($"{SModManifest.UniqueID}/HappyBirthday");
		}

		private static bool IsBirthdayDay()
		{
			Type happyBirthdayModCore = HappyBirthdayAPI.GetType().Assembly.GetType("Omegasis.HappyBirthday.HappyBirthdayModCore");
			object instance = happyBirthdayModCore.GetField("Instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
			object birthdayManager = instance.GetType().GetField("birthdayManager").GetValue(instance);
			bool hasChosenBirthday = (bool)birthdayManager.GetType().GetMethod("hasChosenBirthday").Invoke(birthdayManager, null);

			if (!hasChosenBirthday)
			{
				return false;
			}

			object playerBirthdayData = birthdayManager.GetType().GetField("playerBirthdayData").GetValue(birthdayManager);
			int birthdayDay = (int)playerBirthdayData.GetType().GetField("BirthdayDay").GetValue(playerBirthdayData);
			string birthdaySeason = (string)playerBirthdayData.GetType().GetField( "BirthdaySeason").GetValue(playerBirthdayData);

			return Game1.player is not null && Game1.dayOfMonth == birthdayDay && Game1.currentSeason == birthdaySeason.ToLower();
		}
	}
}
