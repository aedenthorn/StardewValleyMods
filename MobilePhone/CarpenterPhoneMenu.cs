using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace MobilePhone
{
    internal class CarpenterPhoneMenu : CarpenterMenu
    {
        private IModHelper helper;

        public CarpenterPhoneMenu(bool magicalConstruction, Farmer farmer, IModHelper helper) : base(magicalConstruction)
        {
            this.helper = helper;
            exitFunction = OnExit;

        }

        public void OnExit()
        {
            MobilePhoneCall.ShowMainCallDialogue(ModEntry.callingNPC);
        }
    }
}