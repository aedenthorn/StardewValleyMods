using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.GameData.Crops;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;

namespace ModifyThis
{
	public partial class ModEntry
	{
		public static void StartWizard()
		{
			cursorTile = Game1.GetPlacementGrabTile();
			cursorTile = new Vector2((int)cursorTile.X, (int)cursorTile.Y);
			thing = GetThingAtCursor(Game1.player.currentLocation, cursorTile);

			if (thing is not null)
			{
				SMonitor.Log($"Starting ModifyThis wizard. (thing: {thing}, cursorTile: {cursorTile})");
				Game1.currentLocation.ShowPagedResponses(SHelper.Translation.Get("welcome", new { thingName = GetThingName(thing) }), GetResponses(), OnResponse);
			}
		}

		private static object GetThingAtCursor(GameLocation location, Vector2 cursorTile)
		{
			if (location.objects.ContainsKey(cursorTile))
			{
				return location.objects[cursorTile];
			}
			if (location.overlayObjects.ContainsKey(cursorTile))
			{
				return location.overlayObjects[cursorTile];
			}

			Furniture furniture = location.furniture.FirstOrDefault(f => f.GetBoundingBox().Contains(cursorTile * Game1.tileSize));

			if (furniture is not null)
			{
				return furniture;
			}

			ResourceClump resourceClump = location.resourceClumps.FirstOrDefault(f => f.occupiesTile((int)cursorTile.X, (int)cursorTile.Y));

			if (resourceClump is not null)
			{
				return resourceClump;
			}
			if (location.terrainFeatures.ContainsKey(cursorTile))
			{
				return location.terrainFeatures[cursorTile];
			}

			LargeTerrainFeature LargeTerrainFeature = location.largeTerrainFeatures.FirstOrDefault(ltf => ltf.getBoundingBox().Contains(cursorTile * Game1.tileSize));

			if (LargeTerrainFeature is not null)
			{
				return LargeTerrainFeature;
			}

			FarmAnimal farmanimal = location.Animals.Pairs.FirstOrDefault(f => f.Value.Tile == cursorTile).Value;

			if (farmanimal is not null)
			{
				return farmanimal;
			}

			NPC npc = location.characters.FirstOrDefault(f => f.Tile == cursorTile);

			if (npc is not null)
			{
				return npc;
			}

			Building building = location.buildings.FirstOrDefault(b => b.occupiesTile(cursorTile));

			if (building is not null)
			{
				return building;
			}
			return null;
		}

        private static string GetThingName(object thing)
        {
            if (thing is Object @object)
			{
                return @object.DisplayName;
			}
            if (thing is HoeDirt hoeDirt)
            {
                if (hoeDirt.crop is not null)
                {
                    return new Object(hoeDirt.crop.indexOfHarvest.Value, 1).DisplayName;
                }
            }
			if (thing is Character character)
			{
				if (thing is NPC || thing is FarmAnimal)
				{
					return character.displayName;
				}
			}
			if (thing is Building building)
			{
				return TokenParser.ParseText(building.GetData().Name);
			}
            return thing.GetType().Name;
        }

