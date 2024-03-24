using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BedTweaks
{
    internal class PillowManager
    {
        // An id (string) => PillowData dictionary for all of the double beds in the game.
        static Dictionary<string, PillowData> pillowData = new Dictionary<string, PillowData>(new KeyValuePair<string, PillowData>[] {
        new KeyValuePair<string, PillowData>("2052", new PillowData("2052", true)), // Double Bed
        new KeyValuePair<string, PillowData>("2058", new PillowData("2058", true)), // Starry Double Bed
        new KeyValuePair<string, PillowData>("2064", new PillowData("2064", true)), // Strawberry Double Bed
        new KeyValuePair<string, PillowData>("2070", new PillowData("2070", true)), // Pirate Double Bed
        new KeyValuePair<string, PillowData>("2180", new PillowData("2180", false)), // Tropical Double Bed
        new KeyValuePair<string, PillowData>("2186", new PillowData("2186", true)), // Deluxe Double Bed
        new KeyValuePair<string, PillowData>("2192", new PillowData("2192", false)), // Modern Double Bed
        new KeyValuePair<string, PillowData>("2496", new PillowData("2496", true)), // Wild Double Bed
        new KeyValuePair<string, PillowData>("2502", new PillowData("2502", true)), // Fisher Double Bed
        new KeyValuePair<string, PillowData>("2508", new PillowData("2508", true)), // Birch Double Bed
        new KeyValuePair<string, PillowData>("2514", new PillowData("2514", true)), // Exotic Double bed
        // The following are unused assets.
        new KeyValuePair<string, PillowData>("BluePinstripeDoubleBed", new PillowData(PillowData.DEFAULT_PILLOW_X, PillowData.DEFAULT_PILLOW_Y, 16, 8, "BluePinstripeDoubleBed", true)), // Blue Pinstripe Double Bed; appears unused
        new KeyValuePair<string, PillowData>("JojaBed", new PillowData("JojaBed", false)), // Joja Bed; appears unused
        new KeyValuePair<string, PillowData>("WizardBed", new PillowData("WizardBed", false)), // Wizard Bed; appears unused
        new KeyValuePair<string, PillowData>("JunimoBed", new PillowData("JunimoBed", false)), // Junimo Bed; appears unused
        new KeyValuePair<string, PillowData>("RetroBed", new PillowData(3, 16, 19, 9, "RetroBed", true)), // Retro Bed; appears unused
        new KeyValuePair<string, PillowData>("MidnightBeachDoubleBed", new PillowData(9, 17, 13, 8, "MidnightBeachDoubleBed", true))}); // Midnight Beach Double Bed; appears unused

        /// <summary>
        /// Gets the pillow data for the given bed.
        /// </summary>
        /// <param name="name">The name of the bed.</param>
        /// <returns></returns>
        public static PillowData getPillowData(string id)
        {
            if(pillowData.ContainsKey(id))
            {
                return pillowData[id];
            }

            return new PillowData(id, true); // If the bed isn't in the dictionary, use the default values.
        }
    }

    readonly struct PillowData
    {
        // Most beds in the game have these values;
        public static int DEFAULT_PILLOW_X = 6;
        public static int DEFAULT_PILLOW_Y = 19;
        public static int DEFAULT_PILLOW_WIDTH = 14;
        public static int DEFAULT_PILLOW_HEIGHT = 6;

        public int startX {get;} //Pixels from the top left corner of the sprite
        public int startY { get; } //Pixels from the top left corner of the sprite
        public int width { get; } //In pixels
        public int height { get; } //In pixels
        public bool shouldRedraw { get; } //For beds with wide pillow, like the modern bed, this would be false
        public string bedId { get; } //The id of the item with this pillow data

        public PillowData(string bedId, bool shouldRedraw)
        {
            startX = DEFAULT_PILLOW_X;
            startY = DEFAULT_PILLOW_Y;
            width = DEFAULT_PILLOW_WIDTH;
            height = DEFAULT_PILLOW_HEIGHT;

            this.bedId = bedId;
            this.shouldRedraw = shouldRedraw;
        }

        public PillowData(int startX, int startY, int width, int height, string bedId, bool shouldRedraw)
        {
            this.startX = startX;
            this.startY = startY;
            this.width = width;
            this.height = height;
            this.bedId = bedId;
            this.shouldRedraw = shouldRedraw;
        }
    }
}
