using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace PersonalTravelingCart
{
	public class TravelingCart
	{
		[JsonIgnore]
		public Texture2D spriteSheet;
		public string spriteSheetPath;
		public string mapPath;
		public Point entryTile = new(6, 6);
		public DirectionData left = new() {
			direction = 3,
			backRect = new(0, 0, 128, 128),
			middleRect = new(0, 128, 128, 128),
			frontRect = new(0, 256, 128, 128),
			clickRect = new(17, 26, 95, 57),
			hitchRect = new(0, 63, 19, 13),
			collisionRect = new(17, 50, 95, 40),
			cartOffset = new (40, -296),
			playerOffset = new (-74, -20)
		};
		public DirectionData right = new()
		{
			direction = 1,
			backRect = new(0, 384, 128, 128),
			middleRect = new(0, 512, 128, 128),
			frontRect = new(0, 640, 128, 128),
			clickRect = new(17, 26, 95, 57),
			hitchRect = new(110, 63, 19, 13),
			collisionRect = new(17, 50, 95, 40),
			cartOffset = new(-444, -296),
			playerOffset = new(74, -20)
		};
		public DirectionData up = new()
		{
			direction = 0,
			backRect = new(0, 768, 128, 128),
			middleRect = Rectangle.Empty,
			frontRect = new(0, 896, 128, 128),
			clickRect = new(42, 27, 44, 87),
			hitchRect = new(52, 0, 23, 27),
			collisionRect = new(42, 51, 44, 67),
			cartOffset = new(-204, -60),
			playerOffset = new(0, -320)
		};
		public DirectionData down = new()
		{
			direction = 2,
			backRect = new(0, 1024, 128, 128),
			middleRect = Rectangle.Empty,
			frontRect = new(0, 1152, 128, 128),
			clickRect = new(42, 14, 44, 87),
			hitchRect = new(52, 88, 23, 27),
			collisionRect = new(42, 38, 44, 76),
			cartOffset = new(-204, -472),
			playerOffset = new(0, 192)
		};

		public DirectionData GetDirectionData(int direction)
		{
			return direction switch
			{
				0 => up,
				1 => right,
				2 => down,
				_ => left,
			};
		}

		public class DirectionData
		{
			public int direction = 0;
			public Rectangle backRect = new();
			public Rectangle middleRect = new();
			public Rectangle frontRect = new();
			public Rectangle clickRect = new();
			public Rectangle hitchRect = new();
			public Rectangle collisionRect = new();
			public Vector2 cartOffset = new();
			public Vector2 playerOffset = new();
			public int frames = 2;
			public int frameRate = 64;
		}
	}
}
