using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
                    if (owner == Game1.player)
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
                            fsd.dustSpriteFamiliars.Add((l.characters[i] as Familiar).SaveData());
                        }
                        else if(l.characters[i] is DinoFamiliar)
                        {
                            fsd.dinoFamiliars.Add((l.characters[i] as Familiar).SaveData());
                        }
                        else if(l.characters[i] is BatFamiliar)
                        {
                            fsd.batFamiliars.Add((l.characters[i] as Familiar).SaveData());
                        }
                        else if(l.characters[i] is JunimoFamiliar)
                        {
                            fsd.junimoFamiliars.Add((l.characters[i] as Familiar).SaveData());
                        }
                        else if(l.characters[i] is ButterflyFamiliar)
                        {
                            fsd.butterflyFamiliars.Add((l.characters[i] as Familiar).SaveData());
                        }
                        l.characters.RemoveAt(i);
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
                ModEntry.ButterflyFamiliarEgg = ModEntry.JsonAssets.GetObjectId("Butterly Familiar Egg");

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
                    Monitor.Log(string.Format("Familiars mod item #4 ID is {0}.", ModEntry.ButterflyFamiliarEgg));
                }
            }
        }

        public static void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            FamiliarsUtils.LoadFamiliars();
            ModEntry.receivedJunimoEggToday = false;
        }
    }
}
