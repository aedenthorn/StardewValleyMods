using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace RoastingMarshmallows
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
		public KeybindList RoastKey { get; set; } = new KeybindList(SButton.Space);
		public int RoastFrames { get; set; } = 1440;
		public int RawMarshmallowHealth { get; set; } = 5;
		public int CookedMarshmallowHealth { get; set; } = 15;
		public int BurntMarshmallowHealth { get; set; } = 10;
		public List<string> Campfires { get; set; } = new()
		{
            "(BC)146"
        };
    }
}
