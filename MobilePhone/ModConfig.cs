using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.IO;

namespace MobilePhone
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public SButton OpenPhoneKey { get; set; } = SButton.End;
        public SButton RotatePhoneKey { get; set; } = SButton.PageDown;
        public bool ShowPhoneIcon { get; set; } = true;
        public string PhoneSkinPath { get; set; } = Path.Combine("assets", "skins", "black.png");
        public string BackgroundPath { get; set; } = Path.Combine("assets", "backgrounds", "clouds.png");
        public string UpArrowTexturePath { get; set; } = "up_arrow.png";
        public string DownArrowTexturePath { get; set; } = "down_arrow.png";
        public string iconTexturePath { get; set; } = "phone_icon.png";
        public string PhonePosition { get; set; } = "bottom-right"; // mid, top-left, bottom-right, etc.
        public string PhoneIconPosition { get; set; } = "top-right"; // mid, top-left, bottom-right, etc.
        public int ToolTipDelayTicks { get; set; } = 40;
        public int TicksToMoveAppIcon { get; set; } = 20;
        public string[] AppList { get; set; } = new string[0];

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

        // Phone book

        public int ContactWidth { get; set; } = 64;
        public int ContactHeight { get; set; } = 64;
        public int ContactMarginX { get; set; } = 16;
        public int ContactMarginY { get; set; } = 16;
        public int ContactArrowWidth { get; set; } = 16;
        public int ContactArrowHeight { get; set; } = 16;
        public Color PhoneBookBackgroundColor { get; set; } = Color.White;
        public Color PhoneBookHeaderColor { get; set; } = Color.DeepSkyBlue;
        public Color PhoneBookHeaderTextColor { get; set; } = Color.White;
        public int AppHeaderHeight { get; set; } = 32;
        public float HeaderTextScale { get; set; } = 0.6f;
        public int MinPointsToCall { get; set; } = 1000;
        public bool ShowNamesInPhoneBook { get; set; } = true;
        public bool UseRealNamesInPhoneBook { get; set; } = true;


        // Themes

        public int ThemeItemWidth { get; set; } = 64;
        public int ThemeItemHeight { get; set; } = 128;
        public int ThemeItemMarginX { get; set; } = 16;
        public int ThemeItemMarginY { get; set; } = 16;

        public Color ThemesHeaderColor { get; set; } = Color.DeepSkyBlue;
        public Color ThemesFooterHighlightColor { get; set; } = Color.AliceBlue;
        public Color ThemesHeaderTextColor { get; set; } = Color.White;
        public Color ThemesHeaderHighlightedTextColor { get; set; } = Color.DarkSlateBlue;
    }
}
