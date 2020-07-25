using Microsoft.Xna.Framework.Graphics;
using System;

namespace MobilePhone
{
    public class MobileApp
    {
        public string name;
        public string keyPress = null;
        public Action action;
        public Texture2D icon;

        public MobileApp(string name, Action action, Texture2D icon)
        {
            this.name = name;
            this.action = action;
            this.icon = icon;
        }
        public MobileApp(string name, string keyPress, Texture2D icon)
        {
            this.name = name;
            this.keyPress = keyPress;
            this.icon = icon;
        }
    }
}