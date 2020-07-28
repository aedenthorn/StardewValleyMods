using Microsoft.Xna.Framework;
using Netcode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickResponses
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool MenuKeySelectFirstResponse { get; set; } = true;
        public bool ShowNumbers { get; set; } = true;
        public Color NumberColor { get; set; } = Color.Black;
    }
}
