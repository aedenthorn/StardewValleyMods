
using Microsoft.Xna.Framework;

namespace DoorFurniture
{
    public class DoorData
    {
        public Rectangle[] Bounds { get; set; } = new Rectangle[]
        {
            new(0, 48, 64, 16),
            new(48, 0, 16, 64),
            new(0, 0, 64, 16),
            new(0, 0, 16, 64)
        };
        public string Type { get; set; } = ModEntry.SHelper.Translation.Get("door-type");
        public string Description { get; set; } = ModEntry.SHelper.Translation.Get("door-description");
        public string KeyItem { get; set; } = ModEntry.keyId;
        public string KeyRingItem { get; set; } = ModEntry.keyringId;
        public string OpenSound { get; set; } = "doorOpen";
        public string CloseSound { get; set; } = "doorClose";
        public string LockSound { get; set; } = "openBox";
        public string UnlockSound { get; set; } = "openBox";
        public bool AutoOpen { get; set; } = false;
        public int AutoCloseDelay { get; set; } = -1;
        public bool Lockable { get; set; } = false;
        public Point ColorSpriteOffset { get; set; }
        public bool Colorable { get; set; } = false;
        public bool DefaultUncolored { get; set; } = true;
        public Color DefaultColor { get; set; } = Color.White;
        public Color[] Colors { get; set; }
    }
}