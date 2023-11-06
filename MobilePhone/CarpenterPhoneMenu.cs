using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace MobilePhone
{
    public class CarpenterPhoneMenu : CarpenterMenu
    {
        private IModHelper helper;

        public CarpenterPhoneMenu(bool magicalConstruction, Farmer farmer, IModHelper helper) : base(Game1.builder_robin)
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