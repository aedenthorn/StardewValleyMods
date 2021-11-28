using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace FurnitureRecolor
{
    public class ColorDictData
    {
        public Dictionary<string, List<RecolorData>> colorDict = new Dictionary<string, List<RecolorData>>();
    }

    public class RecolorData
    {
        public int X;
        public int Y;
        public Color color;
        public string name;
    }
}