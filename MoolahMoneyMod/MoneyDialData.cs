using System.Numerics;

namespace MoolahMoneyMod
{
	public class MoneyDialData
	{
		public BigInteger currentValue = new();
		public BigInteger previousTargetValue = new();
		public BigInteger speed = new();
		public BigInteger soundTimer = new();
		public BigInteger moneyMadeAccumulator = new();
		public BigInteger moneyShineTimer = new();

		public void Reset()
		{
			currentValue = 0;
			previousTargetValue = 0;
			speed = 0;
			soundTimer = 0;
			moneyMadeAccumulator = 0;
			moneyShineTimer = 0;
		}
	}
}
