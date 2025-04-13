using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace ModifyThis
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
		public SButton WizardKey { get; set; } = SButton.Pause;
	}
}
