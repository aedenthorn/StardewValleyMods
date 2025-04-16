using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace CustomStarterFurniture
{
	public partial class ModEntry
	{
		public class FarmHouse_Patch
		{
			public static void Postfix(FarmHouse __instance)
			{
				if (!Config.ModEnabled)
					return;

				customStarterFurnitureDictionary = Game1.content.Load<Dictionary<string, StarterFurnitureData>>(dictionaryPath);
				foreach (StarterFurnitureData sfData in customStarterFurnitureDictionary.Values)
				{
					if (IsFarm(sfData.FarmType) && sfData.Clear)
					{
						__instance.furniture.Clear();
						break;
					}
				}
				foreach (StarterFurnitureData sfData in customStarterFurnitureDictionary.Values)
				{
					if (IsFarm(sfData.FarmType))
					{
						if (sfData.Furniture is not null)
						{
							foreach (StarterFurnitureData.FurnitureData fData in sfData.Furniture)
							{
								Furniture furniture = GetFurniture(fData.NameOrIndex, fData.X, fData.Y);

								if (furniture is not null)
								{
									if (fData.HeldObjectNameOrIndex is not null)
									{
										Object heldObject = GetObject(fData.HeldObjectNameOrIndex, fData.HeldObjectType);

										furniture.heldObject.Value = heldObject;
									}
									for (int i = 0; i < fData.Rotation; i++)
									{
										furniture.rotate();
									}
									__instance.furniture.Add(furniture);
								}
							}
						}
						if (sfData.BigCraftable is not null)
						{
							foreach (StarterFurnitureData.BigCraftableData fData in sfData.BigCraftable)
							{
								Object bigCraftable = GetBigCraftable(fData.NameOrIndex, fData.X, fData.Y);

								if (bigCraftable is not null)
								{
									__instance.tryPlaceObject(new Vector2(fData.X, fData.Y), bigCraftable);
								}
							}
						}
					}
				}
			}
		}
	}
}
