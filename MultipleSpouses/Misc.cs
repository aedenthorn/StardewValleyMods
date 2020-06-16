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

        public static List<string> GetAllSpouseNamesOfficialFirst(Farmer farmer)
        {
            List<string> mySpouses = ModEntry.spouses.Keys.ToList();
            if (farmer.spouse != null)
            {
                mySpouses.Insert(0, farmer.spouse);
            }
            return mySpouses;
        }
        
        public static void GetSpouseRoomPosition(FarmHouse farmHouse, string spouse)
        {
        }

        public static void PlaceSpousesInFarmhouse()
        {
            Farmer farmer = Game1.player;
            FarmHouse farmHouse = Utility.getHomeOfFarmer(farmer);

            List<NPC> mySpouses = ModEntry.spouses.Values.ToList();
            if (farmer.spouse != null)
            {
                mySpouses.Insert(0, farmer.getSpouse());
            }

            foreach (NPC j in mySpouses) { 
                Monitor.Log("placing " + j.Name);

                if (ModEntry.outdoorSpouse == j.Name && !Game1.isRaining && !Game1.IsWinter && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Sat") && !j.Name.Equals("Krobus"))
                {
                    Monitor.Log("going to outdoor patio");
                    j.setUpForOutdoorPatioActivity();
                    continue;
                }

                if (j.currentLocation != farmHouse)
                {
                    continue;
                }


                Monitor.Log("in farm house");
                j.shouldPlaySpousePatioAnimation.Value = false;

                Vector2 spot = (farmHouse.upgradeLevel == 1) ? new Vector2(32f, 5f) : new Vector2(38f, 14f);

                if (ModEntry.bedSpouse != null)
                {
                    foreach (NPC character in farmHouse.characters)
                    {
                        if (character.isVillager() && Misc.GetAllSpouses().ContainsKey(character.Name) && Misc.IsInBed(character.GetBoundingBox()))
                        {
                            Monitor.Log($"{character.Name} is already in bed");
                            ModEntry.bedSpouse = character.Name;
                            character.position.Value = Misc.GetSpouseBedPosition(farmHouse, character.name);
                            break;
                        }
                    }
                }

                if (ModEntry.kitchenSpouse == j.Name)
                {
                    Monitor.Log($"{j.Name} is in kitchen");
                    j.setTilePosition(farmHouse.getKitchenStandingSpot());
                    ModEntry.kitchenSpouse = null;
                }
                else if (ModEntry.bedSpouse == j.Name)
                {
                    Monitor.Log($"{j.Name} is in bed");
                    j.position.Value = Misc.GetSpouseBedPosition(farmHouse, j.name);
                    j.faceDirection(ModEntry.myRand.NextDouble() > 0.5 ? 1 : 3);
                    ModEntry.bedSpouse = null;
                }
                else if (!ModEntry.config.BuildAllSpousesRooms && farmer.spouse != j.Name)
                {
                    j.setTilePosition(farmHouse.getRandomOpenPointInHouse(ModEntry.myRand));
                }
                else
                {
                    Misc.ResetSpouses(farmer);

                    List<NPC> roomSpouses = mySpouses.FindAll((s) => Maps.roomIndexes.ContainsKey(s.Name) || Maps.tmxSpouseRooms.ContainsKey(s.Name));


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
        public static void SetBedmates()
        {
            if (ModEntry.allRandomSpouses == null)
            {
                ModEntry.allRandomSpouses = GetRandomSpouses(true).Keys.ToList();
            }

            List<string> bedmates = new List<string>();
            bedmates.Add("Game1.player");
            for (int i = 0; i < ModEntry.allRandomSpouses.Count; i++)
            {
                bedmates.Add(ModEntry.allRandomSpouses[i]);
            }
            ModEntry.allBedmates = new List<string>(bedmates);
        }
        public static Vector2 GetSpouseBedPosition(FarmHouse fh, string name)
        {
            SetBedmates();
            int bedWidth = GetBedWidth(fh);
            Point bedStart = GetBedStart(fh);
            int x = (int)(ModEntry.allBedmates.IndexOf(name) / (float)ModEntry.allBedmates.Count * (bedWidth - 1) * 64);
            return new Vector2(bedStart.X * 64 + x, bedStart.Y * 64 + 64 + ModEntry.bedSleepOffset + (name == "Game1.player"?32:0));
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

        public static void ResetSpouseRoles()
        {
            ModEntry.spouseRolesDate = new WorldDate().TotalDays;
            ModEntry.outdoorSpouse = null;
            ModEntry.kitchenSpouse = null;
            ModEntry.bedSpouse = null;
            ResetSpouses(Game1.player);
            List<NPC> allSpouses = GetAllSpouses().Values.ToList();
            Monitor.Log("num spouses: " + allSpouses.Count);

            int n = allSpouses.Count;
            while (n > 1)
            {
                n--;
                int k = ModEntry.myRand.Next(n + 1);
                NPC value = allSpouses[k];
                allSpouses[k] = allSpouses[n];
                allSpouses[n] = value;
            }

            Game1.getFarm().addSpouseOutdoorArea("");

            foreach (NPC spouse in allSpouses)
            {
                int maxType = 4;


                int type = ModEntry.myRand.Next(0, maxType);

                Monitor.Log("spouse type: " + type);
                switch (type)
                {
                    case 1:
                        if (ModEntry.bedSpouse == null)
                        {
                            Monitor.Log("made bed spouse: " + spouse.Name);
                            ModEntry.bedSpouse = spouse.Name;
                        }
                        break;
                    case 2:
                        if (ModEntry.kitchenSpouse == null)
                        {
                            Monitor.Log("made kitchen spouse: " + spouse.Name);
                            ModEntry.kitchenSpouse = spouse.Name;
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
        }

        internal static bool SpotHasSpouse(Vector2 position, GameLocation location)
        {
            foreach(NPC spouse in ModEntry.spouses.Values)
            {
                if (spouse.currentLocation == location)
                {
                    Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle((int)position.X + 1, (int)position.Y + 1, 62, 62);
                    if(spouse.GetBoundingBox().Intersects(rect))
                        return true;
                }
            }
            return false;
        }

        public static void ResetSpouses(Farmer f)
        {
            if (f.spouse == null)
            {
                if(ModEntry.officialSpouse != null && f.friendshipData[ModEntry.officialSpouse] != null && (f.friendshipData[ModEntry.officialSpouse].IsMarried() || f.friendshipData[ModEntry.officialSpouse].IsEngaged()))
                {
                    f.spouse = ModEntry.officialSpouse;
                }
            }
            ModEntry.officialSpouse = f.spouse;

            ModEntry.spouses.Clear();
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
                        ModEntry.officialSpouse = name;
                    }
                    continue;
                }
                if (f.friendshipData[name].IsMarried() && f.spouse != name)
                {
                    if (f.friendshipData[name].WeddingDate != null)
                    {
                        //Monitor.Log($"wedding date {f.friendshipData[name].WeddingDate.TotalDays} " + name);
                    }
                    if (f.spouse != null && f.friendshipData[f.spouse] != null && !f.friendshipData[f.spouse].IsMarried() && !f.friendshipData[f.spouse].IsEngaged())
                    {
                        Monitor.Log("invalid ospouse, setting: " + name);
                        f.spouse = name;
                        ModEntry.officialSpouse = name;
                        continue;
                    }
                    if (f.spouse == null)
                    {
                        Monitor.Log("null ospouse, setting: " + name);
                        f.spouse = name;
                        ModEntry.officialSpouse = name;
                        continue;
                    }

                    NPC npc = Game1.getCharacterFromName(name);
                    if(npc == null)
                    {
                        foreach(GameLocation l in Game1.locations)
                        {
                            foreach(NPC c in l.characters)
                            {
                                if(c.Name == name)
                                {
                                    npc = c;
                                    goto next;
                                }
                            }
                        }
                    }
                    if(npc == null)
                    {
                        continue;
                    }
                    next:
                    Monitor.Log("adding spouse: " + name);
                    ModEntry.spouses.Add(name,npc);
                }
            }
            Monitor.Log("official spouse: " + ModEntry.officialSpouse);
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

        public static Dictionary<string,NPC> GetAllSpouses()
        {
            Dictionary<string, NPC> npcs = new Dictionary<string, NPC>(ModEntry.spouses);
            NPC ospouse = Game1.player.getSpouse();
            if (ospouse != null)
            {
                npcs.Add(ospouse.Name, ospouse);
            }
            return npcs;
        }
        public static Dictionary<string,NPC> GetRandomSpouses(bool all = false)
        {
            Dictionary<string, NPC> npcs = new Dictionary<string, NPC>(ModEntry.spouses);
            if (all)
            {
                NPC ospouse = Game1.player.getSpouse();
                if (ospouse != null)
                {
                    npcs.Add(ospouse.Name, ospouse);
                }
            }

            ShuffleDic(ref npcs);

            return npcs;
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