using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

        public bool GetPhoneRotated()
        {
            return PhoneRotated;
        }
        public void SetPhoneRotated(bool value)
        {
            PhoneRotated = value;
        }
        public bool GetPhoneOpened()
        {
            return PhoneOpened;
        }
        public void SetPhoneOpened(bool value)
        {
            PhoneOpened = value;
        }
        public bool GetAppRunning()
        {
            return AppRunning;
        }
        public void SetAppRunning(bool value)
        {
            AppRunning = value;
        }

        public bool PhoneRotated  
        {
            get
            {
                return ModEntry.phoneRotated;
            }
            set
            {
                ModEntry.phoneRotated = value;
            }
        }
        public bool PhoneOpened  
        {
            get
            {
                return ModEntry.phoneOpen;
            }
            set
            {
                ModEntry.TogglePhone(value);
            }
        }
        public bool AppRunning  
        {
            get
            {
                return ModEntry.appRunning;
            }
            set
            {
                ModEntry.ToggleApp(value);
            }
        }
    }
}