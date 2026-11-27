namespace PacifistValley
{
    class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public int MillisecondsPerLove { get; set; } = 300;
        public int DeviceSpeedFactor { get; set; } = 2;
        public int AreaOfKissEffectModifier { get; set; } = 20;
        public bool PreventUnlovedMonsterDamage { get; set; } = true;
        public bool ShowMonsterHeartEmote { get; set; } = true;
        public bool LovedMonstersStillSwarm { get; set; } = false;
        public bool MonstersIgnorePlayer { get; set; } = false;
    }
}
