using StardewModdingAPI;

namespace BetterElevator
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
		public bool Unrestricted { get; set; } = false;
		public SButton ModKey { get; set; } = SButton.LeftShift;
	}
}
