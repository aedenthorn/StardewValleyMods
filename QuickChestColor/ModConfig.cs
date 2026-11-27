using StardewModdingAPI;

namespace QuickChestColor
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
		public bool HalfByDefault { get; set; } = false;
		public SButton NextKey { get; set; } = SButton.Up;
		public SButton PrevKey { get; set; } = SButton.Down;
		public SButton ModKey { get; set; } = SButton.LeftAlt;
		public SButton CopyPasteKey { get; set; } = SButton.MouseMiddle;
    }
}
