using System;
using System.Collections.Generic;
using StardewValley;
using StardewValley.GameData.Crops;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace CropHat
{
	public partial class ModEntry
	{
		private static void NewDay(Hat hat)
		{
			if (hat is null || !hat.modData.ContainsKey(daysKey))
				return;

			int days = Convert.ToInt32(hat.modData[daysKey]);
			int phase = Convert.ToInt32(hat.modData[phaseKey]);
			int row = Convert.ToInt32(hat.modData[rowKey]);
			bool fullyGrown = hat.modData[grownKey] == "true";

			if (!Game1.cropData.TryGetValue(hat.modData[seedKey], out CropData cropData))
				return;

			List<int> phaseDays = new();

			for (int i = 0; i < cropData.DaysInPhase.Count; i++)
			{
				phaseDays.Add(cropData.DaysInPhase[i]);
			}
			phaseDays.Add(99999);
			if (fullyGrown)
			{
				days--;
			}
			else
			{
				days = Math.Min(days + 1, (phaseDays.Count > 0) ? phaseDays[Math.Min(phaseDays.Count - 1, phase)] : 0);
			}
			if (days >= ((phaseDays.Count > 0) ? phaseDays[Math.Min(phaseDays.Count - 1, phase)] : 0) && phase < phaseDays.Count - 1)
			{
				phase++;
				days = 0;
			}
			while (phase < phaseDays.Count - 1 && phaseDays.Count > 0 && phaseDays[phase] <= 0)
			{
				phase++;
			}
			hat.modData[daysKey] = days + "";
			hat.modData[phaseKey] = phase + "";

			var x = GetSourceX(row, phase, days, fullyGrown, false) + "";

			hat.modData[xKey] = x;
		}

		private static int GetSourceX(int row, int phase, int day, bool fullyGrown, bool alt = false)
		{
			return Math.Min(240, (fullyGrown ? ((day <= 0) ? 6 : 7) : (phase + (alt ? -1 : 0) + 1)) * 16 + ((row % 2 != 0) ? 128 : 0));
		}

		private static int GetSourceY(int row)
		{
			return row / 2 * 16 * 2;
		}

		private static bool ReadyToHarvest(Hat hat)
		{
			if(!hat.modData.ContainsKey(phasesKey))
			{
				if (!Game1.cropData.TryGetValue(hat.modData[seedKey], out CropData cropData))
					return false;
				hat.modData[phasesKey] = cropData.DaysInPhase.Count.ToString();
			}
			if(!hat.modData.ContainsKey(grownKey))
			{
				hat.modData[grownKey] = "false";
			}

			var grown = hat.modData[grownKey];
			var phase = hat.modData[phaseKey];
			var phases = hat.modData[phasesKey];
			var days = hat.modData[daysKey];

			return (grown != "true" || Convert.ToInt32(days) <= 0) && Convert.ToInt32(phase) >= Convert.ToInt32(phases) - 1;
		}

		private static void HarvestHatCrop(Farmer farmer)
		{
			Crop crop = new(farmer.hat.Value.modData[seedKey], 0, 0, farmer.currentLocation);

			crop.currentPhase.Value = crop.phaseDays.Count - 1;
			crop.dayOfCurrentPhase.Value = 0;

			HoeDirt soil = new(0, crop);
			int regrowDays = crop.GetData().RegrowDays;

			crop.harvest(farmer.TilePoint.X, farmer.TilePoint.Y, soil);
			if(regrowDays != -1)
			{
				farmer.hat.Value.modData[grownKey] = "true";
				farmer.hat.Value.modData[daysKey] = regrowDays.ToString();

				int phase = Convert.ToInt32(farmer.hat.Value.modData[phaseKey]);
				int row = Convert.ToInt32(farmer.hat.Value.modData[rowKey]);

				farmer.hat.Value.modData[xKey] = GetSourceX(row, phase, regrowDays, true, false) + "";
			}
			else
			{
				farmer.hat.Value = null;
			}
		}
	}
}
