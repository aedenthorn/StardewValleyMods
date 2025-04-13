using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace BeePaths
{
	public class HiveData
	{
		public Vector2 hiveTile;
		public Vector2 cropTile;
		public bool isIndoorPot;
		public List<BeeData> bees;

		public HiveData(Vector2 hiveTile, Vector2 cropTile, bool isIndoorPot)
		{
			this.hiveTile = hiveTile;
			this.cropTile = cropTile;
			this.isIndoorPot = isIndoorPot;
			bees = new();
		}
	}

	public class BeeData
	{
		public Vector2 position;
		public Vector2 startPosition;
		public Vector2 endPosition;
		public float speed;
		public bool isGoingToFlower;
	}
}
