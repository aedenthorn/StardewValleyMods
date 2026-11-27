using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BedTweaks
{
    internal class BedManager
    {
        // An id (string) => PillowData dictionary for all of the double beds in the game.
        static Dictionary<string, BedData> pillowData = new Dictionary<string, BedData>(new KeyValuePair<string, BedData>[] {
        new KeyValuePair<string, BedData>("2052", new BedData("2052")), // Double Bed
        new KeyValuePair<string, BedData>("2058", new BedData("2058")), // Starry Double Bed
        new KeyValuePair<string, BedData>("2064", new BedData("2064")), // Strawberry Double Bed
        new KeyValuePair<string, BedData>("2070", new BedData("2070")), // Pirate Double Bed
        new KeyValuePair<string, BedData>("2180", new BedData("2180", false, lowerBedframeTop: 42)), // Tropical Double Bed
        new KeyValuePair<string, BedData>("2186", new BedData("2186", hasLittlePillows: true, lowerBedframeTop: 44)), // Deluxe Double Bed
        new KeyValuePair<string, BedData>("2192", new BedData("2192", false, lowerBedframeTop : 55)), // Modern Double Bed
        new KeyValuePair<string, BedData>("2496", new BedData("2496", pillowStartY: 19, pillowHeight: 6)), // Wild Double Bed
        new KeyValuePair<string, BedData>("2502", new BedData("2502", pillowStartY: 19, pillowHeight: 6, lowerBedframeTop: 43)), // Fisher Double Bed
        new KeyValuePair<string, BedData>("2508", new BedData("2508", lowerBedframeTop : 40)), // Birch Double Bed
        new KeyValuePair<string, BedData>("2514", new BedData("2514")), // Exotic Double bed
        // The following are unused assets.
        new KeyValuePair<string, BedData>("BluePinstripeDoubleBed", new BedData("BluePinstripeDoubleBed", pillowWidth: 16, pillowEndX: 44, pillowStartY: 19, pillowHeight: 8, lowerBedframeTop: 36)), // Blue Pinstripe Double Bed; appears unused
        new KeyValuePair<string, BedData>("JojaBed", new BedData("JojaBed", false, lowerBedframeTop: 44)), // Joja Bed; appears unused
        new KeyValuePair<string, BedData>("WizardBed", new BedData("WizardBed", false, lowerBedframeTop: 51)), // Wizard Bed; appears unused
        new KeyValuePair<string, BedData>("JunimoBed", new BedData("JunimoBed", false, lowerBedframeTop: 47)), // Junimo Bed; appears unused
        new KeyValuePair<string, BedData>("RetroBed", new BedData("RetroBed", true, 3, 16, 45, 19, 9, 56)), // Retro Bed; appears unused
        new KeyValuePair<string, BedData>("MidnightBeachDoubleBed", new BedData("MidnightBeachDoubleBed", true, 9, 17, 39, 13, 8))}); // Midnight Beach Double Bed; appears unused

        /// <summary>
        /// Gets the pillow data for the given bed.
        /// </summary>
        /// <param id="id">The unqualified item id of the bed.</param>
        /// <returns>The BedData of the bed. Uses the default values if the bed has no known BedData.</returns>
        public static BedData getBedData(string id)
        {
            if (pillowData.TryGetValue(id, out BedData data))
            {
                return data;
            }

            return new BedData(id); // If the bed isn't in the dictionary, use the default values.
        }
    }

    public struct BedData
    {
        // Most beds in the game have these values;
        public const int DEFAULT_PILLOW_X = 6;
        public const int DEFAULT_PILLOW_Y = 18;
        public const int DEFAULT_PILLOW_END_X = 42;
        public const int DEFAULT_PILLOW_WIDTH = 14;
        public const int DEFAULT_PILLOW_HEIGHT = 7;
        public const int DEFAULT_LOWER_BEDFRAME_TOP = 41;

        public int pillowStartX { get; set;} //Pixels from the top left corner of the sprite
        public int pillowEndX { get; set;} // The rightmost pixel of the rightmost pillow, in pixels from the top left corner of the sprite
        public int pillowStartY { get; set;} //Pixels from the top left corner of the sprite
        public int pillowWidth { get; set;} //In pixels
        public int pillowHeight { get; set;} //In pixels

        public int lowerBedframeTop { get; set;} // The top of the lower bedframe (should not be transparent) Necessary for the modern bed because it doesn't have a bedframe that obscures the sheets (for sheet transparency). Pixels from the top of texture.
        public bool shouldRedrawPillow { get; set;} //For beds with wide pillow, like the modern bed, this would be false
        public bool hasLittlePillows { get; set;} // Those little pillows in the middle of the pillows on the deluxe bed
        public string bedId { get; set;} //The id of the item with this bed data

        /// <summary>
        /// Constructor for BedData
        /// </summary>
        /// <param name="itemId">The id of the bed item with this data.</param>
        /// <param name="shouldRedrawPillows">Whether this bed's middle pillows should be redrawn (false for beds with wide pillows like the modern bed)</param>
        /// <param name="pillowStartX">The x position of the pillow in the sprite, in pixels from the top left corner.</param>
        /// <param name="pillowStartY">The y position of the pillow in the sprite, in pixels from the top left corner.</param>
        /// <param name="pillowEndX">The rightmost pixel of the rightmost pillow, in pixels from the top left corner of the sprite.</param>
        /// <param name="pillowWidth">The width of the pillow.</param>
        /// <param name="pillowHeight">The height of the pillow, from the top of the pillow to the top of the covers.</param>
        /// <param name="lowerBedframeTop">The top of the lower bedframe of the bed, in pixels from the top left corner of the sprite. i.e. y value of the first pixel that is not part of the bedsheets (0-indexed)</param>
        /// <param name="hasLittlePillows">Whether the bed has little pillows between its main pillows. See the Deluxe Double Bed.</param>
        public BedData(string itemId, bool shouldRedrawPillows = true, int pillowStartX = DEFAULT_PILLOW_X, int pillowStartY = DEFAULT_PILLOW_Y, int pillowEndX = DEFAULT_PILLOW_END_X, int pillowWidth = DEFAULT_PILLOW_WIDTH, int pillowHeight = DEFAULT_PILLOW_HEIGHT, int lowerBedframeTop = DEFAULT_LOWER_BEDFRAME_TOP, bool hasLittlePillows = false)
        {
            this.pillowStartX = pillowStartX;
            this.pillowStartY = pillowStartY;
            this.pillowEndX = pillowEndX;
            this.pillowWidth = pillowWidth;
            this.pillowHeight = pillowHeight;
            this.bedId = itemId;
            this.shouldRedrawPillow = shouldRedrawPillows;
            this.lowerBedframeTop = lowerBedframeTop;
            this.hasLittlePillows = hasLittlePillows;
        }
    }
}