using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.GameData.Weapons;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;

namespace GalaxyWeaponChoice
{
	public partial class ModEntry
	{
		private static string chosenWeapon;
		private static Dictionary<string, string> weaponNames;

		private static void ShowChoiceMenu()
		{
			if (!Game1.weaponData.TryGetValue("4", out WeaponData sword) || !Game1.weaponData.TryGetValue("23", out WeaponData dagger) || !Game1.weaponData.TryGetValue("29", out WeaponData hammer))
				return;

			weaponNames = new Dictionary<string, string>()
			{
				{"4", TokenParser.ParseText(sword.DisplayName) },
				{"23", TokenParser.ParseText(dagger.DisplayName) },
				{"29", TokenParser.ParseText(hammer.DisplayName) }
			};
			Response[] responses =
			{
				new("4", weaponNames["4"]),
				new("23", weaponNames["23"]),
				new("29", weaponNames["29"]),
				new("cancel", string.Empty)
			};

			var action = new GameLocation.afterQuestionBehavior(GetWeapon);

			Game1.currentLocation.createQuestionDialogue(string.Empty, responses, action);
		}

		private static void GetWeapon(Farmer who, string response)
		{
			if (response.Equals("cancel"))
				return;

			chosenWeapon = response;
			SMonitor.Log($"Chose {response}");
			Game1.player.Halt();
			Game1.player.faceDirection(2);
			Game1.player.showCarrying();
			Game1.player.jitterStrength = 1f;
			Game1.pauseThenDoFunction(7000, new Game1.afterFadeFunction(ActuallyGetWeapon));
			Game1.changeMusicTrack("none");
			Game1.currentLocation.playSound("crit");
			Game1.screenGlowOnce(new Color(30, 0, 150), true, 0.01f, 0.999f);
			DelayedAction.playSoundAfterDelay("stardrop", 1500);
			Game1.screenOverlayTempSprites.AddRange(Utility.sparkleWithinArea(new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), 500, Color.White, 10, 2000, ""));
			Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, new Game1.afterFadeFunction(delegate ()
			{
				Game1.stopMusicTrack(StardewValley.GameData.MusicContext.Event);
			}));
		}

		private static void ActuallyGetWeapon()
		{
			Game1.flashAlpha = 1f;
			Game1.player.holdUpItemThenMessage(new MeleeWeapon(chosenWeapon), true);
			Game1.player.reduceActiveItemByOne();
			if (!Game1.player.addItemToInventoryBool(new MeleeWeapon(chosenWeapon), false))
			{
				Game1.createItemDebris(new MeleeWeapon(chosenWeapon), Game1.player.getStandingPosition(), 1, null, -1);
			}
			Game1.player.mailReceived.Add("galaxySword");
			Game1.player.jitterStrength = 0f;
			Game1.screenGlowHold = false;
			Game1.Multiplayer.globalChatInfoMessage("GalaxySword", new string[]
			{
				Game1.player.Name
			});
		}
	}
}
