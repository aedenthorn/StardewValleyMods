using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using StardewValley;
using StardewValley.Menus;
using StardewValley.SDKs;
using System.Collections;
using System.Collections.Generic;

namespace ContentPatcherEditor
{
    public class ChangeSet
    {
        public Dictionary<Vector2, string> labels = new();
        public ClickableComponent ActionCC;
        public ClickableTextureComponent WhenAddCC;
        public ClickableTextureComponent DeleteCC;
        public List<ClickableTextureComponent> WhenSubCC = new();
        public TextBox Action;
        public List<TextBox[]> When = new();
        public TextBox LogName;
        public List<ClickableComponent> Update = new();
        public ChangeSet(int xStart, int yStart, int width, int lineHeight, ref int lines, JObject change, Dictionary<string, List<KeyValuePair<string, JToken?>>> lists)
        {
            if (ModEntry.CanShowLine(lines))
            {
                DeleteCC = new ClickableTextureComponent("Remove", new Rectangle(xStart - 16, yStart + (lines - ContentPackMenu.scrolled) * lineHeight + 16, 56, 56), "", ModEntry.SHelper.Translation.Get("remove"), Game1.mouseCursors, new Rectangle(269, 471, 14, 15), 1)
                {
                    myID = lines * 1000 + 1,
                    upNeighborID = (lines - 1) * 1000,
                    leftNeighborID = lines * 1000,
                };
                Action = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
                {
                    X = xStart + (int)Game1.dialogueFont.MeasureString("Action").X,
                    Y = yStart + lineHeight * (lines - ContentPackMenu.scrolled),
                    Width = width - (int)Game1.dialogueFont.MeasureString("Action").X,
                    Text = (string)change["Action"],
                    limitWidth = false
                };
                ActionCC = new ClickableComponent(new Rectangle(Action.X, Action.Y, Action.Width, Action.Height), "Action");
                labels.Add(new Vector2(xStart, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), "Action");
            }
            if (ModEntry.CanShowLine(++lines))
            {
                if (!change.TryGetValue("LogName", out var logNameObj))
                    logNameObj = "";
                var logName = (string)logNameObj;
                LogName = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
                {
                    X = xStart + (int)Game1.dialogueFont.MeasureString("LogName").X,
                    Y = yStart + lineHeight * (lines - ContentPackMenu.scrolled),
                    Width = width - (int)Game1.dialogueFont.MeasureString("LogName").X,
                    Text = logName,
                    limitWidth = false
                };
                labels.Add(new Vector2(xStart, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), "LogName");
            }

            if (ModEntry.CanShowLine(++lines))
            {

                if (!change.TryGetValue("Update", out var updateObj))
                {
                    updateObj = "OnDayStart";
                }
                string update = (string)updateObj;
                var nameWidth = (int)Game1.dialogueFont.MeasureString("Update").X;
                var w = width - nameWidth;
                Update = new()
                {
                    new ClickableComponent(new Rectangle(xStart + nameWidth, yStart + (lines - ContentPackMenu.scrolled) * lineHeight, w / 3, lineHeight), "OnDayStart", update.Contains("OnDayStart") ? "active" : "inactive"),
                    new ClickableComponent(new Rectangle(xStart + nameWidth + w / 3, yStart + (lines - ContentPackMenu.scrolled) * lineHeight, w / 3, lineHeight), "OnLocationChange", update.Contains("OnLocationChange") ? "active" : "inactive"),
                    new ClickableComponent(new Rectangle(xStart + nameWidth + w * 2 / 3, yStart + (lines - ContentPackMenu.scrolled) * lineHeight, w / 3, lineHeight), "OnTimeChange", update.Contains("OnTimeChange") ? "active" : "inactive"),
                };
                labels.Add(new Vector2(xStart, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), "Update");
            }
            if (!lists.TryGetValue("When", out var dict))
            {
                dict = new List<KeyValuePair<string, JToken?>>();
            }
            ModEntry.TryAddDict("When", xStart, yStart, width, lineHeight, ref lines, dict, labels, When, ref WhenAddCC, WhenSubCC);
        }
    }
    public class LoadSet : ChangeSet
    {
        public TextBox Target;
        public TextBox FromFile;
        public LoadSet(int xStart, int yStart, int width, int lineHeight, ref int lines, JObject change, Dictionary<string, List<KeyValuePair<string, JToken?>>> lists) : base(xStart, yStart, width, lineHeight, ref lines, change, lists)
        {
            if (ModEntry.CanShowLine(lines))
            {
                Target = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
                {
                    X = xStart + (int)Game1.dialogueFont.MeasureString("Target").X,
                    Y = yStart + lineHeight * (lines - ContentPackMenu.scrolled),
                    Width = width - (int)Game1.dialogueFont.MeasureString("Target").X,
                    Text = (string)change["Target"],
                    limitWidth = false
                };
                labels.Add(new Vector2(xStart, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), "Target");
            }
            if (ModEntry.CanShowLine(++lines))
            {
                FromFile = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
                {
                    X = xStart + (int)Game1.dialogueFont.MeasureString("FromFile").X,
                    Y = yStart + lineHeight * (lines - ContentPackMenu.scrolled),
                    Width = width - (int)Game1.dialogueFont.MeasureString("FromFile").X,
                    Text = (string)change["FromFile"],
                    limitWidth = false
                };
                labels.Add(new Vector2(xStart, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), "FromFile");
            }
            lines++;
        }
    }
    public class EditDataSet : ChangeSet
    {
        public TextBox Target;
        public List<TextBox[]> Fields = new();
        public List<TextBox[]> Entries = new();
        public List<TextBox[]> MoveEntries = new();
        public List<TextBox[]> TextOperations = new();
        public ClickableTextureComponent FieldsAddCC;
        public List<ClickableTextureComponent> FieldsSubCC = new();
        public ClickableTextureComponent EntriesAddCC;
        public List<ClickableTextureComponent> EntriesSubCC = new();
        public ClickableTextureComponent MoveEntriesAddCC;
        public List<ClickableTextureComponent> MoveEntriesSubCC = new();
        public ClickableTextureComponent TextOperationsAddCC;
        public List<ClickableTextureComponent> TextOperationsSubCC = new();

