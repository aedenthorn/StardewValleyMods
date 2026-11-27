namespace AdvancedAutoGrabber
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
		public bool Debug { get; set; } = false;
		public bool GrabOnDugUp { get; set; } = true;
		public int GrabRange { get; set; } = -1;
		public int ChestRange { get; set; } = -1;
		public bool SendToChests { get; set; } = false;
		public bool IncludeBuildingChests { get; set; } = false;
		public int OpacityPercent { get; set; } = 75;
    }
}
