namespace ZombieOutbreak
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
		public int DailyZombificationChance { get; set; } = 10;
		public int InfectionRadius { get; set; } = 128;
		public int InfectionChancePerSecond { get; set; } = 5;
		public int GreenTint { get; set; } = 80;
	}
}
