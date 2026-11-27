using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace YetAnotherJumpMod
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
		public SButton JumpButton { get; set; } = SButton.Space;
		public bool PlayJumpSound { get; set; } = true;
		public bool DrawHorseShadow { get; set; } = true;
		public string JumpSound { get; set; } = "dwop";
		public int MaxJumpDistance { get; set; } = 10;
		public float OrdinaryJumpHeight { get; set; } = 8f;
		public Vector2 HorseShadowOffset { get; set; } = Vector2.Zero;
	}
}
