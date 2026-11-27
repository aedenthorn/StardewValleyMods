using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace SocialNetwork
{
    public class Utils
    {

        public static ModConfig Config;
        internal static IMonitor Monitor;
        public static IModHelper Helper;

        public static Dictionary<string,Dictionary<string,string>> npcStrings = new Dictionary<string, Dictionary<string, string>>();
        public static Dictionary<string, string> socialNetworkStrings = new Dictionary<string, string>();

        public static void GetRandomPost(string friend)
        {
            IDictionary<string, string> data;
            try
            {
                data = Helper.Content.Load<Dictionary<string, string>>($"Characters\\Dialogue\\{friend}", ContentSource.GameContent);
            }
            catch
            {
                return;
            }
            if (data == null)
                return;

            string NPCLikes;
            Game1.NPCGiftTastes.TryGetValue(friend, out NPCLikes);
            if (NPCLikes == null)
                return;
            string[] likes = NPCLikes.Split('/')[1].Split(' ');
            if (likes.Length == 0)
                return;
            List<Object> foods = new List<Object>();
            List<Object> favs = new List<Object>();
            foreach(string str in likes)
            {
                if (!int.TryParse(str, out int idx))
                    continue;
                var obj = new Object(idx, 1, false, -1, 0);
                if (obj.Type == "Cooking")
                    foods.Add(obj);
                else
                    favs.Add(obj);
            }

            if(foods.Count > 0)
            {
                string itemName = foods[Game1.random.Next(foods.Count)].DisplayName;
                IEnumerable<KeyValuePair<string, string>> strings = data.Where(s => s.Key.StartsWith("SocialNetwork_Food_"));

                if (strings.Count() > 0)
                {
                    string str = strings.ElementAt(Game1.random.Next(strings.Count())).Value;
                    str = str.Replace("{0}", itemName);
                    NPC npc = Game1.getCharacterFromName(friend);
                    Texture2D portrait = npc.Sprite.Texture;
                    Rectangle sourceRect = npc.getMugShotSourceRect();
                    ModEntry.postList.Add(new SocialPost(npc, portrait, sourceRect, str));
                    Monitor.Log($"added food post from {npc.displayName}");
                    return;
                }
            }
            if(favs.Count > 0)
            {
                string itemName = favs[Game1.random.Next(favs.Count)].DisplayName;
                IEnumerable<KeyValuePair<string, string>> strings = data.Where(s => s.Key.StartsWith("SocialNetwork_Favorite_"));
                if (strings.Count() > 0)
                {
                    string str = strings.ElementAt(Game1.random.Next(strings.Count())).Value;
                    str = str.Replace("{0}", itemName);
                    NPC npc = Game1.getCharacterFromName(friend);
                    Texture2D portrait = npc.Sprite.Texture;
                    Rectangle sourceRect = npc.getMugShotSourceRect();
                    ModEntry.postList.Add(new SocialPost(npc, portrait, sourceRect, str));
                    Monitor.Log($"added favorite post from {npc.displayName}");
                    return;
                }
            }
        }

        public static int GetPictureHeight(Texture2D picture)
        {
            return  (int)(picture.Height * (ModEntry.api.GetScreenSize().X - Config.PostMarginX * 2) / (float) picture.Width);
        }

        public static string GetSocialNetworkString(string name, string key, bool npcSpecific)
        {
            IDictionary<string, string> data;
            try
            {
                if (npcSpecific)
                {
                    if (npcStrings.ContainsKey(name) && npcStrings[name].ContainsKey(key))
                        return npcStrings[name][key];
                    data = Helper.Content.Load<Dictionary<string, string>>($"Characters\\Dialogue\\{name}", ContentSource.GameContent);
                    if (data.ContainsKey(key))
                    {
                        string str = data[key];
                        if (!npcStrings.ContainsKey(name))
                            npcStrings.Add(name, new Dictionary<string, string>() { { key, str } });
                        else
                            npcStrings[name].Add(key, str);
                        return str;
                    }
                    else
                        return null;
                }
                else
                {
                    if (socialNetworkStrings.ContainsKey(key))
                        return socialNetworkStrings[key];
                    data = Helper.Content.Load<Dictionary<string, string>>($"SocialNetworkStrings", ContentSource.GameContent);
                    string str = data[key];
                    socialNetworkStrings.Add(key, str);
                    return str;
                }
            }
            catch
            {
                return null;
            }
        }

        public static Texture2D GetPicture(string val)
        {
            return Helper.Content.Load<Texture2D>(val, ContentSource.GameContent);
        }

        public static void MakeTextures()
        {
            Vector2 screenSize = ModEntry.api.GetScreenSize();
            Texture2D texture = new Texture2D(Game1.graphics.GraphicsDevice, (int)screenSize.X, (int)screenSize.Y);
            Color[] data = new Color[texture.Width * texture.Height];
            for (int pixel = 0; pixel < data.Length; pixel++)
            {
                data[pixel] = Config.BackgroundColor;
            }
            texture.SetData(data);
            ModEntry.backgroundTexture = texture;

            texture = new Texture2D(Game1.graphics.GraphicsDevice, (int)screenSize.X - Config.PostMarginX * 2, Config.PostHeight);
            data = new Color[texture.Width * texture.Height];
            for (int pixel = 0; pixel < data.Length; pixel++)
            {
                data[pixel] = Config.PostBackgroundColor;
            }
            texture.SetData(data);
            ModEntry.postBackgroundTexture = texture;
        }

        public static void GetDailyPosts()
        {
            string[] postKeys = new string[] {
                $"SocialNetwork_Date_{Game1.Date.Season}_{Game1.Date.DayOfMonth}",
                $"SocialNetwork_EveryDay",
            };

            ModEntry.postList.Clear();

            AddPosts(postKeys);

            ModEntry.postList = ModEntry.postList.OrderBy(a => ModEntry.myRand.NextDouble()).ToList();


            int posts = 0; 
            foreach (KeyValuePair<string, Netcode.NetRef<Friendship>> kvp in Game1.player.friendshipData.FieldDict)
            {
                Dictionary<string, string> data1;
                Dictionary<string, string> data2;
                try
                {
                    data1 = Helper.Content.Load<Dictionary<string, string>>($"Characters\\Dialogue\\{kvp.Key}", ContentSource.GameContent).Where(k => k.Key.StartsWith("SocialNetwork_Time_")).ToDictionary(s => s.Key, s => s.Value);
                    data2 = Helper.Content.Load<Dictionary<string, string>>($"Characters\\Dialogue\\{kvp.Key}", ContentSource.GameContent).Where(k => k.Key.StartsWith($"SocialNetwork_DateTime_{Game1.Date.Season}_{Game1.Date.DayOfMonth}_")).ToDictionary(s => s.Key, s => s.Value);
                }
                catch
                {
                    continue;
                }

                if ((data1 == null && data2 == null) || (data1.Count == 0 && data2.Count == 0))
                {
                    Monitor.Log($"NPC {kvp.Key} has no timed dialogues today");
                    continue;
                }

                ModEntry.todaysPosts[kvp.Key] = new Dictionary<string, string>();

                if (data1.Count > 0)
                    foreach(KeyValuePair<string,string> kvp2 in data1)
                        ModEntry.todaysPosts[kvp.Key].Add(kvp2.Key,kvp2.Value);
                if (data2.Count > 0)
                    foreach (KeyValuePair<string, string> kvp2 in data1)
                        ModEntry.todaysPosts[kvp.Key].Add(kvp2.Key, kvp2.Value);

                Monitor.Log($"NPC {kvp.Key} has {ModEntry.todaysPosts[kvp.Key].Count} timed dialogue(s) today");

            }
        }
        public static int AddPosts(string[] postKeys)
        {
            int posts = 0; 
            foreach (KeyValuePair<string, Netcode.NetRef<Friendship>> kvp in Game1.player.friendshipData.FieldDict)
            {
                IDictionary<string, string> data;
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

                foreach(string postKey in postKeys)
                {
                    if (data.ContainsKey(postKey))
                    {
                        NPC npc = Game1.getCharacterFromName(kvp.Key);
                        Texture2D portrait = npc.Sprite.Texture;
                        Rectangle sourceRect = npc.getMugShotSourceRect();
                        ModEntry.postList.Add(new SocialPost(npc, portrait, sourceRect, data[postKey]));
                        Monitor.Log($"added {postKey} post from {npc.displayName}");
                        posts++;
                    }
                }
            }
            return posts;
        }


        public static int GetPostListHeight(bool refresh)
        {
            int y = 0;
            for (int i = 0; i < ModEntry.postList.Count; i++)
            {
                if (refresh)
                    ModEntry.postList[i].Refresh();
                y += ModEntry.postList[i].postHeight + Config.PostMarginY * 2;
            }
            return y;
        }

        public static Vector2 GetPostPos(int x)
        {
            int y = Config.PostMarginY;
            for (int i = 0; i < x; i++)
            {
                y += ModEntry.postList[i].postHeight + Config.PostMarginY;
            }
            return new Vector2(ModEntry.api.GetScreenPosition().X + Config.PostMarginX, ModEntry.api.GetScreenPosition().Y + y - ModEntry.yOffset);
        }
        public static List<string> GetTextLines(string text)
        {
            List<string> lines = new List<string>();
            int lineLength = ((int)ModEntry.api.GetScreenSize().X - ModEntry.Config.PostMarginX * 2 - 64) / 9;

            var words = text.Split(' ');
            var line = "";
            for (int i = 0; i < words.Length; i++)
            {
                line += (line.Length == 0 ? "" : " ") + words[i];
                if (line.Length + (i == words.Length - 1 ? 0 : words[i + 1].Length) >= lineLength)
                {
                    lines.Add(line);
                    line = "";
                }
            }
            if (line.Length > 0)
                lines.Add(line);
            return lines;
        }
    }
}
