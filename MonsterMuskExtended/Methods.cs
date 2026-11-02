using StardewValley;
using StardewValley.Network;

namespace MonsterMuskExtended
{
    public partial class ModEntry
    {

        public static bool AnyOnlineFarmerHasBuff(string which_buff)
        {
            using (FarmerCollection.Enumerator enumerator = Game1.getOnlineFarmers().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.hasBuff(which_buff))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private static int DoubleNumber(int value)
        {
            if (!Config.ModEnabled || !AnyOnlineFarmerHasBuff("24"))
                return value;
            return value * 2;
        }
    }
}