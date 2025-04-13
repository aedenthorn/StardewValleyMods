using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace RainbowTrail
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
		public KeybindList ToggleKeys { get; set; } = new KeybindList(SButton.NumPad1);
		public string EnableSound { get; set; } = "yoba";
		public int MoveSpeed { get; set; } = 10;
		public int StaminaUse { get; set; } = 0;
		public bool UseMana { get; set; } = true;
		public int MaxDuration { get; set; } = 500;
	}
}
