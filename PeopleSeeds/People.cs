using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Characters;
using StardewValley.TokenizableStrings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PeopleSeeds
{
    internal class People
    {

        internal static Dictionary<string, NPC> CloneTemplates { get; set; }


        internal static Dictionary<string, int> CloneGenerations { get; set; }

        public static string GetAngryFace(int personId)
        {
            string person = People.GetPerson(personId);
            if (personId < 12)
            {
                return "$5";
            }
            if (person == "Caroline")
            {
                return "$3";
            }
            if (person == "Clint")
            {
                return "$3";
            }
            if (person == "Demetrius")
            {
                return "$4";
            }
            if (person == "Evelyn")
            {
                return "$2";
            }
            if (person == "George")
            {
                return "$3";
            }
            if (person == "Gus")
            {
                return "$3";
            }
            if (person == "Jodi")
            {
                return "$3";
            }
            if (person == "Lewis")
            {
                return "$4";
            }
            if (person == "Linus")
            {
                return "$3";
            }
            if (person == "Marnie")
            {
                return "$3";
            }
            if (person == "Pam")
            {
                return "$4";
            }
            if (person == "Pierre")
            {
                return "$3";
            }
            if (person == "Robin")
            {
                return "$3";
            }
            if (person == "Sandy")
            {
                return "$2";
            }
            if (person == "Willy")
            {
                return "$2";
            }
            if (person == "Wizard")
            {
                return "$1";
            }
            return "$0";
        }

        // Token: 0x0600001F RID: 31 RVA: 0x00005384 File Offset: 0x00003584
        public static string GetScaredFace(int personId)
        {
            string person = People.GetPerson(personId);
            if (person == "Abigail" || person == "Alex")
            {
                return "$7";
            }
            if (person == "Emily")
            {
                return "$6";
            }
            if (person == "Haley")
            {
                return "$3";
            }
            if (person == "Maru")
            {
                return "$9";
            }
            if (person == "Sam")
            {
                return "$8";
            }
            if (person == "Shane")
            {
                return "$10";
            }
            if (personId < 12)
            {
                return "$2";
            }
            if (person == "Caroline")
            {
                return "$2";
            }
            if (person == "Clint")
            {
                return "$4";
            }
            if (person == "Demetrius")
            {
                return "$6";
            }
            if (person == "Evelyn")
            {
                return "$2";
            }
            if (person == "George")
            {
                return "$2";
            }
            if (person == "Gus")
            {
                return "$2";
            }
            if (person == "Jodi")
            {
                return "$4";
            }
            if (person == "Lewis")
            {
                return "$3";
            }
            if (person == "Linus")
            {
                return "$3";
            }
            if (person == "Marnie")
            {
                return "$4";
            }
            if (person == "Pam")
            {
                return "$3";
            }
            if (person == "Pierre")
            {
                return "$4";
            }
            if (person == "Robin")
            {
                return "$5";
            }
            if (person == "Sandy")
            {
                return "$2";
            }
            if (person == "Willy")
            {
                return "$1";
            }
            if (person == "Wizard")
            {
                return "$1";
            }
            return "$0";
        }

        // Token: 0x06000020 RID: 32 RVA: 0x00005558 File Offset: 0x00003758
        public static string GetStrangeFace(int personId)
        {
            string person = People.GetPerson(personId);
            if (person == "Alex")
            {
                return "$8";
            }
            if (person == "Emily")
            {
                return "$7";
            }
            if (person == "Haley")
            {
                return "$8";
            }
            if (personId < 12)
            {
                return "$4";
            }
            if (person == "Caroline")
            {
                return "$1";
            }
            if (person == "Clint")
            {
                return "$5";
            }
            if (person == "Demetrius")
            {
                return "$1";
            }
            if (person == "Evelyn")
            {
                return "$1";
            }
            if (person == "George")
            {
                return "$1";
            }
            if (person == "Gus")
            {
                return "$3";
            }
            if (person == "Jodi")
            {
                return "$1";
            }
            if (person == "Lewis")
            {
                return "$2";
            }
            if (person == "Linus")
            {
                return "$1";
            }
            if (person == "Marnie")
            {
                return "$2";
            }
            if (person == "Pam")
            {
                return "$1";
            }
            if (person == "Pierre")
            {
                return "$2";
            }
            if (person == "Robin")
            {
                return "$4";
            }
            if (person == "Sandy")
            {
                return "$3";
            }
            if (person == "Willy")
            {
                return "$3";
            }
            if (person == "Wizard")
            {
                return "$1";
            }
            return "$0";
        }

        // Token: 0x06000021 RID: 33 RVA: 0x000056E8 File Offset: 0x000038E8
        internal static void OnLoad()
        {
            People.CloneTemplates = new Dictionary<string, NPC>();
            People.CloneGenerations = new Dictionary<string, int>();
            foreach (string text in People.PersonNames)
            {
                NPC characterFromName = Game1.getCharacterFromName(text, false);
                People.CloneTemplates.Add(text, new NPC(new AnimatedSprite("Characters\\" + text, 0, 16, 32), characterFromName.position.Value, characterFromName.getHome().Name, 2, text, characterFromName.datable.Value, Game1.content.Load<Texture2D>("Portraits\\" + text)));
                People.CloneGenerations.Add(text, 0);
            }
        }

        // Token: 0x06000022 RID: 34 RVA: 0x000057E2 File Offset: 0x000039E2
        internal static bool IsSeed(string itemIndex)
        {
            return itemIndex.StartsWith(ModEntry.SHelper.ModRegistry.ModID);
        }


        internal static string GetPerson(int index)
        {
            return People.PersonNames[index];
        }

        internal static NPC ClonePerson(int index)
        {
            return People.ClonePerson(People.GetPerson(index));
        }

        internal static NPC ClonePerson(NPC npc)
        {
            return People.ClonePerson(npc.Name);
        }

        internal static NPC ClonePerson(string name)
        {
            NPC npc = People.CloneTemplates[name];
            Dictionary<string, int> cloneGenerations = People.CloneGenerations;
            cloneGenerations[name]++;
            NPC npc2 = new NPC(new AnimatedSprite("Characters\\" + npc.Name, 0, 16, 32), new Vector2(-5000f, -5000f), "BugLand", npc.FacingDirection, npc.Name, npc.datable.Value, Game1.content.Load<Texture2D>("Portraits\\" + npc.Name));
            npc2.FacingDirection = Game1.random.Next(4);
            npc2.id = 100 + (People.CloneGenerations[name] * 100 + npc.id);
            npc2.displayName = (npc2.Name = npc.Name);
            Game1.player.friendshipData[npc2.displayName] = new Friendship(0);
            return npc2;
        }

        internal static string GetModItemId(string name)
        {
            return ModEntry.SHelper.ModRegistry.ModID + "." + name + "Seeds";
        }

        public static string[] PersonNames {
            get
            {
                return ModEntry.SHelper.GameContent.Load<Dictionary<string, CharacterData>>("Data/Characters").Values.Where(npc => npc.CanReceiveGifts).Select(npc => TokenParser.ParseText(npc.DisplayName, null, null, null)).ToArray();
            }
        }
        public static HashSet<string> Banished = new HashSet<string>();
    }
}
