namespace Alarms
{
	public class ClockSound
	{
		public string sound = ModEntry.Config.DefaultSound;
		public bool enabled;
		public bool menu;
		public string notification;
		public int hours = 6;
		public int minutes;
		public bool[] seasons = new bool[4] { true,true,true,true };
		public bool[] daysOfWeek;
		public bool[] daysOfMonth;
	}
}
