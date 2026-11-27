using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace WeaponsIgnoreGrass
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
		public bool IgnoreEnabled { get; set; } = true;
		public bool WeaponsIgnoreGrass { get; set; } = true;
		public bool ScythesIgnoreGrass { get; set; } = false;
		public bool ShowEnabledMessage { get; set; } = true;
		public bool ShowDisabledMessage { get; set; } = true;
		public Keybind ToggleKey { get; set; } = new Keybind(SButton.RightControl);
	}
}
