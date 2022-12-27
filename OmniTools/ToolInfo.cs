using Newtonsoft.Json;
using StardewValley;
using StardewValley.Tools;
using System;
using System.Collections.Generic;

namespace OmniTools
{
    public class ToolInfo
    {
        public ToolDescription description;
        public string displayName;
        public List<string> enchantments = new();
        public List<ObjectInfo> attachments = new();
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
            foreach (var o in tool.attachments)
            {
                attachments.Add(o is not null ? new ObjectInfo(o.ParentSheetIndex, o.Stack, o.Quality) : null);
            }
        }
    }

    public class ObjectInfo
    {
        public int parentSheetIndex;
        public int stack;
        public int quality;

        public ObjectInfo(int parentSheetIndex, int stack, int quality)
        {
            this.parentSheetIndex = parentSheetIndex;
            this.stack = stack;
            this.quality = quality;
        }
    }
}