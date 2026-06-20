using System.Collections.Generic;
using StardewModdingAPI;

namespace AllChestsMenu
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
		public bool ModToOpen { get; set; } = false;
		public bool LimitToCurrentLocation { get; set; } = false;


		// New independent filter options
		public bool FilterChestLabel { get; set; } = true;
		public bool FilterItemName { get; set; } = false;
		public bool FilterItemCategory { get; set; } = false;
		public bool FilterItemDescription { get; set; } = false;
		public bool EnableControllerKeyboard { get; set; } = true;
		public List<string> SelectedLocations { get; set; } = new();
		public bool IncludeFridge { get; set; } = true;
		public bool IncludeMiniFridges { get; set; } = true;
		public bool IncludeShippingBin { get; set; } = true;
		public bool UnrestrictedShippingBin { get; set; } = false;
		public bool IncludeMiniShippingBins { get; set; } = true;
		public bool IncludeJunimoChests { get; set; } = true;
		public bool IncludeAutoGrabbers { get; set; } = true;
		public AllChestsMenu.Sort CurrentSort { get; set; } = AllChestsMenu.Sort.NA;
		public string SecondarySortingPriority { get; set; } = "Y";
		public bool KeyboardRequireModifierToOpen { get; set; } = false;
		public bool ControllerRequireModifierToOpen { get; set; } = false;
		public SButton KeyboardOpenModifierKey { get; set; } = SButton.LeftShift;
		public SButton ControllerOpenModifierButton { get; set; } = SButton.LeftTrigger;
		public SButton ModKey { get; set; } = SButton.LeftShift;
		public SButton ModKey2 { get; set; } = SButton.LeftControl;
		public SButton SwitchButton { get; set; } = SButton.ControllerBack;
		public SButton KeyboardMenuKey { get; set; } = SButton.F2;
		public SButton ControllerMenuButton { get; set; } = SButton.None;
		public SButton MenuKey { get; set; } = SButton.None;
	}
}
