using StardewModdingAPI;

namespace MultiStorageMenu
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool ModToOpen { get; set; } = true;
        public int ChestRows { get; set; } = 3;
        public StorageMenu.Sort CurrentSort { get; set; } = StorageMenu.Sort.NA;
        public SButton ModKey { get; set; } = SButton.LeftShift;
        public SButton ModKey2 { get; set; } = SButton.LeftControl;
        public SButton SwitchButton { get; set; } = SButton.LeftShoulder;
        public SButton MenuKey { get; set; } = SButton.F2;

    }
}
