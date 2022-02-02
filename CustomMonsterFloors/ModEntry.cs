using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CustomMonsterFloors
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        public static ModConfig Config;
        private static ModEntry context;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<ModConfig>();
            context = this;
            if (!Config.EnableMod)
                return;

            helper.Events.GameLoop.DayStarted += OnDayStarted;

            var harmony = new Harmony(this.ModManifest.UniqueID);

            if (Config.EnableFloorTypeChanges)
            {
                harmony.Patch(
                   original: AccessTools.Method(typeof(MineShaft), "loadLevel"),
                   postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.loadLevel_Postfix))
                );
            }
            if (Config.EnableTileChanges)
            {
                harmony.Patch(
                   original: AccessTools.Method(typeof(MineShaft), "adjustLevelChances"),
                   postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.adjustLevelChances_Postfix))
                );
                harmony.Patch(
                   original: AccessTools.Method(typeof(MineShaft), "chooseStoneType"),
                   prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.chooseStoneType_Prefix)),
                   postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.chooseStoneType_Postfix))
                );
                harmony.Patch(
                   original: AccessTools.Method(typeof(MineShaft), "checkStoneForItems"),
                   postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.checkStoneForItems_Postfix))
                );
                harmony.Patch(
                   original: AccessTools.Method(typeof(MineShaft), "tryToAddAreaUniques"),
                   transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.MineShaft_tryToAddAreaUniques_Transpiler))
                );
                harmony.Patch(
                   original: AccessTools.Method(typeof(MineShaft), "populateLevel"),
                   transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.MineShaft_populateLevel_Transpiler))
                );
            }
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            monsterFloors.Clear();
            treasureFloors.Clear();
            GotShaft = false;
        }
        public static List<int> monsterFloors = new List<int>();
        public static List<int> treasureFloors = new List<int>();

        public static void loadLevel_Postfix(ref MineShaft __instance, int level, ref NetBool ___netIsTreasureRoom, ref NetBool ___netIsMonsterArea, ref NetBool ___netIsSlimeArea, ref NetBool ___netIsDinoArea, NetBool ___netIsQuarryArea, ref NetString ___mapImageSource, bool ___loadedDarkArea, Random ___mineRandom, ref NetString ___mapPath)
        {
            GotShaft = false;

            if (__instance.getMineArea(level) == 77377 || level == 220)
            {
                return;
            }
            if (___netIsTreasureRoom) // not sure about these
            {
                treasureFloors.Add(level);
                return;
            }
            context.Monitor.Log($"Loaded level postfix {__instance.getMineArea(level)} {level}");

            if (__instance.getMineArea(level) == 121)
            {
                double treasureChance = 0.01;
                treasureChance += Game1.player.team.AverageDailyLuck(Game1.currentLocation) / 10.0 + Game1.player.team.AverageLuckLevel(Game1.currentLocation) / 100.0;
                context.Monitor.Log($"checking for treasure level chance: {treasureChance * Config.TreasureChestFloorMultiplier - treasureChance}");
                if (Game1.random.NextDouble() < treasureChance * Config.TreasureChestFloorMultiplier - treasureChance)
                {
                    context.Monitor.Log("Creating treasure floor");
                    treasureFloors.Add(level);
                    ___netIsTreasureRoom.Value = true;
                    __instance.loadedMapNumber = 10;
                    if (___netIsSlimeArea || ___netIsDinoArea)
                    {
                        RevertMapImageSource(ref ___mapImageSource, ref ___loadedDarkArea, level, __instance.getMineArea(-1), __instance.getMineArea(level), __instance.loadedMapNumber);
                    }
                    return;
                }
            }

            bool IsMonsterFloor = Game1.random.Next(0, 100) < Config.PercentChanceMonsterFloor;

            if (IsMonsterFloor)
            {

                if (IsBelowMinFloorsApart(level))
                {
                    if (___netIsSlimeArea || ___netIsDinoArea)
                    {
                        RevertMapImageSource(ref ___mapImageSource, ref ___loadedDarkArea, level, __instance.getMineArea(-1), __instance.getMineArea(level), __instance.loadedMapNumber);
                    }
                    ___netIsDinoArea.Value = false;
                    ___netIsSlimeArea.Value = false;
                    ___netIsMonsterArea.Value = false;
                    return;
                }

                monsterFloors.Add(level);

                if (___netIsQuarryArea)
                {
                    ___netIsMonsterArea.Value = true;
                }
                else
                {
                    int roll = Game1.random.Next(0, 100);
                    ___netIsMonsterArea.Value = true;
                    if (__instance.getMineArea(-1) == 121)
                    {
                        string[] chances = Config.SlimeDinoMonsterSplitPercents.Split(':');
                        int slimeChance = int.Parse(chances[0]);
                        int dinoChance = int.Parse(chances[1]);
                        if (roll < slimeChance)
                        {
                            ___netIsDinoArea.Value = false;
                            ___netIsSlimeArea.Value = true;
                            ___netIsMonsterArea.Value = false;

                        }
                        else if (roll < dinoChance + slimeChance)
                        {
                            ___netIsDinoArea.Value = true;
                            ___netIsSlimeArea.Value = false;
                            ___netIsMonsterArea.Value = false;
                            ___mapImageSource.Value = "Maps\\Mines\\mine_dino";
                        }
                        else if (___netIsSlimeArea || ___netIsDinoArea)
                        {
                            RevertMapImageSource(ref ___mapImageSource, ref ___loadedDarkArea, level, __instance.getMineArea(-1), __instance.getMineArea(level), __instance.loadedMapNumber);
                        }
                    }
                    else
                    {
                        string[] chances = Config.SlimeMonsterSplitPercents.Split(':');
                        if (roll < int.Parse(chances[0]))
                        {
                            ___netIsDinoArea.Value = false;
                            ___netIsSlimeArea.Value = true;
                            ___netIsMonsterArea.Value = false;
                            ___mapImageSource.Value = "Maps\\Mines\\mine_slime";
                        }
                    }
                }
            }
            else
            {
                ___netIsMonsterArea.Value = false;
                ___netIsDinoArea.Value = false;
                ___netIsSlimeArea.Value = false;
            }
        }

        private static bool IsBelowMinFloorsApart(int level)
        {
            if (Config.MinFloorsBetweenMonsterFloors <= 0)
                return false;

            foreach(int i in monsterFloors)
            {
                if(Math.Abs(level-i) <= Config.MinFloorsBetweenMonsterFloors){
                    return true;
                }
            }
            return false;
        }

        private static void RevertMapImageSource(ref NetString mapImageSource, ref bool loadedDarkArea, int level, int mineAreaNeg, int mineAreaLevel, int mapNumberToLoad)
        {

            if (mineAreaNeg == 0 || mineAreaNeg == 10 || (mineAreaLevel != 0 && mineAreaLevel != 10))
            {
                if (mineAreaLevel == 40)
                {
                    mapImageSource.Value = "Maps\\Mines\\mine_frost";
                    if (level >= 70)
                    {
                        NetString netString = mapImageSource;
                        netString.Value += "_dark";
                        loadedDarkArea = true;
                    }
                }
                else if (mineAreaLevel == 80)
                {
                    mapImageSource.Value = "Maps\\Mines\\mine_lava";
                    if (level >= 110 && level != 120)
                    {
                        NetString netString2 = mapImageSource;
                        netString2.Value += "_dark";
                        loadedDarkArea = true;
                    }
                }
                else if (mineAreaLevel == 121)
                {
                    mapImageSource.Value = "Maps\\Mines\\mine_desert";
                    if (mapNumberToLoad % 40 >= 30)
                    {
                        NetString netString3 = mapImageSource;
                        netString3.Value += "_dark";
                        loadedDarkArea = true;
                    }
                }
                else
                {
                    mapImageSource.Value = "Maps\\Mines\\mine";
                }
            }
            else
            {
                mapImageSource.Value = "Maps\\Mines\\mine";
            }
        }
        private static void adjustLevelChances_Postfix(NetBool ___netIsMonsterArea, NetBool ___netIsSlimeArea, NetBool ___netIsDinoArea, ref double stoneChance, ref double monsterChance, ref double itemChance, ref double gemStoneChance)
        {
            if (___netIsDinoArea)
            {
                monsterChance *= Config.MonsterMultiplierOnDinoFloors;
                itemChance *= Config.ItemMultiplierOnDinoFloors;
            }
            else if (___netIsSlimeArea)
            {
                monsterChance *= Config.MonsterMultiplierOnSlimeFloors;
                itemChance *= Config.ItemMultiplierOnSlimeFloors;
            }
            else if (___netIsMonsterArea)
            {
                monsterChance *= Config.MonsterMultiplierOnMonsterFloors;
                itemChance *= Config.ItemMultiplierOnMonsterFloors;
            }
            else
            {
                monsterChance *= Config.MonsterMultiplierOnRegularFloors;
                itemChance *= Config.ItemMultiplierOnRegularFloors;
                stoneChance *= Config.StoneMultiplierOnRegularFloors;
                gemStoneChance *= Config.GemstoneMultiplierOnRegularFloors;
            }
        }
        private static void chooseStoneType_Prefix(ref double chanceForPurpleStone, ref double chanceForMysticStone)
        {
            chanceForPurpleStone *= Config.PurpleStoneMultiplier;
            chanceForMysticStone *= Config.MysticStoneMultiplier;
        }
        private static void chooseStoneType_Postfix(MineShaft __instance, ref StardewValley.Object __result, Vector2 tile)
        {
            if(__result == null)
            {
                return;
            }
            List<int> ores = new List<int>() { 765, 764, 290, 751 };
            var x = __result.ParentSheetIndex;
            if (!(x >= 31 && x <= 42 || x >= 47 && x <= 54 || ores.Contains(x)))
            {
                return;
            }

            if (__instance.getMineArea(-1) == 0 || __instance.getMineArea(-1) == 10)
            {
                double chanceForOre = 0.029 * Config.ChanceForOresMultiplierInMines - 0.029;

                if (__instance.mineLevel != 1 && __instance.mineLevel % 5 != 0 && Game1.random.NextDouble() < chanceForOre)
                {
                    __result = new StardewValley.Object(tile, 751, "Stone", true, false, false, false)
                    {
                        MinutesUntilReady = 3
                    };
                }
            }
            else if (__instance.getMineArea(-1) == 40)
            {
                double chanceForOre = 0.029 * Config.ChanceForOresMultiplierInMines - 0.029;
                if (__instance.mineLevel % 5 != 0 && Game1.random.NextDouble() < chanceForOre)
                {
                    __result = new StardewValley.Object(tile, 290, "Stone", true, false, false, false)
                    {
                        MinutesUntilReady = 4
                    };
                }
            }
            else if (__instance.getMineArea(-1) == 80)
            {
                double chanceForOre = 0.029 * Config.ChanceForOresMultiplierInMines - 0.029;
                if (__instance.mineLevel % 5 != 0 && Game1.random.NextDouble() < chanceForOre)
                {
                    __result = new StardewValley.Object(tile, 764, "Stone", true, false, false, false)
                    {
                        MinutesUntilReady = 8
                    };
                }
            }
            else if (__instance.getMineArea(-1) == 77377)
            {
                return;
            }
            else
            {
                int skullCavernMineLevel = __instance.mineLevel - 120;
                double chanceForOre = 0.02 + (double)skullCavernMineLevel * 0.0005;
                if (__instance.mineLevel >= 130)
                {
                    chanceForOre += 0.01 * (double)((float)(Math.Min(100, skullCavernMineLevel) - 10) / 10f);
                }
                double iridiumBoost = 0.0;
                if (__instance.mineLevel >= 130)
                {
                    iridiumBoost += 0.001 * (double)((float)(skullCavernMineLevel - 10) / 10f);
                }
                iridiumBoost = Math.Min(iridiumBoost, 0.004);
                if (skullCavernMineLevel > 100)
                {
                    iridiumBoost += (double)skullCavernMineLevel / 1000000.0;
                }

                chanceForOre = chanceForOre * Config.ChanceForOreMultiplier - chanceForOre;

                if (ores.Contains(x) || Game1.random.NextDouble() < chanceForOre) // if already an ore, don't check again
                {
                    double chanceForIridium = (double)Math.Min(100, skullCavernMineLevel) * (0.0003 + iridiumBoost);
                    double chanceForGold = 0.01 + (double)(__instance.mineLevel - Math.Min(150, skullCavernMineLevel)) * 0.0005;
                    double chanceForIron = Math.Min(0.5, 0.1 + (double)(__instance.mineLevel - Math.Min(200, skullCavernMineLevel)) * 0.005);

                    chanceForIridium *= Config.ChanceForIridiumMultiplier;
                    chanceForGold *= Config.ChanceForGoldMultiplier;
                    chanceForIron *= Config.ChanceForIronMultiplier;

                    if (Game1.random.NextDouble() < chanceForIridium)
                    {
                        __result = new StardewValley.Object(tile, 765, "Stone", true, false, false, false)
                        {
                            MinutesUntilReady = 16
                        };
                    }
                    else if (Game1.random.NextDouble() < chanceForGold)
                    {
                        __result = new StardewValley.Object(tile, 764, "Stone", true, false, false, false)
                        {
                            MinutesUntilReady = 8
                        };
                    }
                    else if (Game1.random.NextDouble() < chanceForIron)
                    {
                        __result = new StardewValley.Object(tile, 290, "Stone", true, false, false, false)
                        {
                            MinutesUntilReady = 4
                        };
                    }
                    else
                    {
                        __result = new StardewValley.Object(tile, 751, "Stone", true, false, false, false)
                        {
                            MinutesUntilReady = 2
                        };
                    }
                }
            }

            /*
            if (!ores.Contains(__result.ParentSheetIndex))
            {
                foreach(CustomOreNode node in CustomOreNodeData)
                {

                }
            }
            */
        }


        private static bool GotShaft = false;

        private static void checkStoneForItems_Postfix(MineShaft __instance, int x, int y, Farmer who, ref NetPointDictionary<bool, NetBool> ___createLadderDownEvent, bool ___ladderHasSpawned, NetIntDelta ___netStonesLeftOnThisLevel)
        {
            if(!___createLadderDownEvent.ContainsKey(new Point(x, y)))
            {
                double chanceForLadderDown = 0.02 + 1.0 / (double)Math.Max(1, ___netStonesLeftOnThisLevel) + (double)who.LuckLevel / 100.0 + Game1.player.DailyLuck / 5.0;
                if (__instance.EnemyCount == 0)
                {
                    chanceForLadderDown += 0.04;
                }

                chanceForLadderDown = chanceForLadderDown * Config.ChanceForLadderInStoneMultiplier - chanceForLadderDown;

                if (!__instance.mustKillAllMonstersToAdvance() && (___netStonesLeftOnThisLevel == 0 || Game1.random.NextDouble() < chanceForLadderDown) && __instance.shouldCreateLadderOnThisLevel())
                {
                    bool isShaft = !GotShaft && __instance.getMineArea(-1) == 121 && !__instance.mustKillAllMonstersToAdvance() && Game1.random.NextDouble() < 0.2 * Config.ChanceLadderIsShaftMultiplier;
                    if(isShaft || !___ladderHasSpawned)
                    {
                        if (isShaft)
                            GotShaft = true;
                        ___createLadderDownEvent[new Point(x, y)] = isShaft;

                    }
                }
            }
            else if(___createLadderDownEvent[new Point(x, y)])
            {
                GotShaft = true;
            }
        }
        private static IEnumerable<CodeInstruction> MineShaft_populateLevel_Transpiler(IEnumerable<CodeInstruction> instructions)
        {

            var codes = new List<CodeInstruction>(instructions);
            bool start = false;
            for (int i = 0; i < codes.Count; i++)
            {
                if (start && codes[i].opcode == OpCodes.Ldc_R8 && (double)codes[i].operand == 0.005)
                {
                    context.Monitor.Log("got 0.005 opcode");
                    codes[i].operand = Config.ResourceClumpChance; 
                    break;
                }
                else if (!start && codes[i].opcode == OpCodes.Call && (codes[i].operand as MethodInfo)?.Name == nameof(MineShaft.getRandomItemForThisLevel))
                {
                    context.Monitor.Log($"got call: {codes[i].operand}");
                    start = true;
                }
            }

            return codes.AsEnumerable();
        }
        private static IEnumerable<CodeInstruction> MineShaft_tryToAddAreaUniques_Transpiler(IEnumerable<CodeInstruction> instructions)
        {

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_R8 && (double)codes[i].operand == 0.1)
                {
                    context.Monitor.Log("got Ldc_R8 opcode try");
                    codes[i].operand = Config.WeedsChance;
                }
                else if (codes[i].opcode == OpCodes.Ldc_I4_7)
                {
                    context.Monitor.Log("got Ldc_I4_7 opcode try");
                    codes[i] = new CodeInstruction(OpCodes.Ldc_I4, (int)Math.Round(7 * Config.WeedsMultiplier));
                    codes[i + 1] = new CodeInstruction(OpCodes.Ldc_I4, (int)Math.Round(24 * Config.WeedsMultiplier));
                    break;
                }
            }

            return codes.AsEnumerable();
        }
    }
}