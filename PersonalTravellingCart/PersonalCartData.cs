using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace PersonalTravellingCart
{
    public class PersonalCartData
    {
        [JsonIgnore]
        public Texture2D spriteSheet;

        public string spriteSheetPath;
        public string mapPath;
        public Point entryTile = new Point(6, 6);
        public DirectionData left = new() {
            backRect = new(0, 0, 128, 128),
            frontRect = new(0, 128, 128, 128),
            clickRect = new(17, 26, 95, 57),
            hitchRect = new(0, 63, 19, 13),
            cartOffset = new (40, -296),
            playerOffset = new (-74, -20)
        };
        public DirectionData right = new()
        {
            backRect = new(0, 256, 128, 128),
            frontRect = new(0, 384, 128, 128),
            clickRect = new(17, 26, 95, 57),
            hitchRect = new(110, 63, 19, 13),
            cartOffset = new(-444, -296),
            playerOffset = new(74, -20)
        };
        public DirectionData up = new()
        {
            backRect = new(0, 512, 128, 128),
            frontRect = new(0, 640, 128, 128),
            clickRect = new(42, 27, 44, 87),
            hitchRect = new(52, 0, 23, 27),
            cartOffset = new(-204, -60),
            playerOffset = new(0, -320)
        };
        public DirectionData down = new()
        {
            backRect = new(0, 768, 128, 128),
            frontRect = new(0, 896, 128, 128),
            clickRect = new(42, 14, 44, 87),
            hitchRect = new(52, 229, 23, 27),
            cartOffset = new(-204, -472),
            playerOffset = new(0, 192)
        };
        public DirectionData GetDirectionData(int direction)
        {
            switch (direction)
            {
                case 0:
                    return up;
                case 1:
                    return right;
                case 2:
                    return down;
                default:
                    return left;
            }
        }
    }

    public class DirectionData
    {
        public Rectangle backRect = new();
        public Rectangle frontRect = new();
        public Rectangle clickRect = new();
        public Rectangle hitchRect = new();
        public Vector2 cartOffset = new();
        public Vector2 playerOffset = new();
        public int frames = 2;
        public int frameRate = 64;
    }
}