using System.Collections.Generic;

namespace ToolUpgraders
{
    public interface IToolUpgradersAPI
    {
        public void AddUpgrader(string id, string shopName, string tileAction, IEnumerable<string> tools, string readyText, string beginText, string noSpaceText, string beginSound, string beginNPC, int upgradeDays);
    }
    public class ToolUpgradersAPI : IToolUpgradersAPI
    {
        public void AddUpgrader(string id, string shopName, string tileAction, IEnumerable<string> tools, string readyText, string beginText, string noSpaceText, string beginSound, string beginNPC, int upgradeDays)
        {
            ModEntry.UpgraderDict[id] = new()
            {
                ShopName = shopName,
                TileAction = tileAction,
                Tools = tools,
                ReadyText = readyText,
                BeginText = beginText,
                NoSpaceText = noSpaceText,
                BeginSound = beginSound,
                BeginNPC = beginNPC,
                UpgradeDays = upgradeDays
            };
        }
    }
}