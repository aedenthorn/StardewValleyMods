using StardewModdingAPI;

namespace AdvancedCooking
{
	public class ModConfig
	{
		public bool ModEnabled { get; set; } = true;
		public bool AllowUnknownRecipes { get; set; } = true;
		public bool LearnUnknownRecipes { get; set; } = true;
		public SButton CookAllModKey { get; set; } = SButton.LeftShift;
		public bool HoldCookedItem { get; set; } = true;
		public bool ConsumeExtraIngredientsOnSucceed { get; set; } = false;
		public bool ConsumeIngredientsOnFail { get; set; } = false;
		public bool GiveTrashOnFail { get; set; } = true;
		public bool ShowProductInfo { get; set; } = true;
		public bool ShowCookTooltip { get; set; } = true;
		public bool ShowProductsInTooltip { get; set; } = true;
		public int MaxItemsInTooltip { get; set; } = 6;
		public int YOffset { get; set; } = 532;
	}
}
