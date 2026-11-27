using StardewValley;
using System.Security.Policy;

namespace ObjectProductDisplay
{
    public partial class ModEntry
    {
        private static float GetDoneFraction(Object instance)
        {
            if (!Config.ShowProgress || !instance.modData.TryGetValue(modKey, out string total))
                return Config.ShowProgressing ? Game1.ticks / 30 % 16 / 16f : 1f;
            return 1 - (float)instance.MinutesUntilReady / int.Parse(total);
        }
    }
}