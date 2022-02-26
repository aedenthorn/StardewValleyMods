using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace UtilityGrid
{
    public class UtilitySystem
    {
        public Dictionary<Vector2, GridPipe> pipes = new Dictionary<Vector2, GridPipe>();
        public List<PipeGroup> groups = new List<PipeGroup>();
        public Dictionary<Vector2, UtilityObjectInstance> objects = new Dictionary<Vector2, UtilityObjectInstance>();

    }
}