using StardewValley.Menus;

namespace SocialPageOrderMenu
{
    public class MyOptionsDropDown : OptionsDropDown
    {
        public MyOptionsDropDown(string label, int whichOption, int x = -1, int y = -1) : base(label, whichOption, x, y)
        {
        }
		public override void leftClickReleased(int x, int y)
		{
			base.leftClickReleased(x, y);
            ModEntry.Config.CurrentSort = selectedOption;
            ModEntry.SHelper.WriteConfig(ModEntry.Config);
            ModEntry.ResortSocialList();
        }
	}
}