using System;
using HarmonyLib;
using StardewValley;
using StardewValley.Menus;
using Object = StardewValley.Object;

namespace GenieLamp
{
	public partial class ModEntry
	{
		public class Object_performUseAction_Patch
		{
			public static bool Prefix(Object __instance)
			{
				if (!Config.ModEnabled || (!__instance.Name.Equals(Config.LampItem) && !__instance.QualifiedItemId.Equals(Config.LampItem)))
					return true;

				int wishes = __instance.modData.TryGetValue(modKey, out var w) ? int.Parse(w) : 0;

				if (wishes >= Config.WishesPerItem)
				{
					Game1.playSound("cancel", null);
					Game1.showRedMessage(SHelper.Translation.Get("NoMoreWishes"));
					return true;
				}
				try
				{
					Game1.playSound(Config.MenuSound, null);
				}
				catch { }
				AccessTools.Method(typeof(ItemRegistry), "RebuildCache").Invoke(null, Array.Empty<object>());

				Game1.activeClickableMenu = new ObjectPickMenu( new NamingMenu.doneNamingBehavior(delegate (string target)
				{
					SpawnItem(target);
				}), string.Format(Config.WishesPerItem - wishes == 1 ? SHelper.Translation.Get("WishMenuTitleSingular") : SHelper.Translation.Get("WishMenuTitlePlural"), Config.WishesPerItem - wishes));
				return false;
			}
		}
	}
}
