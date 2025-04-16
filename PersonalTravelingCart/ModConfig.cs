using StardewModdingAPI;

namespace PersonalTravelingCart
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
		public bool DrawCartExterior { get; set; } = true;
		public bool DrawCartExteriorWeather { get; set; } = true;
		public bool CollisionsEnabled { get; set; } = true;
		public SButton HitchButton { get; set; } = SButton.Back;
		public bool WarpHorsesOnDayStart { get; set; } = false;
		public bool Debug { get; set; } = false;
	}
}
