using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ModThis
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {

        private static ModConfig Config;
        public static Vector2 cursorLoc;
        private object thing;
        private string dialogPrefix = "ModThis_Wizard_Questions";

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.Enabled)
                return;

            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            //Monitor.Log($"last question key: {Game1.player?.currentLocation?.lastQuestionKey}");

            if (Context.CanPlayerMove && e.Button == Config.WizardKey)
                StartWizard();
            else if (Game1.activeClickableMenu != null && Game1.player?.currentLocation?.lastQuestionKey?.StartsWith(dialogPrefix) == true)
            {

                IClickableMenu menu = Game1.activeClickableMenu;
                if (menu == null || menu.GetType() != typeof(DialogueBox))
                    return;

                DialogueBox db = menu as DialogueBox;
                int resp = db.selectedResponse;
                List<Response> resps = db.responses;

                if (resp < 0 || resps == null || resp >= resps.Count || resps[resp] == null)
                    return;
                Monitor.Log($"Answered {Game1.player.currentLocation.lastQuestionKey} with {resps[resp].responseKey}");

                WizardDialogue(Game1.player.currentLocation.lastQuestionKey, resps[resp].responseKey);
                return;
            }
        }

        public void StartWizard()
        {

            cursorLoc = Game1.GetPlacementGrabTile();
            cursorLoc = new Vector2((int)cursorLoc.X, (int)cursorLoc.Y);
            thing = FindThingAtCursor(Game1.player.currentLocation, cursorLoc);
            
            Monitor.Log($"Starting ModThis wizard. thing: {thing}, cursorLoc: {cursorLoc}");

            if (thing == null)
                return;

            List<Response> responses = new List<Response>();


            if (thing is Object && ((thing as Object).type.Equals("Crafting") || (thing as Object).Type.Equals("interactive")))
                responses.Add(new Response("ModThis_Wizard_Questions_Pickup", Helper.Translation.Get("pickup")));

            responses.Add(new Response("ModThis_Wizard_Questions_Remove", Helper.Translation.Get("remove")));

            if(thing is Object || thing is TerrainFeature || thing is LargeTerrainFeature || thing is ResourceClump)
                responses.Add(new Response("ModThis_Wizard_Questions_Move", Helper.Translation.Get("move")));
            
            if(thing is Tree || thing is FruitTree || Game1.currentLocation.isCropAtTile((int)cursorLoc.X, (int)cursorLoc.Y) || thing is IndoorPot)
                responses.Add(new Response("ModThis_Wizard_Questions_Grow", Helper.Translation.Get("grow")));

            if (Game1.currentLocation.isCropAtTile((int)cursorLoc.X, (int)cursorLoc.Y) || thing is IndoorPot)
                responses.Add(new Response("ModThis_Wizard_Questions_Water", Helper.Translation.Get("water")));

            if (thing is Monster)
                responses.Add(new Response("ModThis_Wizard_Questions_Kill", Helper.Translation.Get("kill")));
            
            responses.Add(new Response("cancel", Helper.Translation.Get("cancel")));

            Game1.player.currentLocation.createQuestionDialogue(string.Format(Helper.Translation.Get("welcome"), GetThingName(thing)), responses.ToArray(), "ModThis_Wizard_Questions");
        }

        private object FindThingAtCursor(GameLocation location, Vector2 cursorLoc)
        {
            if (location.objects.ContainsKey(cursorLoc))
                return location.objects[cursorLoc];
            if (location.overlayObjects.ContainsKey(cursorLoc))
                return location.overlayObjects[cursorLoc];
            if (location.resourceClumps.FirstOrDefault(f => f.occupiesTile((int)cursorLoc.X, (int)cursorLoc.Y)) != null)
                return location.resourceClumps.FirstOrDefault(f => f.occupiesTile((int)cursorLoc.X, (int)cursorLoc.Y));
            if (location.terrainFeatures.ContainsKey(cursorLoc))
                return location.terrainFeatures[cursorLoc];
            if (location.largeTerrainFeatures.FirstOrDefault(f => f.getBoundingBox().Contains(new Point((int)cursorLoc.X * 64, (int)cursorLoc.Y * 64))) != null)
                return location.largeTerrainFeatures.FirstOrDefault(f => f.getBoundingBox().Contains(new Point((int)cursorLoc.X * 64, (int)cursorLoc.Y * 64)));
            foreach (NPC character in location.characters)
            {
                if (character.getTileLocation() == cursorLoc)
                    return character;
            }
            return null;
        }

        private string GetThingName(object thing)
        {
            if (thing is Object)
                return (thing as Object).name;
            if (thing is HoeDirt)
            {
                if((thing as HoeDirt).crop != null)
                {
                    return new Object((thing as HoeDirt).crop.indexOfHarvest, 1).Name;
                }
            }
            return thing.GetType().Name;
        }
        public void WizardDialogue(string whichQuestion, string whichAnswer)
        {
            Monitor.Log($"question: {whichQuestion}, answer: {whichAnswer}");
            if (whichAnswer == "cancel")
                return;

            List<Response> responses = new List<Response>();
            string header = "";
            string newQuestion = whichAnswer;
            switch (whichQuestion)
            {
                case "ModThis_Wizard_Questions":
                    switch (whichAnswer)
                    {
                        case "ModThis_Wizard_Questions_Pickup":
                            PickupThis();
                            return;
                        case "ModThis_Wizard_Questions_Remove":
                            header = Helper.Translation.Get("confirm-remove");
                            responses.Add(new Response($"ModThis_Wizard_Questions_Remove_Yes", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes")));
                            break;
                        case "ModThis_Wizard_Questions_Move":
                            header = Helper.Translation.Get("confirm-move");
                            responses.Add(new Response($"up", Helper.Translation.Get("up")));
                            responses.Add(new Response($"down", Helper.Translation.Get("down")));
                            responses.Add(new Response($"left", Helper.Translation.Get("left")));
                            responses.Add(new Response($"right", Helper.Translation.Get("right")));
                            break;
                        case "ModThis_Wizard_Questions_Grow":
                            GrowThis();
                            return;
                        case "ModThis_Wizard_Questions_Water":
                            WaterThis();
                            return;
                        case "ModThis_Wizard_Questions_Kill":
                            KillThis();
                            return;
                    }
                    break;
                case "ModThis_Wizard_Questions_Remove":
                    RemoveThis();
                    return;
                case "ModThis_Wizard_Questions_Move":
                    MoveThis(whichAnswer);
                    return;
                default:
                    return;
            }
            responses.Add(new Response("cancel", Helper.Translation.Get("cancel")));
            Game1.player.currentLocation.createQuestionDialogue($"{header}", responses.ToArray(), newQuestion);
        }

        private void PickupThis()
        {
            Monitor.Log($"Picking up");
            if (thing is Object && (Game1.currentLocation.objects.ContainsKey(cursorLoc) || Game1.currentLocation.objects.ContainsKey(cursorLoc)))
            {
                Object obj = thing as Object;
                Monitor.Log($"{obj.name} is an object");
                if (obj.heldObject.Value != null)
                {
                    Game1.player.currentLocation.debris.Add(new Debris(obj.heldObject, obj.tileLocation.Value * 64f + new Vector2(32f, 32f)));
                }
                Game1.player.currentLocation.debris.Add(new Debris(obj.bigCraftable ? (-obj.ParentSheetIndex) : obj.ParentSheetIndex, cursorLoc * 64f, obj.tileLocation.Value * 64f + new Vector2(32f, 32f)));
                if (Game1.currentLocation.objects.ContainsKey(cursorLoc))
                    Game1.currentLocation.objects.Remove(cursorLoc);
                else
                    Game1.currentLocation.overlayObjects.Remove(cursorLoc);
            }
        }

        private void RemoveThis()
        {
            Monitor.Log($"Removing");
            if (thing is Object) 
            {
                Monitor.Log($"{(thing as Object).name} is an object");
                if (Game1.currentLocation.objects.ContainsKey(cursorLoc))
                {
                    Game1.currentLocation.objects.Remove(cursorLoc);
                    Monitor.Log($"removed {(thing as Object).name} from objects");
                    return;
                }
                if (Game1.currentLocation.overlayObjects.ContainsKey(cursorLoc))
                {
                    Game1.currentLocation.overlayObjects.Remove(cursorLoc);
                    Monitor.Log($"removed {(thing as Object).name} from overlayObjects");
                    return;
                }
            }
            if (thing is ResourceClump) 
            {
                Monitor.Log($"thing is a ResourceClump");
                if (Game1.currentLocation.resourceClumps.Contains(thing))
                {
                    Game1.currentLocation.resourceClumps.Remove(thing as ResourceClump);
                    Monitor.Log($"removed {(thing as ResourceClump).GetType()} from resourceClumps");
                    return;
                }
            }
            if (thing is LargeTerrainFeature) 
            {
                Monitor.Log($"thing is a LargeTerrainFeature");
                if (Game1.currentLocation.largeTerrainFeatures.Contains(thing))
                {
                    Game1.currentLocation.largeTerrainFeatures.Remove(thing as LargeTerrainFeature);
                    Monitor.Log($"removed {(thing as LargeTerrainFeature).GetType()} from largeTerrainFeature");
                    return;
                }
            }
            if (thing is TerrainFeature) 
            {
                Monitor.Log($"thing is a terrain feature");
                if (Game1.currentLocation.terrainFeatures.ContainsKey(cursorLoc))
                {
                    Game1.currentLocation.terrainFeatures.Remove(cursorLoc);
                    Monitor.Log($"removed {(thing as TerrainFeature).GetType()} from terrainFeatures");
                    return;
                }
            }
            if (thing is NPC) 
            {
                Monitor.Log($"thing is an NPC");
                if (Game1.currentLocation.characters.Contains(thing))
                {
                    Game1.currentLocation.characters.Remove(thing as NPC);
                    Monitor.Log($"removed {(thing as NPC).Name} from characters");
                    return;
                }
            }
        }
        private void MoveThis(string dir)
        {
            Monitor.Log($"Moving");
            Vector2 shiftV = Vector2.Zero;
            switch (dir)
            {
                case "up":
                    shiftV = new Vector2(0, -1);
                    break;
                case "down":
                    shiftV = new Vector2(0, 1);
                    break;
                case "left":
                    shiftV = new Vector2(-1, 0);
                    break;
                case "right":
                    shiftV = new Vector2(1, 0);
                    break;
            }
            if (thing is Object)
            {
                Monitor.Log($"{(thing as Object).name} is an object");
                if (Game1.currentLocation.objects.ContainsKey(cursorLoc) && !Game1.currentLocation.objects.ContainsKey(cursorLoc + shiftV))
                {
                    Game1.currentLocation.objects[cursorLoc + shiftV] = Game1.currentLocation.objects[cursorLoc];
                    Game1.currentLocation.objects.Remove(cursorLoc);
                    Monitor.Log($"moved {(thing as Object).name} {dir}");
                    return;
                }
                if (Game1.currentLocation.overlayObjects.ContainsKey(cursorLoc) && !Game1.currentLocation.objects.ContainsKey(cursorLoc + shiftV))
                {
                    Game1.currentLocation.overlayObjects[cursorLoc + shiftV] = Game1.currentLocation.overlayObjects[cursorLoc];
                    Game1.currentLocation.overlayObjects.Remove(cursorLoc);
                    Monitor.Log($"moved {(thing as Object).name} {dir}");
                    return;
                }
            }
            if (thing is ResourceClump)
            {
                Monitor.Log($"thing is a ResourceClump");
                if (Game1.currentLocation.resourceClumps.Contains(thing))
                {
                    Game1.currentLocation.resourceClumps.Remove(thing as ResourceClump);
                    (thing as ResourceClump).currentTileLocation += shiftV;
                    (thing as ResourceClump).tile.Value += shiftV;
                    Game1.currentLocation.resourceClumps.Add(thing as ResourceClump);
                    Monitor.Log($"moved {(thing as ResourceClump).GetType()} {dir}");
                    return;
                }
            }
            if (thing is LargeTerrainFeature)
            {
                Monitor.Log($"thing is a LargeTerrainFeature");
                if (Game1.currentLocation.largeTerrainFeatures.Contains(thing))
                {
                    Game1.currentLocation.largeTerrainFeatures.Remove(thing as LargeTerrainFeature);
                    Monitor.Log($"old loc: {(thing as LargeTerrainFeature).currentTileLocation} old pos: {(thing as LargeTerrainFeature).tilePosition}");
                    (thing as LargeTerrainFeature).currentTileLocation += shiftV;
                    (thing as LargeTerrainFeature).tilePosition.Value += shiftV;
                    Monitor.Log($"new loc: {(thing as LargeTerrainFeature).currentTileLocation} new pos: {(thing as LargeTerrainFeature).tilePosition}");
                    Game1.currentLocation.largeTerrainFeatures.Add(thing as LargeTerrainFeature);

                    Monitor.Log($"moved {(thing as LargeTerrainFeature).GetType()} {dir}");
                    return;
                }
            }
            if (thing is TerrainFeature)
            {
                Monitor.Log($"thing is a terrain feature");
                if (Game1.currentLocation.terrainFeatures.ContainsKey(cursorLoc) && !Game1.currentLocation.terrainFeatures.ContainsKey(cursorLoc + shiftV))
                {
                    Game1.currentLocation.terrainFeatures[cursorLoc + shiftV] = Game1.currentLocation.terrainFeatures[cursorLoc];
                    Game1.currentLocation.terrainFeatures.Remove(cursorLoc);
                    Monitor.Log($"moved {(thing as TerrainFeature).GetType()} {dir}");
                    return;
                }
            }
        }
        private void GrowThis()
        {
            Monitor.Log($"Growing");
            if (Game1.currentLocation.isCropAtTile((int)cursorLoc.X, (int)cursorLoc.Y) && !(Game1.currentLocation.terrainFeatures[new Vector2(cursorLoc.X, cursorLoc.Y)] as HoeDirt).crop.fullyGrown)
            {
                (Game1.currentLocation.terrainFeatures[new Vector2((int)cursorLoc.X, (int)cursorLoc.Y)] as HoeDirt).crop.growCompletely();
                Monitor.Log($"Grew crop");
                return;
            }

            Microsoft.Xna.Framework.Rectangle tileRect = new Microsoft.Xna.Framework.Rectangle((int)cursorLoc.X * 64, (int)cursorLoc.Y * 64, 64, 64);

            foreach (NPC i in Game1.currentLocation.characters)
            {
                if (i != null && i is Child && i.GetBoundingBox().Intersects(tileRect) && (i.Age < 3))
                {
                    i.Age = 3;
                    Monitor.Log($"Grew child");
                    return;
                }
            }

            try
            {
                NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>> dict = Helper.Reflection.GetField<NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>>>(Game1.currentLocation, "animals").GetValue();
                foreach (KeyValuePair<long, FarmAnimal> i in dict.Pairs)
                {
                    if (i.Value.age < i.Value.ageWhenMature)
                    {
                        i.Value.age.Value = i.Value.ageWhenMature;
                        i.Value.Sprite.LoadTexture("Animals\\" + i.Value.type.Value);
                        if (i.Value.type.Value.Contains("Sheep"))
                        {
                            i.Value.currentProduce.Value = i.Value.defaultProduceIndex;
                        }
                        i.Value.daysSinceLastLay.Value = 99;
                        Monitor.Log($"Grew animal");
                        return;
                    }
                }
            }
            catch { }


            foreach (KeyValuePair<Vector2, TerrainFeature> v in Game1.currentLocation.terrainFeatures.Pairs)
            {
                if (v.Value.getBoundingBox(v.Key).Intersects(tileRect) && v.Value is Tree && (v.Value as Tree).growthStage < 5)
                {
                    (Game1.currentLocation.terrainFeatures[v.Key] as Tree).growthStage.Value = 5;
                    (Game1.currentLocation.terrainFeatures[v.Key] as Tree).fertilized.Value = true;
                    (Game1.currentLocation.terrainFeatures[v.Key] as Tree).dayUpdate(Game1.currentLocation, v.Key);
                    (Game1.currentLocation.terrainFeatures[v.Key] as Tree).fertilized.Value = false;

                    Monitor.Log($"Grew tree");
                    return;
                }
                if (v.Value.getBoundingBox(v.Key).Intersects(tileRect) && v.Value is FruitTree && (v.Value as FruitTree).growthStage < 4)
                {
                    FruitTree tree = v.Value as FruitTree;
                    tree.daysUntilMature.Value = 0;
                    tree.growthStage.Value = 4;
                    Game1.currentLocation.terrainFeatures[v.Key] = tree;

                    Monitor.Log($"Grew fruit tree");
                    return;
                }
            }

            foreach (KeyValuePair<Vector2, Object> v in Game1.currentLocation.objects.Pairs)
            {
                if (v.Value.getBoundingBox(v.Key).Intersects(tileRect) && v.Value is IndoorPot && !(v.Value as IndoorPot).hoeDirt.Value.crop.fullyGrown)
                {
                    (v.Value as IndoorPot).hoeDirt.Value.crop.growCompletely();

                    Monitor.Log($"Grew pot crop");
                    return;
                }
            }
        }
        private void WaterThis()
        {
            Monitor.Log($"Watering");
            if (Game1.currentLocation.terrainFeatures[new Vector2(cursorLoc.X, cursorLoc.Y)] is HoeDirt && (Game1.currentLocation.terrainFeatures[new Vector2(cursorLoc.X, cursorLoc.Y)] as HoeDirt).state != 1)
            {
                (Game1.currentLocation.terrainFeatures[new Vector2((int)cursorLoc.X, (int)cursorLoc.Y)] as HoeDirt).state.Value = 1;
                Monitor.Log($"Watered crop");
                return;
            }

            Microsoft.Xna.Framework.Rectangle tileRect = new Microsoft.Xna.Framework.Rectangle((int)cursorLoc.X * 64, (int)cursorLoc.Y * 64, 64, 64);


            foreach (KeyValuePair<Vector2, Object> v in Game1.currentLocation.objects.Pairs)
            {
                if (v.Value.getBoundingBox(v.Key).Intersects(tileRect) && v.Value is IndoorPot && (v.Value as IndoorPot).hoeDirt.Value.state.Value != 1)
                {
                    (v.Value as IndoorPot).hoeDirt.Value.state.Value = 1;

                    Monitor.Log($"Watered pot crop");
                    return;
                }
            }
        }

        private void KillThis()
        {
            Monitor.Log($"Killing");
            foreach (NPC character in Game1.currentLocation.characters)
            {
                if (character.getTileLocation() == cursorLoc)
                {
                    (character as Monster).Health = -1;
                    Monitor.Log($"killed monster");
                    return;
                }
            }
        }

    }
}