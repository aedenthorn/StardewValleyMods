using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace WeaponsIgnoreGrass
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
		public bool WeaponsIgnoreGrass { get; set; } = true;
		public bool ScythesIgnoreGrass { get; set; } = true;
		public bool ShowEnabledMessage { get; set; } = true;
		public bool ShowDisabledMessage { get; set; } = true;
		public KeybindList ToggleKeys { get; set; } = new KeybindList(new Keybind(SButton.RightControl));
	}
}
