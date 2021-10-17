using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Events;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MultipleSpouses
{
    public static class Pregnancy
    {
        private static IMonitor Monitor;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
        }

        public static bool Utility_pickPersonalFarmEvent_Prefix(ref FarmEvent __result)
        {
            lastBirthingSpouse = null;
            lastPregnantSpouse = null;
            ModEntry.PMonitor.Log("picking event");
            if (Game1.weddingToday)
            {
                __result = null;
                return false;
            }

            List<NPC> allSpouses = Misc.GetSpouses(Game1.player,1).Values.ToList();

            Misc.ShuffleList(ref allSpouses);
            
            foreach (NPC spouse in allSpouses)
            {
                if (spouse == null)
                {
                    Monitor.Log($"Utility_pickPersonalFarmEvent_Prefix spouse is null");
                    continue;
                }
                Farmer f = spouse.getSpouse();

                Friendship friendship = f.friendshipData[spouse.Name];

                if (friendship.DaysUntilBirthing <= 0 && friendship.NextBirthingDate != null)
                {
                    lastBirthingSpouse = spouse;
                    __result = new BirthingEvent();
                    return false;
                }
            }
            foreach (NPC spouse in allSpouses)
            {
                Farmer f = spouse.getSpouse();
                if (!ModEntry.config.RoommateRomance && f.friendshipData[spouse.Name].RoommateMarriage)
                    continue;

                int heartsWithSpouse = f.getFriendshipHeartLevelForNPC(spouse.Name);
                Friendship friendship = f.friendshipData[spouse.Name];
                List<Child> kids = f.getChildren();
                bool can = spouse.daysAfterLastBirth <= 0 && Utility.getHomeOfFarmer(f).upgradeLevel >= 2 && friendship.DaysUntilBirthing < 0 && heartsWithSpouse >= 10 && friendship.DaysMarried >= 7 && (ModEntry.config.MaxChildren < 0 || kids.Count == 0 || (kids.Count < ModEntry.config.MaxChildren));
                ModEntry.PMonitor.Log($"Checking ability to get pregnant: {spouse.Name} {can}:{(Utility.getHomeOfFarmer(f).upgradeLevel < 2 ? $" house level too low {Utility.getHomeOfFarmer(f).upgradeLevel}":"")}{(friendship.DaysMarried < 7 ? $" not married long enough {friendship.DaysMarried}":"")}{(friendship.DaysUntilBirthing >= 0 ? "Already pregnant! Gives birth in: "+friendship.DaysUntilBirthing:"")}");
                if (can && Game1.player.currentLocation == Game1.getLocationFromName(Game1.player.homeLocation) && ModEntry.myRand.NextDouble() < ModEntry.config.BabyRequestChance)
                {
                    ModEntry.PMonitor.Log("Making a baby!");
                    lastPregnantSpouse = spouse;
                    __result = new QuestionEvent(1);
                    return false;
                }
            }
            return true;
        }

        public static NPC lastPregnantSpouse;
        private static NPC lastBirthingSpouse;

        public static bool QuestionEvent_setUp_Prefix(int ___whichQuestion, ref bool __result)
        {
            if(___whichQuestion == 1)
            {
                if (lastPregnantSpouse == null)
                {
                    __result = true;
                    return false;
                }
                Response[] answers = new Response[]
                {
                    new Response("Yes", Game1.content.LoadString("Strings\\Events:HaveBabyAnswer_Yes")),
                    new Response("Not", Game1.content.LoadString("Strings\\Events:HaveBabyAnswer_No"))
                };

                if (!lastPregnantSpouse.isGaySpouse() || ModEntry.config.AllowGayPregnancies)
                {
                    Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Events:HavePlayerBabyQuestion", lastPregnantSpouse.Name), answers, new GameLocation.afterQuestionBehavior(answerPregnancyQuestion), lastPregnantSpouse);
                }
                else
                {
                    Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Events:HavePlayerBabyQuestion_Adoption", lastPregnantSpouse.Name), answers, new GameLocation.afterQuestionBehavior(answerPregnancyQuestion), lastPregnantSpouse);
                }
                Game1.messagePause = true;
                __result = false;
                return false;
            }
            return true;
        }

        public static bool BirthingEvent_tickUpdate_Prefix(GameTime time, BirthingEvent __instance, ref bool __result, ref int ___timer, string ___soundName, ref bool ___playedSound, string ___message, ref bool ___naming, bool ___getBabyName, bool ___isMale, string ___babyName)
        {
            if (!ModEntry.config.EnableMod || !___getBabyName)
                return true;

            Game1.player.CanMove = false;
            ___timer += time.ElapsedGameTime.Milliseconds;
            Game1.fadeToBlackAlpha = 1f;

            if (!___naming)
            {
                Game1.activeClickableMenu = new NamingMenu(new NamingMenu.doneNamingBehavior(__instance.returnBabyName), Game1.content.LoadString(___isMale ? "Strings\\Events:BabyNamingTitle_Male" : "Strings\\Events:BabyNamingTitle_Female"), "");
                ___naming = true;
            }
            if (___babyName != null && ___babyName != "" && ___babyName.Length > 0)
            {
                double chance = (lastBirthingSpouse.Name.Equals("Maru") || lastBirthingSpouse.Name.Equals("Krobus")) ? 0.5 : 0.0;
                chance += (Game1.player.hasDarkSkin() ? 0.5 : 0.0);
                bool isDarkSkinned = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed).NextDouble() < chance;
                string newBabyName = ___babyName;
                DisposableList<NPC> all_characters = Utility.getAllCharacters();
                bool collision_found = false;
                do
                {
                    collision_found = false;
                    using (DisposableList<NPC>.Enumerator enumerator = all_characters.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            if (enumerator.Current.Name.Equals(newBabyName))
                            {
                                newBabyName += " ";
                                collision_found = true;
                                break;
                            }
                        }
                    }
                }
                while (collision_found);
                Child baby = new Child(newBabyName, ___isMale, isDarkSkinned, Game1.player)
                {
                    Age = 0,
                    Position = new Vector2(16f, 4f) * 64f + new Vector2(0f + ModEntry.myRand.Next(-64, 48), -24f + ModEntry.myRand.Next(-24, 24)),
                };
                baby.modData["aedenthorn.MultipleSpouses/OtherParent"] = lastBirthingSpouse.Name;

                Utility.getHomeOfFarmer(Game1.player).characters.Add(baby);
                Game1.playSound("smallSelect");
                Game1.getCharacterFromName(lastBirthingSpouse.Name).daysAfterLastBirth = 5;
                Game1.player.friendshipData[lastBirthingSpouse.Name].NextBirthingDate = null;
                if (Game1.player.getChildrenCount() == 2)
                {
                    Game1.getCharacterFromName(lastBirthingSpouse.Name).shouldSayMarriageDialogue.Value = true;
                    Game1.getCharacterFromName(lastBirthingSpouse.Name).currentMarriageDialogue.Insert(0, new MarriageDialogueReference("Data\\ExtraDialogue", "NewChild_SecondChild" + ModEntry.myRand.Next(1, 3), true, new string[0]));
                    Game1.getSteamAchievement("Achievement_FullHouse");
                }
                else if (lastBirthingSpouse.isGaySpouse() && !ModEntry.config.AllowGayPregnancies)
                {
                    Game1.getCharacterFromName(lastBirthingSpouse.Name).currentMarriageDialogue.Insert(0, new MarriageDialogueReference("Data\\ExtraDialogue", "NewChild_Adoption", true, new string[]
                    {
                        ___babyName
                    }));
                }
                else
                {
                    Game1.getCharacterFromName(lastBirthingSpouse.Name).currentMarriageDialogue.Insert(0, new MarriageDialogueReference("Data\\ExtraDialogue", "NewChild_FirstChild", true, new string[]
                    {
                        ___babyName
                    }));
                }
                Game1.morningQueue.Enqueue(delegate
                {
                    ModEntry.mp.globalChatInfoMessage("Baby", new string[]
                    {
                        Lexicon.capitalize(Game1.player.Name),
                        Game1.player.spouse,
                        Lexicon.getGenderedChildTerm(___isMale),
                        Lexicon.getPronoun(___isMale),
                        baby.displayName
                    });
                });
                if (Game1.keyboardDispatcher != null)
                {
                    Game1.keyboardDispatcher.Subscriber = null;
                }
                Game1.player.Position = Utility.PointToVector2(Utility.getHomeOfFarmer(Game1.player).getBedSpot()) * 64f;
                Game1.globalFadeToClear(null, 0.02f);
                lastBirthingSpouse = null;
                __result = true;
                return false;
            }
            __result = false;
            return false;
        }
        public static bool BirthingEvent_setUp_Prefix(ref bool ___isMale, ref string ___message, ref bool __result)
        {
            if(lastBirthingSpouse == null)
            {
                __result = true;
                return false;
            }
            NPC spouse = lastBirthingSpouse;
            Game1.player.CanMove = false;
            ___isMale = ModEntry.myRand.NextDouble() > ModEntry.config.FemaleBabyChance;
            if (spouse.isGaySpouse() && !ModEntry.config.AllowGayPregnancies)
            {
                ___message = Game1.content.LoadString("Strings\\Events:BirthMessage_Adoption", Lexicon.getGenderedChildTerm(___isMale));
            }
            else if (spouse.Gender == 0)
            {
                ___message = Game1.content.LoadString("Strings\\Events:BirthMessage_PlayerMother", Lexicon.getGenderedChildTerm(___isMale));
            }
            else
            {
                ___message = Game1.content.LoadString("Strings\\Events:BirthMessage_SpouseMother", Lexicon.getGenderedChildTerm(___isMale), spouse.displayName);
            }
            __result = false;
            return false;
        }

        public static void answerPregnancyQuestion(Farmer who, string answer)
        {
            if (answer.Equals("Yes"))
            {
                WorldDate birthingDate = new WorldDate(Game1.Date);
                birthingDate.TotalDays += ModEntry.config.PregnancyDays;
                who.friendshipData[lastPregnantSpouse.Name].NextBirthingDate = birthingDate;
                lastPregnantSpouse.isGaySpouse();
            }
        }
    }
}
