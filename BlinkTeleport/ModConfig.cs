using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace BlinkTeleport
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
		public bool OutdoorsOnly { get; set; } = true;
		public KeybindList BlinkKey { get; set; } = new KeybindList(new Keybind(SButton.LeftControl, SButton.B));
		public bool ActionsEnabled { get; set; } = false;
		public string BlinkSound { get; set; } = "cowboy_explosion";
		public int StaminaUse { get; set; } = 10;
		public bool UseMana { get; set; } = true;
	}
}
