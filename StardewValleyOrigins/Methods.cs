using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace StardewValleyOrigins
{
    public partial class ModEntry
    {
        private void GetAllowedWorldState()
        {
            int lastCheckedState = 0;
            allowedNPCs.Clear();
            allowedEvents.Clear();
            allowedMail.Clear();
            townBoard = false;
            specialOrdersBoard = false;
            minecarts = false;
            farmHouse = false;
            blacksmith = false;
            marniesLivestock = false;
            foreach (var kvp in worldStateDict)
            {
                if (kvp.Key <= worldState)
                {
                    allowedNPCs.AddRange(kvp.Value.npcs);
                    allowedEvents.AddRange(kvp.Value.events);
                    allowedMail.AddRange(kvp.Value.mail);
                    if(kvp.Key < lastCheckedState) 
                    {
                        bus = kvp.Value.bus;
                        townBoard = kvp.Value.townBoard;
                        specialOrdersBoard = kvp.Value.specialOrdersBoard;
                        minecarts = kvp.Value.minecarts;
                        farmHouse = kvp.Value.farmHouse;
                        marniesLivestock = kvp.Value.marniesLivestock;
                        blacksmith = kvp.Value.blacksmith;
                    }
                    lastCheckedState = kvp.Key;
                }
            }
        }
    }
}