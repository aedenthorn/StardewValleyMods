using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace FishSpotBait
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
		public SButton ModKey { get; set; } = SButton.None;
		public int RandomRadius { get; set; } = 1;
		public int MaxRange { get; set; } = 6;
	}
}
