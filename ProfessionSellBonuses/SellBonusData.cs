using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace ProfessionSellBonuses
{
    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
    public enum BonusType
    {
        Mult,
        Add
    }
    public class SellBonusData
    {
        public BonusType Type { get; set; }
        public float Amount { get; set; }
        public List<object> Items { get; set; } = new();
    }
    public class SellBonusDataWhich : SellBonusData
    {
        public string Which { get; set; }
    }
}