using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using Object = StardewValley.Object;

namespace MoveableMailbox
{
	public partial class ModEntry
	{
		private static void InitMailBoxesList()
		{
			mailboxes.Clear();
			foreach (GameLocation location in Game1.locations)
			{
				foreach (KeyValuePair<Vector2, Object> kvp in location.objects.Pairs)
				{
					if (kvp.Value.Name.Equals("Mailbox"))
					{
						mailboxes.Add(kvp.Value);
					}
				}
			}
		}

		private static bool OnAllMailboxes(Func<Object, bool> function)
		{
			foreach (Object mailbox in mailboxes)
			{
				if (function(mailbox))
					return true;
			}
			return false;
		}

		private static bool AnyOwnedMailbox(string owner)
		{
			return OnAllMailboxes((Object m) => {
				if (m.modData.TryGetValue(ownerKey, out string o) && o.Equals(owner))
				{
					return true;
				}
				return false;
			});
		}

		private static bool SetOwnershipOnNonOwnedMailbox(string owner)
		{
			return OnAllMailboxes((Object m) => {
				if (!m.modData.ContainsKey(ownerKey))
				{
					m.modData[ownerKey] = owner;
					return true;
				}
				return false;
			});
		}

		private static bool RemoveOwnershipOfOwnedMailbox(Object mailbox, string owner)
		{
			return OnAllMailboxes((Object m) => {
				if ((m.Location != mailbox.Location || m.TileLocation != mailbox.TileLocation) && m.modData.TryGetValue(ownerKey, out string o) && o.Equals(owner))
				{
					m.modData.Remove(ownerKey);
					SMonitor.Log($"Removed");
					return true;
				}
				return false;
			});
		}

		private static bool DrawBubble(string owner, GameLocation location, SpriteBatch b)
		{
			return OnAllMailboxes((Object m) => {
				bool flag = m.modData.TryGetValue(ownerKey, out string o);

				if (location == m.Location && ((!flag && Config.MultipleMailboxes) || (flag && o.Equals(owner))))
				{
					float num = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
					Point mailboxPosition = Utility.Vector2ToPoint(m.TileLocation);
					float num2 = (mailboxPosition.X + 1) * 64 / 10000f + mailboxPosition.Y * 64 / 10000f;
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(mailboxPosition.X * 64, mailboxPosition.Y * 64 - 96 - 48 + num)), new Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, num2 + 1E-06f);
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(mailboxPosition.X * 64 + 32 + 4, mailboxPosition.Y * 64 - 64 - 24 - 8 + num)), new Rectangle(189, 423, 15, 13), Color.White, 0f, new Vector2(7f, 6f), 4f, SpriteEffects.None, num2 + 1E-05f);
					return !Config.MultipleMailboxes;
				}
				return false;
			});
		}
	}
}
