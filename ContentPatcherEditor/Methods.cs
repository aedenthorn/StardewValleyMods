using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;

namespace ContentPatcherEditor
{
    public partial class ModEntry
    {
        public static JObject CreateNewChange(string type)
        {
            switch (type)
            {
                case "EditData":
                    return new JObject()
                    {
                        { "Action", "EditData" },
                        { "Target", "" },
                        { "LogName", "" },
                    };
                case "EditImage":
                    return new JObject()
                    {
                        { "Action", "EditImage" },
                        { "Target", "" },
                        { "FromFile", "" },
                        { "LogName", "" },
                    };
                case "EditMap":
                    return new JObject()
                    {
                        { "Action", "EditMap" },
                        { "Target", "" },
                        { "FromFile", "" },
                        { "LogName", "" },
                    };
                case "Include":
                    return new JObject()
                    {
                        { "Action", "Include" },
                        { "FromFile", "" },
                        { "LogName", "" },
                    };
                default:
                    return new JObject()
                    {
                        { "Action", "Load" },
                        { "FromFile", "assets/" },
                        { "LogName", "" },
                        { "Target", "" }
                    };
            }
        }
        public static void CreateNewContentPatcherPack()
        {
            var manifest = new MyManifest()
            {
                Name = "New Pack",
                Description = "New Content Patcher Content Pack",
                Version = new System.Version("0.1.0"),
                Author = Game1.player.Name,
                MinimumApiVersion = new System.Version("3.15.0"),
                UniqueID = $"{Game1.player.Name}.NewContentPatcherPack",
            };
            var content = new ContentPatcherContent()
            {
                Format = "1.29.0",
                Changes = new()
            };
            int count = 0;
            while (Directory.Exists(Path.Combine(Constants.GamePath, "Mods", $"[CP] New Content Pack{(count == 0 ? "" : " " + count)}")))
            {
                count++;
            }
            var dir = Path.Combine(Constants.GamePath, "Mods", $"[CP] New Content Pack{(count == 0 ? "" : " " + count)}");
            SaveContentPack(new ContentPatcherPack
            {
                directory = dir,
                manifest = manifest,
                content = content
            });
        }

        public static void SaveContentPack(ContentPatcherPack pack)
        {
            Directory.CreateDirectory(pack.directory);
            File.WriteAllText(Path.Combine(pack.directory, "manifest.json"), JsonConvert.SerializeObject(pack.manifest, Formatting.Indented, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            }));
            File.WriteAllText(Path.Combine(pack.directory, "content.json"), JsonConvert.SerializeObject(pack.content, Formatting.Indented, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            }));
        }
        public static bool CanShowLine(int lines)
        {
            return lines >= ContentPackMenu.scrolled && lines < ContentPackMenu.scrolled + ContentPackMenu.linesPerPage;
        }

        public static void TryAddList(string key, int xStart, int yStart, int width, int lineHeight, ref int lines, JObject change, Dictionary<Vector2, string> labels, List<TextBox[]> textBoxes, ref ClickableTextureComponent addCC, List<ClickableTextureComponent> subCCs)
        {

            int ws = 8;
            int wl = (int)Game1.dialogueFont.MeasureString(key).X + 8;
            if (!change.TryGetValue(key, out var keyObj))
            {
                keyObj = new JObject();
            }
            var dict = (JObject)keyObj;
            textBoxes.Clear();
            if (CanShowLine(++lines))
            {
                labels.Add(new Vector2(xStart, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), key);
                addCC = new ClickableTextureComponent("Add", new Rectangle(xStart + wl, yStart + (lines - ContentPackMenu.scrolled) * lineHeight + 16, 56, 56), "", SHelper.Translation.Get("add"), Game1.mouseCursors, new Rectangle(1, 412, 14, 14), 1)
                {
                    myID = lines * 1000 + 1,
                    upNeighborID = (lines - 1) * 1000,
                    leftNeighborID = lines * 1000,
                };
            }

            if (dict.Count > 0)
            {
                wl += 16;
                int ww = (width - wl - ws) / 2;
                foreach (var c in dict)
                {
                    if (CanShowLine(lines))
                    {

                        textBoxes.Add(new TextBox[]
                        {
                            new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
                            {
                                X = xStart + wl,
                                Y = yStart + lineHeight * (lines - ContentPackMenu.scrolled),
                                Width = ww,
                                Text = c.Key
                            },
                            new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
                            {
                                X = xStart + wl + ww + ws,
                                Y = yStart + lineHeight * (lines - ContentPackMenu.scrolled),
                                Width = ww,
                                Text = c.Value.Type == JTokenType.String ? c.Value.ToString() : ""
                            },
                        });
                        subCCs.Add(new ClickableTextureComponent("Remove", new Rectangle(xStart + wl + ww + ws + ww + 2, yStart + (lines - ContentPackMenu.scrolled) * lineHeight + 16, 56, 56), "", SHelper.Translation.Get("remove"), Game1.mouseCursors, new Rectangle(269, 471, 14, 15), 1)
                        {
                            myID = lines * 1000 + 1,
                            upNeighborID = (lines - 1) * 1000,
                            leftNeighborID = lines * 1000,
                        });
                    }
                    lines++;
                }
            }
            else
            {
                lines++;
            }
        }
    }
}