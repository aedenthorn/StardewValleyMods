using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.GameData.Crops;
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
			if(Game1.objectData.TryGetValue(crop.netSeedIndex.Value, out var cropData))
            {
				cropName = cropData.Name;
			}
			if(Game1.objectData.TryGetValue(crop.indexOfHarvest.Value, out var harvestData))
            {
				harvestName = harvestData.Name;
			}
			Crop.TryGetData(crop.netSeedIndex.Value, out var seedData);
			
			seedIndex = crop.netSeedIndex.Value;
			harvestIndex = crop.netSeedIndex.Value;
            rowInSpriteSheet = crop.rowInSpriteSheet.Value;
			phaseToShow = crop.phaseToShow.Value;
			currentPhase = crop.currentPhase.Value;
			dayOfCurrentPhase = crop.dayOfCurrentPhase.Value;
			whichForageCrop = crop.whichForageCrop.Value;
			tintColor = crop.tintColor.Value;
			flip = crop.flip.Value;
			fullyGrown = crop.fullyGrown.Value;
			programColored = crop.programColored.Value;
			dead = crop.dead.Value;
			forageCrop = crop.forageCrop.Value;
			drawPosition = AccessTools.FieldRefAccess<Crop, Vector2>(crop, "drawPosition");
			tilePosition = AccessTools.FieldRefAccess<Crop, Vector2>(crop, "tilePosition");
			layerDepth = AccessTools.FieldRefAccess<Crop, float>(crop, "layerDepth");;
			coloredLayerDepth = AccessTools.FieldRefAccess<Crop, float>(crop, "coloredLayerDepth");
			sourceRect = AccessTools.FieldRefAccess<Crop, Rectangle>(crop, "sourceRect");
			coloredSourceRect = AccessTools.FieldRefAccess<Crop, Rectangle>(crop, "coloredSourceRect");
		}
        public Crop ToCrop(GameLocation location)
        {
			Crop crop = null;
			try
			{

                if (seedIndex == null)
                {
                    harvestIndex = Game1.objectData.First(kvp => kvp.Value.Name == harvestName).Key;
                    seedIndex = Game1.cropData.First(kvp => kvp.Value.HarvestItemId == harvestIndex).Key;
                }
                crop = new Crop(seedIndex, (int)tilePosition.X, (int)tilePosition.Y, location);
                crop.phaseDays.AddRange(phaseDays);
                crop.rowInSpriteSheet.Value = rowInSpriteSheet;
                crop.phaseToShow.Value = phaseToShow;
                crop.currentPhase.Value = currentPhase;
                crop.dayOfCurrentPhase.Value = dayOfCurrentPhase;
                crop.whichForageCrop.Value = whichForageCrop;
                crop.tintColor.Value = tintColor;
                crop.flip.Value = flip;
                crop.fullyGrown.Value = fullyGrown;
                crop.programColored.Value = programColored;
                crop.dead.Value = dead;
                crop.forageCrop.Value = forageCrop;
                AccessTools.FieldRefAccess<Crop, Vector2>(crop, "drawPosition") = drawPosition;
                AccessTools.FieldRefAccess<Crop, Vector2>(crop, "tilePosition") = tilePosition;
                AccessTools.FieldRefAccess<Crop, float>(crop, "layerDepth") = layerDepth; ;
                AccessTools.FieldRefAccess<Crop, float>(crop, "coloredLayerDepth") = coloredLayerDepth;
                AccessTools.FieldRefAccess<Crop, Rectangle>(crop, "sourceRect") = sourceRect; ;
                AccessTools.FieldRefAccess<Crop, Rectangle>(crop, "coloredSourceRect") = coloredSourceRect;
                return crop;
            }
            catch
			{
				return null;
			}
		}
		public List<int> phaseDays = new List<int>();

		public string seedIndex;

		public string harvestIndex;
		
		public string cropName;

		public string harvestName;
		
		public int rowInSpriteSheet;

		public int phaseToShow = -1;

		public int currentPhase;

		public HarvestMethod harvestMethod;

		public bool regrowAfterHarvest;

		public int dayOfCurrentPhase;

		public int minHarvest;

		public int maxHarvest;

		public int maxHarvestIncreasePerFarmingLevel;

		public int daysOfUnclutteredGrowth;

		public string whichForageCrop;

		public List<Season> seasonsToGrowIn = new List<Season>();

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