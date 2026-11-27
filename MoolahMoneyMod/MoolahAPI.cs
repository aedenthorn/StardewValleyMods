using System.Numerics;
using StardewValley;

namespace MoolahMoneyMod
{
	public class MoolahMoneyModAPI : IMoolahMoneyModAPI
	{
		public BigInteger GetMoolah(Farmer who)
		{
			return ModEntry.GetMoolah(who);
		}

		public void AddMoolah(Farmer who, BigInteger value)
		{
			ModEntry.SetMoolah(who, ModEntry.GetMoolah(who) + value);
		}
	}

	public interface IMoolahMoneyModAPI
	{
		public BigInteger GetMoolah(Farmer f);
		public void AddMoolah(Farmer f, BigInteger add);
	}
}
