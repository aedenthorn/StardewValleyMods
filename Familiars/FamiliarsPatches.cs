using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Network;
using System;
using xTile.Dimensions;
using Object = StardewValley.Object;

namespace Familiars
{
    public class FamiliarsPatches
	{
		private static IMonitor Monitor;
		private static ModConfig Config;
		private static IModHelper Helper;

		public static void Initialize(IMonitor monitor, IModHelper helper, ModConfig config)
		{
			Monitor = monitor;
			Config = config;
			Helper = helper;
		}

        public static void Object_performObjectDropInAction_Postfix(Object __instance, Item dropInItem, ref bool __result, bool probe, Farmer who)
		{
			if (__instance.isTemporarilyInvisible)
			{
				return;
			}
			if (!(dropInItem is Object))
			{
				return;
			}
			Object dropIn = dropInItem as Object;

			if (__instance.name.Equals("Slime Incubator"))
			{
				if (__instance.heldObject.Value == null && dropIn.name.Contains("Familiar Egg"))
				{
					__instance.heldObject.Value = new Object(dropIn.parentSheetIndex, 1, false, -1, 0);
					if (!probe)
					{
						who.currentLocation.playSound("coin", NetAudio.SoundContext.Default);
						__instance.minutesUntilReady.Value = Config.FamiliarHatchMinutes;
						if (who.professions.Contains(2))
						{
							__instance.minutesUntilReady.Value /= 2;
						}
						int num = __instance.ParentSheetIndex;
						__instance.ParentSheetIndex = num + 1;
					}
					__result = true;
				}
			}
		}
		
        public static void Object_DayUpdate_Postfix(Object __instance, GameLocation location)
		{
			if (__instance.minutesUntilReady <= 0 && __instance.heldObject.Value != null)
			{
				Vector2 v = new Vector2((float)((int)__instance.tileLocation.X), (float)((int)__instance.tileLocation.Y + 1)) * 64f;
				string name = __instance.heldObject.Value.name;

				if (!name.EndsWith("Familiar Egg"))
					return;

				Familiar familiar = null;

                switch (name)
                {
					case "Dino Familiar Egg":
						familiar = new DinoFamiliar(v, Game1.getFarmer(__instance.owner));
						break;
					case "Dust Sprite Familiar Egg":
						familiar = new DustSpriteFamiliar(v, Game1.getFarmer(__instance.owner));
						break;
					case "Bat Familiar Egg":
						familiar = new BatFamiliar(v, Game1.getFarmer(__instance.owner));
						break;
                }

				if (familiar != null)
				{
					Game1.showGlobalMessage(string.Format(Helper.Translation.Get("familiar-hatched"), Helper.Translation.Get(familiar.Name)));
					familiar.setTilePosition((int)__instance.tileLocation.X, (int)__instance.tileLocation.Y + 1);
					location.characters.Add(familiar);
					__instance.heldObject.Value = null;
					__instance.ParentSheetIndex = 156;
					__instance.minutesUntilReady.Value = -1;
				}
			}
		}
				
        public static void Object_minutesElapsed_Postfix(Object __instance, int minutes, GameLocation environment)
		{
			if (__instance.heldObject.Value != null && __instance.heldObject.Value.name.EndsWith("Familiar Egg") && __instance.minutesUntilReady <= 0)
			{
				Vector2 v = new Vector2((float)((int)__instance.tileLocation.X), (float)((int)__instance.tileLocation.Y + 1)) * 64f;
				Familiar familiar = null;

                switch (__instance.heldObject.Value.name)
                {
					case "Dino Familiar Egg":
						familiar = new DinoFamiliar(v, Game1.getFarmer(__instance.owner));
						break;
					case "Dust Sprite Familiar Egg":
						familiar = new DustSpriteFamiliar(v, Game1.getFarmer(__instance.owner));
						break;
					case "Bat Familiar Egg":
						familiar = new BatFamiliar(v, Game1.getFarmer(__instance.owner));
						break;
                }

				if (familiar != null)
				{
					Game1.showGlobalMessage(string.Format(Helper.Translation.Get("familiar-hatched"), Helper.Translation.Get(familiar.Name)));
					familiar.setTilePosition((int)__instance.tileLocation.X, (int)__instance.tileLocation.Y + 1);
					environment.characters.Add(familiar);
					__instance.heldObject.Value = null;
					__instance.ParentSheetIndex = 156;
					__instance.minutesUntilReady.Value = -1;
				}
			}
		}

        public static void GameLocation_drawAboveFrontLayer_Postfix(GameLocation __instance, SpriteBatch b)
		{
			foreach (Character c in __instance.characters)
			{
				if (c is Familiar)
				{
					(c as Familiar).drawAboveAllLayers(b);
				}
			}
		}
        public static void GameLocation_checkAction_Postfix(GameLocation __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result)
		{
			if (!(__instance is SlimeHutch))
				return;

			Microsoft.Xna.Framework.Rectangle tileRect = new Microsoft.Xna.Framework.Rectangle(tileLocation.X * 64, tileLocation.Y * 64, 64, 64);

			foreach (NPC i in __instance.characters)
			{
				if (i != null &&  i is Familiar && (i as Familiar).owner.Equals(who) && i.GetBoundingBox().Intersects(tileRect))
				{
					(i as Familiar).followingPlayer = !(i as Familiar).followingPlayer;
					__instance.playSound("dwop");
					Monitor.Log($"familiar following player: {(i as Familiar).followingPlayer}");
					__result = true;
					return;
				}
			}
		}
		public static void Utility_checkForCharacterInteractionAtTile_Postfix(Vector2 tileLocation, Farmer who, ref bool __result)
		{
			if (!(who.currentLocation is SlimeHutch))
				return;

			NPC character = Game1.currentLocation.isCharacterAtTile(tileLocation);
			if (character != null && character is Familiar && (character as Familiar).owner.Equals(who))
            {
				Game1.mouseCursor = 4;
				__result = true;
			}
		}
    }
}