
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace ChestContentDisplay
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
		public bool WhenFacing { get; set; } = true;
		public bool WhenHovering { get; set; } = true;
        public SButton EnableKey { get; set; } = SButton.None;
        public int DelayTicksHover { get; set; } = 30;
        public int DelayTicksFace { get; set; } = 10;
    }
}
