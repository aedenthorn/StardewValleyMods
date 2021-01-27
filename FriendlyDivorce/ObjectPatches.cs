using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;

namespace FriendlyDivorce
{
    public static class ObjectPatches
    {
        private static IMonitor Monitor;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
        }
        public static void Farmer_doDivorce_Prefix(ref Farmer __instance, ref string __state)
        {
            try
            {
                if (__instance.spouse != null)
                {
                    __state = __instance.getSpouse().name;
                }
                else if (__instance.team.GetSpouse(__instance.UniqueMultiplayerID) != null)
                {
                    long spouseID = __instance.team.GetSpouse(__instance.UniqueMultiplayerID).Value;
                    __state = spouseID.ToString();
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Farmer_doDivorce_Prefix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static void Farmer_doDivorce_Postfix(ref Farmer __instance, ref string __state)
        {
            try
            {
                int points = ModEntry.Config.PointsAfterDivorce;
                points -= ModEntry.heartsLost * 250;
                if (long.TryParse(__state,out long result))
                {
                    Friendship f = __instance.team.GetFriendship(__instance.uniqueMultiplayerID,result);
                    f.Points = Math.Max(0, points);
                    f.Status = points < 1000 ? FriendshipStatus.Divorced : FriendshipStatus.Friendly;

                }
                else
                {
                    __instance.friendshipData[__state].Points = Math.Max(0, points);
                    __instance.friendshipData[__state].Status = points < 1000 ? FriendshipStatus.Divorced : FriendshipStatus.Friendly;
                    Monitor.Log($"final points {__instance.friendshipData[__state].Points}");
                }

                ModEntry.heartsLost = 0;
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Farmer_doDivorce_Postfix)}:\n{ex}", LogLevel.Error);
            }

        }

        public static bool ManorHouse_performAction_Prefix(ManorHouse __instance, string action, Farmer who, ref bool __result)
        {
            try
            {
                if (action != null && who.IsLocalPlayer && Game1.player.isMarried())
                {
                    string a = action.Split(new char[]
                    {
                    ' '
                    })[0];
                    if (a == "DivorceBook")
                    {
                        string s2 = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:ManorHouse_DivorceBook_Question_" + Game1.player.spouse);
                        if (s2 == null)
                        {
                            s2 = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:ManorHouse_DivorceBook_Question");
                        }
                        List<Response> responses = new List<Response>();
                        responses.Add(new Response("divorce_Yes", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes")));
                        responses.Add(new Response("divorce_Complex", ModEntry.PHelper.Translation.Get("divorce_complex")));
                        responses.Add(new Response("divorce_No", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")));
                        __instance.createQuestionDialogue(s2, responses.ToArray(), ModEntry.AnswerDialogue);
                        __result = true;
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(ManorHouse_performAction_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }

        public static void GameLocation_answerDialogue_prefix(GameLocation __instance, Response answer)
        {
            try
            {
                if (answer.responseKey.StartsWith("divorce_"))
                    __instance.afterQuestion = ModEntry.AnswerDialogue;

            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(GameLocation_answerDialogue_prefix)}:\n{ex}", LogLevel.Error);
            }
        }
    }
}
