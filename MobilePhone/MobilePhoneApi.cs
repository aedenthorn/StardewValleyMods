using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;

namespace MobilePhone
{
    public class MobilePhoneApi
    {

        public bool AddApp(string id, string name, Action action, Texture2D icon)
        {
            if (ModEntry.apps.ContainsKey(name))
            {
                return false;
            }
            ModEntry.apps.Add(id, new MobileApp(name, action, icon));
            return true;
        }

        public Vector2 GetScreenPosition()
        {
            return ModEntry.GetScreenPosition();
        }
        public Vector2 GetScreenSize()
        {
            return ModEntry.GetScreenSize();
        }

        public bool AddOnPhoneRotated(EventHandler action)
        {
            ModEntry.OnScreenRotated += action;
            return true;
        }

        public Texture2D GetBackgroundTexture(bool rotated)
        {
            return rotated ? ModEntry.backgroundRotatedTexture : ModEntry.backgroundTexture;
        }

        public bool GetPhoneRotated()
        {
            return ModEntry.phoneRotated;
        }
        public void SetPhoneRotated(bool value)
        {
            ModEntry.phoneRotated = value;
        }
        public bool GetPhoneOpened()
        {
            return ModEntry.phoneOpen;
        }
        public void SetPhoneOpened(bool value)
        {
            ModEntry.TogglePhone(value);
        }
        public bool GetAppRunning()
        {
            return ModEntry.appRunning;
        }
        public void SetAppRunning(bool value)
        {
            ModEntry.appRunning = value;
        }

    }
}