using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace MapTeleport
{
    public class ModEntry : Mod 
	{
		public static ModEntry context;

		public static ModConfig Config;
        private CoordinatesList coordinates;
        private bool isSVE;

        public override void Entry(IModHelper helper)
		{
            context = this;
			Config = Helper.ReadConfig<ModConfig>();
			if (!Config.EnableMod)
				return;

            
            isSVE = Helper.ModRegistry.IsLoaded("FlashShifter.SVECode");
            if (isSVE)
            {
                Monitor.Log("Using Stardew Valley Extended Map", LogLevel.Warn);
                coordinates = Helper.Data.ReadJsonFile<CoordinatesList>("assets/sve_coordinates.json");
            }
            else
                coordinates = Helper.Data.ReadJsonFile<CoordinatesList>("assets/coordinates.json");

            Helper.Events.Input.ButtonReleased += Input_ButtonReleased;
        }


        private void Input_ButtonReleased(object sender, ButtonReleasedEventArgs e)
        {
            if (Game1.activeClickableMenu is GameMenu && (Game1.activeClickableMenu as GameMenu).tabs[(Game1.activeClickableMenu as GameMenu).currentTab].name == "map" && e.Button == Config.TeleportKey)
            {
                MapPage mp = (Game1.activeClickableMenu as GameMenu).pages[(Game1.activeClickableMenu as GameMenu).currentTab] as MapPage;
                Monitor.Log("Trying to teleport");
                int x = Game1.getMouseX();
                int y = Game1.getMouseY();

                foreach (ClickableComponent c in mp.points)
                {
                    if (c.containsPoint(x, y))
                    {
                        Coordinates co = coordinates.coordinates.Find(o => o.id == c.myID);
                        if (co == null)
                        {
                            Monitor.Log($"Teleport location {c.name} not found!", LogLevel.Warn);
                            return;
                        }
                        Monitor.Log($"Teleporting to {c.name}, {co.mapName}, {co.x},{co.y}", LogLevel.Debug);
                        Game1.activeClickableMenu.exitThisMenu(true);
                        Game1.warpFarmer(co.mapName, co.x, co.y, false);
                    }
                }
            }
        }

    }
}
