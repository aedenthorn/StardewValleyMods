using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace PushPull
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
        public Keybind Key { get; set; } = new(SButton.LeftControl);
		public string Sound { get; set; } = "dirtyHit";
		public int Speed { get; set; } = 2;
		public int Delay { get; set; } = 30;
		public bool Pull { get; set; } = true;
		public bool Rocks { get; set; } = true;
		public bool Sticks { get; set; } = true;
		public bool Clumps { get; set; } = true;
		public bool Constructs { get; set; } = true;
	}
}
