using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SocialNetwork
{
    public class HelperEvents
    {
        internal static IModHelper Helper;
        internal static IMonitor Monitor;
        internal static ModConfig Config;
        private static Rectangle lastScreenRect = new Rectangle();

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

            foreach(KeyValuePair<string,Dictionary<string,string>> kvp in ModEntry.todaysPosts)
            {
                foreach(string postKey in postKeys)
                {
                    if (kvp.Value.ContainsKey(postKey))
                    {
                        NPC npc = Game1.getCharacterFromName(kvp.Key);
                        Texture2D portrait = npc.Sprite.Texture;
                        Rectangle sourceRect = npc.getMugShotSourceRect();
                        ModEntry.postList.Add(new SocialPost(npc, portrait, sourceRect, kvp.Value[postKey]));
                        Monitor.Log($"added {postKey} post from {npc.displayName}");
                    }
                }
            }

            if(Game1.random.NextDouble() <= Config.TimeOfDayChangePostChance)
            {
                string[] friends = Game1.player.friendshipData.FieldDict.Keys.ToArray();
                string friend = friends[Game1.random.Next(friends.Length)];
                Utils.GetRandomPost(friend);
            }
        }

        public static void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            Utils.GetDailyPosts();
        }

        public static void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            ModEntry.api = Helper.ModRegistry.GetApi<IMobilePhoneApi>("aedenthorn.MobilePhone");
            if (ModEntry.api != null)
            {
                Texture2D appIcon = Helper.Content.Load<Texture2D>(Path.Combine("assets", "app_icon.png"));
                bool success = ModEntry.api.AddApp(Helper.ModRegistry.ModID, SHelper.Translation.Get("Mod.App.Name"), ModEntry.OpenFeed, appIcon);
                Monitor.Log($"loaded app successfully: {success}", LogLevel.Debug);
                Utils.MakeTextures();
            }
        }

        public static void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!ModEntry.api.GetAppRunning() || !ModEntry.api.GetPhoneOpened() || !(Game1.activeClickableMenu is SocialNetworkMenu))
                return;
            if(e.Button == SButton.MouseLeft)
            {
                if (ModEntry.api.GetScreenRectangle().Contains(Game1.getMousePosition()))
                {
                    ModEntry.dragging = true;
                    ModEntry.lastMousePosition = Game1.getMousePosition();
                }
                else if (!ModEntry.api.GetPhoneRectangle().Contains(Game1.getMousePosition()))
                {
                    Game1.activeClickableMenu = null;
                    ModEntry.api.SetAppRunning(false);
                    ModEntry.api.SetRunningApp(null);
                    Helper.Input.Suppress(SButton.MouseLeft);
                }
            }
        }
        public static void Input_ButtonReleased(object sender, ButtonReleasedEventArgs e)
        {
            if (e.Button == SButton.MouseLeft)
            {
                ModEntry.dragging = false;
            }
        }
        public static void Display_RenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            if (!ModEntry.api.GetAppRunning() || !ModEntry.api.GetPhoneOpened() || !(Game1.activeClickableMenu is SocialNetworkMenu))
            {
                if(ModEntry.api.GetRunningApp() == Helper.ModRegistry.ModID)
                {
                    ModEntry.api.SetAppRunning(false);
                    ModEntry.api.SetRunningApp(null);
                }
                if (Game1.activeClickableMenu is SocialNetworkMenu)
                    Game1.activeClickableMenu = null;

                Helper.Events.Display.RenderedWorld -= Display_RenderedWorld;
                Helper.Events.Input.ButtonPressed -= Input_ButtonPressed;
                return;
            }
            bool refresh = false;
            if(lastScreenRect != ModEntry.api.GetScreenRectangle())
            {
                lastScreenRect = ModEntry.api.GetScreenRectangle();
                refresh = true;
            }

            e.SpriteBatch.Draw(ModEntry.backgroundTexture, lastScreenRect, Color.White);

            if (Helper.Input.IsDown(SButton.MouseLeft) && ModEntry.dragging)
            {
                Point mousePos = Game1.getMousePosition();
                if (mousePos.Y != ModEntry.lastMousePosition.Y)
                {
                    ModEntry.yOffset += ModEntry.lastMousePosition.Y - mousePos.Y;
                    ModEntry.yOffset = (int)Math.Max(0, Math.Min(ModEntry.yOffset, Utils.GetPostListHeight(refresh) - ModEntry.api.GetScreenSize().Y));
                    ModEntry.lastMousePosition = mousePos;
                }
            }


            for (int i = 0; i < ModEntry.postList.Count; i++)
            {
                SocialPost post = ModEntry.postList[i];
                if (refresh)
                    post.Refresh();
                int postHeight = post.postHeight;
                Vector2 postPos = Utils.GetPostPos(i);
                int currentY = 0;
                Vector2 screenPos = ModEntry.api.GetScreenPosition();
                Vector2 screenSize = ModEntry.api.GetScreenSize();
                int screenTop = (int)screenPos.Y;
                int screenBottom = lastScreenRect.Bottom;
                if (postPos.Y <= screenTop - postHeight)
                    continue;
                if (postPos.Y >= screenBottom)
                    continue;
                int topCut = 0;
                if (postPos.Y < screenTop)
                    topCut = screenTop - (int)postPos.Y;
                int bottomCut = 0;
                if (postPos.Y + postHeight > screenBottom)
                    bottomCut = (int)postPos.Y + postHeight- screenBottom;

                e.SpriteBatch.Draw(ModEntry.postBackgroundTexture, new Rectangle((int)postPos.X, (int)Math.Max(screenTop, postPos.Y), (int)screenSize.X - Config.PostMarginX * 2, postHeight - bottomCut - topCut), Color.White);

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

                List<string> lines = post.lines;
                for (int j = 0; j < lines.Count; j++)
                {
                    Vector2 linePos = postPos + new Vector2(48, (j + 1) * 20);
                    if (linePos.Y < screenTop)
                        continue;
                    if (linePos.Y > screenBottom - 15)
                        break;
                    e.SpriteBatch.DrawString(Game1.tinyFont, lines[j], linePos, Color.Black, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0.86f);
                }
                currentY += (lines.Count + 1) * 20;

                if (post.picture != null)
                {
                    Vector2 picPos = postPos + new Vector2(0, currentY);
                    Vector2 picPosTemp = postPos + new Vector2(0, currentY);
                    Rectangle pictureRect = new Rectangle(0, 0, post.picture.Width, post.picture.Height);
                    Rectangle picRect = pictureRect;
                    float scale = Utils.GetPictureHeight(post.picture) / post.picture.Height;
                    int cutTop = 0;
                    int cutBottom = 0;
                    if (picPos.Y < screenPos.Y)
                    {
                        cutTop = (int)Math.Round((screenPos.Y - (int)picPos.Y) / scale);
                        picPosTemp = new Vector2(picPos.X, screenPos.Y);
                    }
                    if (picPos.Y > screenBottom - pictureRect.Height * scale)
                    {
                        cutBottom = (int)Math.Round((screenBottom - pictureRect.Height * scale - (int)picPos.Y) / scale);
                    }
                    picPos = picPosTemp;
                    picRect = new Rectangle(pictureRect.X, pictureRect.Y + cutTop, pictureRect.Width, pictureRect.Height - cutTop + cutBottom);

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
                        //e.SpriteBatch.DrawString(Game1.tinyFont, Utils.GetSocialNetworkString(null, "reacted", false), likePos + new Vector2(20 * (post.postReactions.Count), 8), Color.Black, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0.86f);
                    }
                    currentY += 20;
                }
                if (post.postComments != null && post.postComments.Count > 0)
                {
                    for (int j = 0; j < post.postComments.Count; j++)
                    {
                        Vector2 commentPos = postPos + new Vector2(4, currentY + 10);
                        SocialPostReaction comment = post.postComments[j];
                        List<string> clines = comment.lines;
                        if (commentPos.Y >= screenTop && commentPos.Y <= screenBottom - 15)
                        {
                            e.SpriteBatch.Draw(comment.portrait, commentPos, comment.sourceRect, Color.White, 0, Vector2.Zero, 1f, SpriteEffects.None, 0.86f);
                        }

                        for (int k = 0; k < clines.Count; k++)
                        {
                            Vector2 linePos = postPos + new Vector2(32, currentY + 20);
                            if (linePos.Y < screenTop)
                                continue;
                            if (linePos.Y > screenBottom - 15)
                                break;
                            e.SpriteBatch.DrawString(Game1.tinyFont, clines[k], linePos, Color.Black, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0.86f);
                            currentY += 20;
                        }
                    }
                }
            }
        }
    }
}
