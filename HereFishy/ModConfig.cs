using StardewModdingAPI;

namespace HereFishy
{
	public class ModConfig
	{
		public bool EnableMod { get; set; } = true;
		public bool PlaySound { get; set; } = true;
		public bool PlayGenderedSound { get; set; } = true;
		public float StaminaCost { get; set; } = 7f;
		public bool AllowMovement { get; set; } = false;
		public bool RequireRod { get; set; } = false;
		public SButton TriggerButton { get; set; } = SButton.MouseRight;
	}
}
