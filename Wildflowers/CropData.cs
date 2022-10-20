using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Wildflowers
{
    public class CropData
    {
        public CropData()
        {

        }
        public CropData(Crop crop)
        {
			if(Game1.objectInformation.TryGetValue(crop.netSeedIndex.Value, out string cropData))
            {
				cropName = cropData.Split('/')[0];
			}
			if(Game1.objectInformation.TryGetValue(crop.indexOfHarvest.Value, out string harvestData))
            {
				harvestName = harvestData.Split('/')[0];
			}

			phaseDays = crop.phaseDays.ToList();
			rowInSpriteSheet = crop.rowInSpriteSheet.Value;
			phaseToShow = crop.phaseToShow.Value;
			currentPhase = crop.currentPhase.Value;
			harvestMethod = crop.harvestMethod.Value;
			indexOfHarvest = crop.indexOfHarvest.Value;
			regrowAfterHarvest = crop.regrowAfterHarvest.Value;
			dayOfCurrentPhase = crop.dayOfCurrentPhase.Value;
			minHarvest = crop.minHarvest.Value;
			maxHarvest = crop.maxHarvest.Value;
			maxHarvestIncreasePerFarmingLevel = crop.maxHarvestIncreasePerFarmingLevel.Value;
			daysOfUnclutteredGrowth = crop.daysOfUnclutteredGrowth.Value;
			whichForageCrop = crop.whichForageCrop.Value;
			seasonsToGrowIn = crop.seasonsToGrowIn.ToList();
			tintColor = crop.tintColor.Value;
			flip = crop.flip.Value;
			fullyGrown = crop.fullyGrown.Value;
			raisedSeeds = crop.raisedSeeds.Value;
			programColored = crop.programColored.Value;
			dead = crop.dead.Value;
			forageCrop = crop.forageCrop.Value;
			chanceForExtraCrops = crop.chanceForExtraCrops.Value;
			netSeedIndex = crop.netSeedIndex.Value;
			drawPosition = AccessTools.FieldRefAccess<Crop, Vector2>(crop, "drawPosition");
			tilePosition = AccessTools.FieldRefAccess<Crop, Vector2>(crop, "tilePosition");
			layerDepth = AccessTools.FieldRefAccess<Crop, float>(crop, "layerDepth");;
			coloredLayerDepth = AccessTools.FieldRefAccess<Crop, float>(crop, "coloredLayerDepth");
			sourceRect = AccessTools.FieldRefAccess<Crop, Rectangle>(crop, "sourceRect");;
			coloredSourceRect = AccessTools.FieldRefAccess<Crop, Rectangle>(crop, "coloredSourceRect");
		}
        public Crop ToCrop()
        {
			Crop crop = new Crop();
			crop.phaseDays.AddRange(phaseDays);
			crop.rowInSpriteSheet.Value = rowInSpriteSheet;
			crop.phaseToShow.Value = phaseToShow;
			crop.currentPhase.Value = currentPhase;
			crop.harvestMethod.Value = harvestMethod;
			crop.indexOfHarvest.Value = indexOfHarvest;
			crop.regrowAfterHarvest.Value = regrowAfterHarvest;
			crop.dayOfCurrentPhase.Value = dayOfCurrentPhase;
			crop.minHarvest.Value = minHarvest;
			crop.maxHarvest.Value = maxHarvest;
			crop.maxHarvestIncreasePerFarmingLevel.Value = maxHarvestIncreasePerFarmingLevel;
			crop.daysOfUnclutteredGrowth.Value = daysOfUnclutteredGrowth;
			crop.whichForageCrop.Value = whichForageCrop;
			crop.seasonsToGrowIn.AddRange(seasonsToGrowIn);
			crop.tintColor.Value = tintColor;
			crop.flip.Value = flip;
			crop.fullyGrown.Value = fullyGrown;
			crop.raisedSeeds.Value = raisedSeeds;
			crop.programColored.Value = programColored;
			crop.dead.Value = dead;
			crop.forageCrop.Value = forageCrop;
			crop.chanceForExtraCrops.Value = chanceForExtraCrops;
			crop.netSeedIndex.Value = netSeedIndex;
			AccessTools.FieldRefAccess<Crop, Vector2>(crop, "drawPosition") = drawPosition;
			AccessTools.FieldRefAccess<Crop, Vector2>(crop, "tilePosition") = tilePosition;
			AccessTools.FieldRefAccess<Crop, float>(crop, "layerDepth") = layerDepth; ;
			AccessTools.FieldRefAccess<Crop, float>(crop, "coloredLayerDepth") = coloredLayerDepth;
			AccessTools.FieldRefAccess<Crop, Rectangle>(crop, "sourceRect") = sourceRect; ;
			AccessTools.FieldRefAccess<Crop, Rectangle>(crop, "coloredSourceRect") = coloredSourceRect;
			return crop;	
		}
		public List<int> phaseDays = new List<int>();

		public string cropName;

		public string harvestName;
		
		public int rowInSpriteSheet;

		public int phaseToShow = -1;

		public int currentPhase;

		public int harvestMethod;

		public int indexOfHarvest;

		public int regrowAfterHarvest;

		public int dayOfCurrentPhase;

		public int minHarvest;

		public int maxHarvest;

		public int maxHarvestIncreasePerFarmingLevel;

		public int daysOfUnclutteredGrowth;

		public int whichForageCrop;

		public List<string> seasonsToGrowIn = new List<string>();

		public Color tintColor = new Color();

		public bool flip;

		public bool fullyGrown;

		public bool raisedSeeds;

		public bool programColored;

		public bool dead;

		public bool forageCrop;

		public double chanceForExtraCrops;

		public int netSeedIndex = -1;

		public Vector2 drawPosition;

		public Vector2 tilePosition;

		public float layerDepth;

		public float coloredLayerDepth;

		public Rectangle sourceRect;

		public Rectangle coloredSourceRect;
	}
}