		private static List<KeyValuePair<string, string>> GetResponses()
		{
			List<KeyValuePair<string, string>> responses = new();

			if ((thing is Object @object && (@object.Type is not null && (@object.Type.Equals("Crafting") || @object.Type.Equals("interactive"))) && (thing is not Chest chest || !chest.Items.Any())) || thing is Furniture)
			{
				responses.Add(new KeyValuePair<string, string>("ModifyThis_Wizard_Questions_Pickup", SHelper.Translation.Get("pickup")));
			}
			if (thing is FarmAnimal || thing is Horse)
			{
				responses.Add(new  KeyValuePair<string, string>("ModifyThis_Wizard_Questions_Rename", SHelper.Translation.Get("rename")));
			}
			if ((thing is HoeDirt hoeDirt && hoeDirt.crop is not null && hoeDirt.crop.currentPhase.Value < hoeDirt.crop.phaseDays.Count - 1 && !hoeDirt.crop.dead.Value) || (thing is IndoorPot indoorPot && ((indoorPot.hoeDirt.Value?.crop is not null && indoorPot.hoeDirt.Value.crop.currentPhase.Value < indoorPot.hoeDirt.Value.crop.phaseDays.Count - 1 && !indoorPot.hoeDirt.Value.crop.dead.Value) || (indoorPot.bush.Value is not null && indoorPot.bush.Value.getAge() < 20))) || (thing is Tree tree && tree.growthStage.Value < 5) || (thing is FruitTree fruitTree && fruitTree.growthStage.Value < 4) || (thing is Bush bush && bush.size.Value == Bush.greenTeaBush && bush.getAge() < 20) || (thing is FarmAnimal farmAnimal && !farmAnimal.isAdult()) || (thing is Child child && child.Age < 3))
			{
				responses.Add(new  KeyValuePair<string, string>("ModifyThis_Wizard_Questions_Grow", SHelper.Translation.Get("grow")));
			}
			if ((thing is HoeDirt hoeDirt2 && !hoeDirt2.isWatered()) || (thing is IndoorPot indoorPot2 && indoorPot2.hoeDirt.Value is not null && !indoorPot2.hoeDirt.Value.isWatered()))
			{
				responses.Add(new  KeyValuePair<string, string>("ModifyThis_Wizard_Questions_Water", SHelper.Translation.Get("water")));
			}
			if ((thing is HoeDirt hoeDirt3 && hoeDirt3.crop is not null && hoeDirt3.crop.dead.Value) || (thing is IndoorPot indoorPot3 && indoorPot3.hoeDirt.Value?.crop is not null && indoorPot3.hoeDirt.Value.crop.dead.Value) || (thing is Tree tree2 && tree2.stump.Value) || (thing is FruitTree fruitTree2 && (fruitTree2.stump.Value || fruitTree2.struckByLightningCountdown.Value > 0)))
			{
				responses.Add(new  KeyValuePair<string, string>("ModifyThis_Wizard_Questions_Revive", SHelper.Translation.Get("revive")));
			}
			if (thing is Object || thing is Building || thing is TerrainFeature || thing is LargeTerrainFeature || thing is ResourceClump || thing is Furniture)
			{
				responses.Add(new  KeyValuePair<string, string>("ModifyThis_Wizard_Questions_Move", SHelper.Translation.Get("move")));
			}
			if (thing is Monster)
			{
				responses.Add(new  KeyValuePair<string, string>("ModifyThis_Wizard_Questions_Kill", SHelper.Translation.Get("kill")));
			}
			responses.Add(new  KeyValuePair<string, string>("ModifyThis_Wizard_Questions_Remove", SHelper.Translation.Get("remove")));
			return responses;
		}

		private static void OnResponse(string response)
		{
			switch (response)
			{
				case "ModifyThis_Wizard_Questions_Pickup":
					PickupThis();
					return;
				case "ModifyThis_Wizard_Questions_Rename":
					RenameThis();
					return;
				case "ModifyThis_Wizard_Questions_Grow":
					GrowThis();
					return;
				case "ModifyThis_Wizard_Questions_Water":
					WaterThis();
					return;
				case "ModifyThis_Wizard_Questions_Revive":
					ReviveThis();
					return;
				case "ModifyThis_Wizard_Questions_Move":
					Game1.currentLocation.ShowPagedResponses(SHelper.Translation.Get("confirm-move"), GetMoveResponses(), OnMoveResponse);
					break;
				case "ModifyThis_Wizard_Questions_Kill":
					KillThis();
					return;
				case "ModifyThis_Wizard_Questions_Remove":
					Game1.currentLocation.ShowPagedResponses(SHelper.Translation.Get("confirm-remove"), GetRemoveResponses(), OnRemoveResponse, addCancel: false);
					break;
				default:
					return;
			}
		}

