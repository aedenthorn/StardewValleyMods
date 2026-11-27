using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using static StardewValley.Minigames.MineCart;
using Object = StardewValley.Object;

namespace PortableBasements
{
    internal class PortableBasementMenu : IClickableMenu
    {
        private StardewValley.Object ladder;
        private GameLocation ladderLocation;
        private int ladderX;
        private int ladderY;
        internal const int windowWidth = 64 * 26;

        public ClickableComponent renameBoxCC;
        public TextBox renameBox;
        public ClickableTextureComponent okButton;
        
        public string locationName;
        public string suggestedLocationText;
        public string suggestedLocationName;
        private float suggestedLocationOffset;
        public string inputString;

        public List<string> locationNames = new();

        public enum Stage
        {
            Location,
            Tile
        }

        public Stage currentStage = Stage.Location;

        public PortableBasementMenu(Object instance, GameLocation location, int x, int y, string input = "", Stage stage = Stage.Location, string locName = ""): base(Game1.uiViewport.Width / 2 - (64 * 16 + borderWidth* 2) / 2, Game1.uiViewport.Height / 2 - (128 + borderWidth * 2 + 64) / 2, 64 * 16 + borderWidth* 2, 128 + borderWidth * 2 + 64, false)
        {
            ladder = instance;
            ladderLocation = location;
            ladderX = x;
            ladderY = y;
            renameBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
            {
                X = xPositionOnScreen + borderWidth,
                Width = width - borderWidth * 2 - 64,
                Y = yPositionOnScreen + borderWidth + 32 + 96,
            };
            renameBoxCC = new ClickableComponent(new Rectangle(renameBox.X, renameBox.Y, renameBox.Width, renameBox.Height), "")
            {
                myID = 0,
                rightNeighborID = 1
            };
            okButton = new ClickableTextureComponent(new Rectangle(renameBox.X + renameBox.Width + 4, renameBox.Y, 48, 48), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46, -1, -1), 0.75f, false)
            {
                myID = 1,
                leftNeighborID = 0
            };
            inputString = input;
            currentStage = stage;
            locationName = locName;
            locationNames = Game1.locations.Select(l => l.Name).ToList();
        }

        public override void draw(SpriteBatch b)
        {
            Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true, null, false, true);

            SpriteText.drawString(b, ModEntry.SHelper.Translation.Get(currentStage == Stage.Location ? "Destination.Name" : "Destination.Tile"), renameBox.X + 16, renameBox.Y - 48);
            renameBox.Draw(b);
            okButton.draw(b);
            if (!string.IsNullOrEmpty(suggestedLocationText))
            {
                b.DrawString(renameBox.Font, suggestedLocationText, new Vector2((float)(renameBox.X + 16 + suggestedLocationOffset), (float)(renameBox.Y + 12)), Color.Gray, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.98f);
            }
            Game1.mouseCursorTransparency = 1f;
            drawMouse(b);
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            emergencyShutDown();
            Game1.activeClickableMenu = new PortableBasementMenu(ladder, ladderLocation, ladderX, ladderY, inputString, currentStage, locationName);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {

            renameBox.Selected = false;

            renameBox.Update();
            if (okButton.containsPoint(x, y))
            {
                ReceiveInput();
                return;
            }
        }

        public override void receiveKeyPress(Keys key)
        {
            if (key == Keys.Enter)
            {
                ReceiveInput();
                return;
            }
            if (currentStage == Stage.Location)
            {
                if(key == Keys.Tab && !string.IsNullOrEmpty(suggestedLocationName))
                {
                    renameBox.Text = suggestedLocationName;
                }
                else if(!string.IsNullOrEmpty(renameBox.Text))
                {
                    var start = renameBox.Text.ToLower();
                    foreach (var l in locationNames)
                    {
                        if (l.ToLower().StartsWith(start))
                        {
                            suggestedLocationText = l.Substring(start.Length);
                            suggestedLocationName = l;
                            suggestedLocationOffset = renameBox.Font.MeasureString(renameBox.Text).X;
                            return;
                        }
                    }
                }
                suggestedLocationText = "";
                suggestedLocationName = "";
            }
            if((int)key < 65 || (int)key > 90)
            {
                base.receiveKeyPress(key);
            }
        }

        private void ReceiveInput()
        {
            if(currentStage == Stage.Location)
            {
                if (locationNames.Contains(renameBox.Text))
                {
                    locationName = renameBox.Text;
                }
                else if (!string.IsNullOrEmpty(suggestedLocationName))
                {
                    locationName = suggestedLocationName;
                }
                else
                {
                    ModEntry.SMonitor.Log($"Tried to set destination to non-existent location {renameBox.Text}");
                    return;
                }
                currentStage = Stage.Tile;
                renameBox.Text = "";
                Game1.playSound("bigSelect");
            }
            else
            {
                var coords = renameBox.Text.Split(',');
                if (coords.Length != 2 || !int.TryParse(coords[0], out var x) || !int.TryParse(coords[1], out var y))
                {
                    ModEntry.SMonitor.Log($"Tried to set improper coordinates {renameBox.Text}");
                    return;
                }
                var loc = Game1.locations.FirstOrDefault(l => l.Name == locationName);

                if (loc is null)
                {
                    ModEntry.SMonitor.Log($"This shouldn't happen: Tried to set destination to non-existent location {renameBox.Text}");
                }
                else
                {
                    if(!loc.Map.GetLayer("Back").IsValidTileLocation(new xTile.Dimensions.Location(x, y)) || !loc.Map.GetLayer("Back").IsValidTileLocation(new xTile.Dimensions.Location(x, y + 1)))
                    {
                        ModEntry.SMonitor.Log($"Tried to set destination to invalid tile");
                        Game1.showRedMessage(ModEntry.SHelper.Translation.Get("InvalidTile"), true);
                        return;
                    }
                    else if(loc.Objects.ContainsKey(new Vector2(x, y)) || loc.Objects.ContainsKey(new Vector2(x, y + 1)) || loc.Map.GetLayer("Buildings").Tiles[x, y] != null || loc.Map.GetLayer("Buildings").Tiles[x, y + 1] != null)
                    {
                        ModEntry.SMonitor.Log($"Tried to set destination to existing object tile");
                        Game1.showRedMessage(ModEntry.SHelper.Translation.Get("DestinationBlocked"), true);
                        return;
                    }
                    else
                    {
                        ladderLocation.Objects[new(ladderX / 64, ladderY / 64)].modData[ModEntry.modKey] = $"{locationName},{x},{y}";
                        var item = ItemRegistry.Create<Object>("(BC)" + ModEntry.ladderUpKey, 1, 0, false);
                        item.modData[ModEntry.modKey] = $"{ladderLocation.Name},{ladderX / 64},{ladderY / 64}";
                        loc.setObject(new Vector2(x, y), item);
                    }
                }
                exitThisMenu();
            }
        }
    }
}