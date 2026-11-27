using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace InventoryIndicators
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
		public bool ShowOnlyInMenu { get; set; } = true;
		public bool ShowFavorites { get; set; } = true;
		public bool ShowUniversalFavorites { get; set; } = true;
		public bool ShowUngiftedFavorites { get; set; } = false;
		public bool ShowBundleItems { get; set; } = true;
		public bool ShowPlantableSeeds { get; set; } = true;
		public int OutlineWidth { get; set; } = 4;
		public float PlantableOpacity { get; set; } = 1f;
		public Color JunimoColor { get; set; } = new Color(0, 1f, 0, 1f);
		public Dictionary<string, Color> Colors { get; set; } = new Dictionary<string, Color>();
	}
}
