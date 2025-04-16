
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace WikiLinks
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
		public KeybindList OpenWikiPageKeys { get; set; } = new KeybindList(new Keybind(SButton.LeftControl, SButton.MouseRight));
	}
}
