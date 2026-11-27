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
            ModEntry.apps[id] = new MobileApp(name, action, icon);
            return true;
        }

        public Vector2 GetScreenPosition()
        {
            return PhoneUtils.GetScreenPosition();
        }
        public Vector2 GetScreenSize()
        {
            return PhoneUtils.GetScreenSize();
        }

        public Vector2 GetScreenSize(bool rotated)
        {
            return PhoneUtils.GetScreenSize(rotated);
        }
        public Rectangle GetPhoneRectangle()
        {
            return ModEntry.phoneRect;
        }
        public Rectangle GetScreenRectangle()
        {
            return ModEntry.screenRect;
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
            PhoneUtils.TogglePhone(value);
        }
        public bool GetAppRunning()
        {
            return ModEntry.appRunning;
        }
        public void SetAppRunning(bool value)
        {
            ModEntry.appRunning = value;
            if (!value)
                ModEntry.runningApp = null;
        }
        public string GetRunningApp()
        {
            return ModEntry.runningApp;
        }
        public void SetRunningApp(string value)
        {
            ModEntry.runningApp = value;
        }

        public void PlayRingTone()
        {
            PhoneUtils.PlayRingTone();
        }
        public void PlayNotificationTone()
        {
            PhoneUtils.PlayNotificationTone();
        }
        public NPC GetCallingNPC()
        {
            return ModEntry.callingNPC;
        }
        public bool IsCallingNPC()
        {
            return ModEntry.callingNPC != null;
        }
    }
}