using StardewModdingAPI;

namespace PersonalTravellingCart
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public SButton HitchButton { get; set; } = SButton.Back;
        public string ThisPlayerCartLocationName { get; set; } = null;
    }
}
