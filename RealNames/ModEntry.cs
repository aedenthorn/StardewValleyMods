using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace RealNames
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {

        public static ModConfig Config;
        private static string[] maleNames;
        private static IMonitor PMonitor;
        private static Random myRand;
        private static string[] femaleNames;
        private static string lastRandomGender;
        private static string[] neuterNames;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.Enabled)
                return;

            PMonitor = Monitor;

            myRand = new Random();

            LoadNames();

            var harmony = new Harmony(this.ModManifest.UniqueID);

            /*
            harmony.Patch(
               original: typeof(NamingMenu).GetConstructor(new[] { typeof(NamingMenu.doneNamingBehavior), typeof(string), typeof(string) }),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(NamingMenu_Prefix))
            );
            */

            harmony.Patch(
               original: typeof(Dialogue).GetMethod(nameof(Dialogue.randomName), BindingFlags.Public | BindingFlags.Static),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(Dialogue_randomName_Prefix))
            );
        }

        private void ConvertNames()
        {
            List<string> men = maleNames.ToList();
            List<string> fem = new List<string>();
            foreach (string name in femaleNames)
            {
                if (!men.Contains(name))
                {
                    fem.Add(name);
                }
            }
            string filePath = $"{Helper.DirectoryPath}/assets/names_female_{Config.LocaleString}3.txt";
            File.WriteAllLines(filePath, fem.ToArray());
        }

        private void LoadNames()
        {
            string filePath = $"{Helper.DirectoryPath}/assets/names_female_{Config.LocaleString}.txt";
            if (File.Exists(filePath))
            {
                femaleNames = File.ReadAllLines(filePath);
                Monitor.Log($"Female names found at {filePath}.", LogLevel.Debug);
            }
            else
            {
                Monitor.Log($"Female names file not found at {filePath}.", LogLevel.Warn);
                femaleNames = new string[0];
            }
            filePath = $"{Helper.DirectoryPath}/assets/names_male_{Config.LocaleString}.txt";
            if (File.Exists(filePath))
            {
                maleNames = File.ReadAllLines(filePath);
                Monitor.Log($"Male names file found at {filePath}.", LogLevel.Debug);
            }
            else
            {
                Monitor.Log($"Male names file not found at {filePath}.", LogLevel.Warn);
                maleNames = new string[0];
            }

            if(Config.NeutralNameGender == "female")
            {
                neuterNames = femaleNames;

            }
            else if(Config.NeutralNameGender == "male")
            {
                neuterNames = maleNames;

            }
            else
            {
                filePath = $"{Helper.DirectoryPath}/assets/names_{Config.LocaleString}.txt";
                if (File.Exists(filePath))
                {
                    neuterNames = File.ReadAllLines(filePath);
                }
                else
                {
                    Monitor.Log($"Gender-neutral names file not found at {filePath}. Using combined male/female strings.", LogLevel.Debug);
                    neuterNames = new string[femaleNames.Length + maleNames.Length];
                    femaleNames.CopyTo(neuterNames, 0);
                    maleNames.CopyTo(neuterNames, femaleNames.Length);
                }
            }
        }


        private static string GetRandomName(Gender gender)
        {
            if(gender == Gender.Female)
            {
                if (femaleNames.Length == 0)
                    return "error";
                return femaleNames[myRand.Next(femaleNames.Length)];
            }
            else if(gender == Gender.Male)
            {
                if (maleNames.Length == 0)
                    return "error";
                return maleNames[myRand.Next(maleNames.Length)];
            }
            else
            {
                if (neuterNames.Length == 0)
                    return "error";
                return neuterNames[myRand.Next(neuterNames.Length)];
            }
        }

        private static bool Dialogue_randomName_Prefix(ref string __result)
        {
            if (!Config.Enabled)
                return true;
            Gender gender = Gender.Undefined;
            if(Game1.activeClickableMenu is NamingMenu namingMenu) 
            {
                string title = AccessTools.FieldRefAccess<NamingMenu, string>(namingMenu, "title");
                if (title == Game1.content.LoadString("Strings\\Events:BabyNamingTitle_Female"))
                {
                    PMonitor.Log("Baby is female.");
                    gender = Gender.Female;
                }
                else if (title == Game1.content.LoadString("Strings\\Events:BabyNamingTitle_Male"))
                {
                    PMonitor.Log("Baby is male.");
                    gender = Gender.Male;
                }
                else if(Config.RealNamesForAnimals && title == Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1236"))
                {
                    
                }
                else
                {
                    return true;
                }
            }
            else if(Config.RealNamesForAnimals && Game1.activeClickableMenu is PurchaseAnimalsMenu pamenu && pamenu.animalBeingPurchased != null)
            {
                gender = pamenu.animalBeingPurchased.Gender;
            }
            else if (!Config.RealNamesForAnimals)
            {
                return true;
            }
            string name = GetRandomName(gender);
            if(name == "" || name == "error")
            {
                PMonitor.Log("Error getting random name, reverting to vanilla method.", LogLevel.Warn);
                return true;
            }
            __result = name;
            return false;
        }
    }
}