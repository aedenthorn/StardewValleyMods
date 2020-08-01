using Microsoft.Xna.Framework;
using Netcode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MobileAudioPlayer
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public Color BackgroundColor { get; set; } = Color.CornflowerBlue;
        public string ListLineOne { get; set; } = "{name}";
        public string ListLineTwo { get; set; } = "{artist} - {album}";
        public Color LineOneColor { get; set; } = Color.White;
        public Color LineTwoColor { get; set; } = Color.LightGray;
        public float MarginX { get; set; } = 4;
        public int MarginY { get; set; } = 4;
    }
}
