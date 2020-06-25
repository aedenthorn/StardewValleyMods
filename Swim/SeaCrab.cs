using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swim
{
	public class SeaCrab : RockCrab
	{
		public SeaCrab() : base()
		{
		}

		public SeaCrab(Vector2 position) : base(position)
		{
			moveTowardPlayerThreshold.Value = 0;
			damageToFarmer.Value = 0;
		}

        public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
        {
			return 0;
        }
    }
}
