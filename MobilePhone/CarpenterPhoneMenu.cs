using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;
using StardewValley.Network;
using System;
using System.Linq;
using System.Threading.Tasks;
using xTile.Dimensions;

namespace MobilePhone
{
    internal class CarpenterPhoneMenu : CarpenterMenu
    {
        private IModHelper helper;
        private LocationRequest locationRequest;

        public CarpenterPhoneMenu(bool magicalConstruction, Farmer farmer, IModHelper helper) : base(magicalConstruction)
        {
			this.helper = helper;
			exitFunction = OnExit;

		}

        private void OnExit()
        {
			MobilePhoneCall.ShowMainCallDialogue(ModEntry.callingNPC);
        }
    }
}