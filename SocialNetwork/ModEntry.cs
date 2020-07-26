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
    public class ModEntry : Mod
    {
        public static ModEntry context;

        internal static ModConfig Config;
        private Random myRand;

        private IMobilePhoneApi api;
        private int topPost;
        private Texture2D backgroundTexture;
        private Texture2D postBackgroundTexture;
        private List<SocialPost> postList = new List<SocialPost>();
        private int postsPerPage = 10;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;

            myRand = new Random(Guid.NewGuid().GetHashCode());
            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
        }

        public void MakeTextures()
        {
            Vector2 screenSize = api.GetScreenSize();
            Texture2D texture = new Texture2D(Game1.graphics.GraphicsDevice, (int)screenSize.X, (int)screenSize.Y);
            Color[] data = new Color[texture.Width * texture.Height];
            for (int pixel = 0; pixel < data.Length; pixel++)
            {
                data[pixel] = Config.BackgroundColor;
            }
            texture.SetData(data);
            backgroundTexture = texture;

            texture = new Texture2D(Game1.graphics.GraphicsDevice, (int)screenSize.X - Config.PostMarginX * 2, Config.PostHeight);
            data = new Color[texture.Width * texture.Height];
            for (int pixel = 0; pixel < data.Length; pixel++)
            {
                data[pixel] = Config.PostBackgroundColor;
            }
            texture.SetData(data);
            postBackgroundTexture = texture;
        }
        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            api = Helper.ModRegistry.GetApi<IMobilePhoneApi>("aedenthorn.MobilePhone");
            if (api != null)
            {
                Texture2D appIcon = Helper.Content.Load<Texture2D>(Path.Combine("assets", "app_icon.png"));
                bool success = api.AddApp(Helper.ModRegistry.ModID, "Social Network", OpenFeed, appIcon);
                Monitor.Log($"loaded app successfully: {success}", LogLevel.Debug);
                MakeTextures();
            }
        }

        private void OpenFeed()
        {
            api.SetAppRunning(true);
            Game1.activeClickableMenu = new SocialNetworkMenu();
            topPost = 0;
            GetOvernightPosts();
            Helper.Events.Display.RenderingActiveMenu += Display_RenderingActiveMenu;
            //Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
        }
        private void GetOvernightPosts()
        {
            string postKey = $"SocialNetwork_{Game1.Date.Season}_{Game1.Date.DayOfMonth}";
            postList.Clear();
            foreach (KeyValuePair<string, Netcode.NetRef<Friendship>> kvp in Game1.player.friendshipData.FieldDict)
            {
                IDictionary<string, string> data = null;
                try
                {
                    data = Helper.Content.Load<Dictionary<string, string>>($"Characters\\Dialogue\\{kvp.Key}", ContentSource.GameContent);
                }
                catch
                {
                    continue;
                }

                if (data == null)
                    continue;

                if (data.ContainsKey(postKey))
                {
                    NPC npc = Game1.getCharacterFromName(kvp.Key);
                    Texture2D portrait = npc.Sprite.Texture;
                    Rectangle sourceRect = npc.getMugShotSourceRect();
                    postList.Add(new SocialPost(npc, portrait, sourceRect, data[postKey]));
                }
            }
            var haley = Game1.getCharacterFromName("Haley");
            postList.Add(new SocialPost(haley, haley.Sprite.Texture, haley.getMugShotSourceRect(), "Hey, look at this picture I took in the forest!"));
            var emily = Game1.getCharacterFromName("Emily");
            postList.Add(new SocialPost(emily, emily.Sprite.Texture, emily.getMugShotSourceRect(), "Has anyone seen Haley? She went into the forest hours ago and hasn't returned!"));
            postList = postList.OrderBy(a => myRand.NextDouble()).ToList();
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Display_RenderingActiveMenu(object sender, StardewModdingAPI.Events.RenderingActiveMenuEventArgs e)
        {
            if (!api.GetAppRunning() || !api.GetPhoneOpened() || !(Game1.activeClickableMenu is SocialNetworkMenu))
            {
                api.SetAppRunning(false);
                api.SetPhoneOpened(false);
                Helper.Events.Display.RenderingActiveMenu -= Display_RenderingActiveMenu;
                Helper.Events.Input.ButtonPressed -= Input_ButtonPressed;
                return;
            }
            e.SpriteBatch.Draw(backgroundTexture, api.GetScreenPosition(), Color.White);

            if (topPost > 0)
            {
                //e.SpriteBatch.Draw(upArrowTexture, upArrowPosition, Color.White);
            }
            if (postList.Count - 1 > topPost)
            {
                //e.SpriteBatch.Draw(downArrowTexture, downArrowPosition, Color.White);
            }

            postsPerPage = (int)(api.GetScreenSize().Y - Config.PostMarginY) / (Config.PostHeight + Config.PostMarginY);

            for (int i = 0; i < postList.Count; i++)
            {
                if (i < topPost || i - topPost >= postsPerPage)
                    continue;

                Vector2 postPos = GetPostPos(i);
                e.SpriteBatch.Draw(postBackgroundTexture, postPos, Color.White);
                e.SpriteBatch.Draw(postList[i].portrait, postPos + new Vector2(8, 0), postList[i].sourceRect, Color.White, 0, Vector2.Zero, 2, SpriteEffects.None, 0.86f);
                string[] lines = GetTextLines(postList[i].text);
                for(int j = 0; j < lines.Length; j++)
                {
                    e.SpriteBatch.DrawString(Game1.tinyFont, lines[j], postPos + new Vector2(48, (j + 1) * 20), Color.Black, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0.86f);
                }
            }
        }

        private string[] GetTextLines(string text)
        {
            List<string> lines = new List<string>();
            int lineLength = ((int)api.GetScreenSize().X - Config.PostMarginX * 2 - 64) / 9;
            if(text.Length >= lineLength * 3)
                text = text.Substring(0, lineLength * 3 - 4) + "...";

            var words = text.Split(' ');
            var line = "";
            for (int i = 0; i < words.Length; i++)
            {
                line += (line.Length == 0 ? "" : " ") + words[i];
                if (line.Length + (i == words.Length - 1 ? 0 : words[i+1].Length) >= lineLength)
                {
                    lines.Add(line);
                    line = "";
                }
            }
            if (line.Length > 0)
                lines.Add(line);
            return lines.ToArray();
        }

        private Vector2 GetPostPos(int i)
        {
            return new Vector2(api.GetScreenPosition().X + Config.PostMarginX, api.GetScreenPosition().Y + Config.PostMarginY + (Config.PostMarginY + Config.PostHeight) * (i - topPost));
        }
    }
}