		private static List<KeyValuePair<string, string>> GetMoveResponses()
		{
			List<KeyValuePair<string, string>> responses = new();

			responses.Add(new KeyValuePair<string, string>($"ModifyThis_Wizard_Questions_Move_Up", Game1.content.LoadString("Strings\\StringsFromCSFiles:Up")));
			responses.Add(new KeyValuePair<string, string>($"ModifyThis_Wizard_Questions_Move_Right", Game1.content.LoadString("Strings\\StringsFromCSFiles:Right")));
			responses.Add(new KeyValuePair<string, string>($"ModifyThis_Wizard_Questions_Move_Down", Game1.content.LoadString("Strings\\StringsFromCSFiles:Down")));
			responses.Add(new KeyValuePair<string, string>($"ModifyThis_Wizard_Questions_Move_Left", Game1.content.LoadString("Strings\\StringsFromCSFiles:Left")));
			return responses;
		}

		private static void OnMoveResponse(string response)
		{
			switch (response)
			{
				case "ModifyThis_Wizard_Questions_Move_Up":
					MoveThis("up");
					return;
				case "ModifyThis_Wizard_Questions_Move_Right":
					MoveThis("right");
					return;
				case "ModifyThis_Wizard_Questions_Move_Down":
					MoveThis("down");
					return;
				case "ModifyThis_Wizard_Questions_Move_Left":
					MoveThis("left");
					return;
				default:
					return;
			}
		}

