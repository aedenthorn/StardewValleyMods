using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Characters;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.TokenizableStrings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ToolUpgraders
{
    public partial class ModEntry
    {
        public static string GetToolReadyString(Tool tool)
        {
            var data = UpgraderDict.Values.Where(d => d.Tools.Contains(tool.ItemId));
            if (data.FirstOrDefault()?.ReadyText is not string str)
                return Game1.content.LoadString("Strings\\StringsFromCSFiles:ToolReady", tool.DisplayName);
            return str.Contains("{0}") ? string.Format(str, tool.DisplayName) : str;
        }
        public static bool TryReturnUpgradedTool(UpgraderData data, string itemId)
        {
            if (Game1.player.freeSpotsInInventory() > 0)
            {
                Tool tool = ItemRegistry.Create<Tool>(itemId);
                Game1.player.holdUpItemThenMessage(tool, true);
                Game1.player.addItemToInventoryBool(tool, false);
                if (Game1.player.team.useSeparateWallets.Value && tool.UpgradeLevel == 4)
                {
                    Game1.Multiplayer.globalChatInfoMessage("IridiumToolUpgrade", new string[]
                    {
                            Game1.player.Name,
                            TokenStringBuilder.ToolName(tool.QualifiedItemId, tool.UpgradeLevel)
                    });
                }
                return true;
            }
            else
            {
                NPC npc = data.BeginNPC == null ? null : Game1.getCharacterFromName(data.BeginNPC, true, false);
                Dialogue d = data.NoSpaceText is not null ? new Dialogue(npc, "ToolUpgrader", data.NoSpaceText) : new Dialogue(npc, "Data\\ExtraDialogue:Clint_NoInventorySpace", false);
                Game1.DrawDialogue(d);
                return false;
            }
        }
        public static bool IsUpgrade(string itemId)
        {
            return UpgraderDict.Values.Any(d => d.Tools.Contains(itemId));
        }
        public static bool TryGetUpgrader(string itemId, out UpgraderData data)
        {
            data = UpgraderDict.Values.FirstOrDefault(d => d.Tools.Contains(itemId));
            return data != null;
        }
        public static bool TryGetUpgrades(Farmer f, out Dictionary<string, int> data)
        {
            data = f.modData.TryGetValue(upgradesKey, out var str) ? JsonConvert.DeserializeObject<Dictionary<string, int>>(str) : new();
            return data.Any();
        }
        public static void SetUpgrades(Farmer f, Dictionary<string, int> data)
        {
            f.modData[upgradesKey] = JsonConvert.SerializeObject(data);
        }
    }
}