using Microsoft.Xna.Framework;
using Netcode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoPlayerMod
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool RightSide { get; set; } = true;
        public bool Bottom { get; set; } = true;
        public int XOffset { get; set; } = 64;
        public int YOffset { get; set; } = 64;
        public int Width { get; set; } = 720;
        public int Height { get; set; } = 480;
        public bool PhoneApp { get; set; } = true;
    }
}
