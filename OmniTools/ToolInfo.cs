using StardewValley;
using StardewValley.Tools;
using System.Collections.Generic;

namespace OmniTools
{
    public class ToolInfo
    {
        public ToolDescription description;
        public string displayName;
        public List<string> enchantments = new();
        public ToolInfo()
        {

        }
        public ToolInfo(Tool tool)
        {
            description = ModEntry.GetDescriptionFromTool(tool).Value;
            ModEntry.skip = true;
            displayName = tool.DisplayName;
            ModEntry.skip = false;
            foreach (var e in tool.enchantments)
            {
                enchantments.Add(e.GetType().ToString());
            }
        }
    }
}