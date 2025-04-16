namespace Dreamscape
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
		public int MaxTrees { get; set; } = 20;
		public int TreeGrowthStage { get; set; } = 1;
		public int TreeSpawnChance { get; set; } = 2;
		public int ObjectSpawnChance { get; set; } = 2;
	}
}
