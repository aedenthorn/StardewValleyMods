using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace UtilityGrid
{
    public class UtilitySystem
    {
        public Dictionary<Vector2, GridPipe> waterPipes = new Dictionary<Vector2, GridPipe>();
        public Dictionary<Vector2, GridPipe> electricPipes = new Dictionary<Vector2, GridPipe>();

        public List<PipeGroup> waterGroups = new List<PipeGroup>();
        public List<PipeGroup> electricGroups = new List<PipeGroup>();
        public Dictionary<Vector2, UtilityObjectInstance> waterUnconnectedObjects = new Dictionary<Vector2, UtilityObjectInstance>();
        public Dictionary<Vector2, UtilityObjectInstance> electricUnconnectedObjects = new Dictionary<Vector2, UtilityObjectInstance>();

    }
}