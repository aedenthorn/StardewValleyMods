using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace CJBInventory.Framework.Models
{
    /// <summary>The mod settings.</summary>
    internal class ModConfig
    {
        /// <summary>Whether to show items which may cause bugs or crashes when spawned.</summary>
        public bool AllowProblematicItems { get; set; } = false;
    }
}
