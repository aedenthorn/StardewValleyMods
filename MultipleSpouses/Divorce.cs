using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace MultipleSpouses
{
    internal class Divorce
    {
        private static IMonitor Monitor;
        private static IModHelper Helper;
        public static string complexDivorceSpouse;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor, IModHelper helper)
        {
            Monitor = monitor;
            Helper = helper;
        }

        internal static void afterDialogueBehavior(Farmer who, string whichAnswer)
        {

            Monitor.Log("answer " + whichAnswer);

            if (Misc.GetSpouses(Game1.player, 1).ContainsKey(whichAnswer))
            {
                Monitor.Log("divorcing " + whichAnswer);
                string s2 = Game1.content.LoadString("Strings\\Locations:ManorHouse_DivorceBook_Question_" + Game1.player.spouse);
                if (s2 == null)
                {
                    s2 = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:ManorHouse_DivorceBook_Question");
                }
                List<Response> responses = new List<Response>();
                responses.Add(new Response($"divorce_Yes_{whichAnswer}", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes")));
                if (ModEntry.config.ComplexDivorce)
                {
                    responses.Add(new Response($"divorce_complex_{whichAnswer}", Helper.Translation.Get("divorce_complex")));
                }
                responses.Add(new Response("No", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")));
                (Game1.activeClickableMenu as DialogueBox)?.closeDialogue();
                Game1.currentLocation.createQuestionDialogue(s2, responses.ToArray(), afterDialogueBehavior);
            }
            else if (whichAnswer.StartsWith("divorce_Yes_"))
            {
                Monitor.Log("confirmed " + whichAnswer);
                string spouse = whichAnswer.Split('_')[2];
                if (Game1.player.Money >= 50000 || spouse == "Krobus")
                {
                    Monitor.Log("divorce initiated successfully");
                    if (!Game1.player.isRoommate(spouse))
                    {
                        Game1.player.Money -= 50000;
                        ModEntry.divorceHeartsLost = ModEntry.config.FriendlyDivorce ? 0 : -1;
                    }
                    else
                    {
                        ModEntry.divorceHeartsLost = 0;
                    }
                    ModEntry.spouseToDivorce = spouse;
                    Game1.player.divorceTonight.Value = true;
                    string s = Game1.content.LoadString("Strings\\Locations:ManorHouse_DivorceBook_Filed_" + spouse, spouse);
                    if (s == null)
                    {
                        s = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:ManorHouse_DivorceBook_Filed");
                    }
                    Game1.drawObjectDialogue(s);
                    if (!Game1.player.isRoommate(spouse))
                    {
                        ModEntry.mp.globalChatInfoMessage("Divorce", new string[]
                        {
                            Game1.player.Name
                        });
                    }
                }
                else
                {
                    Monitor.Log("not enough money to divorce");
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
                }
            }
            else if (whichAnswer.StartsWith("divorce_complex_"))
            {
                complexDivorceSpouse = whichAnswer.Replace("divorce_complex_", "");
                ModEntry.divorceHeartsLost = 1;
                ShowNextDialogue("divorce_fault_", Game1.currentLocation);
            }
            else if (whichAnswer.StartsWith("divorce_fault_"))
            {
                Monitor.Log("divorce fault");
                string r = Helper.Translation.Get(whichAnswer);
                if (r != null)
                {
                    if (int.TryParse(r.Split('#')[r.Split('#').Length - 1], out int lost))
                    {
                        ModEntry.divorceHeartsLost += lost;
                    }
                }
                string nextKey = $"divorce_{r.Split('#')[r.Split('#').Length - 2]}reason_";
                Translation test = Helper.Translation.Get(nextKey + "q");
                if (!test.HasValue())
                {
                    ShowNextDialogue($"divorce_method_", Game1.currentLocation);
                    return;
                }
                ShowNextDialogue($"divorce_{r.Split('#')[r.Split('#').Length - 2]}reason_", Game1.currentLocation);
            }
            else if (whichAnswer.Contains("reason_"))
            {
                Monitor.Log("divorce reason");
                string r = Helper.Translation.Get(whichAnswer);
                if (r != null)
                {
                    if (int.TryParse(r.Split('#')[r.Split('#').Length - 1], out int lost))
                    {
                        ModEntry.divorceHeartsLost += lost;
                    }
                }

                ShowNextDialogue($"divorce_method_", Game1.currentLocation);
            }
            else if (whichAnswer.StartsWith("divorce_method_"))
            {
                Monitor.Log("divorce method");
                ModEntry.spouseToDivorce = complexDivorceSpouse;
                string r = Helper.Translation.Get(whichAnswer);
                if (r != null)
                {
                    if (int.TryParse(r.Split('#')[r.Split('#').Length - 1], out int lost))
                    {
                        ModEntry.divorceHeartsLost += lost;
                    }
                }

                if (Game1.player.Money >= 50000 || complexDivorceSpouse == "Krobus")
                {
                    if (!Game1.player.isRoommate(complexDivorceSpouse))
                    {
                        int money = 50000;
                        if (int.TryParse(r.Split('#')[r.Split('#').Length - 2], out int mult))
                        {
                            money = (int)Math.Round(money * mult / 100f);
                        }
                        Monitor.Log($"money cost {money}");
                        Game1.player.Money -= money;
                    }
                    Game1.player.divorceTonight.Value = true;
                    string s = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:ManorHouse_DivorceBook_Filed_" + complexDivorceSpouse);
                    if (s == null)
                    {
                        s = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:ManorHouse_DivorceBook_Filed");
                    }
                    Game1.drawObjectDialogue(s);
                    if (!Game1.player.isRoommate(complexDivorceSpouse))
                    {
                        ModEntry.mp.globalChatInfoMessage("Divorce", new string[]
                        {
                                    Game1.player.Name
                        });
                    }
                    Monitor.Log($"hearts lost {ModEntry.divorceHeartsLost}");
                }
                else
                {
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
                }
            }
        }

        private static void ShowNextDialogue(string key, GameLocation l)
        {
            (Game1.activeClickableMenu as DialogueBox)?.closeDialogue();
            Translation s2 = Helper.Translation.Get($"{key}q");
            if (!s2.HasValue())
            {
                Monitor.Log("no dialogue: " + s2.ToString(), LogLevel.Error);
                return;
            }
            Monitor.Log("has dialogue: " + s2.ToString());
            List<Response> responses = new List<Response>();
            int i = 1;
            while (true)
            {
                Translation r = Helper.Translation.Get($"{key}{i}");
                if (!r.HasValue())
                    break;
                string str = r.ToString().Split('#')[0];
                Monitor.Log(str);

                responses.Add(new Response(key + i, str));
                i++;
            }
            Monitor.Log("next question: " + s2.ToString());
            Game1.currentLocation.lastQuestionKey = "";
            Game1.isQuestion = true;
            Game1.dialogueUp = true;
            l.createQuestionDialogue(s2, responses.ToArray(), afterDialogueBehavior);
        }
    }
}