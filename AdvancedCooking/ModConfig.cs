
using StardewModdingAPI;

namespace AdvancedCooking
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public SButton CookAllModKey { get; set; } = SButton.LeftShift;
        public bool StoreOtherHeldItemOnCook { get; set; } = true;
        public bool ConsumeIngredientsOnFail { get; set; } = false;
        public bool GiveTrashOnFail { get; set; } = true;
        public bool ConsumeExtraIngredientsOnSucceed { get; set; } = false;
        public bool AllowUnknownRecipes { get; set; } = true;
        public bool LearnUnknownRecipes { get; set; } = true;
        public bool ShowCookTooltip { get; set; } = true;
        public bool ShowProductsInTooltip { get; set; } = true;
        public bool ShowProductInfo { get; set; } = true;
        public int MaxTypesInTooltip { get; set; } = 3;
        public int YOffset { get; set; } = 532;
    }
}
