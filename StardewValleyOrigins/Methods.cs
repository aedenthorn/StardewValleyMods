using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;

namespace StardewValleyOrigins
{
    public partial class ModEntry
    {
        private void GetAllowedWorldState()
        {
            allowedNPCs.Clear();
            allowedEvents.Clear();
            allowedMail.Clear();
            allowedMapPoints = new()
            {
                1001,
                1002,
                1003,
                1004,
                1024,
                1034,
                1035

            };
            shippingBin = false;
            bus = false;
            townBoard = false;
            specialOrdersBoard = false;
            minecarts = false;
            farmHouse = false;
            blacksmith = false;
            marniesLivestock = false;
            foreach (var kvp in worldStateDict)
            {
                allowedNPCs.AddRange(kvp.Value.npcs);
                allowedEvents.AddRange(kvp.Value.events);
                allowedMail.AddRange(kvp.Value.mail);
                allowedMapPoints.AddRange(kvp.Value.mapPoints);
                if (kvp.Value.shippingBin)
                    shippingBin = true;
                if (kvp.Value.bus)
                    bus = true;
                if (kvp.Value.townBoard)
                    townBoard = true;
                if (kvp.Value.specialOrdersBoard)
                    specialOrdersBoard = true;
                if (kvp.Value.minecarts)
                    minecarts = true;
                if (kvp.Value.farmHouse)
                    farmHouse = true;
                if (kvp.Value.marniesLivestock)
                    marniesLivestock = true;
                if (kvp.Value.blacksmith)
                    blacksmith = true;
                if (kvp.Value.linusCampfire)
                    linusCampfire = true;
            }
        }
        private static string[] RemoveDefaultFriendships(string[] array)
        {
            if (!Config.ModEnabled)
                return array;
            return new string[0];
        }
    }
}