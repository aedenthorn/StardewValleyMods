using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace DoorKnock
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public bool WakeWhenSleeping { get; set; } = false;
        public bool OverrideRebuff { get; set; } = true;
        public KeybindList KnockButton { get; set; } = new KeybindList(SButton.Enter);
        public string KnockSound { get; set; } = "axchop";
        public int KnockNumber { get; set; } = 3;
        public int KnockInterval { get; set; } = 300;
        public int AnswerDelay { get; set; } = 60;
        public int WaitTime { get; set; } = 200;
    }
}
