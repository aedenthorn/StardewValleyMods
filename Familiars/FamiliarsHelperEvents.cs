using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Monsters;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Object = StardewValley.Object;

namespace Familiars
{
    public class FamiliarsHelperEvents
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

        public static void Player_Warped(object sender, WarpedEventArgs e)
        {
            if (e.OldLocation.characters == null)
                return;

            e.OldLocation.characters.OnValueRemoved -= Characters_OnValueRemoved;
            e.NewLocation.characters.OnValueRemoved += Characters_OnValueRemoved;

            for (int i = e.OldLocation.characters.Count - 1; i >= 0; i--)
            {
                NPC npc = e.OldLocation.characters[i];
                if (npc is Familiar)
                {
                    Farmer owner = Game1.getFarmer(Helper.Reflection.GetField<long>(npc, "ownerId").GetValue());
                    if (owner == Game1.player && (npc as Familiar).followingOwner)
                    {
                        FamiliarsUtils.warpFamiliar(npc, e.NewLocation, Game1.player.getTileLocation());
                    }
                }
            }
        }

        private static void Characters_OnValueRemoved(NPC value)
        {
        }

        public static void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // load json assets

            ModEntry.JsonAssets = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            bool flag = ModEntry.JsonAssets == null;
            if (flag)
            {
                Monitor.Log("Can't load Json Assets API for Familiars mod");
            }
            else
            {
                ModEntry.JsonAssets.LoadAssets(Path.Combine(Helper.DirectoryPath, "assets/json-assets"));
            }

        }
        public static void GameLoop_Saving(object sender, SavingEventArgs e)
        {
            FamiliarSaveData fsd = new FamiliarSaveData();

            foreach(GameLocation l in Game1.locations)
            {
                for(int i = l.characters.Count - 1; i >= 0; i--)
                {
                    if(l.characters[i] is Familiar)
                    {
                        if(l.characters[i] is DustSpriteFamiliar)
                        {
                            fsd.dustSpriteFamiliars.Add((l.characters[i] as Familiar).SaveData(l));
                        }
                        else if(l.characters[i] is DinoFamiliar)
                        {
                            fsd.dinoFamiliars.Add((l.characters[i] as Familiar).SaveData(l));
                        }
                        else if(l.characters[i] is BatFamiliar)
                        {
                            fsd.batFamiliars.Add((l.characters[i] as Familiar).SaveData(l));
                        }
                        else if(l.characters[i] is JunimoFamiliar)
                        {
                            fsd.junimoFamiliars.Add((l.characters[i] as Familiar).SaveData(l));
                        }
                        else if(l.characters[i] is ButterflyFamiliar)
                        {
                            fsd.butterflyFamiliars.Add((l.characters[i] as Familiar).SaveData(l));
                        }
                        Monitor.Log($"removing {l.characters[i].GetType()} from {l.Name}");
                        l.characters.RemoveAt(i);
                    }
                }
            }

            foreach (Building b in Game1.getFarm().buildings)
            {
                if (b.indoors == null || b.indoors.Value == null || b.indoors.Value.characters == null || b.indoors.Value.Name != "Slime Hutch")
                    continue;
                for (int i = b.indoors.Value.characters.Count - 1; i >= 0; i--)
                {
                    NPC npc = b.indoors.Value.characters[i];
                    if (npc is Familiar)
                    {
                        
                        if (npc is DustSpriteFamiliar)
                        {
                            fsd.dustSpriteFamiliars.Add((npc as Familiar).SaveData(b.indoors.Value));
                        }
                        else if (npc is DinoFamiliar)
                        {
                            fsd.dinoFamiliars.Add((npc as Familiar).SaveData(b.indoors.Value));
                        }
                        else if (npc is BatFamiliar)
                        {
                            fsd.batFamiliars.Add((npc as Familiar).SaveData(b.indoors.Value));
                        }
                        else if (npc is JunimoFamiliar)
                        {
                            fsd.junimoFamiliars.Add((npc as Familiar).SaveData(b.indoors.Value));
                        }
                        else if (npc is ButterflyFamiliar)
                        {
                            fsd.butterflyFamiliars.Add((npc as Familiar).SaveData(b.indoors.Value));
                        }
                        Monitor.Log($"removing {npc.GetType()} from {b.indoors.Value.Name}");
                        b.indoors.Value.characters.RemoveAt(i);
                    }
                }
            }

            Helper.Data.WriteSaveData("familiars", fsd);
        }

        public static void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            //load familiars
            //FamiliarsUtils.LoadFamiliars();

            // load egg ids

            if (ModEntry.JsonAssets != null)
            {
                ModEntry.BatFamiliarEgg = ModEntry.JsonAssets.GetObjectId("Bat Familiar Egg");
                ModEntry.DustFamiliarEgg = ModEntry.JsonAssets.GetObjectId("Dust Sprite Familiar Egg");
                ModEntry.DinoFamiliarEgg = ModEntry.JsonAssets.GetObjectId("Dino Familiar Egg");
                ModEntry.JunimoFamiliarEgg = ModEntry.JsonAssets.GetObjectId("Junimo Familiar Egg");
                ModEntry.ButterflyFamiliarEgg = ModEntry.JsonAssets.GetObjectId("Butterfly Familiar Egg");
                ModEntry.ButterflyDust = ModEntry.JsonAssets.GetObjectId("Butterfly Dust");

                if (ModEntry.BatFamiliarEgg == -1)
                {
                    Monitor.Log("Can't get ID for Familiars mod item #1. Some functionality will be lost.");
                }
                else
                {
                    Monitor.Log(string.Format("Familiars mod item #1 ID is {0}.", ModEntry.BatFamiliarEgg));
                }
                if (ModEntry.DustFamiliarEgg == -1)
                {
                    Monitor.Log("Can't get ID for Familiars mod item #2. Some functionality will be lost.");
                }
                else
                {
                    Monitor.Log(string.Format("Familiars mod item #2 ID is {0}.", ModEntry.DustFamiliarEgg));
                }
                if (ModEntry.DinoFamiliarEgg == -1)
                {
                    Monitor.Log("Can't get ID for Familiars mod item #3. Some functionality will be lost.");
                }
                else
                {
                    Monitor.Log(string.Format("Familiars mod item #3 ID is {0}.", ModEntry.DinoFamiliarEgg));
                }
                if (ModEntry.JunimoFamiliarEgg == -1)
                {
                    Monitor.Log("Can't get ID for Familiars mod item #4. Some functionality will be lost.");
                }
                else
                {
                    Monitor.Log(string.Format("Familiars mod item #4 ID is {0}.", ModEntry.JunimoFamiliarEgg));
                }
                if (ModEntry.ButterflyFamiliarEgg == -1)
                {
                    Monitor.Log("Can't get ID for Familiars mod item #5. Some functionality will be lost.");
                }
                else
                {
                    Monitor.Log(string.Format("Familiars mod item #5 ID is {0}.", ModEntry.ButterflyFamiliarEgg));
                }
                if (ModEntry.ButterflyDust == -1)
                {
                    Monitor.Log("Can't get ID for Familiars mod item #6. Some functionality will be lost.");
                }
                else
                {
                    Monitor.Log(string.Format("Familiars mod item #6 ID is {0}.", ModEntry.ButterflyDust));
                }
            }

            // fix bug
            if (Game1.IsMasterGame && Config.TryToFixOldBugs)
            {
                Monitor.Log("Fixing bugs");
                foreach (Building l in Game1.getFarm().buildings)
                {
                    if (l is Coop)
                    {
                        foreach (Object o in (l as Coop).indoors.Value.Objects.Values)
                        {
                            if (o.bigCraftable && o.Name.Contains("Incubator") && o.heldObject.Value != null)
                            {
                                int egg = o.heldObject.Value.ParentSheetIndex;
                                Monitor.Log($"egg id {egg}");
                                if (new int[] { ModEntry.BatFamiliarEgg, ModEntry.ButterflyFamiliarEgg, ModEntry.DinoFamiliarEgg, ModEntry.DustFamiliarEgg, ModEntry.JunimoFamiliarEgg }.Contains(egg))
                                {
                                    Monitor.Log($"familiar egg, removing.", LogLevel.Warn);
                                    o.heldObject.Value = null;
                                    o.minutesUntilReady.Value = -1;
                                    o.ParentSheetIndex = 101;
                                    Game1.player.addItemToInventory(new Object(egg, 1));
                                }
                            }
                        }
                    }
                }

                foreach (GameLocation l in Game1.locations)
                {
                    for (int i = l.Objects.Count() - 1; i >= 0; i--)
                    {
                        Object o = l.Objects.Values.ElementAt(i);
                        if (o.Name.Equals("Butterfly Dust"))
                        {
                            Monitor.Log($"Removing placed butterfly dust.", LogLevel.Warn);
                            Game1.currentLocation.debris.Add(new Debris(o.ParentSheetIndex, l.Objects.Keys.ElementAt(i), Game1.player.position));
                            l.objects.Remove(l.Objects.Keys.ElementAt(i));
                        }
                    }
                }
            }
        }

        public static void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            FamiliarsUtils.LoadFamiliars();
            ModEntry.receivedJunimoEggToday = false;
        }

        public static void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            Rectangle box = new Rectangle((int)Game1.lastCursorTile.X * Game1.tileSize, (int)Game1.lastCursorTile.Y * Game1.tileSize, Game1.tileSize, Game1.tileSize);

            if (Context.IsPlayerFree && (e.Button == SButton.MouseLeft || e.Button == SButton.MouseRight) && Game1.player.currentLocation != null)
            {
                foreach (Familiar f in Game1.player.currentLocation.characters.Where(n => n is Familiar))
                {
                    if ((f.ownerId == Game1.player.UniqueMultiplayerID || f.ownerId == 0 || !f.followingOwner) && f.GetBoundingBox().Intersects(box))
                    {
                        if (!f.followingOwner)
                            f.ownerId = Game1.player.UniqueMultiplayerID;

                        if (Game1.player.currentLocation is SlimeHutch)
                            f.followingOwner = !f.followingOwner;
                        else if (!f.followingOwner)
                            f.followingOwner = true;

                        if (!f.followingOwner && f.currentLocation.getTileIndexAt(f.getTileLocationPoint(), "Back") == -1)
                            f.followingOwner = true;

                        Game1.player.currentLocation.playSound("dwop");
                        Monitor.Log($"familiar following player: {f.followingOwner}");
                        return;
                    }
                }

                List<Critter> critters = Helper.Reflection.GetField<List<Critter>>(Game1.player.currentLocation, "critters").GetValue();
                if (critters == null)
                    return;
                foreach(Critter c in critters)
                {
                    
                    if (c is Butterfly && Helper.Reflection.GetField<int>(c, "flapSpeed").GetValue() <= 80 && Vector2.Distance(new Vector2(box.Center.X, box.Center.Y), new Vector2(c.position.X - 32, c.position.Y - 96)) < 32)
                    {
                        Monitor.Log($"flap speed {Helper.Reflection.GetField<int>(c, "flapSpeed").GetValue()} distance: {Vector2.Distance(new Vector2(box.Center.X, box.Center.Y), new Vector2(c.position.X - 32, c.position.Y - 96))}");
                        if (!Game1.player.addItemToInventoryBool(new Object(ModEntry.ButterflyDust, 1), false))
                        {
                            Game1.createItemDebris(new Object(ModEntry.ButterflyDust, 1), Game1.player.getStandingPosition(), 1, null, -1);
                        }
                        Game1.playSound("daggerswipe");
                        Helper.Reflection.GetField<int>(c, "flapSpeed").SetValue(81);
                        break;
                    }
                }
            }
        }

    }
}
