using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace RightToLeft
{
    public partial class ModEntry
    {
        private void ReloadFonts()
        {
            foreach(var key in languageDict.Keys.ToArray())
            {
                try
                {
                    languageDict[key].dialogueFont = MakeFont(languageDict[key],  "dialogueFont", languageDict[key].dialogueFontLineSpacing, languageDict[key].dialogueFontSpacing);
                    languageDict[key].smallFont = MakeFont(languageDict[key], "smallFont", languageDict[key].smallFontLineSpacing, languageDict[key].smallFontSpacing);
                    languageDict[key].tinyFont = MakeFont(languageDict[key], "tinyFont", languageDict[key].tinyFontLineSpacing, languageDict[key].tinyFontSpacing);
                }
                catch (Exception ex)
                {
                    Monitor.Log($"Error loading language {key}:\n\n{ex}", StardewModdingAPI.LogLevel.Error);
                }
            }
        }

        private SpriteFont MakeFont(LanguageInfo info, string name, int lineSpacing, float spacing)
        {
            Monitor.Log($"Making font {name}");
            var fontTexture = Helper.ModContent.Load<Texture2D>($"{info.path}/{name}_0.png");
            var fontMap = new Dictionary<string, Mapping>();
            XmlReader xmlReader = XmlReader.Create(Path.Combine(Helper.DirectoryPath, info.path, $"{name}.fnt"));
            Monitor.Log("reading xml font");
            char defaultChar = info.defaultCharacter;
            while (xmlReader.Read())
            {
                if (xmlReader.NodeType != XmlNodeType.Element)
                    continue;
                try
                {
                    if (xmlReader.Name.Equals("common"))
                    {
                        lineSpacing = int.Parse(xmlReader.GetAttribute("lineHeight"));
                    }
                    else if (xmlReader.Name.Equals("char"))
                    {
                        fontMap[xmlReader.GetAttribute("id")] = new Mapping()
                        {
                            x = int.Parse(xmlReader.GetAttribute("x")),
                            y = int.Parse(xmlReader.GetAttribute("y")),
                            width = int.Parse(xmlReader.GetAttribute("width")),
                            height = int.Parse(xmlReader.GetAttribute("height")),
                            xo = int.Parse(xmlReader.GetAttribute("xoffset")),
                            yo = int.Parse(xmlReader.GetAttribute("yoffset")),
                            xa = int.Parse(xmlReader.GetAttribute("xadvance")),
                        };
                    }

                }
                catch
                {

                }
            }
            /*
            var fontObject = new SpriteFontMapping()
            {
                DefaultCharacter = info.defaultCharacter,
                LineSpacing = lineSpacing,
                Spacing = spacing
            };
            */
            var glyphs = new List<Rectangle>();
            var cropping = new List<Rectangle>();
            var charMap = new List<char>();
            var kerning = new List<Vector3>();
            Mapping last = new Mapping();
            foreach (var m in fontMap)
            {
                var ch = (char)int.Parse(Convert.ToString(int.Parse(m.Key), 16), NumberStyles.HexNumber);
                var glyph = new Rectangle(m.Value.x, m.Value.y, m.Value.width, m.Value.height);
                var crop = new Rectangle(info.xOffset + m.Value.xo, m.Value.yo, m.Value.width, m.Value.height);
                var kern = new Vector3(0, m.Value.width, info.useXAdvance ? m.Value.xa - m.Value.width : 0);
                /*
                fontObject.Characters.Add(ch);
                fontObject.Glyphs.Add(ch, new SpriteFont.Glyph()
                {
                    BoundsInTexture = glyph,
                    Cropping = crop,
                    Character = ch,
                    LeftSideBearing = kern.X,
                    RightSideBearing = kern.Z,
                    Width = kern.Y,
                    WidthIncludingBearings = kern.X + kern.Y + kern.Z
                });
                */
                glyphs.Add(glyph);
                cropping.Add(crop);
                charMap.Add(ch);
                kerning.Add(kern);
                last = m.Value;
            }
            //File.WriteAllText(Path.Combine(Helper.DirectoryPath, "assets", "SpriteFont1.ar.json"), JsonConvert.SerializeObject(fontObject, Newtonsoft.Json.Formatting.Indented));
            return new SpriteFont(fontTexture, glyphs, cropping, charMap, lineSpacing, spacing, kerning, defaultChar);
            //var spriteFont = SHelper.GameContent.Load<SpriteFont>("Fonts/SpriteFont1.he-HE");
        }
        private static void FixForRTL(ref SpriteFont spriteFont, ref string text, ref Vector2 position)
        {
            if (!Config.ModEnabled || text?.Length == 0 || LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.mod || !languageDict.ContainsKey(LocalizedContentManager.CurrentModLanguage.LanguageCode) || Regex.IsMatch(text, @"[a-zA-Z0-9]", RegexOptions.Compiled))
                return;

            string inter = "";
            for (int i = text.Length - 1; i >= 0; i--)
            {
                inter += text[i];
            }
            text = inter;
        }
        private static void FixForRTL(ref SpriteFont spriteFont, ref StringBuilder text, ref Vector2 position)
        {
            if (!Config.ModEnabled || text?.Length == 0 || LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.mod || !languageDict.ContainsKey(LocalizedContentManager.CurrentModLanguage.LanguageCode) || Regex.IsMatch(text.ToString(), @"[a-zA-Z0-9]", RegexOptions.Compiled))
                return;
            StringBuilder inter = new StringBuilder();
            for (int i = text.Length - 1; i >= 0; i--)
            {
                inter.Append(text[i]);
            }
            text = inter;
        }

    }
}