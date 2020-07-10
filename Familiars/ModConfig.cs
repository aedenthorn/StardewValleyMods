using Microsoft.Xna.Framework;
using Netcode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Familiars
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public string BatTexture { get; set; } = "Characters/Monsters/Bat";
        public string DinoTexture { get; set; } = "Characters/Monsters/Pepper Rex";
        public string DustTexture { get; set; } = "Characters/Monsters/Dust Spirit";
        public string JunimoTexture { get; set; } = "Characters/Junimo";
        public string BatColorType { get; set; } = "default";
        public string DinoColorType { get; set; } = "default";
        public string DustColorType { get; set; } = "default";
        public string JunimoColorType { get; set; } = "default";
        public Color BatMainColor { get; set; } = new Color(0.7f, 0, 0.7f);
        public Color BatRedColor { get; set; } = new Color(0, 0, 0.5f);
        public Color BatGreenColor { get; set; } = new Color(0, 0.5f, 0.5f);
        public Color BatBlueColor { get; set; } = new Color(0, 0.5f, 0.5f);
        public Color DinoMainColor { get; set; } = new Color(1f, 0, 0.8f);
        public Color DinoRedColor { get; set; } = new Color(1f, 0, 0);
        public Color DinoGreenColor { get; set; } = new Color(0, 1f, 0);
        public Color DinoBlueColor { get; set; } = new Color(0, 0, 1f);
        public Color DustMainColor { get; set; } = new Color(1f, 0, 0.8f);
        public Color DustRedColor { get; set; } = new Color(1f, 0, 1f);
        public Color DustGreenColor { get; set; } = new Color(0, 0.5f, 0.5f);
        public Color DustBlueColor { get; set; } = new Color(0, 0.5f, 0.5f);
        public Color JunimoMainColor { get; set; } = new Color(1f, 0, 0.8f);
        public Color JunimoRedColor { get; set; } = new Color(1f, 0, 1f);
        public Color JunimoGreenColor { get; set; } = new Color(0, 0.5f, 0.5f);
        public Color JunimoBlueColor { get; set; } = new Color(0, 0.5f, 0.5f);
        public bool IAmAStinkyCheater { get; set; } = false;
        public int FamiliarHatchMinutes { get; set; } = 4000;
        public int BatFamiliarEggMinutes { get;  set; } = 1200;
    }
}
