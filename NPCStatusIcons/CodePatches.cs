using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Characters;
using System.Linq;

namespace NPCStatusIcons
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(NPC), nameof(NPC.draw))]
        public class NPC_draw_Patch
        {
            public static void Postfix(NPC __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled || (Config.RequireModKey && !SHelper.Input.IsDown(Config.ModKey)) || !Game1.player.friendshipData.TryGetValue(__instance.Name, out Friendship f))
                    return;
                int yOffset = 100;
                if (f.GiftsToday < 1)
                {
                    if (Config.ShowBirthday && __instance.isBirthday(Game1.currentSeason, Game1.dayOfMonth))
                    {
                        b.Draw(Game1.objectSpriteSheet, new Rectangle(Utility.Vector2ToPoint(Game1.GlobalToLocal(__instance.Position + new Vector2(0, -yOffset - 4))), new Point(32, 32)), new Rectangle(80, 144, 16, 16), Color.White, 0, Vector2.Zero, SpriteEffects.None, 5f);
                    }
                    else if (Config.ShowGiftable && (f.GiftsThisWeek < 2 || f.IsMarried() || f.IsRoommate() || __instance is Child))
                    {
                        b.Draw(Game1.mouseCursors, new Rectangle(Utility.Vector2ToPoint(Game1.GlobalToLocal(__instance.Position + new Vector2(0, -yOffset))), new Point(28, 28)), new Rectangle(229, 410, 14, 14), Color.White, 0, Vector2.Zero, SpriteEffects.None, 5f);
                    }
                }
                if (Config.ShowTalkable && (__instance.CurrentDialogue.Count > 0 || __instance.currentMarriageDialogue.Count > 0))
                {
                    b.Draw(Game1.mouseCursors, new Rectangle(Utility.Vector2ToPoint(Game1.GlobalToLocal(__instance.Position + new Vector2(36, -yOffset))), new Point(28, 24)), new Rectangle(66, 4, 14, 12), Color.White, 0, Vector2.Zero, SpriteEffects.None, 5f);
                }
            }
        }
    }
}