using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace Alarms
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
		public string DefaultSound { get; set; } = "rooster";
		public KeybindList MenuButton { get; set; } = new KeybindList(new Keybind(SButton.LeftShift, SButton.OemPipe));
	}
}
