using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using StardewValley;
using StardewValley.Menus;
using StardewValley.SDKs;
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
        public ChangeSet(int xStart, int yStart, int width, int lineHeight, ref int lines, JObject change)
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
                    Text = (string)change["Action"]
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
                    Text = logName
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
            ModEntry.TryAddList("When", xStart, yStart, width, lineHeight, ref lines, change, labels, When, ref WhenAddCC, WhenSubCC);
        }
    }
    public class LoadSet : ChangeSet
    {
        public TextBox Target;
        public TextBox FromFile;
        public LoadSet(int xStart, int yStart, int width, int lineHeight, ref int lines, JObject change): base(xStart, yStart, width, lineHeight, ref lines, change)
        {
            if (ModEntry.CanShowLine(lines))
            {
                Target = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
                {
                    X = xStart + (int)Game1.dialogueFont.MeasureString("Target").X,
                    Y = yStart + lineHeight * (lines - ContentPackMenu.scrolled),
                    Width = width - (int)Game1.dialogueFont.MeasureString("Target").X,
                    Text = (string)change["Target"]
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
                    Text = (string)change["FromFile"]
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

        public EditDataSet(int xStart, int yStart, int width, int lineHeight, ref int lines, JObject change) : base(xStart, yStart, width, lineHeight, ref lines, change)
        {
            if (ModEntry.CanShowLine(lines))
            {
                Target = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
                {
                    X = xStart + (int)Game1.dialogueFont.MeasureString("Target").X,
                    Y = yStart + lineHeight * (lines - ContentPackMenu.scrolled),
                    Width = width - (int)Game1.dialogueFont.MeasureString("Target").X,
                    Text = (string)change["Target"]
                };
                labels.Add(new Vector2(xStart, yStart + (lines - ContentPackMenu.scrolled) * lineHeight), "Target");
            }
            ModEntry.TryAddList("Entries", xStart, yStart, width, lineHeight, ref lines, change, labels, Entries, ref EntriesAddCC, EntriesSubCC);
        }
    }
    public class EditImageSet : ChangeSet
    {
        public TextBox Target;
        public TextBox FromFile;
        public TextBox[] FromArea;
        public TextBox[] ToArea;
        public ClickableTextureComponent PatchMode;

        public EditImageSet(int xStart, int yStart, int width, int lineHeight, ref int lines, JObject change) : base(xStart, yStart, width, lineHeight, ref lines, change)
        {
        }
    }
    public class EditMapSet : ChangeSet
    {
        public TextBox Target;
        public TextBox FromFile;
        public TextBox[] FromArea;
        public TextBox[] ToArea;
        public ClickableComponent PatchMode;
        public List<TextBox[]> MapProperties;
        public List<TextBox> AddWarps;

        public EditMapSet(int xStart, int yStart, int width, int lineHeight, ref int lines, JObject change) : base(xStart, yStart, width, lineHeight, ref lines, change)
        {
        }
    }
    public class IncludeSet : ChangeSet
    {
        public TextBox FromFile;

        public IncludeSet(int xStart, int yStart, int width, int lineHeight, ref int lines, JObject change) : base(xStart, yStart, width, lineHeight, ref lines, change)
        {
        }
    }
}