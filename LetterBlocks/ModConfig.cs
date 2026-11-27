using StardewModdingAPI;

namespace LetterBlocks
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
		public SButton ShiftKey { get; set; } = SButton.LeftShift;
		public SButton ColorKey { get; set; } = SButton.LeftControl;
    }
}
