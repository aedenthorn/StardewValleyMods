using Microsoft.Xna.Framework.Graphics;
using System;

namespace MobilePhone
{
    public class MobileApp
    {
        public string name;
        public string keyPress = null;
        public bool closePhone;
        public Action action;
        public Texture2D icon;
        public string dllName;
        public string className;
        public string methodName;

        public MobileApp(string name, Action action, Texture2D icon)
        {
            this.name = name;
            this.action = action;
            this.icon = icon;
        }

        public MobileApp(string name, string keyPress, bool closePhone, Texture2D icon)
        {
            this.name = name;
            this.keyPress = keyPress;
            this.closePhone = closePhone;
            this.icon = icon;
        }
        public MobileApp(string name, string dllName, string className, string methodName, string keyPress, Texture2D icon)
        {
            this.name = name;
            this.dllName = dllName;
            this.className = className;
            this.methodName = methodName;
            this.keyPress = keyPress;
            this.icon = icon;
        }
    }
}