        public EditDataSet(int xStart, int yStart, int width, int lineHeight, ref int lines, JObject change, Dictionary<string, List<KeyValuePair<string, JToken?>>> lists) : base(xStart, yStart, width, lineHeight, ref lines, change, lists)
        {
            if (ModEntry.CanShowLine(lines))
            {
                Target = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
                {
                    X = xStart + (int)Game1.dialogueFont.MeasureString("Target").X,
                    Y = yStart + lineHeight * (lines - ContentPackMenu.scrolled),
                    Width = width - (int)Game1.dialogueFont.MeasureString("Target").X,
                    Text = (string)change["Target"],
                    limitWidth = false
                };
                labels.Add(new Vector2(xStart, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), "Target");
            }
            if (!lists.TryGetValue("Entries", out var dict))
            {
                dict = new List<KeyValuePair<string, JToken?>>();
                lists["Entries"] = dict;
            }
            ModEntry.TryAddDict("Entries", xStart, yStart, width, lineHeight, ref lines, dict, labels, Entries, ref EntriesAddCC, EntriesSubCC);
        }
    }
    public class EditImageSet : ChangeSet
    {
        public TextBox Target;
        public TextBox FromFile;
        public TextBox[] FromArea;
        public TextBox[] ToArea;
        public ClickableComponent PatchMode;

