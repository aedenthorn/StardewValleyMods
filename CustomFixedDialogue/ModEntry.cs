using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace CustomFixedDialogue
{
    public partial class ModEntry : Mod
    {
        private static IMonitor SMonitor;
        private static IModHelper SHelper;
        public override void Entry(IModHelper helper)
        {
            SMonitor = Monitor;
            SHelper = Helper;
            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Constructor(typeof(Dialogue), new Type[] { typeof(string), typeof(NPC) }),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Dialogue_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Constructor(typeof(DialogueBox), new Type[] { typeof(Dialogue) }),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Dialogue_Box_Prefix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(Dialogue), nameof(Dialogue.convertToDwarvish)),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.convertToDwarvish_Prefix))
            );
            /*
            harmony.Patch(
                original: AccessTools.Method(typeof(LocalizedContentManager), nameof(LocalizedContentManager.LoadString), new Type[] { typeof(string), typeof(object), typeof(object), typeof(object) }),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.LocalizedContentManager_LoadString_Postfix3))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(LocalizedContentManager), nameof(LocalizedContentManager.LoadString), new Type[] { typeof(string), typeof(object), typeof(object) }),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.LocalizedContentManager_LoadString_Postfix2))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(LocalizedContentManager), nameof(LocalizedContentManager.LoadString), new Type[] { typeof(string), typeof(object) }),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.LocalizedContentManager_LoadString_Postfix1))
            );
            */
            harmony.Patch(
                original: AccessTools.Method(typeof(LocalizedContentManager), nameof(LocalizedContentManager.LoadString), new Type[] { typeof(string) }),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.LocalizedContentManager_LoadString_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.showTextAboveHead)),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.NPC_showTextAboveHead_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.getHi)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.NPC_getHi_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.getTermOfSpousalEndearment)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.NPC_getTermOfSpousalEndearment_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Summit), nameof(Summit.GetSummitDialogue)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.GetSummitDialogue_Patch))
            );

            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            return;
            if (e.Button == SButton.F2)
			{
				var person = Game1.getCharacterFromName("Haley");
                Game1.warpCharacter(person, Game1.player.currentLocation, Game1.player.getTileLocation() + new Microsoft.Xna.Framework.Vector2(0, 1));
                Random r = Game1.random;
                Stack<Dialogue> currentDialogue = new Stack<Dialogue>();
                person.CurrentDialogue.Clear();
                Object i = new Object(0, 1);
                string whatToCallPlayer = (Game1.random.NextDouble() < (double)(Game1.player.getFriendshipLevelForNPC(person.Name) / 1250)) ? Game1.player.Name : "farmer";
                person.setNewDialogue(Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_2_QualityHigh", whatToCallPlayer, (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en) ? Lexicon.getProperArticleForWord(i.name) : "", i.DisplayName), true, false);
                return;


                Dictionary<string, string> npcDispositions = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
				string disposition;
                if (npcDispositions.TryGetValue(person.Name, out disposition))
                {
                    string[] relatives = disposition.Split('/', StringSplitOptions.None)[9].Split(' ', StringSplitOptions.None);
                    if (relatives.Length > 1)
                    {
                        int index = r.Next(relatives.Length / 2) * 2;
                        string relativeName = relatives[index];
                        string relativeDisplayName = relativeName;
                        if (LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.en && Game1.getCharacterFromName(relativeName, true, false) != null)
                        {
                            relativeDisplayName = Game1.getCharacterFromName(relativeName, true, false).displayName;
                        }
                        string relativeTitle = relatives[index + 1].Replace("'", "").Replace("_", " ");
                        string relativeProps;
                        bool relativeIsMale = npcDispositions.TryGetValue(relativeName, out relativeProps) && relativeProps.Split('/', StringSplitOptions.None)[4].Equals("male");
                        Dictionary<string, string> npcGiftTastes = Game1.content.Load<Dictionary<string, string>>("Data\\NPCGiftTastes");

                        if (npcGiftTastes.ContainsKey(relativeName))
                        {
                            string itemName = null;
                            string nameAndTitle = (relativeTitle.Length > 2 && LocalizedContentManager.CurrentLanguageCode != LocalizedContentManager.LanguageCode.ja) ? (relativeIsMale ? Game1.LoadStringByGender(person.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4079", new object[]
                            {
                                relativeTitle
                            }) : Game1.LoadStringByGender(person.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4080", new object[]
                            {
                                relativeTitle
                            })) : relativeDisplayName;
                            string message = Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4083", nameAndTitle);
                            int item;
                            if (r.NextDouble() < 0.5)
                            {
                                string[] loveItems = npcGiftTastes[relativeName].Split('/', StringSplitOptions.None)[1].Split(' ', StringSplitOptions.None);
                                item = Convert.ToInt32(loveItems[r.Next(loveItems.Length)]);
                                if (person.Name == "Penny" && relativeName == "Pam")
                                {
                                    while (item == 303 || item == 346 || item == 348 || item == 459)
                                    {
                                        item = Convert.ToInt32(loveItems[r.Next(loveItems.Length)]);
                                    }
                                }
                                string itemDetails;
                                if (Game1.objectInformation.TryGetValue(item, out itemDetails))
                                {
                                    itemName = itemDetails.Split('/', StringSplitOptions.None)[4];
                                    message += Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4084", itemName);
                                    if (person.Age == 2)
                                    {
                                        message = Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4086", relativeDisplayName, itemName) + (relativeIsMale ? Game1.LoadStringByGender(person.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4088") : Game1.LoadStringByGender(person.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4089"));
                                    }
                                    else
                                    {
                                        switch (r.Next(5))
                                        {
                                            case 0:
                                                message = Game1.LoadStringByGender(person.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4091", new object[]
                                                {
                                                nameAndTitle,
                                                itemName
                                                });
                                                break;
                                            case 1:
                                                message = (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4094", nameAndTitle, itemName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4097", nameAndTitle, itemName));
                                                break;
                                            case 2:
                                                message = (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4100", nameAndTitle, itemName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4103", nameAndTitle, itemName));
                                                break;
                                            case 3:
                                                message = Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4106", nameAndTitle, itemName);
                                                break;
                                        }
                                        if (r.NextDouble() < 0.65)
                                        {
                                            switch (r.Next(5))
                                            {
                                                case 0:
                                                    message += (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4109") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4111"));
                                                    break;
                                                case 1:
                                                    message += (relativeIsMale ? ((r.NextDouble() < 0.5) ? Game1.LoadStringByGender(person.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4113") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4114")) : ((r.NextDouble() < 0.5) ? Game1.LoadStringByGender(person.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4115") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4116")));
                                                    break;
                                                case 2:
                                                    message += (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4118") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4120"));
                                                    break;
                                                case 3:
                                                    message += Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4125");
                                                    break;
                                                case 4:
                                                    message += (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4126") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4128"));
                                                    break;
                                            }
                                            if (relativeName.Equals("Abigail") && r.NextDouble() < 0.5)
                                            {
                                                message = Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4128", relativeDisplayName, itemName);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                try
                                {
                                    item = Convert.ToInt32(npcGiftTastes[relativeName].Split('/', StringSplitOptions.None)[7].Split(' ', StringSplitOptions.None)[r.Next(npcGiftTastes[relativeName].Split('/', StringSplitOptions.None)[7].Split(' ', StringSplitOptions.None).Length)]);
                                }
                                catch (Exception)
                                {
                                    item = Convert.ToInt32(npcGiftTastes["Universal_Hate"].Split(' ', StringSplitOptions.None)[r.Next(npcGiftTastes["Universal_Hate"].Split(' ', StringSplitOptions.None).Length)]);
                                }
                                if (Game1.objectInformation.ContainsKey(item))
                                {
                                    itemName = Game1.objectInformation[item].Split('/', StringSplitOptions.None)[4];
                                    message += (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4135", itemName, Lexicon.getRandomNegativeFoodAdjective(null)) : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4138", itemName, Lexicon.getRandomNegativeFoodAdjective(null)));
                                    if (person.Age == 2)
                                    {
                                        message = (relativeIsMale ? Game1.LoadStringByGender(person.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4141", new object[]
                                        {
                                            relativeDisplayName,
                                            itemName
                                        }) : Game1.LoadStringByGender(person.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4144", new object[]
                                        {
                                            relativeDisplayName,
                                            itemName
                                        }));
                                    }
                                    else
                                    {
                                        switch (r.Next(4))
                                        {
                                            case 0:
                                                message = ((r.NextDouble() < 0.5) ? Game1.LoadStringByGender(person.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4146") : "") + Game1.LoadStringByGender(person.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4147", new object[]
                                                {
                                                nameAndTitle,
                                                itemName
                                                });
                                                break;
                                            case 1:
                                                message = (relativeIsMale ? ((r.NextDouble() < 0.5) ? Game1.LoadStringByGender(person.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4149", new object[]
                                                {
                                                nameAndTitle,
                                                itemName
                                                }) : Game1.LoadStringByGender(person.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4152", new object[]
                                                {
                                                nameAndTitle,
                                                itemName
                                                })) : ((r.NextDouble() < 0.5) ? Game1.LoadStringByGender(person.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4153", new object[]
                                                {
                                                nameAndTitle,
                                                itemName
                                                }) : Game1.LoadStringByGender(person.Gender, "Strings\\StringsFromCSFiles:NPC.cs.4154", new object[]
                                                {
                                                nameAndTitle,
                                                itemName
                                                })));
                                                break;
                                            case 2:
                                                message = (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4161", nameAndTitle, itemName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4164", nameAndTitle, itemName));
                                                break;
                                        }
                                        if (r.NextDouble() < 0.65)
                                        {
                                            switch (r.Next(5))
                                            {
                                                case 0:
                                                    message += Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4170");
                                                    break;
                                                case 1:
                                                    message += Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4171");
                                                    break;
                                                case 2:
                                                    message += (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4172") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4174"));
                                                    break;
                                                case 3:
                                                    message += (relativeIsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4176") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4178"));
                                                    break;
                                                case 4:
                                                    message += Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4180");
                                                    break;
                                            }
                                            if (person.Name.Equals("Lewis") && r.NextDouble() < 0.5)
                                            {
                                                message = Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.4182", relativeDisplayName, itemName);
                                            }
                                        }
                                    }
                                }
                            }
                            if (itemName != null)
                            {
                                if (Game1.getCharacterFromName(relativeName, true, false) != null)
                                {
                                    message = message + "%revealtaste" + relativeName + item.ToString();
                                }
                                currentDialogue.Clear();
                                if (message.Length > 0)
                                {
                                    try
                                    {
                                        message = message.Substring(0, 1).ToUpper() + message.Substring(1, message.Length - 1);
                                    }
                                    catch (Exception)
                                    {
                                    }
                                }
                                currentDialogue.Push(new Dialogue(message, person));
                                person.CurrentDialogue = currentDialogue;
                            }
                        }
                    }
                }
                return;
            }
            if (e.Button == SButton.F3)
            {
                var person = Game1.getCharacterFromName("Marnie");
                person.sayHiTo(Game1.getCharacterFromName("Lewis"));
                return;
            }
        }
    }
}