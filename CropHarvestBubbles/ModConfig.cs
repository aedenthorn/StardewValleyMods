using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace CropHarvestBubbles
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
		public bool Toggled { get; set; } = true;
		public bool RequireKeyPress { get; set; } = false;
		public bool KeyPressToggle { get; set; } = false;
		public bool IgnoreFlowers { get; set; } = true;
		public bool IgnorePots { get; set; } = true;
		public int OpacityPercent { get; set; } = 75;
		public int SizePercent { get; set; } = 100;
		public KeybindList PressKeys { get; set; } = new KeybindList(SButton.None);
	}
}
