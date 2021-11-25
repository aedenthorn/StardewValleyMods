using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace UtilityGrid
{
    public class UtilityGridData
    {
        public List<int[]> waterData = new List<int[]>();
        public List<int[]> electricData = new List<int[]>();

        public UtilityGridData()
        {
        }

        public UtilityGridData(Dictionary<Vector2, GridPipe> waterPipes, Dictionary<Vector2, GridPipe> electricPipes)
        {
            foreach (var kvp in waterPipes)
            {
                waterData.Add(new int[]{ (int)kvp.Key.X, (int)kvp.Key.Y, (int)kvp.Value.index, (int)kvp.Value.rotation });
            }
            foreach (var kvp in electricPipes)
            {
                electricData.Add(new int[] { (int)kvp.Key.X, (int)kvp.Key.Y, (int)kvp.Value.index, (int)kvp.Value.rotation });
            }
        }
    }
}