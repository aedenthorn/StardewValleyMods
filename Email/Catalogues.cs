using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Email
{
    public partial class ModEntry
    {
        public void OpenMail(int index)
        {
			string id = Game1.player.mailbox[index];
			if (!Game1.player.mailReceived.Contains(id) && !id.Contains("passedOut") && !id.Contains("Cooking"))
			{
				Game1.player.mailReceived.Add(id);
			}
			string mailTitle = id;
			Game1.mailbox.RemoveAt(index);
			Dictionary<string, string> mails = Game1.content.Load<Dictionary<string, string>>("Data\\mail");
			string mail = mails.ContainsKey(mailTitle) ? mails[mailTitle] : "";
			if (mailTitle.StartsWith("passedOut "))
			{
				string[] split = mailTitle.Split(' ', StringSplitOptions.None);
				int moneyTaken = (split.Length > 1) ? Convert.ToInt32(split[1]) : 0;
				switch (new Random(moneyTaken).Next((Game1.player.getSpouse() != null && Game1.player.getSpouse().Name.Equals("Harvey")) ? 2 : 3))
				{
					case 0:
						if (Game1.MasterPlayer.hasCompletedCommunityCenter() && !Game1.MasterPlayer.mailReceived.Contains("JojaMember"))
						{
							mail = string.Format(mails["passedOut4"], moneyTaken);
						}
						else
						{
							mail = string.Format(mails["passedOut1_" + ((moneyTaken > 0) ? "Billed" : "NotBilled") + "_" + (Game1.player.IsMale ? "Male" : "Female")], moneyTaken);
						}
						break;
					case 1:
						mail = string.Format(mails["passedOut2"], moneyTaken);
						break;
					case 2:
						mail = string.Format(mails["passedOut3_" + ((moneyTaken > 0) ? "Billed" : "NotBilled")], moneyTaken);
						break;
				}
			}
			else if (mailTitle.StartsWith("passedOut"))
			{
				string[] split2 = mailTitle.Split(' ', StringSplitOptions.None);
				if (split2.Length > 1)
				{
					int moneyTaken2 = Convert.ToInt32(split2[1]);
					mail = string.Format(mails[split2[0]], moneyTaken2);
				}
			}
			if (mail.Length == 0)
			{
				return;
			}
			Game1.activeClickableMenu = new LetterViewerMenu(mail, mailTitle, false);
		}


        private async void DelayedOpen(ShopMenu menu)
        {
            await Task.Delay(100);
            Monitor.Log("Really opening email");
            Game1.activeClickableMenu = menu;
        }
    }
}
