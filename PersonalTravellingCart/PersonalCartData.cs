using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace PersonalTravellingCart
{
    public class PersonalCartData
    {
        [JsonIgnore]
        public Texture2D spriteSheet;

        public string mapPath;
        public Point entryTile = new Point(6, 6);
        public string spriteSheetPath;
        public DirectionData left = new() {
            backRect = new(0, 0, 128, 128),
            frontRect = new(0, 128, 128, 128),
            clickRect = new(17, 26, 95, 57),
            cartOffset= new (40, -296),
            playerOffset = new (-74, -20)
        };
        public DirectionData right = new()
        {
            backRect = new(0, 256, 128, 128),
            frontRect = new(0, 384, 128, 128),
            clickRect = new(17, 26, 95, 57),
            cartOffset = new(-444, -296),
            playerOffset = new(74, -20)
        };
        public DirectionData up = new()
        {
            backRect = new(0, 512, 128, 128),
            frontRect = new(0, 640, 128, 128),
            clickRect = new(42, 27, 44, 87),
            cartOffset = new(-204, -60),
            playerOffset = new(0, -320)
        };
        public DirectionData down = new()
        {
            backRect = new(0, 768, 128, 128),
            frontRect = new(0, 896, 128, 128),
            clickRect = new(42, 14, 44, 87),
            cartOffset = new(-204, -472),
            playerOffset = new(0, 192)
        };
    }

    public class DirectionData
    {
        public Rectangle backRect = new();
        public Rectangle frontRect = new();
        public Rectangle clickRect = new();
        public Vector2 cartOffset = new();
        public Vector2 playerOffset = new();
        public int frames = 2;
        public int frameRate = 64;
    }
}