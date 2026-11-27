using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace CustomMounts
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
		public bool AllowMultipleMounts { get; set; } = true;
		public SButton RenameModKey { get; internal set; } = SButton.LeftAlt;
    }
}
