using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.Menus;
using StardewValley.Objects;

namespace AllChestsMenu
{
	public class ChestData
	{
		public int originalIndex;
		public int index;
		public string location;
		public string name;
		public string label;
		public bool collapsed;
		public Chest chest;
		public ChestMenu menu;
		public List<ClickableTextureComponent> inventoryButtons = new();
		public Vector2 tile;
	}
}
