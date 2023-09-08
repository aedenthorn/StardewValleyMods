using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;

namespace DeathTweaks
{
    public partial class ModEntry
    {

        private static double CheckDropEverything(double itemLossRate)
        {
            if(!Config.ModEnabled)
                return itemLossRate;
            if(Config.DropEverything)
                return 1;
            if(Config.DropNothing)
                return 0;
            return itemLossRate;
        }
        private static void CheckCreateChest()
        {
            if (!Config.ModEnabled || !Config.CreateTombstone || Game1.player.itemsLostLastDeath.Count == 0 || deathData is null) 
                return;
            deathData.Value.chest = new Chest(0, new List<Item>(Game1.player.itemsLostLastDeath), deathData.Value.position, true, 0);
            deathData.Value.chest.dropContents.Value = true;
            deathData.Value.chest.modData[modKey] = Game1.player.Name + "";
            deathData.Value.location.objects[deathData.Value.position] = deathData.Value.chest;
            if (deathData.Value.location is not MineShaft || Game1.IsClient || Game1.IsMultiplayer)
            {
                deathData.Value = null;
            }
            Game1.player.itemsLostLastDeath.Clear();
        }
        private static int SetMoneyLost(int moneyLost)
        {
            if(!Config.ModEnabled) 
                return moneyLost;
            return (int)Math.Round(moneyLost * Config.MoneyLostMult);
        }

    }
}