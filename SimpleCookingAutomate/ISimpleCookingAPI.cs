using StardewValley;

namespace SimpleCookingAutomate
{
    public interface ISimpleCookingAPI
    {
        bool IsCooker(Object obj);
        bool TryGetCookingDataForCookable(Object obj, out ICookingData data);
        bool TryGetCookingDataForCooker(Object obj, out ICookingData data);
        void SetCookingDataForCooker(Object obj, ICookingData data);
    }

    public interface ICookingData
    {
        public int MinutesProgress { get; set; }
        public int MinutesToCook { get; set; }
        public int LastCheckTime { get; set; }
        public float Progress { get; }
        public bool Burned { get; }
        public float BurntAt { get; set; }
        public int Quality { get; set; }
        public string InputID { get; set; }
        public string ProductID { get; set; }
        public string BurntID { get; set; }
        public bool Smoke { get; set; }
        public string PlacedSound { get; set; }
        public string CookedSound { get; set; }
        public string BurntSound { get; set; }
        public Object GetProduct();
        public bool WillCook(int time);
        public bool WillBurn(int time);
        public void Update(Object obj, int timeOfDay);
    }

}