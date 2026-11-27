using StardewValley;

namespace StardewRPG
{
    public interface IStardewRPGApi
    {
        public int GetStatMod(int statValue);
        public int GetStatValue(Farmer farmer, string stat, int defaultValue = -1);
        public void GainExperience(Farmer farmer, int howMuch);
    }

    public class StardewRPGApi
    {
        public int GetStatMod(int statValue)
        {
            return ModEntry.GetStatMod(statValue);
        }
        public int GetStatValue(Farmer farmer, string stat, int defaultValue = -1)
        {
            return ModEntry.GetStatValue(farmer, stat, defaultValue);
        }
        public void GainExperience(ref Farmer farmer, int howMuch)
        {
            ModEntry.GainExperience(ref farmer, howMuch);
        }
    }
}