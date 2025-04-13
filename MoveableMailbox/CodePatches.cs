using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using Object = StardewValley.Object;

namespace MoveableMailbox
{
	public partial class ModEntry : Mod
	{
		class Farm_GetMainMailboxPosition_Patch
		{
			public static void Postfix(Farm __instance, ref Point __result)
			{
				string masterPlayerUniqueMultiplayerID = Game1.MasterPlayer.UniqueMultiplayerID.ToString();

				__instance.mapMainMailboxPosition = new Point(-1, -1);
				foreach (Object mailbox in mailboxes)
				{
					if (mailbox.modData.TryGetValue(ownerKey, out string o) && o.Equals(masterPlayerUniqueMultiplayerID))
					{
						__result = Utility.Vector2ToPoint(mailbox.TileLocation);
						return;
					}
				}
				__result = new Point(-1, -1);
			}
		}

		class Object_placementAction_Patch
		{
			public static void Postfix(Object __instance, GameLocation location, int x, int y, Farmer who, ref bool __result)
			{
				if (!__instance.Name.Equals("Mailbox") || !__result)
					return;

				Object mailbox = location.Objects[new Vector2(x / 64, y / 64)];

				if (mailbox is not null)
				{
					RemoveOwnershipOfOwnedMailbox(mailbox, who.UniqueMultiplayerID.ToString());
					mailbox.modData[ownerKey] = who.UniqueMultiplayerID.ToString();
					mailboxes.Add(mailbox);
				}
			}
		}

		class Object_performRemoveAction_Patch
		{
			public static void Postfix(Object __instance)
			{
				if (__instance.Name.Equals("Mailbox") && __instance.modData.TryGetValue(ownerKey, out string owner))
				{
					SMonitor.Log("Removed mailbox");
					mailboxes.Remove(__instance);
					SetOwnershipOnNonOwnedMailbox(owner);
				}
			}
		}

		class Object_checkForAction_Patch
		{
			public static bool Prefix(Object __instance, Farmer who, bool justCheckingForActivity, ref bool __result)
			{
				if (!__instance.Name.Equals("Mailbox") || justCheckingForActivity)
					return true;

				SMonitor.Log("Clicked on mailbox");
				if (__instance.modData.TryGetValue(ownerKey, out string owner))
				{
					if (!owner.Equals(who.UniqueMultiplayerID.ToString()))
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Farm_OtherPlayerMailbox"));
					}
					else
					{
						who.currentLocation.mailbox();
					}
				}
				else
				{
					if (Config.MultipleMailboxes)
					{
						who.currentLocation.mailbox();
					}
					else
					{
						Game1.currentLocation.createQuestionDialogue(SHelper.Translation.Get("dialoguebox.message"), Game1.currentLocation.createYesNoResponses(), (Farmer who, string whichAnswer) => {
							if (whichAnswer.Equals("Yes"))
							{
								RemoveOwnershipOfOwnedMailbox(__instance, who.UniqueMultiplayerID.ToString());
								__instance.modData[ownerKey] = who.UniqueMultiplayerID.ToString();
							}
						});
					}
				}
				__result = true;
				return false;
			}
		}

		class Object_hoverAction_Patch
		{
			public static void Postfix(Object __instance)
			{
				if (__instance.Name.Equals("Mailbox"))
				{
					Game1.mouseCursor = Game1.cursor_grab;
				}
			}
		}

		class Gamelocation_draw_Patch
		{
			public static void Postfix(GameLocation __instance, SpriteBatch b)
			{
				if (Game1.mailbox.Count > 0)
				{
					DrawBubble(Game1.player.UniqueMultiplayerID.ToString(), __instance, b);
				}

				Object mailbox = __instance.getObjectAtTile((int)Game1.currentCursorTile.X, (int)Game1.currentCursorTile.Y);

				if (mailbox is not null && mailbox.Name.Equals("Mailbox"))
				{
					Game1.mouseCursor = Game1.cursor_grab;
				}
				else
				{
					mailbox = __instance.getObjectAtTile((int)Game1.currentCursorTile.X, (int)Game1.currentCursorTile.Y + 1);
					if (mailbox is not null && mailbox.Name.Equals("Mailbox"))
					{
						Game1.mouseCursor = Game1.cursor_grab;
					}
				}
			}
		}
	}
}
