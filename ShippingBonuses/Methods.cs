using StardewValley;
using StardewValley.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace ShippingBonuses
{
    public partial class ModEntry
    {

        public static BonusData GetRandomBonusData()
        {
            List<BonusType> types = new List<BonusType>();
            foreach(var t in Enum.GetValues(typeof(BonusType)))
            {
                BonusType bt = (BonusType)t;
                if (!Config.AllowMultSame && todayBonuses.Exists(b => b.type == bt))
                    continue;
                types.Add((BonusType)t);
            }
            int totalWeight = Config.StarWeight + Config.CategoryWeight + Config.VegetableWeight + Config.FruitWeight + Config.ArtisanWeight + Config.FlowerWeight + Config.MineralWeight;
            int which = Game1.random.Next(totalWeight);
            int cumulative = 0;
            cumulative += Config.StarWeight;
            if(cumulative < which)
            {
                return new BonusData() { type = BonusType.Star, which = Game1.random.Next(4)+"" };
            }
            cumulative += Config.CategoryWeight;
            if(cumulative < which)
            {
                return new BonusData() { type = BonusType.Category, which = Game1.random.ChooseFrom(typeof(Object).GetFields().Where(fi => fi.Name.ToLower().Contains("category") && fi.FieldType == typeof(int)).ToList()).GetValue(null)+"" };
            }
            cumulative += Config.VegetableWeight;
            if(cumulative < which)
            {
                return new BonusData() { type = BonusType.Vegetable, which = Game1.random.ChooseFrom(Game1.objectData.Where(kvp => kvp.Value.Category == Object.VegetableCategory).Select(kvp => kvp.Key).ToList()) };
            }
            cumulative += Config.FruitWeight;
            if(cumulative < which)
            {
                return new BonusData() { type = BonusType.Fruit, which = Game1.random.ChooseFrom(Game1.objectData.Where(kvp => kvp.Value.Category == Object.FruitsCategory).Select(kvp => kvp.Key).ToList()) };
            }
            cumulative += Config.ArtisanWeight;
            if(cumulative < which)
            {
                return new BonusData() { type = BonusType.Aritsan, which = Game1.random.ChooseFrom(Game1.objectData.Where(kvp => kvp.Value.Category == Object.EggCategory || kvp.Value.Category == Object.MilkCategory || kvp.Value.Category == Object.artisanGoodsCategory || kvp.Value.Category == Object.syrupCategory).Select(kvp => kvp.Key).ToList()) };
            }
            cumulative += Config.FlowerWeight;
            if(cumulative < which)
            {
                return new BonusData() { type = BonusType.Flower, which = Game1.random.ChooseFrom(Game1.objectData.Where(kvp => kvp.Value.Category == Object.flowersCategory).Select(kvp => kvp.Key).ToList()) };
            }
            return new BonusData() { type = BonusType.Mineral, which = Game1.random.ChooseFrom(Game1.objectData.Where(kvp => kvp.Value.Category == Object.mineralsCategory).Select(kvp => kvp.Key).ToList()) };
        }
    }
}