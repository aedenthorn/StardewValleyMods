using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Tiles;

namespace OmniTools
{
    public class OmniToolsAPI : IOmniToolsAPI
    {
        public Tool SmartSwitch(Tool tool, GameLocation gameLocation, Vector2 tile)
        {
            return ModEntry.SmartSwitch(tool, gameLocation, tile);
        }

        public Tool SwitchTool(Tool tool, Type toolType)
        {
            return ModEntry.SwitchTool(tool, toolType);
        }
        public bool IsOmniTool(Tool tool)
        {
            return tool.modData.ContainsKey(ModEntry.toolsKey);
        }
        public string[] GetToolNames(Tool tool)
        {
            if (!tool.modData.TryGetValue(ModEntry.toolsKey, out var toolsString))
                return null;
            return JsonConvert.DeserializeObject<List<ToolInfo>>(toolsString).Select(i => i.displayName).ToArray();
        }
        public Tool[] GetTools(Tool tool)
        {
            if (!tool.modData.TryGetValue(ModEntry.toolsKey, out var toolsString))
                return null;
            var infos = JsonConvert.DeserializeObject<List<ToolInfo>>(toolsString);
            var list = new List<Tool>();
            foreach(var i in infos)
            {
                Tool t = ModEntry.GetToolFromInfo(i);
                if(t is not null)
                    list.Add(t);
            }
            return list.ToArray();
        }
    }

    public interface IOmniToolsAPI
    {
        /// <summary>Switch tools based on a tile being acted upon.</summary>
        /// <param name="tool">The omni-tool to be switched.</param>
        /// <param name="gameLocation">The current game location.</param>
        /// <param name="tile">The coordinates of the tile being acted upon.</param>
        /// <returns>The altered tool or <c>null</c> if no appropriate change is detected.</returns>
        public Tool SmartSwitch(Tool tool, GameLocation gameLocation, Vector2 tile);

        /// <summary>Switch tools to a specific tool type.</summary>
        /// <param name="tool">The omni-tool to be switched.</param>
        /// <param name="toolType">The tool type to switch to.</param>
        /// <returns>The altered tool or <c>null</c> if no tool of the type is found in the omni-tool.</returns>
        public Tool SwitchTool(Tool tool, Type toolType);

        /// <summary>Check if a tool is an omni-tool.</summary>
        /// <param name="tool">The omni-tool to be checked.</param>
        /// <returns>True if the tool is an omni-tool.</returns>
        public bool IsOmniTool(Tool tool);

        /// <summary>Get names of tools stored in an omni-tool.</summary>
        /// <param name="tool">The omni-tool.</param>
        /// <returns>An array of tool display names stored in the omni-tool (does not include the tool itself).</returns>
        public string[] GetToolNames(Tool tool);

        /// <summary>Get tools stored in an omni-tool.</summary>
        /// <param name="tool">The omni-tool.</param>
        /// <returns>An array of tools stored in the omni-tool (does not include the tool itself).</returns>
        public Tool[] GetTools(Tool tool);
    }
}