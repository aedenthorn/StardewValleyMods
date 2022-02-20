using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace UtilityGrid
{
    public class PipeGroup
    {
        public List<Vector2> pipes = new List<Vector2>();
        public Dictionary<Vector2, UtilityObjectInstance> objects = new Dictionary<Vector2, UtilityObjectInstance>();
        public float excessUse;
    }
}