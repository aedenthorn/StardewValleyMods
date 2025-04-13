namespace BedTweaks
{
	public class BedTweaksAPI
	{
		public static int GetBedWidth()
		{
			if (!ModEntry.Config.EnableMod)
				return 3;
			return ModEntry.Config.BedWidth;
		}
	}
}
