using FashionSense.Framework.Models.Appearances;
using FashionSense.Framework.Models.Appearances.Accessory;
using FashionSense.Framework.Models.Appearances.Body;
using FashionSense.Framework.Models.Appearances.Hair;
using FashionSense.Framework.Models.Appearances.Hat;
using FashionSense.Framework.Models.Appearances.Pants;
using FashionSense.Framework.Models.Appearances.Shirt;
using FashionSense.Framework.Models.Appearances.Shoes;
using FashionSense.Framework.Models.Appearances.Sleeves;
using FashionSense.Framework.UI;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FashionSenseAppearanceMenu
{
    public class FashionSenseAppearanceMenuMenu : IClickableMenu
    {
        internal const string UI_HAND_MIRROR_FILTER_BUTTON = "FashionSense.UI.HandMirror.SelectedFilterButton";

        internal const string ACCESSORY_FILTER_BUTTON = "AccessoryFilter";
        internal const string HAIR_FILTER_BUTTON = "HairFilter";
        internal const string HAT_FILTER_BUTTON = "HatFilter";
        internal const string SHIRT_FILTER_BUTTON = "ShirtFilter";
        internal const string PANTS_FILTER_BUTTON = "PantsFilter";
        internal const string SLEEVES_FILTER_BUTTON = "SleevesFilter";
        internal const string SHOES_FILTER_BUTTON = "ShoesFilter";
        internal const string BODY_FILTER_BUTTON = "BodyFilter";

        internal const string CUSTOM_HAIR_ID = "FashionSense.CustomHair.Id";
        internal const string CUSTOM_ACCESSORY_ID = "FashionSense.CustomAccessory.Id";
        internal const string CUSTOM_ACCESSORY_COLLECTIVE_ID = "FashionSense.CustomAccessory.Collective.Id";
        internal const string CUSTOM_HAT_ID = "FashionSense.CustomHat.Id";
        internal const string CUSTOM_SHIRT_ID = "FashionSense.CustomShirt.Id";
        internal const string CUSTOM_PANTS_ID = "FashionSense.CustomPants.Id";
        internal const string CUSTOM_SLEEVES_ID = "FashionSense.CustomSleeves.Id";
        internal const string CUSTOM_SHOES_ID = "FashionSense.CustomShoes.Id";
        internal const string CUSTOM_BODY_ID = "FashionSense.CustomBody.Id";

        public HandMirrorMenu menu;
        private Farmer farmer;
        private string whichFilter;
        public List<Texture2D> allEntries = new();
        public List<ClickableTextureComponent> entries = new();
        public ClickableTextureComponent  leftButton;
        public ClickableTextureComponent  rightButton;
        public int scrolled = 0;
        private int count;
        private int cols;
        private int oneHeight;
        private int oneWidth;
        private int rows;
        private int fit;
        private object tm;
        private IModHelper mh;
        private AppearanceContentPack last;
        private int lastIndex;

        public FashionSenseAppearanceMenuMenu(HandMirrorMenu m, Farmer f, string w): base(Game1.uiViewport.Width / 2 - (632 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (648 + IClickableMenu.borderWidth * 2) / 2 - 64, 632 + IClickableMenu.borderWidth * 2, 648 + IClickableMenu.borderWidth * 2 + 64)
        {
            menu = m;
            farmer = f;
            whichFilter = w;
            var types = m.GetType().Assembly.GetTypes().Where(t => t.Name.Contains("FashionSense"));
            var fi = AccessTools.Field(typeof(FashionSense.FashionSense), "textureManager");
            tm = fi.GetValue(null);
            mh = (IModHelper)AccessTools.Field(typeof(FashionSense.FashionSense), "modHelper").GetValue(null);
            List<AppearanceContentPack> appearanceModels = new List<AppearanceContentPack>();
            var mi = tm.GetType().GetMethods().First(i => i.Name.Contains("GetAllAppearanceModels") && !i.IsGenericMethod);
            var allAppearances = mi.Invoke(tm, Array.Empty<object>()) as List<AppearanceContentPack>;
            string modDataKey = "null";
            switch (whichFilter)
            {

                case HAIR_FILTER_BUTTON:
                    modDataKey = CUSTOM_HAIR_ID;
                    appearanceModels = allAppearances.Where(m => m is HairContentPack).ToList();
                    break;
                case ACCESSORY_FILTER_BUTTON:
                    modDataKey = CUSTOM_ACCESSORY_COLLECTIVE_ID;
                    appearanceModels = allAppearances.Where(m => m is AccessoryContentPack).ToList();
                    break;
                case HAT_FILTER_BUTTON:
                    modDataKey = CUSTOM_HAT_ID;
                    appearanceModels = allAppearances.Where(m => m is HatContentPack).ToList();
                    break;
                case SHIRT_FILTER_BUTTON:
                    modDataKey = CUSTOM_SHIRT_ID;
                    appearanceModels = allAppearances.Where(m => m is ShirtContentPack).ToList();
                    break;
                case PANTS_FILTER_BUTTON:
                    modDataKey = CUSTOM_PANTS_ID;
                    appearanceModels = allAppearances.Where(m => m is PantsContentPack).ToList();
                    break;
                case SLEEVES_FILTER_BUTTON:
                    modDataKey = CUSTOM_SLEEVES_ID;
                    appearanceModels = allAppearances.Where(m => m is SleevesContentPack).ToList();
                    break;
                case SHOES_FILTER_BUTTON:
                    modDataKey = CUSTOM_SHOES_ID;
                    appearanceModels = allAppearances.Where(m => m is ShoesContentPack).ToList();
                    break;
                case BODY_FILTER_BUTTON:
                    modDataKey = CUSTOM_BODY_ID;
                    appearanceModels = allAppearances.Where(m => m is BodyContentPack).ToList();
                    break;
            }
            last = appearanceModels.FirstOrDefault(m => (string)AccessTools.Property(typeof(AppearanceContentPack), "Id").GetValue(m) == Game1.player.modData[modDataKey]);
            lastIndex = appearanceModels.IndexOf(last);
            count = appearanceModels.Count() + 1;
            oneHeight = 128 + 32;
            oneWidth = 64 + 78;
            CreatePreviews();
            cols = 4;
            rows = height / oneHeight;
            fit = rows * cols;
            scrolled = MathHelper.Clamp(lastIndex / cols, 0, (int)Math.Ceiling((count - fit) / (float)cols));
            RebuildComponents();
        }

        private void CreatePreviews()
        {
            allEntries.Clear();
            DoChange(-1);
            for (int i = 0; i < count; i++)
            {
                Texture2D tex = new(Game1.graphics.GraphicsDevice, oneWidth, oneHeight);
                RenderTarget2D renderTarget = new RenderTarget2D(Game1.graphics.GraphicsDevice, oneWidth, oneHeight);
                renderTarget.SetData(new Color[oneWidth * oneHeight]);
                Rectangle destinationRectangle = new Rectangle(0, 0, oneWidth, oneHeight);

                Game1.graphics.GraphicsDevice.SetRenderTarget(renderTarget);
                Game1.graphics.GraphicsDevice.Clear(Color.Transparent);

                var renderBatch = new SpriteBatch(Game1.graphics.GraphicsDevice);

                renderBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);
                DrawFarmer(renderBatch, new(0, 0, oneWidth, oneHeight));
                DoChange();
                renderBatch.End();

                Game1.graphics.GraphicsDevice.SetRenderTarget(null);
                Color[] data = new Color[renderTarget.Width * renderTarget.Height];
                renderTarget.GetData(data);
                tex.SetData(data);
                allEntries.Add(tex);
            }
            DoChange(lastIndex);
        }

        private void RebuildComponents()
        {
            xPositionOnScreen = Game1.uiViewport.Width / 2 - (632 + IClickableMenu.borderWidth * 2) / 2;
            yPositionOnScreen = Game1.uiViewport.Height / 2 - (648 + IClickableMenu.borderWidth * 2) / 2 - 64;
            width = 632 + IClickableMenu.borderWidth * 2;
            height = 648 + IClickableMenu.borderWidth * 2 + 64;

            int start = cols * scrolled;
            entries.Clear();
            int c = 0;
            var end = Math.Min(start + fit, count);
            for (int i = start; i < end; i++)
            {
                var x = c % cols * oneWidth;
                var y = c / cols * oneHeight;
                entries.Add(new((i - 1).ToString(), new(xPositionOnScreen + 64 + x, yPositionOnScreen + 108 + y, oneWidth, oneHeight), null, (i - 1).ToString(), allEntries[i], new(0, 0, oneWidth, oneHeight), 1f)
                {
                    myID = c,
                    upNeighborID = c - cols,
                    leftNeighborID = c - 1,
                    rightNeighborID = c + 1,
                    downNeighborID = c + cols
                });
                c++;
            }
            leftButton = new ClickableTextureComponent("Direction", new Rectangle(xPositionOnScreen + width / 2 - 96, yPositionOnScreen + height, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44, -1, -1), 1f, false)
            {
                myID = c,
                upNeighborID = 0,
                leftNeighborID = -99998,
                rightNeighborID = c + 1,
                downNeighborID = -99998
            };
            rightButton = new ClickableTextureComponent("Direction", new Rectangle(xPositionOnScreen + width / 2 + 32, yPositionOnScreen + height, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33, -1, -1), 1f, false)
            {
                myID = c + 1,
                upNeighborID = 0,
                leftNeighborID = c,
                rightNeighborID = -99998,
                downNeighborID = -99998
            };
        }

        private void DrawFarmer(SpriteBatch b, Rectangle bounds)
        {
            var x = bounds.Width / 2 - 32;
            var y = bounds.Height / 2 - 64;
            farmer.FarmerRenderer.draw(b, farmer.FarmerSprite.CurrentAnimationFrame, farmer.FarmerSprite.CurrentFrame, farmer.FarmerSprite.SourceRect, new Vector2(x, y), Vector2.Zero, 1f, Color.White, 0f, 1f, farmer);
        }

        private void DoChange()
        {
            ModEntry.mute.Value = true;
            AccessTools.Method(typeof(HandMirrorMenu), "UpdateAppearance").Invoke(menu, new object[] { 1, false });
            ModEntry.mute.Value = false;
        }

        private void DoChange(int i)
        {
            ModEntry.mute.Value = true;
            AccessTools.Method(typeof(HandMirrorMenu), "UpdateAppearance").Invoke(menu, new object[] { i, true });
            ModEntry.mute.Value = false;
        }
        private bool CantScrollDown()
        {
            return (scrolled * cols + fit) / cols >= (int)Math.Ceiling(count / (float)cols);
        }

        public override void draw(SpriteBatch b)
        {

            //menu.draw(b);
            b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), Color.Black * 0.5f);

            var descriptionName = Game1.player.modData.ContainsKey(UI_HAND_MIRROR_FILTER_BUTTON) ? Game1.player.modData[UI_HAND_MIRROR_FILTER_BUTTON] : String.Empty;
            string name = null;
            switch (descriptionName)
            {
                case ACCESSORY_FILTER_BUTTON:
                    name = mh.Translation.Get("ui.fashion_sense.title.accessory");
                    break;
                case HAT_FILTER_BUTTON:
                    name = mh.Translation.Get("ui.fashion_sense.title.hat");
                    break;
                case SHIRT_FILTER_BUTTON:
                    name = mh.Translation.Get("ui.fashion_sense.title.shirt");
                    break;
                case PANTS_FILTER_BUTTON:
                    name = mh.Translation.Get("ui.fashion_sense.title.pants");
                    break;
                case SLEEVES_FILTER_BUTTON:
                    name = mh.Translation.Get("ui.fashion_sense.title.sleeves");
                    break;
                case SHOES_FILTER_BUTTON:
                    name = mh.Translation.Get("ui.fashion_sense.title.shoes");
                    break;
                case BODY_FILTER_BUTTON:
                    name = mh.Translation.Get("ui.fashion_sense.title.body");
                    break;
                default:
                    name = mh.Translation.Get("ui.fashion_sense.title.hair");
                    break;
            }

            SpriteText.drawStringWithScrollCenteredAt(b, name, xPositionOnScreen + width / 2, yPositionOnScreen);
            Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true, null, false, true, -1, -1, -1);
            foreach (var cc in entries)
            {
                var bounds = GetRect(cc);
                if (bounds.Contains(Game1.getMouseX(true), Game1.getMouseY(true)))
                {
                    b.Draw(Game1.staminaRect, bounds, Color.White * 0.75f);
                }
                else if (cc.name == lastIndex.ToString())
                {
                    b.Draw(Game1.staminaRect, bounds, Color.White * 0.5f);
                }
                cc.draw(b);
            }
            if (scrolled > 0)
            {
                b.Draw(Game1.mouseCursors, new Rectangle(xPositionOnScreen + width - 68, yPositionOnScreen + 104, 24, 24), new Rectangle(421, 459, 12, 12), Color.White);
            }
            if (!CantScrollDown())
            {
                b.Draw(Game1.mouseCursors, new Rectangle(xPositionOnScreen + width - 68, yPositionOnScreen + height - 64, 24, 24), new Rectangle(421, 472, 12, 12), Color.White);
            }
            leftButton.draw(b);
            rightButton.draw(b);
            drawMouse(b);
        }

        private Rectangle GetRect(ClickableTextureComponent cc)
        {
            return cc.bounds;
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            foreach (var cc in entries)
            {
                if(GetRect(cc).Contains(x, y))
                {
                    var i = int.Parse(cc.name);
                    DoChange(i);
                    Game1.playSound("bigSelect");
                    Game1.activeClickableMenu = menu;
                    return;
                }
            }
            int dir;
            if (leftButton.containsPoint(x, y))
            {
                dir = 1;
            }
            else if (rightButton.containsPoint(x, y))
            {
                dir = -1;
            }
            else return;
            farmer.faceDirection((farmer.FacingDirection + dir + 4) % 4);
            farmer.FarmerSprite.StopAnimation();
            farmer.completelyStopAnimatingOrDoingAction();
            Game1.playSound("pickUpItem", null);
            CreatePreviews();
            RebuildComponents();
        }
        public override void receiveKeyPress(Keys key)
        {
            if (key == Keys.None)
            {
                return;
            }
            if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && this.readyToClose())
            {
                Game1.playSound("bigDeSelect");
                Game1.activeClickableMenu = menu;
                return;
            }
        }
        public override void receiveScrollWheelAction(int direction)
        {
            var newScrolled = scrolled - Math.Sign(direction);
            if (newScrolled < 0)
            {
                return;
            }
            if (newScrolled > scrolled && CantScrollDown())
            {
                return;
            }
            scrolled = newScrolled;
            Game1.playSound("shiny4");
            RebuildComponents();
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            menu.gameWindowSizeChanged(oldBounds, newBounds);
            base.gameWindowSizeChanged(oldBounds, newBounds);
            RebuildComponents();
        }
    }
}