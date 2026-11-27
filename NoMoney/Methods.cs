using StardewModdingAPI;
using StardewValley;

namespace NoMoney
{
	public partial class ModEntry : Mod
    {
        public static void ToggleEnabled()
        {

            if (Game1.player.modData.ContainsKey(modKey))
            {
                Game1.player.modData.Remove(modKey);
                IsEnabled = false;
            }
            else
            {
                Game1.player.modData[modKey] = "true";
                IsEnabled = true;
            }
        }
    }
}
