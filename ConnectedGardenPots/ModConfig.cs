using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace ConnectedGardenPots
{
	public class ModConfig
	{
		public bool EnableMod { get; set; } = true;
		public KeybindList DisconnectKeys { get; set; } = new KeybindList(new Keybind(SButton.LeftControl, SButton.J));
	}
}
