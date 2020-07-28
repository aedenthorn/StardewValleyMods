using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobilePhone
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public SButton OpenPhoneKey { get; set; } = SButton.End;
        public SButton RotatePhoneKey { get; set; } = SButton.PageDown;
        public bool ShowPhoneIcon { get; set; } = true;
        public string PhoneTexturePath { get; set; } = "phone.png";
        public string BackgroundTexturePath { get; set; } = "background.png";
        public string PhoneRotatedTexturePath { get; set; } = "phone_landscape.png";
        public string BackgroundRotatedTexturePath { get; set; } = "background_landscape.png";
        public string UpArrowTexturePath { get; set; } = "up_arrow.png";
        public string DownArrowTexturePath { get; set; } = "down_arrow.png";
        public string iconTexturePath { get; set; } = "phone_icon.png";
        public string PhonePosition { get; set; } = "bottom-right"; // mid, top-left, bottom-right, etc.
        public string PhoneIconPosition { get; set; } = "top-right"; // mid, top-left, bottom-right, etc.
        public int PhoneWidth{ get; set; } = 286;
        public int PhoneHeight { get; set; } = 503;
        public int ScreenWidth { get; set; } = 260;
        public int ScreenHeight { get; set; } = 429;
        public int PhoneIconWidth { get; set; } = 36;
        public int PhoneIconHeight { get; set; } = 64;
        public int PhoneOffsetX { get; set; } = -64;
        public int PhoneOffsetY { get; set; } = -64;
        public int ScreenOffsetX { get; set; } = 13;
        public int ScreenOffsetY { get; set; } = 37;
        public int PhoneIconOffsetX { get; set; } = -5;
        public int PhoneIconOffsetY { get; set; } = 260;
        public int PhoneRotatedWidth{ get; set; } = 503;
        public int PhoneRotatedHeight { get; set; } = 286;
        public int ScreenRotatedWidth { get; set; } = 429;
        public int ScreenRotatedHeight { get; set; } = 260;
        public int PhoneRotatedOffsetX { get; set; } = 0;
        public int PhoneRotatedOffsetY { get; set; } = 0;
        public int ScreenRotatedOffsetX { get; set; } = 37;
        public int ScreenRotatedOffsetY { get; set; } = 13;
        public int IconWidth { get; set; } = 48;
        public int IconHeight { get; set; } = 48;
        public int IconMarginX { get; set; } = 12;
        public int IconMarginY { get; set; } = 12;
        public int ContactWidth { get; set; } = 64;
        public int ContactHeight { get; set; } = 64;
        public int ContactMarginX { get; set; } = 16;
        public int ContactMarginY { get; set; } = 16;
        public int ContactArrowWidth { get; set; } = 16;
        public int ContactArrowHeight { get; set; } = 16;
        public Color PhoneBookBackgroundColor { get; set; } = Color.White;
        public int MinPointsToCall { get; set; } = 1000;
        public bool ShowNamesInPhoneBook { get; set; } = true;
        public bool UseRealNamesInPhoneBook { get; set; } = true;
        public int ToolTipDelayTicks { get; set; } = 40;
    }
}
