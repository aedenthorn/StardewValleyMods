using StardewModdingAPI;

namespace CraftAndBuildFromContainers
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
		public SButton ToggleButton { get; set; } = SButton.None;
		public bool EnableForShopTrading { get; set; } = true;
		public bool EnableForCrafting { get; set; } = true;
		public bool EnableForBuilding { get; set; } = true;
		public bool EnableEverywhere { get; set; } = false;
		public bool IncludeFridge { get; set; } = true;
		public bool IncludeMiniFridges { get; set; } = true;
		public bool IncludeShippingBin { get; set; } = false;
		public bool UnrestrictedShippingBin { get; set; } = false;
		public bool IncludeMiniShippingBins { get; set; } = false;
		public bool IncludeJunimoChests { get; set; } = true;
		public bool IncludeAutoGrabbers { get; set; } = true;
	}
}
