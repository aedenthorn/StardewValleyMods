// ModConfig.cs

using StardewModdingAPI;

namespace ToolSmartSwitch
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = true;
        public bool SwitchEnabled { get; set; } = true;
        public SButton ToggleButton { get; set; } = SButton.None;
        public bool HoldingTool { get; set; } = true;
        public bool FromWeapon { get; set; } = true;
        public bool SwitchForObjects { get; set; } = true;
        public bool SwitchForTrees { get; set; } = true;
        public bool SwitchForResourceClumps { get; set; } = true;
        public bool SwitchForGrass { get; set; } = true;
        public bool SwitchForCrops { get; set; } = true;
        public bool HarvestWithScythe { get; set; } = false;
        public bool SwitchForPan { get; set; } = true;
        public bool SwitchForWateringCan { get; set; } = true;
        public bool SwitchForFishing { get; set; } = true;
        public bool SwitchForWatering { get; set; } = true;
        public bool SwitchForTilling { get; set; } = true;
        public bool SwitchForAnimals { get; set; } = true;
        public bool SwitchForMonsters { get; set; } = true;
        public float MonsterMaxDistance { get; set; } = 128; // Increased from 64 to detect monsters earlier

    }
}
