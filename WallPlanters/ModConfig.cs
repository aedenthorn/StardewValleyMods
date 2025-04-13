
using StardewModdingAPI;

namespace WallPlanters
{
	public class ModConfig
	{
		public bool EnableMod { get; set; } = true;
		public SButton ContentOffsetKey { get; set; } = SButton.LeftShift;
		public SButton UpKey { get; set; } = SButton.Up;
		public SButton DownKey { get; set; } = SButton.Down;
		public int OffsetY { get; set; } = 32;
		public int InnerOffsetY { get; set; } = 0;
	}
}
