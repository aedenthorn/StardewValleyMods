
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace CustomWarpPoints
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public string AdditionalWarpDictFilePath { get; set; } = "CustomWarpPoints";
        public Dictionary<string, string> WarpDict { get; set; } = new Dictionary<string, string>();
    }
}