        public EditImageSet(int xStart, int yStart, int width, int lineHeight, ref int lines, JObject change, Dictionary<string, List<KeyValuePair<string, JToken?>>> lists) : base(xStart, yStart, width, lineHeight, ref lines, change, lists)
        {
            if (ModEntry.CanShowLine(lines))
            {
                Target = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
                {
                    X = xStart + (int)Game1.dialogueFont.MeasureString("Target").X,
                    Y = yStart + lineHeight * (lines - ContentPackMenu.scrolled),
                    Width = width - (int)Game1.dialogueFont.MeasureString("Target").X,
                    Text = (string)change["Target"],
                    limitWidth = false
                };
                labels.Add(new Vector2(xStart, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), "Target");
            }
            if (ModEntry.CanShowLine(++lines))
            {
                FromFile = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
                {
                    X = xStart + (int)Game1.dialogueFont.MeasureString("FromFile").X,
                    Y = yStart + lineHeight * (lines - ContentPackMenu.scrolled),
                    Width = width - (int)Game1.dialogueFont.MeasureString("FromFile").X,
                    Text = (string)change["FromFile"],
                    limitWidth = false
                };
                labels.Add(new Vector2(xStart, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), "FromFile");
            }
            if (ModEntry.CanShowLine(++lines))
            {
                if (!change.TryGetValue("PatchMode", out var patchObj))
                {
                    patchObj = "ReplaceByLayer";
                }
                string patch = (string)patchObj;
                PatchMode = new ClickableComponent(new Rectangle(xStart + (int)Game1.dialogueFont.MeasureString("PatchMode").X, yStart + (lines - ContentPackMenu.scrolled) * lineHeight, width, lineHeight), patch);
                labels.Add(new Vector2(xStart, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), "PatchMode");
            }
            int spacer = 16;
            int mX = (int)Game1.dialogueFont.MeasureString("X").X + spacer;
            if (ModEntry.CanShowLine(++lines))
            {
                if (!change.TryGetValue("FromArea", out JToken? fromAreaT))
                {
                    fromAreaT = new JObject()
                    {
                        { "X", 0 },
                        { "Y", 0 },
                        { "Width", 0 },
                        { "Height", 0 }
                    };
                }
                JObject fromArea = (JObject)fromAreaT;
                int mS = (int)Game1.dialogueFont.MeasureString("FromArea").X + spacer;
                int textWidth = (width - mS) / 4 - mX;
                FromArea = new TextBox[]
                {
                    new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
                    {
                        X = xStart + mS + mX,
                        Y = yStart + lineHeight * (lines - ContentPackMenu.scrolled),
                        Width = textWidth,
                        Text = fromArea["X"].ToString(),
                        limitWidth = false
                    },
                    new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
                    {
                        X = xStart + mS + mX + textWidth + mX,
                        Y = yStart + lineHeight * (lines - ContentPackMenu.scrolled),
                        Width = textWidth,
                        Text = fromArea["Y"].ToString(),
                        limitWidth = false
                    },
                    new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
                    {
                        X = xStart + mS + (mX + textWidth) * 2 + mX,
                        Y = yStart + lineHeight * (lines - ContentPackMenu.scrolled),
                        Width = textWidth,
                        Text = fromArea["Width"].ToString(),
                        limitWidth = false
                    },
                    new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
                    {
                        X = xStart + mS + (mX + textWidth) * 3 + mX,
                        Y = yStart + lineHeight * (lines - ContentPackMenu.scrolled),
                        Width = textWidth,
                        Text = fromArea["Height"].ToString(),
                        limitWidth = false
                    }
                };
                labels.Add(new Vector2(xStart, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), "FromArea:");
                labels.Add(new Vector2(xStart + mS + spacer / 2, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), "X");
                labels.Add(new Vector2(xStart + mS + mX + textWidth + spacer / 2, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), "Y");
                labels.Add(new Vector2(xStart + mS + (mX + textWidth) * 2 + spacer / 2, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), "W");
                labels.Add(new Vector2(xStart + mS + (mX + textWidth) * 3 + spacer / 2, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), "H");
            }
            if (ModEntry.CanShowLine(++lines))
            {
                if (!change.TryGetValue("ToArea", out JToken? ToAreaT))
                {
                    ToAreaT = new JObject()
                    {
                        { "X", 0 },
                        { "Y", 0 },
                        { "Width", 0 },
                        { "Height", 0 }
                    };
                }
                JObject toArea = (JObject)ToAreaT;
                int mS = (int)Game1.dialogueFont.MeasureString("ToArea").X + spacer;
                int textWidth = (width - mS) / 4 - mX;
                ToArea = new TextBox[]
                {
                    new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
                    {
                        X = xStart + mS + mX,
                        Y = yStart + lineHeight * (lines - ContentPackMenu.scrolled),
                        Width = textWidth,
                        Text = toArea["X"].ToString(),
                        limitWidth = false
                    },
                    new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
                    {
                        X = xStart + mS + mX + textWidth + mX,
                        Y = yStart + lineHeight * (lines - ContentPackMenu.scrolled),
                        Width = textWidth,
                        Text = toArea["Y"].ToString(),
                        limitWidth = false
                    },
                    new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
                    {
                        X = xStart + mS + (mX + textWidth) * 2 + mX,
                        Y = yStart + lineHeight * (lines - ContentPackMenu.scrolled),
                        Width = textWidth,
                        Text = toArea["Width"].ToString(),
                        limitWidth = false
                    },
                    new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
                    {
                        X = xStart + mS + (mX + textWidth) * 3 + mX,
                        Y = yStart + lineHeight * (lines - ContentPackMenu.scrolled),
                        Width = textWidth,
                        Text = toArea["Height"].ToString(),
                        limitWidth = false
                    }
                };
                labels.Add(new Vector2(xStart, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), "ToArea:");
                labels.Add(new Vector2(xStart + mS + spacer / 2, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), "X");
                labels.Add(new Vector2(xStart + mS + mX + textWidth + spacer / 2, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), "Y");
                labels.Add(new Vector2(xStart + mS + (mX + textWidth) * 2 + spacer / 2, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), "W");
                labels.Add(new Vector2(xStart + mS + (mX + textWidth) * 3 + spacer / 2, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), "H");
            }
            lines++;
        }
    }
    public class EditMapSet : ChangeSet
    {
        public TextBox Target;
        public TextBox FromFile;
        public TextBox[] FromArea;
        public TextBox[] ToArea;
        public ClickableComponent PatchMode;
        public ClickableTextureComponent MapPropertiesAddCC;
        public List<ClickableTextureComponent> MapPropertiesSubCC = new();
        public List<TextBox[]> MapProperties = new();
        public ClickableTextureComponent AddWarpsAddCC;
        public List<ClickableTextureComponent> AddWarpsSubCC = new();
        public List<TextBox> AddWarps = new();

