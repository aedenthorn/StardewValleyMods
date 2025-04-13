namespace BirthdayBuff
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
		public int Farming { get; set; } = 1;
		public int Fishing { get; set; } = 1;
		public int Mining { get; set; } = 1;
		public int Luck { get; set; } = 1;
		public int Foraging { get; set; } = 1;
		public int MaxStamina { get; set; } = 30;
		public int MagneticRadius { get; set; } = 1;
		public int Speed { get; set; } = 1;
		public int Defense { get; set; } = 1;
		public int Attack { get; set; } = 1;
		public string Sound { get; set; } = "yoba";
		public string GlowColor { get; set; } = "White";
		public float GlowRate { get; set; } = 0.005f;
	}
}
