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
            string mailTitle = Game1.mailbox[index];
            if (!mailTitle.Contains("passedOut") && !mailTitle.Contains("Cooking"))
            {
                Game1.player.mailReceived.Add(mailTitle);
            }
            Game1.mailbox.RemoveAt(index);
            Dictionary<string, string> mails = DataLoader.Mail(Game1.content);
            string mail = mails.GetValueOrDefault(mailTitle, "");
            if (mailTitle.StartsWith("passedOut"))
            {
                if (mailTitle.StartsWith("passedOut "))
                {
                    string[] split = ArgUtility.SplitBySpace(mailTitle);
                    int moneyTaken = ((split.Length > 1) ? Convert.ToInt32(split[1]) : 0);
                    int num = Utility.CreateDaySaveRandom((double)moneyTaken, 0.0, 0.0).Next((Game1.player.getSpouse() != null && Game1.player.getSpouse().Name.Equals("Harvey")) ? 2 : 3);
                    string translationKey;
                    if (num != 0)
                    {
                        if (num != 1)
                        {
                            translationKey = "passedOut3_" + ((moneyTaken > 0) ? "Billed" : "NotBilled");
                        }
                        else
                        {
                            translationKey = "passedOut2";
                        }
                    }
                    else
                    {
                        translationKey = ((Game1.MasterPlayer.hasCompletedCommunityCenter() && !Game1.MasterPlayer.mailReceived.Contains("JojaMember")) ? "passedOut4" : ("passedOut1_" + ((moneyTaken > 0) ? "Billed" : "NotBilled") + "_" + (Game1.player.IsMale ? "Male" : "Female")));
                    }
                    mail = Dialogue.applyGenderSwitchBlocks(Game1.player.Gender, mails[translationKey]);
                    mail = string.Format(mail, moneyTaken);
                }
                else
                {
                    string[] split2 = ArgUtility.SplitBySpace(mailTitle);
                    if (split2.Length > 1)
                    {
                        int moneyTaken2 = Convert.ToInt32(split2[1]);
                        mail = Dialogue.applyGenderSwitchBlocks(Game1.player.Gender, mails[split2[0]]);
                        mail = string.Format(mail, moneyTaken2);
                    }
                }
            }
            if (mail.Length > 0)
            {
                Game1.activeClickableMenu = new LetterViewerMenu(mail, mailTitle, false);
                return;
            }
		}


        private async void DelayedOpen(ShopMenu menu)
        {
            await Task.Delay(100);
            Monitor.Log("Really opening email");
            Game1.activeClickableMenu = menu;
        }
    }
}
