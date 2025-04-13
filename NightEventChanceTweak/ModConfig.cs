namespace NightEventChanceTweak
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
		public bool IgnoreEventConditions { get; set; } = false;
		public bool CumulativeChance { get; set; } = false;
		public float CropFairyChance { get; set; } = 1;
		public float WitchChance { get; set; } = 1;
		public float MeteorChance { get; set; } = 1;
		public float StoneOwlChance { get; set; } = 0.5f;
		public float StrangeCapsuleChance { get; set; } = 0.8f;
	}
}
