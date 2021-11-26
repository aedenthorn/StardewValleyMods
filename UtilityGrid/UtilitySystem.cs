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
        public Dictionary<Vector2, UtilityObject> waterUnconnectedObjects = new Dictionary<Vector2, UtilityObject>();
        public Dictionary<Vector2, UtilityObject> electricUnconnectedObjects = new Dictionary<Vector2, UtilityObject>();

    }
}