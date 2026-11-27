using System.Collections.Generic;

namespace FenceDecorations
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool RemoveEndDrawOffsets { get; set; } = true;
        public bool ReturnOnDestroy { get; set; } = true;
        public List<string> DisallowedDecorations { get; set; } = new List<string>();
        
    }
}
