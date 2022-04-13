using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Object = StardewValley.Object;

namespace CustomFarmAnimals
{
    public partial class ModEntry
    {
        private static string GetTypeConditionalOnBuilding(string type)
        {
            switch (type)
            {
                case "Coop":
                    return (Game1.getFarm().isBuildingConstructed("Coop") || Game1.getFarm().isBuildingConstructed("Big Coop") || Game1.getFarm().isBuildingConstructed("Deluxe Coop")) ? null : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5926");
                case "Big Coop":
                    return (Game1.getFarm().isBuildingConstructed("Big Coop") || Game1.getFarm().isBuildingConstructed("Deluxe Coop")) ? null : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5940");
                case "Deluxe Coop":
                    return (Game1.getFarm().isBuildingConstructed("Big Coop") || Game1.getFarm().isBuildingConstructed("Deluxe Coop")) ? null : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5947");
                case "Barn":
                    return (Game1.getFarm().isBuildingConstructed("Barn")|| Game1.getFarm().isBuildingConstructed("Big Barn") || Game1.getFarm().isBuildingConstructed("Deluxe Barn")) ? null : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5931"); 
                case "Big Barn":
                    return (Game1.getFarm().isBuildingConstructed("Barn")|| Game1.getFarm().isBuildingConstructed("Big Barn") || Game1.getFarm().isBuildingConstructed("Deluxe Barn")) ? null : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5933"); 
                case "Deluxe Barn":
                    return Game1.getFarm().isBuildingConstructed("Deluxe Barn") ? null : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5950"); 
                default:
                    return Game1.getFarm().isBuildingConstructed(type) ? null : Game1.content.LoadString(modPath + type); 
            }
            
        }
        private static string GetGenericName(string key)
        {
            switch (key)
            {
                case "Brown Cow":
                case "White Cow":
                    return "Dairy Cow";
                case "Brown Chicken":
                case "White Chicken":
                case "Blue Chicken":
                case "Void Chicken":
                case "Golden Chicken":
                    return "Chicken";
                default:
                    return key;
            }
        }
        private static string GetName(string key)
        {
            string rawData;
            Game1.content.Load<Dictionary<string, string>>("Data\\FarmAnimals").TryGetValue(key, out rawData);
            if (rawData != null)
            {
                return rawData.Split('/', StringSplitOptions.None)[25];
            }
            return null;
        }
    }
}