using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System.Collections.Generic;
using System.Linq;

namespace StardewImpact
{
    public partial class ModEntry
    {
        public static void LoadTextures(string name)
        {

            if (string.IsNullOrEmpty(characterDict[name].PortraitPath))
            {
                characterDict[name].Portrait = MakePortrait(Game1.content.Load<Texture2D>("Portraits\\" + name));
            }
            else
            {
                characterDict[name].Portrait = Game1.content.Load<Texture2D>(characterDict[name].PortraitPath);
            }
            if (string.IsNullOrEmpty(characterDict[name].SpritePath))
            {
                characterDict[name].Sprite = Game1.content.Load<Texture2D>("Characters\\" + name);
            }
            else
            {
                characterDict[name].Sprite = Game1.content.Load<Texture2D>(characterDict[name].SpritePath);
            }
            if (string.IsNullOrEmpty(characterDict[name].SkillIconPath))
            {
                characterDict[name].SkillIcon = defaultSkillIcon;
            }
            else
            {
                characterDict[name].SkillIcon = Game1.content.Load<Texture2D>(characterDict[name].SkillIconPath);
            }
            if (string.IsNullOrEmpty(characterDict[name].BurstIconPath))
            {
                characterDict[name].BurstIcon = defaultBurstIcon;
            }
            else
            {
                characterDict[name].BurstIcon = Game1.content.Load<Texture2D>(characterDict[name].BurstIconPath);
            }
        }
        public static Texture2D MakePortrait(Texture2D texture)
        {
            Color[] data = new Color[texture.Width * texture.Height];
            Color[] data2 = new Color[64 * 64];
            texture.GetData(data);
            Vector2 middle = new Vector2(32, 32);
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    Vector2 pos = new Vector2(x, y);
                    if (Vector2.Distance(pos, middle) > (middle.X + middle.Y) / 2 - 1)
                    {
                        data2[y * 64 + x] = Color.Transparent;
                    }
                    else
                    {
                        data2[y * 64 + x] = data[y * texture.Width + x];
                    }
                }
            }
            Texture2D newTexture = new Texture2D(Game1.graphics.GraphicsDevice, 64, 64);
            newTexture.SetData(data2);
            return newTexture;
        }

        public static List<Rectangle> GetCharacterRectangles()
        {
            List<Rectangle> result = new List<Rectangle>();
            float scale = Config.PortraitScale;
            float spacing = Config.PortraitSpacing;
            Vector2 start = new Vector2(Game1.viewport.Width - frameTexture.Width * scale, Game1.viewport.Height / 2 - (frameTexture.Height * 2 + spacing * 1.5f) * scale);
            for (int i = 1; i < 5; i++)
            {
                if (GetAvailableCharacters().Count < i)
                    return result;
                result.Add(new Rectangle(Utility.Vector2ToPoint(start + new Vector2(-spacing, (i - 1) * (frameTexture.Height * scale + spacing))), new Point((int)(frameTexture.Width * scale), (int)(frameTexture.Height * scale))));
            }
            return result;
        }

        public static Dictionary<string, CharacterData> GetAvailableCharacters()
        {
            Dictionary<string, CharacterData> result = new Dictionary<string, CharacterData>();
            foreach(var kvp in characterDict)
            {
                if (Game1.player.friendshipData.TryGetValue(kvp.Key, out Friendship f) && f.Points >= Config.MinPoints)
                {
                    result.Add(kvp.Key, kvp.Value);
                }
            }
            return result;
        }

        public static CharacterData GetCurrentCharacter()
        {
            if (!Game1.player.modData.TryGetValue(currentSlotKey, out string key) || !Game1.player.modData.TryGetValue(slotPrefix + key, out string name) || !GetAvailableCharacters().TryGetValue(name, out CharacterData data))
                return null;
            return data;
        }
        private void PressedButton(int slot, bool set)
        {
            if (!set)
            {
                Monitor.Log($"Removing character in slot {slot}");
                Game1.player.modData.Remove(slotPrefix + slot);
                return;
            }
            var names = GetAvailableCharacters().Keys.ToList();
            names.Sort();
            if (!Game1.player.modData.TryGetValue(slotPrefix + slot, out string name) || !names.Contains(name))
            {
                name = names[names.Count - 1];
            }
            for (int i = 0; i < characterDict.Count; i++)
            {
                name = names[(names.IndexOf(name) + 1) % names.Count];

                bool isFree = true;
                for (int j = 1; j < 5; j++)
                {
                    if (Game1.player.modData.TryGetValue(slotPrefix + j, out string n) && n == name)
                    {
                        isFree = false;
                        break;
                    }
                }
                if (isFree)
                {
                    Game1.currentLocation.playSound("bigSelect");
                    Game1.player.modData[slotPrefix + slot] = name;
                    Monitor.Log($"Set slot {slot} to {name}");
                    return;
                }
            }
        }
    }
}