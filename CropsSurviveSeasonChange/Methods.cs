using StardewValley;
using StardewValley.GameData.Crops;
using StardewValley.TerrainFeatures;

namespace CropsSurviveSeasonChange
{
	public partial class ModEntry
	{
		private static bool ShouldDestroyCrop(bool outdoors, HoeDirt hoeDirt, GameLocation environment)
		{
			return ShouldKillCrop(outdoors, hoeDirt.crop, environment);
		}

		private static bool ShouldKillCrop(bool outdoors, Crop crop, GameLocation environment)
		{
			if (!Config.ModEnabled || crop is null)
				return outdoors;

			CropData cropData = crop.GetData();

			if (crop.forageCrop.Value || crop.dead.Value || (!Config.IncludeRegrowables && cropData is not null && cropData.RegrowDays != -1) || (environment.GetSeason() == Season.Winter && !Config.IncludeWinter))
			{
				return outdoors;
			}
			return false;
		}
	}
}
