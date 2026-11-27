using Microsoft.Xna.Framework;

namespace Familiars
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public string BatTexture { get; set; } = "Characters/Monsters/Bat";
        public string DinoTexture { get; set; } = "Characters/Monsters/Pepper Rex";
        public string DustTexture { get; set; } = "Characters/Monsters/Dust Spirit";
        public string ButterflyTexture { get; set; } = "TileSheets\\critters";
        public string JunimoTexture { get; set; } = "Characters/Junimo";
        public string BatColorType { get; set; } = "default";
        public string DinoColorType { get; set; } = "default";
        public string DustColorType { get; set; } = "default";
        public string JunimoColorType { get; set; } = "default";
        public string ButterflyColorType { get; set; } = "default";
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
        public Color ButterflyMainColor { get; set; } = new Color(1f, 0, 0.8f);
        public Color ButterflyRedColor { get; set; } = new Color(1f, 0, 1f);
        public Color ButterflyGreenColor { get; set; } = new Color(0, 0.5f, 0.5f);
        public Color ButterflyBlueColor { get; set; } = new Color(0, 0.5f, 0.5f);
        public float StartScale { get; set; } = 0.5f;
        public float MaxScale { get; set; } = 1f;
        public float ScalePerDay { get; set; } = 0.01f;
        public bool IAmAStinkyCheater { get; set; } = false;
        public int FamiliarHatchMinutes { get; set; } = 4000;
        public int FamiliarEggMinutes { get;  set; } = 1200;
        public float MaxFamiliarDistance { get; set; } = 1280;
        public bool BatSoundEffects { get; set; } = false;
        public bool ButterflySoundEffects { get; set; } = false;
        public bool DinoSoundEffects { get; set; } = true;
        public bool DustSoundEffects { get; set; } = false;
        public bool JunimoSoundEffects { get; set; } = false;
        public double DustSpriteStealChanceMult { get; set; } = 1;
        public double BatDamageMult { get; set; } = 1;
        public double BatAttackIntervalMult { get; set; } = 1;
        public double ButterflyBuffIntervalMult { get; set; } = 1;
        public double ButterflyBuffChanceMult { get; set; } = 1;
        public double JunimoHealAmountMult { get; set; } = 1;
        public double JunimoHealChanceMult { get; set; } = 1;
        public double JunimoHealIntervalMult { get; set; } = 1;
        public double DinoFireDistanceMult { get; set; } = 1;
        public double DinoDamageMult { get; set; } = 1;
        public bool TryToFixOldBugs { get; set; } = false;
        public bool Invincible { get; set; } = true;
    }
}
