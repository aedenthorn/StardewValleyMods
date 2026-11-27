using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;
using System.Linq;

namespace EventCreator
{
    public class EventCreatorMenu : IClickableMenu
    {
        private DropDown locationDropDown;
        public DropDown eventDropDown;
        public string currentLocation;
        public Dictionary<string, string> currentEvents;
        public EventData currentEvent;
        public string newString;
        public EventCreatorMenu() : base()
        {
            newString = ModEntry.SHelper.Translation.Get("new");
            RebuildElements();
        }

        public void RebuildElements()
        {
            width = Game1.uiViewport.Width + spaceToClearSideBorder * 2 + borderWidth * 2;
            height = Game1.uiViewport.Height + spaceToClearTopBorder;
            List<string> locations = Game1.locations.Select(l => l.Name).ToList();
            locations.Sort();
            locationDropDown = new DropDown(this, locations, xPositionOnScreen + spaceToClearSideBorder, yPositionOnScreen + 16, 100, 44);
        }


        public override void draw(SpriteBatch spriteBatch)
        {
            Game1.drawDialogueBox(-spaceToClearSideBorder, 16 - spaceToClearTopBorder, width, height, false, true);
            locationDropDown.draw(spriteBatch, xPositionOnScreen + spaceToClearSideBorder, yPositionOnScreen + 16);
            eventDropDown?.draw(spriteBatch, locationDropDown.bounds.X, locationDropDown.bounds.Y);
            drawMouse(spriteBatch, true);
        }


        public override void receiveKeyPress(Keys key)
        {
            base.receiveKeyPress(key);

        }
        public override void receiveScrollWheelAction(int direction)
        {
        }
        public override void leftClickHeld(int x, int y)
        {
            base.leftClickHeld(x, y);

            if (locationDropDown.clicked || locationDropDown.dropDownBounds.Contains(x, y))
            {
                locationDropDown.leftClickHeld(x, y);
            }
            else if (eventDropDown?.clicked == true || eventDropDown?.dropDownBounds.Contains(x, y) == true)
            {
                eventDropDown.leftClickHeld(x, y);
            }
        }
        public override void releaseLeftClick(int x, int y)
        {
            base.releaseLeftClick(x, y);
            if (locationDropDown.clicked)
            {
                locationDropDown.leftClickReleased(x, y);
                FillEvents();
            }
            else if (eventDropDown?.clicked == true)
            {
                eventDropDown.leftClickReleased(x, y);
                LoadEvent();
            }
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (locationDropDown.bounds.Contains(x, y))
            {
                locationDropDown.receiveLeftClick(x, y);
                FillEvents();
            }
            else if (eventDropDown?.bounds.Contains(x, y) == true)
            {
                eventDropDown.receiveLeftClick(x, y);
                LoadEvent();
            }
            else
            {
                base.receiveLeftClick(x, y, playSound);
            }
        }
        public void FillEvents()
        {
            if (locationDropDown.selectedOption < 0)
            {
                eventDropDown = null;

                return;
            }
            var which = locationDropDown.GetCurrentItem();
            if(which != currentLocation)
            {
                currentLocation = which;
                string assetName = "Data\\Events\\" + which;
                if (!Game1.content.DoesAssetExist<Dictionary<string, string>>(assetName))
                {
                    eventDropDown = null;
                    currentEvents = null;
                }
                else
                {
                    currentEvents = Game1.content.Load<Dictionary<string, string>>(assetName);
                }
            }
            if(currentEvents != null)
            {
                var list = currentEvents.Keys.ToList().Select(s => s.IndexOf("/") < 0 ? s : s.Substring(0, s.IndexOf("/"))).ToList();
                list.Add(newString);
                eventDropDown = new DropDown(this, list, xPositionOnScreen + locationDropDown.bounds.Width, yPositionOnScreen + 16, 100, 44);
            }
        }

        public void LoadEvent()
        {
            var e = eventDropDown.GetCurrentItem();
            if (e == newString)
            {
                currentEvent = new EventData();
            }
            else 
            {
                currentEvent = new EventData(currentEvents.First(p => p.Key == e || p.Key.StartsWith(e + "/")));
                    
            }
        }
    }
}