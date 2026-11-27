using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace Email
{
    public partial class ModEntry
    {
        public bool opening;

        public void OpenEmailApp()
        {
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            api.SetAppRunning(true);
            api.SetRunningApp(Helper.ModRegistry.ModID);
            Helper.Events.Display.RenderedWorld += Display_RenderedWorld;
            opening = true;
        }

        public void CloseApp()
        {
            api.SetAppRunning(false);
            api.SetRunningApp(null);
            Helper.Events.Input.ButtonPressed -= Input_ButtonPressed;
            Helper.Events.Display.RenderedWorld -= Display_RenderedWorld;
        }


        public void ClickRow(Point mousePos)
        {
            int idx = (int)((mousePos.Y - api.GetScreenPosition().Y - Config.MarginY - offsetY - Config.AppHeaderHeight) / (Config.MarginY + Config.AppRowHeight));
            Monitor.Log($"clicked index: {idx}");
            if (idx < Game1.player.mailbox.Count && idx >= 0)
            {
                OpenMail(idx);
            }
        }
    }
}