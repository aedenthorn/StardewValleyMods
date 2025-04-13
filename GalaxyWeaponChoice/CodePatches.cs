using System;
using StardewValley;
using StardewValley.Tools;

namespace GalaxyWeaponChoice
{
	public partial class ModEntry
	{
		public class Multiplayer_receiveChatInfoMessage_Patch
		{
			public static bool Prefix(string messageKey, string[] args)
			{
				if (!Config.ModEnabled || messageKey != "GalaxySword" || Game1.chatBox is null)
					return true;

				if (chosenWeapon == "23")
				{
					Game1.chatBox.addInfoMessage(SHelper.Translation.Get("Chat_GalaxyDagger", new { PlayerName = args[0] }));
					return false;
				}
				if (chosenWeapon == "29")
				{
					Game1.chatBox.addInfoMessage(SHelper.Translation.Get("Chat_GalaxyHammer", new { PlayerName = args[0] }));
					return false;
				}
				return true;
			}
		}

		public class GameLocation_performTouchAction_Patch
		{
			public static bool Prefix(string fullActionString)
			{
				if (!Config.ModEnabled)
					return true;

				string text = fullActionString.Split(' ', StringSplitOptions.None)[0];

				if (text == "legendarySword" && Game1.player.ActiveObject != null && Utility.IsNormalObjectAtParentSheetIndex(Game1.player.ActiveObject, "74") && !Game1.player.mailReceived.Contains("galaxySword"))
				{
					ShowChoiceMenu();
					return false;
				}
				return true;
			}
		}

		public class Weapon_checkForSpecialItemHoldUpMeessage_Patch
		{
			public static void Postfix(MeleeWeapon __instance, ref string __result)
			{
				if (!Config.ModEnabled)
					return;

				if (__instance.QualifiedItemId == "(W)23")
				{
					__result = SHelper.Translation.Get("MeleeWeapon_GalaxyDagger");
				}
				else if (__instance.QualifiedItemId == "(W)29")
				{
					__result = SHelper.Translation.Get("MeleeWeapon_GalaxyHammer");
				}
			}
		}
	}
}
