using StardewModdingAPI;
using System.Collections.Generic;

namespace PrismaticFlowers
{
	public enum PrismaticPattern
	{
		None,
		Random,
		Up,
		Down,
		Left,
		Right
	}


    public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
		public bool Debug { get; set; } = false;
		public int PrismaticChance { get; set; } = 5;
		public List<string> Ignore { get; set; } = new();
		public float ObjectSpeed { get; set; } = 1;
		public float CropSpeed { get; set; } = 1;
		public float PriceMultiplier { get; set; } = 2f;
		public PrismaticPattern CropPattern { get; set; } = PrismaticPattern.None;
		public PrismaticPattern ObjectPattern { get; set; } = PrismaticPattern.None;
    }
}
