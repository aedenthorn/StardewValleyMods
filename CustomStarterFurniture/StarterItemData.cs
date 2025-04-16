using System.Collections.Generic;

namespace CustomStarterFurniture
{
	public class StarterFurnitureData
	{
		public int FarmType = -1;
		public bool Clear;
		public List<FurnitureData> Furniture;
		public List<BigCraftableData> BigCraftable;

		public class FurnitureData
		{
			public string NameOrIndex;
			public string HeldObjectType;
			public string HeldObjectNameOrIndex;
			public int X;
			public int Y;
			public int Rotation;
		}

		public class BigCraftableData
		{
			public string NameOrIndex;
			public int X;
			public int Y;
		}
	}
}
