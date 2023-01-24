using StardewValley;
using System.Numerics;

namespace Moolah
{
    public class MoolahAPI : IMoolahAPI
    {
        public BigInteger GetTotalMoolah(Farmer f)
        {
            return ModEntry.GetTotalMoolah(f);
        }
        public void AddMoolah(Farmer f, BigInteger add)
        {
            BigInteger total = ModEntry.GetTotalMoolah(f) + add;
            f._money = ModEntry.AdjustMoney(f, total);
        }
    }
    public interface IMoolahAPI
    {
        public BigInteger GetTotalMoolah(Farmer f);
        public void AddMoolah(Farmer f, BigInteger add);
    }
}