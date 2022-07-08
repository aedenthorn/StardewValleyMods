using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Minigames;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Object = StardewValley.Object;

namespace TwoPlayerPrairieKing
{
    public partial class ModEntry
    {
        public static int namePage;
        public static string GetCoopName()
        {
            if (!Config.ModEnabled || coopName is null)
                return "Abigail";
            return coopName;
        }

        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.answerDialogueAction))]
        public class GameLocation_answerDialogueAction_Patch
        {
            public static bool Prefix(GameLocation __instance, string questionAndAnswer, ref bool __result)
            {
                if (!Config.ModEnabled)
                    return true;
                if(questionAndAnswer == "CowboyGame_CoopGame")
                {
                    SMonitor.Log("Starting coop game");

                    CreateNameListQuestion(__instance);

                    __result = true;
                    return false;
                } 
                if(questionAndAnswer == "CowboyGame_NewGame")
                {
                    SMonitor.Log("Starting non-coop game");
                    coopName = null;
                }
                else if (questionAndAnswer.StartsWith("CowboyCoopGame_"))
                {
                    string name = questionAndAnswer.Substring("CowboyCoopGame_".Length);
                    if(name == "prev_page")
                    {
                        namePage--;
                        CreateNameListQuestion(__instance);
                    }
                    else if(name == "next_page")
                    {
                        namePage++;
                        CreateNameListQuestion(__instance);
                    }
                    else
                    {
                        coopName = name;
                        SMonitor.Log($"Starting coop game with {coopName}");
                        Game1.player.jotpkProgress.Value = null;
                        Game1.currentMinigame = new AbigailGame(false);
                    }
                    __result = true;
                    return false;
                }
                return true;
            }

            private static void CreateNameListQuestion(GameLocation __instance)
            {
                var names = Game1.player.friendshipData.Keys.Where(k => Game1.player.friendshipData[k].Points / 250 >= Config.MinHearts).ToList();

                names.Sort(delegate (string a, string b) {
                    return Game1.player.friendshipData[b].Points.CompareTo(Game1.player.friendshipData[a].Points);
                });

                if (Config.SameLocation)
                {
                    for(int i = names.Count - 1; i >= 0; i--)
                    {
                        if (Game1.player.currentLocation.characters.FirstOrDefault(n => n.Name == names[i]) == null)
                            names.RemoveAt(i);
                    }
                }


                int totalNames = names.Count;

                names = names.Skip(namePage * Config.NamesPerPage).Take(Config.NamesPerPage).ToList();

                List<Response> responses = new List<Response>();

                if (namePage > 0)
                    responses.Add(new Response("prev_page", "..."));
                foreach (var name in names)
                {
                    responses.Add(new Response(name, name));
                }
                if (Config.NamesPerPage * (namePage + 1) < totalNames)
                    responses.Add(new Response("next_page", "..."));

                __instance.createQuestionDialogue(SHelper.Translation.Get("which-npc"), responses.ToArray(), "CowboyCoopGame");
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.showPrairieKingMenu))]
        public class GameLocation_showPrairieKingMenu_Patch
        {
            public static bool Prefix(GameLocation __instance)
            {
                coopName = null;
                if (!Config.ModEnabled)
                    return true;
                List<Response> responses = new List<Response>();
                if (Game1.player.jotpkProgress.Value != null)
                    responses.Add(new Response("Continue", Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Cowboy_Continue")));

                responses.Add(new Response("NewGame", Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Cowboy_NewGame")));

                var names = Game1.player.friendshipData.Keys.Where(k => Game1.player.friendshipData[k].Points / 250 >= Config.MinHearts);
                if(names.Any())
                    responses.Add(new Response("CoopGame", SHelper.Translation.Get("coop-game")));

                responses.Add(new Response("Exit", Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Minecart_Exit")));
                __instance.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Cowboy_Menu"), responses.ToArray(), "CowboyGame");
                return false;
            }
        }
        [HarmonyPatch(typeof(AbigailGame), nameof(AbigailGame.reset))]
        public class AbigailGame_reset_Patch
        {
            public static void Postfix(AbigailGame __instance, bool playingWithAbby)
            {
                if (!Config.ModEnabled || coopName is null)
                    return;
                SMonitor.Log($"reset with coop name {coopName}");
                if (playingWithAbby)
                {
                    SMonitor.Log("event active, resetting coop name");
                    coopName = null;
                    return;
                }

                AbigailGame.player2Position = new Vector2(432f, 384f);
                __instance.player2BoundingBox = new Rectangle(9 * AbigailGame.TileSize, 8 * AbigailGame.TileSize, AbigailGame.TileSize, AbigailGame.TileSize);
            }
        }
        [HarmonyPatch(typeof(AbigailGame), "_ProcessInputs")]
        public class AbigailGame__ProcessInputs_Patch
        {
            public static void Prefix(ref bool __state)
            {
                if (!Config.ModEnabled || coopName is null || !AbigailGame.playingWithAbigail)
                    return;
                AbigailGame.playingWithAbigail = false;
                __state = true;
            }
            public static void Postfix(bool __state)
            {
                if (!Config.ModEnabled || coopName is null || !__state)
                    return;
                AbigailGame.playingWithAbigail = true;
            }
        }
        [HarmonyPatch(typeof(AbigailGame), nameof(AbigailGame.tick))]
        public class AbigailGame_tick_Patch
        {
            public static void Prefix(AbigailGame __instance, GameTime time, ref bool __state)
            {
                if (!Config.ModEnabled || coopName is null)
                    return;
                if (__instance.quit)
                {
                    coopName = null;
                    return;
                }
                if(AbigailGame.waveTimer - time.ElapsedGameTime.Milliseconds <= 0)
                {
                    if(AbigailGame.monsters.Count == 0 && __instance.isSpawnQueueEmpty())
                        __instance.startAbigailPortrait(1, Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11898"));
                    return;
                }
                AbigailGame.playingWithAbigail = true;
                __state = true;
            }
            public static void Postfix(AbigailGame __instance, bool __state)
            {
                if (!__state)
                    return;

                AbigailGame.playingWithAbigail = false;
            }
        }
        [HarmonyPatch(typeof(AbigailGame), nameof(AbigailGame.playerDie))]
        public class AbigailGame_playerDie_Patch
        {
            public static void Prefix(ref bool __state)
            {
                if (!Config.ModEnabled || coopName is null || !AbigailGame.playingWithAbigail)
                    return;
                AbigailGame.playingWithAbigail = false;
            }
            public static void Postfix()
            {
                if (!Config.ModEnabled || coopName is null)
                    return;
                AbigailGame.playingWithAbigail = true;
            }
        }
        [HarmonyPatch(typeof(AbigailGame), nameof(AbigailGame.draw))]
        public class AbigailGame_draw_Patch
        {
            public static void Prefix()
            {
                if (!Config.ModEnabled || coopName is null)
                    return;
                AbigailGame.playingWithAbigail = true;
            }
            public static void Postfix()
            {
                if (!Config.ModEnabled || coopName is null)
                    return;
                AbigailGame.playingWithAbigail = false;
            }
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling AbigailGame.draw");

                var codes = new List<CodeInstruction>(instructions);

                int idx = codes.FindIndex(c => c.opcode == OpCodes.Ldstr && (string)c.operand == "Abigail");
                if (idx >= 0)
                {
                    codes[idx].opcode = OpCodes.Call;
                    codes[idx].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetCoopName));
                }

                return codes.AsEnumerable();
            }

        }
        [HarmonyPatch(typeof(AbigailGame), nameof(AbigailGame.startAbigailPortrait))]
        public class AbigailGame_startAbigailPortrait_Patch
        {
            public static void Prefix(ref string sayWhat, ref int whichExpression)
            {
                if (!Config.ModEnabled || coopName is null)
                    return;
                //SMonitor.Log($"{coopName} saying {sayWhat}");
                string which = "start";
                if (sayWhat == Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11898"))
                    which = "end";
                else if (sayWhat == Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11897"))
                    which = "mid";
                else if (sayWhat == Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11901") || sayWhat == Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11902"))
                    which = "die";
                else if(coopName != "Abigail")
                {
                    whichExpression = 0;
                }

                //SMonitor.Log($"which is {which}");

                var dict = Game1.content.Load<Dictionary<string, string>>("Characters\\Dialogue\\" + coopName);
                if (dict == null)
                    return;
                List<string> keys = dict.Keys.ToList().Where(s => s.StartsWith("TwoPlayerPrairieKing_"+which)).ToList();
                if (keys.Any())
                {
                    sayWhat = dict[keys[Game1.random.Next(keys.Count)]];
                    //SMonitor.Log($"{coopName} saying {sayWhat}");
                }
            }
        }
    }
}