		private static List<KeyValuePair<string, string>> GetRemoveResponses()
		{
			List<KeyValuePair<string, string>> responses = new();

			responses.Add(new KeyValuePair<string, string>($"ModifyThis_Wizard_Questions_Remove_Yes", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes")));
			responses.Add(new KeyValuePair<string, string>($"ModifyThis_Wizard_Questions_Remove_No", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")));
			return responses;
		}

		private static void OnRemoveResponse(string response)
		{
			switch (response)
			{
				case "ModifyThis_Wizard_Questions_Remove_Yes":
					RemoveThis();
					return;
				default:
					return;
			}
		}

		private static void PickupThis()
		{
			SMonitor.Log($"Picking up");
			if (thing is Object @object && (Game1.currentLocation.objects.ContainsKey(cursorTile) || Game1.currentLocation.overlayObjects.ContainsKey(cursorTile)))
			{
				if (@object.heldObject.Value is not null)
				{
					Game1.player.currentLocation.debris.Add(new Debris(@object.heldObject.Value, @object.TileLocation * Game1.tileSize + new Vector2(32f, 32f)));
				}
				Game1.player.currentLocation.debris.Add(new Debris(@object, @object.TileLocation * Game1.tileSize + new Vector2(32f, 32f)));
				if (Game1.currentLocation.objects.ContainsKey(cursorTile))
				{
					Game1.currentLocation.objects.Remove(cursorTile);
				}
				else
				{
					Game1.currentLocation.overlayObjects.Remove(cursorTile);
				}
			}
			else if (thing is Furniture furniture && Game1.currentLocation.furniture.Contains(furniture))
			{
				if (furniture.heldObject.Value is not null)
				{
					Game1.player.currentLocation.debris.Add(new Debris(furniture.heldObject.Value, furniture.TileLocation * Game1.tileSize + new Vector2(32f, 32f)));
				}
				Game1.player.currentLocation.debris.Add(new Debris(furniture, furniture.TileLocation * Game1.tileSize + new Vector2(32f, 32f)));
				Game1.currentLocation.furniture.Remove(furniture);
			}
		}

		private static void RenameThis()
		{
			SMonitor.Log($"Renaming");

			if (thing is FarmAnimal || thing is Horse)
			{
				string defaultName = null;

				if (thing is FarmAnimal farmAnimal)
				{
					defaultName = farmAnimal.Name;
				}
				else if (thing is Horse horse)
				{
					defaultName = horse.Name;
				}
				Game1.afterDialogues = () => {
					Game1.activeClickableMenu = new NamingMenu(new NamingMenu.doneNamingBehavior(DoneNaming), SHelper.Translation.Get("what-name"), defaultName);
				};
				return;
			}
		}


		private static void DoneNaming(string s)
		{
			if(!string.IsNullOrEmpty(s))
			{
				if (thing is FarmAnimal farmAnimal)
				{
					farmAnimal.Name = s;
				}
				else if (thing is Horse horse)
				{
					horse.nameHorse(s);
				}
			}
			Game1.exitActiveMenu();
		}

		private static void GrowThis()
		{
			SMonitor.Log($"Growing");
			if (thing is HoeDirt hoeDirt && hoeDirt.crop is not null && hoeDirt.crop.currentPhase.Value < hoeDirt.crop.phaseDays.Count - 1 && !hoeDirt.crop.dead.Value)
			{
				hoeDirt.crop.growCompletely();
				SMonitor.Log($"Grew crop");
				return;
			}
			if (thing is IndoorPot indoorPot)
			{
				if (indoorPot.hoeDirt.Value?.crop is not null && indoorPot.hoeDirt.Value.crop.currentPhase.Value < indoorPot.hoeDirt.Value.crop.phaseDays.Count - 1 && !indoorPot.hoeDirt.Value.crop.dead.Value)
				{
					indoorPot.hoeDirt.Value.crop.growCompletely();
					SMonitor.Log($"Grew pot crop");
					return;
				}
				if (indoorPot.bush.Value is not null && indoorPot.bush.Value.getAge() < 20)
				{
					indoorPot.bush.Value.datePlanted.Value -= 20 - indoorPot.bush.Value.getAge();
					indoorPot.bush.Value.loadSprite();
					SMonitor.Log($"Grew pot bush");
					return;
				}
			}
			if (thing is Tree tree && tree.growthStage.Value < 5)
			{
				tree.growthStage.Value = 5;
				tree.fertilized.Value = true;
				tree.dayUpdate();
				tree.fertilized.Value = false;
				SMonitor.Log($"Grew tree");
				return;
			}
			if (thing is FruitTree fruitTree && fruitTree.growthStage.Value < 4)
			{
				fruitTree.growthStage.Value = 4;
				fruitTree.daysUntilMature.Value = 0;
				SMonitor.Log($"Grew fruit tree");
				return;
			}
			if (thing is Bush bush && bush.size.Value == Bush.greenTeaBush && bush.getAge() < 20)
			{
				bush.datePlanted.Value -= 20 - bush.getAge();
				bush.loadSprite();
				SMonitor.Log($"Grew tea bush tree");
				return;
			}
			if (thing is FarmAnimal farmAnimal && !farmAnimal.isAdult())
			{
				farmAnimal.growFully();
				SMonitor.Log($"Grew animal");
				return;
			}
			if (thing is NPC npc && npc is Child && npc.Age < 3)
			{
				npc.Age = 3;
				SMonitor.Log($"Grew child");
				return;
			}
		}

		private static void WaterThis()
		{
			SMonitor.Log($"Watering");
			if (thing is HoeDirt hoeDirt && !hoeDirt.isWatered())
			{
				hoeDirt.state.Value = 1;
				SMonitor.Log($"Watered crop");
				return;
			}
			if (thing is IndoorPot indoorPot)
			{
				if (indoorPot.hoeDirt.Value is not null && !indoorPot.hoeDirt.Value.isWatered())
				{
					indoorPot.Water();
					SMonitor.Log($"Watered pot crop");
					return;
				}
			}
		}

		private static void ReviveThis()
		{
			SMonitor.Log($"Reviving");

			if (thing is HoeDirt hoeDirt && hoeDirt.crop is not null && hoeDirt.crop.dead.Value)
			{
				CropData cropdata = hoeDirt.crop.GetData();

				hoeDirt.crop.dead.Value = false;
				if (cropdata is not null && hoeDirt.crop.GetData().IsRaised)
				{
					hoeDirt.crop.raisedSeeds.Value = true;
				}
				hoeDirt.crop.growCompletely();
				SMonitor.Log($"Revived crop");
				return;
			}
			if (thing is IndoorPot indoorPot && indoorPot.hoeDirt.Value?.crop is not null && indoorPot.hoeDirt.Value.crop.dead.Value)
			{
				CropData cropdata = indoorPot.hoeDirt.Value.crop.GetData();

				indoorPot.hoeDirt.Value.crop.dead.Value = false;
				if (cropdata is not null && indoorPot.hoeDirt.Value.crop.GetData().IsRaised)
				{
					indoorPot.hoeDirt.Value.crop.raisedSeeds.Value = true;
				}
				indoorPot.hoeDirt.Value.crop.growCompletely();
				SMonitor.Log($"Revived pot crop");
				return;
			}
			if (thing is Tree tree && tree.stump.Value)
			{
				tree.stump.Value = false;
				tree.health.Value = 10f;
				tree.shakeRotation = 0f;
			}
			if (thing is FruitTree fruitTree && (fruitTree.stump.Value || fruitTree.struckByLightningCountdown.Value > 0))
			{
				if (fruitTree.struckByLightningCountdown.Value > 0)
				{
					fruitTree.performUseAction(fruitTree.Tile);
					fruitTree.struckByLightningCountdown.Value = 0;
				}
				fruitTree.stump.Value = false;
				fruitTree.health.Value = 10f;
				fruitTree.shakeRotation = 0f;
			}
		}

		private static void MoveThis(string direction)
		{
			SMonitor.Log($"Moving");
			Vector2 shiftVector = direction switch
			{
				"up" => new Vector2(0, -1),
				"down" => new Vector2(0, 1),
				"left" => new Vector2(-1, 0),
				"right" => new Vector2(1, 0),
				_ => new Vector2(0, 0)
			};

			if (thing is Object @object && (Game1.currentLocation.objects.ContainsKey(cursorTile) || Game1.currentLocation.overlayObjects.ContainsKey(cursorTile)) && !Game1.currentLocation.objects.ContainsKey(cursorTile + shiftVector))
			{
				SMonitor.Log($"{@object.name} is an object");
				if (Game1.currentLocation.objects.ContainsKey(cursorTile))
				{
					Game1.currentLocation.objects[cursorTile + shiftVector] = Game1.currentLocation.objects[cursorTile];
					Game1.currentLocation.objects.Remove(cursorTile);
					SMonitor.Log($"moved {@object.name} {direction}");
					return;
				}
				else
				{
					Game1.currentLocation.overlayObjects[cursorTile + shiftVector] = Game1.currentLocation.overlayObjects[cursorTile];
					Game1.currentLocation.overlayObjects.Remove(cursorTile);
					SMonitor.Log($"moved {@object.name} {direction}");
					return;
				}
			}
			if (thing is Furniture furniture && Game1.currentLocation.furniture.Contains(furniture))
			{
				SMonitor.Log($"{furniture.name} is furniture");
				furniture.TileLocation += shiftVector;
				furniture.updateDrawPosition();
				SMonitor.Log($"moved {furniture.name} {direction}");
				return;
			}
			if (thing is ResourceClump resourceClump && Game1.currentLocation.resourceClumps.Contains(resourceClump))
			{
				SMonitor.Log($"thing is a ResourceClump");
				Game1.currentLocation.resourceClumps.Remove(resourceClump);
				resourceClump.Tile += shiftVector;
				Game1.currentLocation.resourceClumps.Add(resourceClump);
				SMonitor.Log($"moved {resourceClump.GetType()} {direction}");
				return;
			}
			if (thing is LargeTerrainFeature largeTerrainFeature && Game1.currentLocation.largeTerrainFeatures.Contains(largeTerrainFeature))
			{
				SMonitor.Log($"thing is a LargeTerrainFeature");
				Game1.currentLocation.largeTerrainFeatures.Remove(largeTerrainFeature);
				largeTerrainFeature.Tile += shiftVector;
				Game1.currentLocation.largeTerrainFeatures.Add(largeTerrainFeature);
				SMonitor.Log($"moved {largeTerrainFeature.GetType()} {direction}");
				return;
			}
			if (thing is TerrainFeature terrainFeature && Game1.currentLocation.terrainFeatures.ContainsKey(cursorTile) && !Game1.currentLocation.terrainFeatures.ContainsKey(cursorTile + shiftVector))
			{
				SMonitor.Log($"thing is a terrain feature");
				Game1.currentLocation.terrainFeatures[cursorTile + shiftVector] = Game1.currentLocation.terrainFeatures[cursorTile];
				Game1.currentLocation.terrainFeatures.Remove(cursorTile);
				SMonitor.Log($"moved {terrainFeature.GetType()} {direction}");
				return;
			}
			if (thing is Building building)
			{
				(Game1.currentLocation as Farm).buildings.Remove(building);
				building.tileX.Value += (int)shiftVector.X;
				building.tileY.Value += (int)shiftVector.Y;
				(Game1.currentLocation as Farm).buildings.Add(building);
				return;
			}
		}

		private static void KillThis()
		{
			SMonitor.Log($"Killing");
			if (thing is Monster monster)
			{
				monster.Health = -1;
				monster.deathAnimation();
				Game1.currentLocation.monsterDrop(monster, (int)monster.Position.X, (int)monster.Position.Y, Game1.player);
				SMonitor.Log($"killed monster");
				return;
			}
		}

		private static void RemoveThis()
		{
			SMonitor.Log($"Removing");
			if (thing is Object @object && (Game1.currentLocation.objects.ContainsKey(cursorTile) || Game1.currentLocation.overlayObjects.ContainsKey(cursorTile))) 
			{
				SMonitor.Log($"{@object.name} is an object");
				if (Game1.currentLocation.objects.ContainsKey(cursorTile))
				{
					Game1.currentLocation.objects.Remove(cursorTile);
					SMonitor.Log($"removed {@object.name} from objects");
					return;
				}
				else
				{
					Game1.currentLocation.overlayObjects.Remove(cursorTile);
					SMonitor.Log($"removed {@object.name} from overlayObjects");
					return;
				}
			}
			if (thing is Furniture furniture && Game1.currentLocation.furniture.Contains(furniture)) 
			{
				SMonitor.Log($"{furniture.name} is furniture");
				Game1.currentLocation.furniture.Remove(furniture);
				SMonitor.Log($"removed {furniture.name} from furniture");
				return;
			}
			if (thing is ResourceClump resourceClump && Game1.currentLocation.resourceClumps.Contains(resourceClump)) 
			{
				SMonitor.Log($"thing is a ResourceClump");
				Game1.currentLocation.resourceClumps.Remove(resourceClump);
				SMonitor.Log($"removed {resourceClump.GetType()} from resourceClumps");
				return;
			}
			if (thing is LargeTerrainFeature largeTerrainFeature && Game1.currentLocation.largeTerrainFeatures.Contains(largeTerrainFeature)) 
			{
				SMonitor.Log($"thing is a LargeTerrainFeature");
				Game1.currentLocation.largeTerrainFeatures.Remove(largeTerrainFeature);
				SMonitor.Log($"removed {largeTerrainFeature.GetType()} from largeTerrainFeature");
				return;
			}
			if (thing is TerrainFeature terrainFeature && Game1.currentLocation.terrainFeatures.ContainsKey(cursorTile)) 
			{
				SMonitor.Log($"thing is a terrain feature");
				Game1.currentLocation.terrainFeatures.Remove(cursorTile);
				SMonitor.Log($"removed {terrainFeature.GetType()} from terrainFeatures");
				return;
			}
			if(thing is FarmAnimal farmAnimal && Game1.currentLocation.animals.ContainsKey(farmAnimal.myID.Value))
			{
				Game1.currentLocation.animals.Remove(farmAnimal.myID.Value);
				SMonitor.Log($"removed {farmAnimal.Name} from animals");
			}
			if (thing is NPC npc && Game1.currentLocation.characters.Contains(npc)) 
			{
				SMonitor.Log($"thing is an NPC");
				Game1.currentLocation.characters.Remove(npc);
				SMonitor.Log($"removed {npc.Name} from characters");
				return;
			}
			if (thing is Building building && Game1.currentLocation.buildings.Contains(building))
			{
				Game1.currentLocation.buildings.Remove(building);
				SMonitor.Log($"removed {building.GetData()?.Name} from buildings");
				return;
			}
		}
	}
}
