using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.IO;
using System.Linq;

namespace SocialNetwork
{
    public class HelperEvents
    {
        internal static IModHelper Helper;
        internal static IMonitor Monitor;
        internal static ModConfig Config;

        public static void GameLoop_TimeChanged(object sender, TimeChangedEventArgs e)
        {
            if (e.NewTime <= e.OldTime)
                return;

            Utils.socialNetworkStrings.Clear();
            Utils.npcStrings.Clear();

            string[] postKeys = new string[] {
                $"SocialNetwork_Time_{e.NewTime}",
                $"SocialNetwork_DateTime_{Game1.Date.Season}_{Game1.Date.DayOfMonth}_{e.NewTime}",
            };
            int posts = Utils.AddPosts(postKeys);

            if(Game1.random.NextDouble() <= Config.TimeOfDayChangePostChance)
            {
                string[] friends = Game1.player.friendshipData.FieldDict.Keys.ToArray();
                string friend = friends[Game1.random.Next(friends.Length)];
                Utils.GetRandomPost(friend);
            }

            if (posts != 0)
            {

            }
        }

        public static void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            Utils.GetOvernightPosts();
        }

        public static void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            ModEntry.api = Helper.ModRegistry.GetApi<IMobilePhoneApi>("aedenthorn.MobilePhone");
            if (ModEntry.api != null)
            {
                Texture2D appIcon = Helper.Content.Load<Texture2D>(Path.Combine("assets", "app_icon.png"));
                bool success = ModEntry.api.AddApp(Helper.ModRegistry.ModID, "Social Network", ModEntry.OpenFeed, appIcon);
                Monitor.Log($"loaded app successfully: {success}", LogLevel.Debug);
                Utils.MakeTextures();
            }
        }

        public static void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!ModEntry.api.GetAppRunning() || !ModEntry.api.GetPhoneOpened() || !(Game1.activeClickableMenu is SocialNetworkMenu))
                return;
            if(e.Button == SButton.MouseLeft && ModEntry.api.GetPhoneRectangle().Contains(Game1.getMousePosition()))
            {
                ModEntry.dragging = true;
                ModEntry.lastMousePosition = Game1.getMousePosition();
            }
        }
        public static void Input_ButtonReleased(object sender, ButtonReleasedEventArgs e)
        {
            if (e.Button == SButton.MouseLeft)
            {
                ModEntry.dragging = false;
            }
        }
        public static void Display_RenderingActiveMenu(object sender, StardewModdingAPI.Events.RenderingActiveMenuEventArgs e)
        {
            if (!ModEntry.api.GetAppRunning() || !ModEntry.api.GetPhoneOpened() || !(Game1.activeClickableMenu is SocialNetworkMenu))
            {
                ModEntry.api.SetAppRunning(false);
                ModEntry.api.SetPhoneOpened(false);
                Helper.Events.Display.RenderingActiveMenu -= Display_RenderingActiveMenu;
                Helper.Events.Input.ButtonPressed -= Input_ButtonPressed;
                return;
            }
            e.SpriteBatch.Draw(ModEntry.backgroundTexture, ModEntry.api.GetScreenPosition(), Color.White);

            if (Helper.Input.IsDown(SButton.MouseLeft) && ModEntry.dragging)
            {
                Point mousePos = Game1.getMousePosition();
                if (mousePos.Y != ModEntry.lastMousePosition.Y)
                {
                    ModEntry.yOffset += ModEntry.lastMousePosition.Y - mousePos.Y;
                    ModEntry.yOffset = (int)Math.Max(0, Math.Min(ModEntry.yOffset, Utils.GetPostListHeight() - ModEntry.api.GetScreenSize().Y));
                    ModEntry.lastMousePosition = mousePos;
                }
            }


            for (int i = 0; i < ModEntry.postList.Count; i++)
            {
                SocialPost post = ModEntry.postList[i];
                Vector2 postPos = Utils.GetPostPos(i);
                int currentY = 0;
                Vector2 screenPos = ModEntry.api.GetScreenPosition();
                Vector2 screenSize = ModEntry.api.GetScreenSize();
                int screenTop = (int)screenPos.Y;
                int screenBottom = ModEntry.api.GetScreenRectangle().Bottom;
                if (postPos.Y <= screenTop - post.postHeight)
                    continue;
                if (postPos.Y >= screenBottom)
                    continue;
                int topCut = 0;
                if (postPos.Y < screenTop)
                    topCut = screenTop - (int)postPos.Y;
                int bottomCut = 0;
                if (postPos.Y + post.postHeight > screenBottom)
                    bottomCut = (int)postPos.Y + post.postHeight- screenBottom;

                e.SpriteBatch.Draw(ModEntry.postBackgroundTexture, new Rectangle((int)postPos.X, (int)Math.Max(screenTop, postPos.Y), (int)screenSize.X - Config.PostMarginX * 2, post.postHeight - bottomCut - topCut), Color.White);

                Vector2 npcPos = postPos + new Vector2(8, 0);
                Rectangle r = post.sourceRect;
                if (npcPos.Y >= screenPos.Y - r.Height * 2 && npcPos.Y < screenBottom)
                {
                    Rectangle NPCRect = r;
                    int cutTop = 0;
                    int cutBottom = 0;
                    if (npcPos.Y < screenPos.Y)
                    {
                        cutTop = (int)Math.Round((screenPos.Y - (int)npcPos.Y) / 2f);
                        NPCRect = new Rectangle(r.X, r.Y + cutTop, r.Width, r.Height - cutTop);
                        npcPos = new Vector2(npcPos.X, screenPos.Y);
                    }
                    else if (npcPos.Y > screenBottom - r.Height * 2)
                    {
                        cutBottom = (int)Math.Round((screenBottom - r.Height * 2 - (int)npcPos.Y) / 2f);
                        NPCRect = new Rectangle(r.X, r.Y, r.Width, r.Height + cutBottom);
                    }
                    e.SpriteBatch.Draw(post.portrait, npcPos, NPCRect, Color.White, 0, Vector2.Zero, 2, SpriteEffects.None, 0.86f);
                }


                for(int j = 0; j < post.lines.Count; j++)
                {
                    Vector2 linePos = postPos + new Vector2(48, (j + 1) * 20);
                    if (linePos.Y < screenTop)
                        continue;
                    if (linePos.Y > screenBottom - 15)
                        break;
                    e.SpriteBatch.DrawString(Game1.tinyFont, post.lines[j], linePos, Color.Black, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0.86f);
                }
                currentY += (post.lines.Count + 1) * 20;

                if (post.picture != null)
                {
                    Vector2 picPos = postPos + new Vector2(0, currentY);
                    Rectangle pictureRect = new Rectangle(0, 0, post.picture.Width, post.picture.Height);
                    Rectangle picRect = pictureRect;
                    float scale = Utils.GetPictureHeight(post.picture) / post.picture.Height;
                    int cutTop = 0;
                    int cutBottom = 0;
                    if (picPos.Y < screenPos.Y)
                    {
                        cutTop = (int)Math.Round((screenPos.Y - (int)picPos.Y) / scale);
                        picRect = new Rectangle(pictureRect.X, pictureRect.Y + cutTop, pictureRect.Width, pictureRect.Height - cutTop);
                        picPos = new Vector2(picPos.X, screenPos.Y);
                    }
                    else if (picPos.Y > screenBottom - pictureRect.Height * scale)
                    {
                        cutBottom = (int)Math.Round((screenBottom - pictureRect.Height * scale - (int)picPos.Y) / scale);
                        picRect = new Rectangle(pictureRect.X, pictureRect.Y, pictureRect.Width, pictureRect.Height + cutBottom);
                    }
                    e.SpriteBatch.Draw(post.picture, new Rectangle((int)picPos.X, (int)picPos.Y, (int)screenSize.X - Config.PostMarginX * 2, (int)(picRect.Height * scale)), picRect, Color.White);

                    currentY += Utils.GetPictureHeight(post.picture);
                }

                if (post.postReactions != null && post.postReactions.Count > 0)
                {
                    Vector2 likePos = postPos + new Vector2(4, currentY);
                    if (likePos.Y >= screenTop && likePos.Y < screenBottom - 20)
                    {
                        for (int j = 0; j < post.postReactions.Count; j++)
                        {
                            SocialPostReaction reaction = post.postReactions[j];
                            e.SpriteBatch.Draw(reaction.portrait, likePos + new Vector2(j * 28, 0), reaction.sourceRect, Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 0.86f);
                            //e.SpriteBatch.DrawString(Game1.tinyFont, post.lines[j], likePos + new Vector2(j * 28, 0), Color.Black, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0.86f);
                        }
                        e.SpriteBatch.DrawString(Game1.tinyFont, Utils.GetSocialNetworkString(null, "reacted", false), likePos + new Vector2(28 * (post.postReactions.Count + 1), 0), Color.Black, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0.86f);
                    }
                    currentY += 20;
                }
                if (post.postComments != null && post.postComments.Count > 0)
                {
                    for (int j = 0; j < post.postComments.Count; j++)
                    {
                        Vector2 commentPos = postPos + new Vector2(4, currentY + 10);
                        SocialPostReaction comment = post.postComments[j];
                        if (commentPos.Y >= screenTop && commentPos.Y <= screenBottom - 15)
                        {
                            e.SpriteBatch.Draw(comment.portrait, commentPos, comment.sourceRect, Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 0.86f);
                        }

                        for (int k = 0; k < comment.lines.Count; k++)
                        {
                            Vector2 linePos = postPos + new Vector2(32, currentY + 20);
                            if (linePos.Y < screenTop)
                                continue;
                            if (linePos.Y > screenBottom - 15)
                                break;
                            e.SpriteBatch.DrawString(Game1.tinyFont, comment.lines[k], linePos, Color.Black, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0.86f);
                            currentY += 20;
                        }
                    }
                }
            }
        }
    }
}
