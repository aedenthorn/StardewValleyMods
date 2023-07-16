using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace OpenFolder
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool AddGameFolderButton { get; set; } = false;
        public bool AddModsFolderButton { get; set; } = true;
        public bool AddGMCMModsFolderButton { get; set; } = true;
    }
}
