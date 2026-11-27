namespace MeteorDefence
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
		public bool StrikeAnywhere { get; set; } = false;
		public int MinimumMeteorites { get; set; } = 1;
		public int MaximumMeteorites { get; set; } = 5;
		public int MeteoritesDestroyedPerObject { get; set; } = 1;
		public string DefenceSound { get; set; } = "debuffSpell";
		public string DestructionSound { get; set; } = "explosion";
		public string ImpactSound { get; set; } = "aedenthorn.MeteorDefence_meteoriteImpact";
		public string SkillsRequired { get; set; } = "Mining 6";
	}
}
