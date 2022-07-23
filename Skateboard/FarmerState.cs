using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Skateboard
{
    public class FarmerState
    {
        public List<int> dirs = new List<int>();
        public Vector2 pos = Vector2.Zero;
        public Vector2 drawOffset = Vector2.Zero;
        public bool shouldShadowBeOffset = false;
    }
}