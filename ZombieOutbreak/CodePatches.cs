using System;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using Object = StardewValley.Object;

namespace ZombieOutbreak
{
	public partial class ModEntry
	{
		public class NPC_receiveGift_Patch
		{
			internal static bool Prefix(NPC __instance, Object o, Farmer giver)
			{
				if (!Config.ModEnabled)
					return true;

				if (o.ItemId == $"{SModManifest.UniqueID}_ZombieCure" && zombieNPCTextures.ContainsKey(__instance.Name))
				{
					RemoveZombieNPC(__instance.Name);
					giver.currentLocation.playSound("slimedead");
					giver.changeFriendship(250, __instance);

					Dialogue dialogue = new(__instance, null, SHelper.Translation.Get($"dialogue.thanks.{new Random().Next(1, 5)}"));

					Game1.DrawDialogue(dialogue);
					SMonitor.Log($"Gave Zombie Cure to {__instance.Name}");
					return false;
				}
				return true;
			}
		}

		public class Farmer_eatObject_Patch
		{
			internal static void Prefix(Farmer __instance, Object o)
			{
				if (!Config.ModEnabled)
					return;

				if (o.ItemId == $"{SModManifest.UniqueID}_ZombieCure")
				{
					if (zombieFarmerTextures.ContainsKey(__instance.UniqueMultiplayerID))
					{
						SMonitor.Log($"zombie farmer {__instance.Name} ate zombie cure");
						RemoveZombieFarmer(__instance.UniqueMultiplayerID);
					}
				}
			}
		}

		public class NPC_draw_Patch
		{
			public static void Prefix(NPC __instance)
			{
				if (!Config.ModEnabled)
					return;

				if (zombieNPCTextures.ContainsKey(__instance.Name) && __instance.Sprite.spriteTexture != zombieNPCTextures[__instance.Name])
				{
					MakeZombieNPCTexture(__instance.Name);
				}
			}
		}

		public class DialogueBox_drawPortrait_Patch
		{
			public static void Prefix(DialogueBox __instance)
			{
				if (!Config.ModEnabled)
					return;

				if (zombieNPCPortraits.ContainsKey(__instance.characterDialogue.speaker.Name) && __instance.characterDialogue.speaker.Portrait != zombieNPCPortraits[__instance.characterDialogue.speaker.Name])
				{
					MakeZombieNPCTexture(__instance.characterDialogue.speaker.Name);
				}
			}
		}

		public class Farmer_draw_Patch
		{
			public static void Prefix(Farmer __instance)
			{
				if (!Config.ModEnabled)
					return;

				if (zombieFarmerTextures.ContainsKey(__instance.UniqueMultiplayerID) && SHelper.Reflection.GetField<Texture2D>(__instance.FarmerRenderer, "baseTexture").GetValue() != zombieFarmerTextures[__instance.UniqueMultiplayerID])
				{
					MakeZombieFarmerTexture(__instance.UniqueMultiplayerID);
				}
			}
		}

		public class Dialogue_getCurrentDialogue_Patch
		{
			public static void Postfix(Dialogue __instance, ref string __result)
			{
				if (!Config.ModEnabled)
					return;

				if (__instance.speaker is not null && zombieNPCTextures.ContainsKey(__instance.speaker.Name))
				{
					MakeZombieSpeak(ref __result);
				}
			}
		}

		public class Dialogue_getResponseOptions_Patch
		{
			public static void Postfix(ref Response[] __result)
			{
				if (!Config.ModEnabled)
					return;

				if (__result is not null && zombieFarmerTextures.ContainsKey(Game1.player.UniqueMultiplayerID))
				{
					foreach (Response response in __result)
					{
						MakeZombieSpeak(ref response.responseText);
					}
				}
			}
		}

		public class NPC_showTextAboveHead_Patch
		{
			public static void Prefix(NPC __instance, ref string text)
			{
				if (!Config.ModEnabled)
					return;

				if (__instance is not null && zombieNPCTextures.ContainsKey(__instance.Name))
				{
					MakeZombieSpeak(ref text);
				}
			}
		}

		public class ShopMenu_Patch
		{
			public static void Postfix(ShopMenu __instance, NPC owner)
			{
				if (!Config.ModEnabled)
					return;

				if (owner is not null && zombieNPCTextures.ContainsKey(owner.Name))
				{
					MakeZombieSpeak(ref __instance.potraitPersonDialogue);
				}
			}
		}

		public class DialogueBox_Patch
		{
			public static void Postfix(Response[] responses)
			{
				if (!Config.ModEnabled)
					return;

				if (responses is not null && zombieFarmerTextures.ContainsKey(Game1.player.UniqueMultiplayerID))
				{
					foreach (Response response in responses)
					{
						MakeZombieSpeak(ref response.responseText);
					}
				}
			}
		}
	}
}
