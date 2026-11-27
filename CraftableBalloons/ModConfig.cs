using StardewModdingAPI;

namespace CraftableBalloons
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
		public int PrismaticChance { get; set; } = 5;
		public float PrismaticSpeed { get; set; } = 1;
		public SButton ModKey { get; set; } = SButton.LeftShift;
    }
}
