namespace AFKPause
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
		public bool FreezeGame { get; set; } = true;
		public int TicksTilAFK { get; set; } = 3600;
		public bool ShowAFKText { get; set; } = false;
		public bool WakeOnMouseMove { get; set; } = true;
		public string AFKText { get; set; } = "AFK";
	}
}
