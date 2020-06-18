using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MultipleSpouses
{
    /// <summary>The mod entry point.</summary>
    public class Misc
    {
        public static IMonitor Monitor;
        public static IModHelper Helper;
        // call this method from your Entry class
        public static void Initialize(IMonitor monitor, IModHelper helper)
        {
            Monitor = monitor;
            Helper = helper;
        }

        public static Dictionary<string, NPC> GetSpouses(Farmer farmer, int all)
        {
            Dictionary<string, NPC> spouses = new Dictionary<string, NPC>();
            if (all < 0)
            {
                NPC ospouse = farmer.getSpouse();
                if (ospouse != null)
                {
                    spouses.Add(ospouse.Name, ospouse);
                }
            }
            foreach (string friend in farmer.friendshipData.Keys)
            {
                if (farmer.friendshipData[friend].IsMarried() && (all > 0 || friend != farmer.spouse))
                {
                    spouses.Add(friend, Game1.getCharacterFromName(friend, true));
                }
            }

            return spouses;
        }

        public static void PlaceSpousesInFarmhouse(FarmHouse farmHouse)
        {
            Farmer farmer = farmHouse.owner;

            if (farmer == null)
                return;

            List<NPC> allSpouses = GetSpouses(farmer, -1).Values.ToList();

            Game1.getFarm().addSpouseOutdoorArea("");

            List<string> bedSpouses = new List<string>();
            string kitchenSpouse = null;
            ModEntry.outdoorSpouse = null;

            foreach (NPC spouse in allSpouses)
            {
                int maxType = 4;


                int type = ModEntry.myRand.Next(0, maxType);

                Monitor.Log("spouse type: " + type);
                switch (type)
                {
                    case 1:
                        if (bedSpouses.Count < GetBedWidth(farmHouse) && (ModEntry.config.RoommateRomance || !farmer.friendshipData[spouse.Name].IsRoommate()))
                        {
                            Monitor.Log("made bed spouse: " + spouse.Name);
                            bedSpouses.Add(spouse.Name);
                        }
                        break;
                    case 2:
                        if (kitchenSpouse == null)
                        {
                            Monitor.Log("made kitchen spouse: " + spouse.Name);
                            kitchenSpouse = spouse.Name;
                        }
                        break;
                    case 3:
                        if (ModEntry.outdoorSpouse == null)
                        {
                            Monitor.Log("made outdoor spouse: " + spouse.Name);
                            ModEntry.outdoorSpouse = spouse.Name;
                            Game1.getFarm().addSpouseOutdoorArea(ModEntry.outdoorSpouse);
                        }
                        break;
                    default:
                        break;
                }
            }
            List<string> allBedSpouses = new List<string>(GetSpouses(farmer, 1).Keys.ToList());
            ShuffleList(ref allBedSpouses);

            foreach (NPC j in allSpouses) { 
                Monitor.Log("placing " + j.Name);

                if (ModEntry.outdoorSpouse == j.Name && !Game1.isRaining && !Game1.IsWinter && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Sat") && !j.Name.Equals("Krobus"))
                {
                    Monitor.Log("going to outdoor patio");
                    j.setUpForOutdoorPatioActivity();
                    continue;
                }

                if (!farmHouse.Equals(j.currentLocation))
                {
                    continue;
                }


                Monitor.Log("in farm house");
                j.shouldPlaySpousePatioAnimation.Value = false;

                Vector2 spot = (farmHouse.upgradeLevel == 1) ? new Vector2(32f, 5f) : new Vector2(38f, 14f);


                if (bedSpouses.Count > 0)
                {
                    if (IsInBed(j.GetBoundingBox()) || bedSpouses.Contains(j.Name))
                    {
                        Monitor.Log($"putting {j.Name} in bed");
                        j.position.Value = GetSpouseBedPosition(farmHouse, allBedSpouses, j.Name);
                        j.faceDirection(ModEntry.myRand.NextDouble() > 0.5 ? 1 : 3);
                    }
                }
                else if (kitchenSpouse == j.Name)
                {
                    Monitor.Log($"{j.Name} is in kitchen");
                    j.setTilePosition(farmHouse.getKitchenStandingSpot());
                }
                else if (!ModEntry.config.BuildAllSpousesRooms && farmer.spouse != j.Name)
                {
                    j.setTilePosition(farmHouse.getRandomOpenPointInHouse(ModEntry.myRand));
                }
                else
                {

                    List<NPC> roomSpouses = allSpouses.FindAll((s) => Maps.roomIndexes.ContainsKey(s.Name) || Maps.tmxSpouseRooms.ContainsKey(s.Name));

                    if (!roomSpouses.Contains(j))
                    {
                        j.setTilePosition(farmHouse.getRandomOpenPointInHouse(ModEntry.myRand));
                        j.faceDirection(ModEntry.myRand.Next(0, 4));
                        Monitor.Log($"{j.Name} spouse random loc");
                        continue;
                    }
                    else
                    {
                        int offset = roomSpouses.IndexOf(j) * 7;
                        j.setTilePosition((int)spot.X + offset, (int)spot.Y);
                        j.faceDirection(ModEntry.myRand.Next(0, 4));
                        Monitor.Log($"{j.Name} loc: {(spot.X + offset)},{spot.Y}");
                    }
                }
            }

        }

        public static bool IsInBed(Rectangle box)
        {
            FarmHouse fh = Utility.getHomeOfFarmer(Game1.player);
            int bedWidth = GetBedWidth(fh);
            Point bedStart = GetBedStart(fh);
            Rectangle bed = new Rectangle(bedStart.X * 64, bedStart.Y * 64 + 64, bedWidth * 64, 3 * 64);
            return box.Intersects(bed);
        }
        public static Vector2 GetSpouseBedPosition(FarmHouse fh, List<string> allBedmates, string name)
        {
            int bedWidth = GetBedWidth(fh);
            Point bedStart = GetBedStart(fh);
            int x = (int)((allBedmates.IndexOf(name) + 1) / (float)(allBedmates.Count + 1) * (bedWidth - 1) * 64);
            return new Vector2(bedStart.X * 64 + x, bedStart.Y * 64 + 64 + ModEntry.bedSleepOffset + (name == "Game1.player"?32:0));
        }
        public static Vector2 GetFarmerBedPosition(FarmHouse fh)
        {
            Point bedStart = GetBedStart(fh);
            return new Vector2(bedStart.X * 64, bedStart.Y * 64 + 64 + ModEntry.bedSleepOffset + 32);
        }

        public static Point GetBedStart(FarmHouse fh)
        {
            bool up = fh.upgradeLevel > 1;
            return new Point(21 - (up ? (GetBedWidth(fh) / 2) - 1: 0) + (up ? 6 : 0), 2 + (up?9:0));
        }

        public static int GetBedWidth(FarmHouse fh)
        {
            if (ModEntry.config.CustomBed)
            {
                bool up = fh.upgradeLevel > 1;
                return Math.Min(up ? 9 : 6, Math.Max(ModEntry.config.BedWidth, 3));
            }
            else
            {
                return 3;
            }
        }

        public static void ResetSpouses(Farmer f)
        {
            Dictionary<string, NPC> spouses = GetSpouses(f,1);
            if (f.spouse == null)
            {
                if(spouses.Count > 0)
                {
                    f.spouse = spouses.First().Key;
                }
            }

            foreach (string name in f.friendshipData.Keys)
            {
                if (f.friendshipData[name].IsEngaged())
                {
                    if(f.friendshipData[name].WeddingDate.TotalDays < new WorldDate(Game1.Date).TotalDays)
                    {
                        Monitor.Log("invalid engagement: " + name);
                        f.friendshipData[name].WeddingDate.TotalDays = new WorldDate(Game1.Date).TotalDays + 1;
                    }
                    if(f.spouse != name)
                    {
                        Monitor.Log("setting spouse to engagee: " + name);
                        f.spouse = name;
                    }
                }
                if (f.friendshipData[name].IsMarried() && f.spouse != name)
                {
                    if (f.spouse != null && f.friendshipData[f.spouse] != null && !f.friendshipData[f.spouse].IsMarried() && !f.friendshipData[f.spouse].IsEngaged())
                    {
                        Monitor.Log("invalid ospouse, setting: " + name);
                        f.spouse = name;
                    }
                    if (f.spouse == null)
                    {
                        Monitor.Log("null ospouse, setting: " + name);
                        f.spouse = name;
                    }
                }
            }
        }
        public static void SetAllNPCsDatable()
        {
            if (!ModEntry.config.RomanceAllVillagers)
                return;
            Farmer f = Game1.player;
            if (f == null)
            {
                return;
            }
            foreach (string friend in f.friendshipData.Keys)
            {
                NPC npc = Game1.getCharacterFromName(friend);
                if (npc != null && !npc.datable && npc is NPC && !(npc is Child) && (npc.Age == 0 || npc.Age == 1))
                {
                    Monitor.Log($"Making {npc.Name} datable.");
                    npc.datable.Value = true;
                }
            }
        }

 
        public static void ShuffleList<T>(ref List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = ModEntry.myRand.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        public static void ShuffleDic<T1,T2>(ref Dictionary<T1,T2> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = ModEntry.myRand.Next(n + 1);
                var value = list[list.Keys.ToArray()[k]];
                list[list.Keys.ToArray()[k]] = list[list.Keys.ToArray()[n]];
                list[list.Keys.ToArray()[n]] = value;
            }
        }


        public static Point getChildBed(FarmHouse farmhouse, string name)
        {
            List<NPC> children = farmhouse.characters.ToList().FindAll((n) => n is Child && n.Age == 3);
            int index = children.FindIndex((n) => n.Name == name);
            int offset = (index * 4) + (ModEntry.config.ExtraCribs * 3);
            if (index > ModEntry.config.ExtraKidsBeds + 1)
            {
                offset = (index % (ModEntry.config.ExtraKidsBeds + 1) * 4) + 1;
            }
            return new Point(22 + offset, 5);
        }
        public static bool ChangingHouse()
        {
            return ModEntry.config.BuildAllSpousesRooms || ModEntry.config.CustomBed || ModEntry.config.ExtraCribs != 0 || ModEntry.config.ExtraKidsBeds != 0 || ModEntry.config.ExtraKidsRoomWidth != 0;
        }
    }
}