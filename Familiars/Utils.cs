using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;

namespace Familiars
{
    class Utils
    {
		public static Vector2 getAwayFromNPCTrajectory(Microsoft.Xna.Framework.Rectangle monsterBox, NPC who)
		{
			float num = (float)(-(float)(who.GetBoundingBox().Center.X - monsterBox.Center.X));
			float ySlope = (float)(who.GetBoundingBox().Center.Y - monsterBox.Center.Y);
			float total = Math.Abs(num) + Math.Abs(ySlope);
			if (total < 1f)
			{
				total = 5f;
			}
			float x = num / total * (float)(50 + Game1.random.Next(-20, 20));
			ySlope = ySlope / total * (float)(50 + Game1.random.Next(-20, 20));
			return new Vector2(x, ySlope);
		}

		public static bool withinMonsterThreshold(Monster m1, Monster m2, int threshold)
		{
			if (m1.Equals(m2) || m1.Health <= 0 || m2.Health <= 0 || m2.IsInvisible || m2.isInvincible())
				return false;

			Vector2 m1l = m1.getTileLocation();
			Vector2 m2l = m2.getTileLocation();
			return Math.Abs(m2l.X - m1l.X) <= (float)threshold && Math.Abs(m2l.Y - m1l.Y) <= (float)threshold;
		}

		public static bool monstersColliding(Monster m1, Monster m2)
		{
			if (m1.Equals(m2) || m1.Health <= 0 || m2.Health <= 0 || m2.IsInvisible)
				return false;

			Rectangle m1l = m1.GetBoundingBox();
			Rectangle m2l = m2.GetBoundingBox();
			return m1l.Intersects(m2l);
		}

        public static void monsterDrop(Monster familiar, Monster monster, Farmer owner)
        {
			IList<int> objects = monster.objectsToDrop;
			if (Game1.player.isWearingRing(526))
			{
				string result = "";
				Game1.content.Load<Dictionary<string, string>>("Data\\Monsters").TryGetValue(monster.Name, out result);
				if (result != null && result.Length > 0)
				{
					string[] objectsSplit = result.Split(new char[]
					{
						'/'
					})[6].Split(new char[]
					{
						' '
					});
					for (int i = 0; i < objectsSplit.Length; i += 2)
					{
						if (Game1.random.NextDouble() < Convert.ToDouble(objectsSplit[i + 1]))
						{
							objects.Add(Convert.ToInt32(objectsSplit[i]));
						}
					}
				}
			}
			if (objects == null || objects.Count == 0)
				return;

			int objectToAdd = objects[Game1.random.Next(objects.Count)];
			if (objectToAdd < 0)
			{
				familiar.currentLocation.debris.Add(Game1.createItemDebris(new StardewValley.Object(Math.Abs(objectToAdd), Game1.random.Next(1, 4)), familiar.position, Game1.random.Next(4)));
			}
			else
			{
				familiar.currentLocation.debris.Add(Game1.createItemDebris(new StardewValley.Object(Math.Abs(objectToAdd), 1), familiar.position, Game1.random.Next(4)));
			}
		}

        public static Texture2D ColorFamiliar(Texture2D texture, Color mainColor, Color redColor, Color greenColor, Color blueColor)
        {
			Color[] data = new Color[texture.Width * texture.Height];
			texture.GetData(data);
			for(int i = 0; i < data.Length; i++)
            {
				if (data[i] == Color.Transparent)
					continue;

				if (data[i].R == data[i].G && data[i].R == data[i].B && data[i].G == data[i].B)
				{
					data[i] = new Color((int)(mainColor.R * (data[i].R / 255f)), (int)(mainColor.G * (data[i].G / 255f)), (int)(mainColor.B * (data[i].B / 255f)));
				}
				else if (data[i].R == 255)
                {
					data[i] = redColor;
				}
				else if (data[i].G == 255)
                {
					data[i] = greenColor;
				}
				else if (data[i].B == 255)
                {
					data[i] = blueColor;
				}
			}
			texture.SetData(data);
			return texture;
        }
    }
}
