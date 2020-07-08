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
        public int DinoDamage { get; set; } = 25;
        public int BatMinDamage { get; set; } = 5;
        public int BatMaxDamage { get; set; } = 10;
        public int DustStealInterval { get; set; } = 10000;
        public double DustStealChance { get; set; } = 0.001;
        public string BatTexture { get; set; } = "Characters/Monsters/Bat";
        public string DinoTexture { get; set; } = "Characters/Monsters/Pepper Rex";
        public string DustTexture { get; set; } = "Characters/Monsters/Dust Spirit";
        public bool DefaultBatColor { get; set; } = true;
        public bool DefaultDinoColor { get; set; } = true;
        public bool DefaultDustColor { get; set; } = true;
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
        public bool IAmAStinkyCheater { get; set; } = false;
        public int FamiliarHatchMinutes { get; set; } = 4000;
    }
}
