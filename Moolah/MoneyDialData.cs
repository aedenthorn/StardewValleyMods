using StardewModdingAPI.Utilities;
using System;
using System.Numerics;

namespace Moolah
{
    public class MoneyDialData
    {
        public BigInteger previousTarget = new();
        public BigInteger currentValue = new();
        public BigInteger flipSpeed = new();
        public BigInteger soundTime = new();
        public BigInteger moneyShineTimer = new();
        public BigInteger moneyMadeAccumulator = new();

        public void Reset()
        {
            previousTarget = 0;
            currentValue = 0;
            flipSpeed = 0;
            soundTime = 0;
            moneyShineTimer = 0;
            moneyMadeAccumulator = 0;
        }
    }
}