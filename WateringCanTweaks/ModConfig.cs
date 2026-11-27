namespace WateringCanTweaks
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
		public bool WaterAdjacentTiles { get; set; } = true;
		public float WateringTileMultiplier { get; set; } = 1;
		public float VolumeMultiplier { get; set; } = 1;
		public float StaminaUseMultiplier { get; set; } = 1;
		public bool StrafeWhileWatering { get; set; } = true;
		public bool AutoEndWateringAnimation { get; set; } = true;
	}
}