        public EditMapSet(int xStart, int yStart, int width, int lineHeight, ref int lines, JObject change, Dictionary<string, List<KeyValuePair<string, JToken?>>> lists) : base(xStart, yStart, width, lineHeight, ref lines, change, lists)
        {
            if (ModEntry.CanShowLine(lines))
            {
                Target = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
                {
                    X = xStart + (int)Game1.dialogueFont.MeasureString("Target").X,
                    Y = yStart + lineHeight * (lines - ContentPackMenu.scrolled),
                    Width = width - (int)Game1.dialogueFont.MeasureString("Target").X,
                    Text = (string)change["Target"],
                    limitWidth = false
                };
                labels.Add(new Vector2(xStart, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), "Target");
            }
            if (ModEntry.CanShowLine(++lines))
            {
                FromFile = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
                {
                    X = xStart + (int)Game1.dialogueFont.MeasureString("FromFile").X,
                    Y = yStart + lineHeight * (lines - ContentPackMenu.scrolled),
                    Width = width - (int)Game1.dialogueFont.MeasureString("FromFile").X,
                    Text = (string)change["FromFile"],
                    limitWidth = false
                };
                labels.Add(new Vector2(xStart, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), "FromFile");
            }
            if (ModEntry.CanShowLine(++lines))
            {
                if (!change.TryGetValue("PatchMode", out var patchObj))
                {
                    patchObj = "ReplaceByLayer";
                }
                string patch = (string)patchObj;
                PatchMode = new ClickableComponent(new Rectangle(xStart + (int)Game1.dialogueFont.MeasureString("PatchMode").X, yStart + (lines - ContentPackMenu.scrolled) * lineHeight, width, lineHeight), patch);
                labels.Add(new Vector2(xStart, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), "PatchMode");
            }
            int spacer = 16;
            int mX = (int)Game1.dialogueFont.MeasureString("X").X + spacer;
            if (ModEntry.CanShowLine(++lines))
            {
                if(!change.TryGetValue("FromArea", out JToken? fromAreaT))
                {
                    fromAreaT = new JObject()
                    {
                        { "X", 0 },
                        { "Y", 0 },
                        { "Width", 0 },
                        { "Height", 0 }
                    };
                }
                JObject fromArea = (JObject)fromAreaT;
                int mS = (int)Game1.dialogueFont.MeasureString("FromArea").X + spacer;
                int textWidth = (width - mS) / 4 - mX;
                FromArea = new TextBox[]
                {
                    new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
                    {
                        X = xStart + mS + mX,
                        Y = yStart + lineHeight * (lines - ContentPackMenu.scrolled),
                        Width = textWidth,
                        Text = fromArea["X"].ToString(),
                        limitWidth = false
                    },
                    new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
                    {
                        X = xStart + mS + mX + textWidth + mX,
                        Y = yStart + lineHeight * (lines - ContentPackMenu.scrolled),
                        Width = textWidth,
                        Text = fromArea["Y"].ToString(),
                        limitWidth = false
                    },
                    new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
                    {
                        X = xStart + mS + (mX + textWidth) * 2 + mX,
                        Y = yStart + lineHeight * (lines - ContentPackMenu.scrolled),
                        Width = textWidth,
                        Text = fromArea["Width"].ToString(),
                        limitWidth = false
                    },
                    new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
                    {
                        X = xStart + mS + (mX + textWidth) * 3 + mX,
                        Y = yStart + lineHeight * (lines - ContentPackMenu.scrolled),
                        Width = textWidth,
                        Text = fromArea["Height"].ToString(),
                        limitWidth = false
                    }
                };
                labels.Add(new Vector2(xStart, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), "FromArea:");
                labels.Add(new Vector2(xStart + mS + spacer / 2, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), "X");
                labels.Add(new Vector2(xStart + mS + mX + textWidth + spacer / 2, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), "Y");
                labels.Add(new Vector2(xStart + mS + (mX + textWidth) * 2 + spacer / 2, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), "W");
                labels.Add(new Vector2(xStart + mS + (mX + textWidth) * 3 + spacer / 2, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), "H");
            }            if (ModEntry.CanShowLine(++lines))
            {
                if(!change.TryGetValue("ToArea", out JToken? ToAreaT))
                {
                    ToAreaT = new JObject()
                    {
                        { "X", 0 },
                        { "Y", 0 },
                        { "Width", 0 },
                        { "Height", 0 }
                    };
                }
                JObject toArea = (JObject)ToAreaT;
                int mS = (int)Game1.dialogueFont.MeasureString("ToArea").X + spacer;
                int textWidth = (width - mS) / 4 - mX;
                ToArea = new TextBox[]
                {
                    new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
                    {
                        X = xStart + mS + mX,
                        Y = yStart + lineHeight * (lines - ContentPackMenu.scrolled),
                        Width = textWidth,
                        Text = toArea["X"].ToString(),
                        limitWidth = false
                    },
                    new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
                    {
                        X = xStart + mS + mX + textWidth + mX,
                        Y = yStart + lineHeight * (lines - ContentPackMenu.scrolled),
                        Width = textWidth,
                        Text = toArea["Y"].ToString(),
                        limitWidth = false
                    },
                    new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
                    {
                        X = xStart + mS + (mX + textWidth) * 2 + mX,
                        Y = yStart + lineHeight * (lines - ContentPackMenu.scrolled),
                        Width = textWidth,
                        Text = toArea["Width"].ToString(),
                        limitWidth = false
                    },
                    new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
                    {
                        X = xStart + mS + (mX + textWidth) * 3 + mX,
                        Y = yStart + lineHeight * (lines - ContentPackMenu.scrolled),
                        Width = textWidth,
                        Text = toArea["Height"].ToString(),
                        limitWidth = false
                    }
                };
                labels.Add(new Vector2(xStart, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), "ToArea:");
                labels.Add(new Vector2(xStart + mS + spacer / 2, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), "X");
                labels.Add(new Vector2(xStart + mS + mX + textWidth + spacer / 2, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), "Y");
                labels.Add(new Vector2(xStart + mS + (mX + textWidth) * 2 + spacer / 2, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), "W");
                labels.Add(new Vector2(xStart + mS + (mX + textWidth) * 3 + spacer / 2, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), "H");
            }
            if (!lists.TryGetValue("MapProperties", out var dict))
            {
                dict = new List<KeyValuePair<string, JToken?>>();
            }

            ModEntry.TryAddDict("MapProperties", xStart, yStart, width, lineHeight, ref lines, dict, labels, MapProperties, ref MapPropertiesAddCC, MapPropertiesSubCC);
            lines--;
            if (!change.TryGetValue("AddWarps", out var keyObj))
            {
                keyObj = new JArray();
            }
            var list = (JArray)keyObj;
            ModEntry.TryAddList("AddWarps", xStart, yStart, width, lineHeight, ref lines, list, labels, AddWarps, ref AddWarpsAddCC, AddWarpsSubCC);
        }
    }
    public class IncludeSet : ChangeSet
    {
        public TextBox FromFile;

        public IncludeSet(int xStart, int yStart, int width, int lineHeight, ref int lines, JObject change, Dictionary<string, List<KeyValuePair<string, JToken?>>> lists) : base(xStart, yStart, width, lineHeight, ref lines, change, lists)
        {
            if (ModEntry.CanShowLine(lines))
            {
                FromFile = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
                {
                    X = xStart + (int)Game1.dialogueFont.MeasureString("FromFile").X,
                    Y = yStart + lineHeight * (lines - ContentPackMenu.scrolled),
                    Width = width - (int)Game1.dialogueFont.MeasureString("FromFile").X,
                    Text = (string)change["FromFile"],
                    limitWidth = false
                };
                labels.Add(new Vector2(xStart, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), "FromFile");
            }
            lines++;
        }
    }
}