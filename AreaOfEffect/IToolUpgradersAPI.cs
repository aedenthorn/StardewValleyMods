using System.Collections.Generic;

namespace AreaOfEffect
{
    public interface IToolUpgradersAPI
    {
        public void AddUpgrader(string id, string shopName, string tileAction, IEnumerable<string> tools, string readyText, string beginText, string noSpaceText, string beginSound, string beginNPC, int upgradeDays);
    }
}