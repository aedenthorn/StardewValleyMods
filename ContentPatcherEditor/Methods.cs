using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection.Emit;
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
                        { "FromFile", "assets/" },
                        { "LogName", "" },
                    };
                case "EditMap":
                    return new JObject()
                    {
                        { "Action", "EditMap" },
                        { "Target", "Maps/" },
                        { "FromFile", "assets/" },
                        { "LogName", "" },
                    };
                case "Include":
                    return new JObject()
                    {
                        { "Action", "Include" },
                        { "FromFile", "assets/" },
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
                Name = "[CP] New Pack",
                Description = "New Content Patcher Content Pack",
                Version = new Version("0.1.0"),
                Author = "YourName",
                MinimumApiVersion = new Version("3.15.0"),
                UniqueID = "yourName.NewContentPatcherPack",
            };
            var content = new ContentPatcherContent()
            {
                Format = SHelper.ModRegistry.Get("Pathoschild.ContentPatcher")?.Manifest.Version.ToString() ?? "1.29.0",
                Changes = new(),
                ConfigSchema = new(),
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
                content = content,
                config = new()
            });
            Game1.activeClickableMenu = new ContentPatcherMenu();
        }
        public static void RebuildLists(ContentPatcherPack pack)
        {
            pack.config = pack.content.ConfigSchema is null ? new List<KeyValuePair<string, ConfigVar>>() : pack.content.ConfigSchema.ToList();
            pack.content.lists = new();
            for (int i = 0; i < pack.content.Changes.Count; i++)
            {
                var change = pack.content.Changes[i];

                pack.content.lists.Add(new Dictionary<string, List<KeyValuePair<string, JToken?>>>());
                if (change.TryGetValue("When", out var obj))
                {
                    pack.content.lists[i]["When"] = new List<KeyValuePair<string, JToken?>>();
                    foreach (var kvp in (JObject)obj)
                    {
                        pack.content.lists[i]["When"].Add(new KeyValuePair<string, JToken?>(kvp.Key, kvp.Value));
                    }
                }
                if (change.TryGetValue("Entries", out obj))
                {
                    pack.content.lists[i]["Entries"] = new List<KeyValuePair<string, JToken?>>();
                    foreach (var kvp in (JObject)obj)
                    {
                        pack.content.lists[i]["Entries"].Add(new KeyValuePair<string, JToken?>(kvp.Key, kvp.Value));
                    }
                }
                if (change.TryGetValue("MapProperties", out obj))
                {
                    pack.content.lists[i]["MapProperties"] = new List<KeyValuePair<string, JToken?>>();
                    foreach (var kvp in (JObject)obj)
                    {
                        pack.content.lists[i]["MapProperties"].Add(new KeyValuePair<string, JToken?>(kvp.Key, kvp.Value));
                    }
                }
            }
        }

        public static void SaveContentPack(ContentPatcherPack pack)
        {
            for(int i = 0; i < pack.content.Changes.Count; i++)
            {
                foreach(var kvp in pack.content.lists[i])
                {
                    JObject j = new JObject();
                    foreach(var l in kvp.Value)
                    {
                        j.TryAdd(l.Key, l.Value);
                    }
                    pack.content.Changes[i][kvp.Key] = j;
                }
            }
            pack.content.lists = null;
            if(pack.config is not null)
            {
                pack.content.ConfigSchema = new();
                foreach (var kvp in pack.config)
                {
                    pack.content.ConfigSchema.TryAdd(kvp.Key, kvp.Value);
                }
            }
            pack.config = null;
            Directory.CreateDirectory(pack.directory);
            string mf = Path.Combine(pack.directory, "manifest.json");
            string cf = Path.Combine(pack.directory, "content.json");
            if (Config.Backup)
            {
                if (File.Exists(mf))
                {
                    File.Move(mf, mf + ".backup", true);
                }
                if (File.Exists(cf))
                {
                    File.Move(cf, cf + ".backup", true);
                }
            }
            File.WriteAllText(mf, JsonConvert.SerializeObject(pack.manifest, Formatting.Indented, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            }));
            File.WriteAllText(cf, JsonConvert.SerializeObject(pack.content, Formatting.Indented, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore
            }));
            SMonitor.Log($"Saved content pack {pack.manifest.Name} to {pack.directory}");
        }
        public static bool CanShowLine(int lines)
        {
            return lines >= ContentPackMenu.scrolled && lines < ContentPackMenu.scrolled + ContentPackMenu.linesPerPage;
        }

        public static void TryAddDict(string key, int xStart, int yStart, int width, int lineHeight, ref int lines, List<KeyValuePair<string, JToken?>> dict, Dictionary<Vector2, string> labels, List<TextBox[]> textBoxes, ref ClickableTextureComponent addCC, List<ClickableTextureComponent> subCCs)
        {

            int ws = 8;
            int wl = (int)Game1.dialogueFont.MeasureString(key).X + 8;
            textBoxes.Clear();
            addCC = null;
            subCCs.Clear();
            if (CanShowLine(++lines))
            {
                labels.Add(new Vector2(xStart, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), key);
                addCC = new ClickableTextureComponent("Add", new Rectangle(xStart + wl, yStart + (lines - ContentPackMenu.scrolled) * lineHeight + 16, 14, 14), "", SHelper.Translation.Get("add"), Game1.mouseCursors, new Rectangle(1, 412, 14, 14), 1)
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
                                Text = c.Key,
                                limitWidth = false
                            },
                            new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
                            {
                                X = xStart + wl + ww + ws,
                                Y = yStart + lineHeight * (lines - ContentPackMenu.scrolled),
                                Width = ww,
                                Text = c.Value.Type == JTokenType.String ? (string)c.Value : "",
                                limitWidth = false
                            },
                        });
                        subCCs.Add(new ClickableTextureComponent("Remove", new Rectangle(xStart + wl + ww + ws + ww + 2, yStart + (lines - ContentPackMenu.scrolled) * lineHeight + 16, 14, 15), "", SHelper.Translation.Get("remove"), Game1.mouseCursors, new Rectangle(269, 471, 14, 15), 1)
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
        public static void TryAddList(string key, int xStart, int yStart, int width, int lineHeight, ref int lines, JArray list, Dictionary<Vector2, string> labels, List<TextBox> textBoxes, ref ClickableTextureComponent addCC, List<ClickableTextureComponent> subCCs)
        {

            int ws = 8;
            int wl = (int)Game1.dialogueFont.MeasureString(key).X + 8;

            textBoxes.Clear();
            addCC = null;
            subCCs.Clear();
            if (CanShowLine(++lines))
            {
                labels.Add(new Vector2(xStart, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), key);
                addCC = new ClickableTextureComponent("Add", new Rectangle(xStart + wl, yStart + (lines - ContentPackMenu.scrolled) * lineHeight + 16, 14, 14), "", SHelper.Translation.Get("add"), Game1.mouseCursors, new Rectangle(1, 412, 14, 14), 1)
                {
                    myID = lines * 1000 + 1,
                    upNeighborID = (lines - 1) * 1000,
                    leftNeighborID = lines * 1000,
                };
            }

            if (list.Count > 0)
            {
                wl += 16;
                int ww = (width - wl);
                foreach (var c in list)
                {
                    if (CanShowLine(lines))
                    {

                        textBoxes.Add(new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
                        {
                            X = xStart + wl,
                            Y = yStart + lineHeight * (lines - ContentPackMenu.scrolled),
                            Width = ww,
                            Text = c.ToString(),
                            limitWidth = false
                        });
                        subCCs.Add(new ClickableTextureComponent("Remove", new Rectangle(xStart + width + 2, yStart + (lines - ContentPackMenu.scrolled) * lineHeight + 16, 14, 15), "", SHelper.Translation.Get("remove"), Game1.mouseCursors, new Rectangle(269, 471, 14, 15), 1)
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
        public static JArray MakeJArray(string values)
        {
            JArray array = new JArray();
            if (values is null)
                return array;
            foreach (var v in values.Split(","))
            {
                array.Add(v.Trim());
            }
            return array;
        }

        public static void TryOpenFolder(string folder)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = folder,
                    UseShellExecute = true,
                    Verb = "open"
                });
                SMonitor.Log($"Opening folder {folder}");
            }
            catch (Exception ex)
            {
                SMonitor.Log($"Error opening folder {folder}: \n\n{ex}", LogLevel.Error);
            }
        }
        public static readonly Lazy<Action<string>> AddToRawCommandQueue = new(() =>
        {
            var scoreType = AccessTools.TypeByName("StardewModdingAPI.Framework.SCore, StardewModdingAPI")!;
            var commandQueueType = AccessTools.TypeByName("StardewModdingAPI.Framework.CommandQueue, StardewModdingAPI")!;

            var scoreGetter = AccessTools.PropertyGetter(scoreType, "Instance")!;
            var rawCommandQueueField = AccessTools.Field(scoreType, "RawCommandQueue")!;
            var commandQueueAddMethod = AccessTools.Method(commandQueueType, "Add");

            var dynamicMethod = new DynamicMethod("AddToRawCommandQueue", null, new Type[] { typeof(string) });
            var il = dynamicMethod.GetILGenerator();

            il.Emit(OpCodes.Call, scoreGetter);
            il.Emit(OpCodes.Ldfld, rawCommandQueueField);
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, commandQueueAddMethod);
            il.Emit(OpCodes.Ret);

            return dynamicMethod.CreateDelegate<Action<string>>();
        });
        public static void ZipContentPack(ContentPatcherPack pack)
        {
            var path = Path.Combine(Constants.GamePath, "Mods", $"{Path.GetFileNameWithoutExtension(pack.directory)}.{pack.manifest.Version}.zip");
            File.Delete(path);
            ZipFile.CreateFromDirectory(pack.directory, path);

            using (ZipArchive archive = ZipFile.Open(path, ZipArchiveMode.Update))
            {
                for (int i = archive.Entries.Count - 1; i >= 0; i--)
                {
                    if (archive.Entries[i].FullName.EndsWith(".backup", StringComparison.OrdinalIgnoreCase) || archive.Entries[i].FullName.Equals("config.json", StringComparison.OrdinalIgnoreCase))
                    {
                        archive.Entries[i].Delete();
                    }
                }
            }
            SMonitor.Log($"Mod zip file created at {path}");
            if (Config.OpenModsFolderAfterZip)
            {
                TryOpenFolder(Path.Combine(Constants.GamePath, "Mods"));
            }
        }

        public static void PlaySound(string v)
        {
            switch (v)
            {
                case "typing":
                    if (Game1.activeClickableMenu is not TitleMenu)
                    {
                        Game1.playSound("cowboy_monsterhit");
                    }
                    break;
            }

        }
    }
}