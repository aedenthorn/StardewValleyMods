using Harmony;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BiggerChests
{
	public class ModEntry : Mod
	{
		internal ModConfig Config { get; private set; }

		public override void Entry(IModHelper helper)
		{
			//helper.Events.GameLoop.UpdateTicking += this.UpdateTicking;
			this.Config = this.Helper.ReadConfig<ModConfig>();

			//ObjectPatches.magnetRangeMult = this.magnetRangeMult;
			var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);
			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}
	}

	//[HarmonyPatch(typeof(IClickableMenu))]
	//[HarmonyPatch(MethodType.Constructor)]
	//[HarmonyPatch(new Type[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool) })]
	static class IClickableMenu_Patch
	{
		static void Prefix(ref int height)
		{
			if(height == 64 * 3)
			{
				height = 64 * 4;
			}
		}
	}
	//[HarmonyPatch(typeof(InventoryMenu))]
	//[HarmonyPatch(MethodType.Constructor)]
	//[HarmonyPatch(new Type[] { typeof(int), typeof(int), typeof(bool), typeof(IList<Item>), typeof(InventoryMenu.highlightThisItem), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool) })]
	static class InventoryMenu_Patch
	{
		static void Prefix(ref int capacity,ref int rows, bool playerInventory)
		{
			if (playerInventory)
				return; 

			if(rows == 3)
			{
				rows = 4;
			}
			if(capacity == -1 || capacity == 36)
			{
				capacity = 12*4;
			}
		}
	}
	[HarmonyPatch(typeof(Chest))]
	[HarmonyPatch("addItem")]
	static class Chest_Patch
	{
		static void Postfix(Item item, ref Item __result, ref Chest __instance)
		{
			if(__result != null)
			{
				__instance.items.Add(item);
				__result = null;
			}
		}
	}
}
