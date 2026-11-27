using Microsoft.VisualBasic;
using Newtonsoft.Json;
using StardewValley;

namespace SimpleCooking
{
    public interface ISimpleCookingAPI
    {
        bool IsCooker(Object obj);
        bool TryGetCookingDataForCookable(Object obj, out ICookingData data);
        bool TryGetCookingDataForCooker(Object obj, out ICookingData data);
        void SetCookingDataForCooker(Object obj, ICookingData data);
    }
    public class SimpleCookingAPI : ISimpleCookingAPI
    {
        public bool IsCooker(Object obj)
        {
            return ModEntry.CookerDict.ContainsKey(obj.QualifiedItemId);
        }

        public bool TryGetCookingDataForCookable(Object obj, out ICookingData data)
        {
            return ModEntry.TryGetCookingDataForCookable(obj, out data);
        }
        public bool TryGetCookingDataForCooker(Object obj, out ICookingData data)
        {
            return ModEntry.TryGetCookingDataForCooker(obj, out data);
        }
        public void SetCookingDataForCooker(Object obj, ICookingData data)
        {
            if(data is null)
            {
                obj.modData.Remove(ModEntry.cookingKey);
            }
            else
            {
                obj.modData[ModEntry.cookingKey] = JsonConvert.SerializeObject(data);
            }
        }
    }
}