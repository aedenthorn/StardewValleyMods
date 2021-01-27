using Harmony;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace FriendlyDivorce
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {

        public static ModConfig Config;
        private static IMonitor PMonitor;
        public static IModHelper PHelper;

        public static int heartsLost = 0;
        public static Multiplayer mp;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            PMonitor = Monitor;
            if (!Config.Enabled)
                return;

            PHelper = Helper;
            var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);
            ObjectPatches.Initialize(Monitor);
            mp = helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();

            if (Config.ComplexDivorce)
            {
                harmony.Patch(
                   original: AccessTools.Method(typeof(ManorHouse), nameof(ManorHouse.performAction)),
                   prefix: new HarmonyMethod(typeof(ObjectPatches), nameof(ObjectPatches.ManorHouse_performAction_Prefix))
                );
                harmony.Patch(
                   original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.answerDialogue)),
                   prefix: new HarmonyMethod(typeof(ObjectPatches), nameof(ObjectPatches.GameLocation_answerDialogue_prefix))
                );
            }

            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.doDivorce)),
               prefix: new HarmonyMethod(typeof(ObjectPatches), nameof(ObjectPatches.Farmer_doDivorce_Prefix)),
               postfix: new HarmonyMethod(typeof(ObjectPatches), nameof(ObjectPatches.Farmer_doDivorce_Postfix))
            );
        }

        public static void AnswerDialogue(Farmer who, string whichAnswer)
        {

            Game1.currentLocation.lastQuestionKey = "";

            PMonitor.Log("answer " + whichAnswer);
            if (whichAnswer == "divorce_Yes")
            {
                if (Game1.player.Money >= 50000 || Game1.player.spouse == "Krobus")
                {
                    if (!Game1.player.isRoommate(Game1.player.spouse))
                    {
                        Game1.player.Money -= 50000;
                    }
                    Game1.player.divorceTonight.Value = true;
                    string s = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:ManorHouse_DivorceBook_Filed_" + Game1.player.spouse);
                    if (s == null)
                    {
                        s = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:ManorHouse_DivorceBook_Filed");
                    }
                    Game1.drawObjectDialogue(s);
                    if (!Game1.player.isRoommate(Game1.player.spouse))
                    {
                        mp.globalChatInfoMessage("Divorce", new string[]
                        {
                            Game1.player.Name
                        });
                    }
                }
                else
                {
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
                }
            }
            else if (whichAnswer == "divorce_Complex")
            {
                heartsLost = 1;
                ShowNextDialogue("divorce_fault_", Game1.currentLocation);
            }
            else if (whichAnswer.StartsWith("divorce_fault_"))
            {
                PMonitor.Log("divorce fault");
                string r = PHelper.Translation.Get(whichAnswer);
                if (r != null)
                {
                    if (int.TryParse(r.Split('#')[r.Split('#').Length - 1], out int lost))
                    {
                        heartsLost += lost;
                    }
                }
                string nextKey = $"divorce_{r.Split('#')[r.Split('#').Length - 2]}reason_";
                Translation test = PHelper.Translation.Get(nextKey + "q");
                if (!test.HasValue())
                {
                    ShowNextDialogue($"divorce_method_", Game1.currentLocation);
                    return;
                }
                ShowNextDialogue($"divorce_{r.Split('#')[r.Split('#').Length - 2]}reason_", Game1.currentLocation);
            }
            else if (whichAnswer.Contains("reason_"))
            {
                PMonitor.Log("divorce reason");
                string r = PHelper.Translation.Get(whichAnswer);
                if (r != null)
                {
                    if (int.TryParse(r.Split('#')[r.Split('#').Length - 1], out int lost))
                    {
                        heartsLost += lost;
                    }
                }

                ShowNextDialogue($"divorce_method_", Game1.currentLocation);
            }
            else if (whichAnswer.StartsWith("divorce_method_"))
            {
                PMonitor.Log("divorce method");
                string r = PHelper.Translation.Get(whichAnswer);
                if (r != null)
                {
                    if (int.TryParse(r.Split('#')[r.Split('#').Length - 1], out int lost))
                    {
                        heartsLost += lost;
                    }
                }

                int money = 50000;
                if (int.TryParse(r.Split('#')[r.Split('#').Length - 2], out int mult))
                {
                    money = (int)Math.Round(money * mult / 100f);
                }
                PMonitor.Log($"money cost {money}");

                if (Game1.player.Money >= money || Game1.player.spouse == "Krobus")
                {
                    if (!Game1.player.isRoommate(Game1.player.spouse))
                    {
                        Game1.player.Money -= money;
                    }
                    Game1.player.divorceTonight.Value = true;
                    string s = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:ManorHouse_DivorceBook_Filed_" + Game1.player.spouse);
                    if (s == null)
                    {
                        s = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:ManorHouse_DivorceBook_Filed");
                    }
                    Game1.drawObjectDialogue(s);
                    if (!Game1.player.isRoommate(Game1.player.spouse))
                    {
                        mp.globalChatInfoMessage("Divorce", new string[]
                        {
                                    Game1.player.Name
                        });
                    }
                    PMonitor.Log($"hearts lost {heartsLost}");
                }
                else
                {
                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
                }
            }
        }


        private static void ShowNextDialogue(string key, GameLocation l)
        {
            Translation s2 = PHelper.Translation.Get($"{key}q");
            if (!s2.HasValue())
            {
                PMonitor.Log("no dialogue: " + s2.ToString(), LogLevel.Error);
                return;
            }
            PMonitor.Log("has dialogue: " + s2.ToString());
            List<Response> responses = new List<Response>();
            int i = 1;
            while (true)
            {
                Translation r = PHelper.Translation.Get($"{key}{i}");
                if (!r.HasValue())
                    break;
                string str = r.ToString().Split('#')[0];
                PMonitor.Log(str);

                responses.Add(new Response(key + i, str));
                i++;
            }
            PMonitor.Log("next question: " + s2.ToString());
            l.createQuestionDialogue(s2, responses.ToArray(), key);
        }
    }
}