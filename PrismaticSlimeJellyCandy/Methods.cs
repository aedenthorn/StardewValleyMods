using System;

namespace PrismaticSlimeJellyCandy
{
    public partial class ModEntry
    {
        private static bool OverrideSpecialOrderActive(bool value)
        {
            return Config.ModEnabled ? true : value;
        }
        private static double MultiplyPrismaticChance(double value)
        {
            return Config.ModEnabled ? value * Config.PrismaticChanceMult : value;
        }
    }
}