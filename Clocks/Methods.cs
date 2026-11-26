using StardewValley;
using System;

namespace Clocks
{
    public partial class ModEntry
    {
        public static float GetHourRotation()
        {
            float hours = (float)(Game1.timeOfDay % 1200 / 100 + (Game1.timeOfDay % 100 / 10 + (Config.SmoothMovement ? ((float)Game1.gameTimeInterval / Game1.realMilliSecondsPerGameTenMinutes) : 0)) / 6f);

            return (float)(Math.PI / 6 * hours);
        }

        public static float GetMinRotation()
        {
            float mins = Game1.timeOfDay % 100 + (Config.SmoothMovement ? ((float)Game1.gameTimeInterval / Game1.realMilliSecondsPerGameTenMinutes) : 0) * 10;
            return (float)(Math.PI / 30 * mins);
        }
        public static float GetHourRotationForTranspiler(float value)
        {
            if (!Config.ModEnabled || !Config.FixMapClocks)
                return value;
            return GetHourRotation();
        }

        public static float GetMinRotationForTranspiler(float value)
        {
            if (!Config.ModEnabled || !Config.FixMapClocks)
                return value;
            return GetMinRotation();
        }
    }
}