using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FreeLove
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {
        private static Dictionary<string, int> topOfHeadOffsets = new Dictionary<string, int>();

        public static void ReloadSpouses(Farmer farmer)
        {
            currentSpouses[farmer.UniqueMultiplayerID] = new Dictionary<string, NPC>();
            currentUnofficialSpouses[farmer.UniqueMultiplayerID] = new Dictionary<string, NPC>();
            string ospouse = farmer.spouse;
            if (ospouse != null)
            {
                var npc = Game1.getCharacterFromName(ospouse);
                if(npc is not null)
                {
                    currentSpouses[farmer.UniqueMultiplayerID][ospouse] = npc;
                }
            }
            SMonitor.Log($"Checking for extra spouses in {farmer.friendshipData.Count()} friends");
            foreach (string friend in farmer.friendshipData.Keys)
            {
                if (farmer.friendshipData[friend].IsMarried() && friend != farmer.spouse)
                {
                    var npc = Game1.getCharacterFromName(friend, true);
                    if(npc != null)
                    {
                        currentSpouses[farmer.UniqueMultiplayerID][friend] = npc;
                        currentUnofficialSpouses[farmer.UniqueMultiplayerID][friend] = npc;
                    }
                }
            }
            if (farmer.spouse is null && currentSpouses[farmer.UniqueMultiplayerID].Any())
                farmer.spouse = currentSpouses[farmer.UniqueMultiplayerID].First().Key;
            SMonitor.Log($"reloaded {currentSpouses[farmer.UniqueMultiplayerID].Count} spouses for {farmer.Name} {farmer.UniqueMultiplayerID}");
        }
        public static Dictionary<string, NPC> GetSpouses(Farmer farmer, bool all)
        {
            if(!currentSpouses.ContainsKey(farmer.UniqueMultiplayerID) || ((currentSpouses[farmer.UniqueMultiplayerID].Count == 0 && farmer.spouse != null)))
            {
                ReloadSpouses(farmer);
            }
            if(farmer.spouse == null && currentSpouses[farmer.UniqueMultiplayerID].Count > 0)
            {
                farmer.spouse = currentSpouses[farmer.UniqueMultiplayerID].First().Key;
            }
            return all ? currentSpouses[farmer.UniqueMultiplayerID] : currentUnofficialSpouses[farmer.UniqueMultiplayerID];
        }

        internal static void ResetDivorces()
        {
            if (!Config.PreventHostileDivorces)
                return;
            List<string> friends = Game1.player.friendshipData.Keys.ToList();
            foreach(string f in friends)
            {
                if(Game1.player.friendshipData[f].Status == FriendshipStatus.Divorced)
                {
                    SMonitor.Log($"Wiping divorce for {f}");
                    if (Game1.player.friendshipData[f].Points < 8 * 250)
                        Game1.player.friendshipData[f].Status = FriendshipStatus.Friendly;
                    else
                        Game1.player.friendshipData[f].Status = FriendshipStatus.Dating;
                }
            }
        }

        public static string GetRandomSpouse(Farmer f)
        {
            var spouses = GetSpouses(f, true);
            if (spouses.Count == 0)
                return null;
            ShuffleDic(ref spouses);
            return spouses.Keys.ToArray()[0];
        }

        public static void PlaceSpousesInFarmhouse(FarmHouse farmHouse)
        {
            Farmer farmer = farmHouse.owner;

            if (farmer == null)
                return;

            List<NPC> allSpouses = GetSpouses(farmer, true).Values.ToList();

            if (allSpouses.Count == 0)
            {
                SMonitor.Log("no spouses");
                return;
            }

            ShuffleList(ref allSpouses);

            List<string> bedSpouses = new List<string>();
            string kitchenSpouse = null;

            foreach (NPC spouse in allSpouses)
            {
                if(spouse is null) 
                    continue;
                if (!farmHouse.Equals(spouse.currentLocation))
                {
                    SMonitor.Log($"{spouse.Name} is not in farm house ({spouse.currentLocation.Name})");
                    continue;
                }
                int type = myRand.Next(0, 100);

                SMonitor.Log($"spouse rand {type}, bed: {Config.PercentChanceForSpouseInBed} kitchen {Config.PercentChanceForSpouseInKitchen}");
                
                if(type < Config.PercentChanceForSpouseInBed)
                {
                    if (bedSpouses.Count < 1 && (Config.RoommateRomance || !farmer.friendshipData[spouse.Name].IsRoommate()) && HasSleepingAnimation(spouse.Name))
                    {
                        SMonitor.Log("made bed spouse: " + spouse.Name);
                        bedSpouses.Add(spouse.Name);
                    }

                }
                else if(type < Config.PercentChanceForSpouseInBed + Config.PercentChanceForSpouseInKitchen)
                {
                    if (kitchenSpouse == null)
                    {
                        SMonitor.Log("made kitchen spouse: " + spouse.Name);
                        kitchenSpouse = spouse.Name;
                    }
                }
                else if(type < Config.PercentChanceForSpouseInBed + Config.PercentChanceForSpouseInKitchen + Config.PercentChanceForSpouseAtPatio)
                {
                    if (!Game1.isRaining && !Game1.IsWinter && !Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Sat") && !spouse.Name.Equals("Krobus") && spouse.Schedule == null)
                    {
                        SMonitor.Log("made patio spouse: " + spouse.Name);
                        spouse.setUpForOutdoorPatioActivity();
                        SMonitor.Log($"{spouse.Name} at {spouse.currentLocation.Name} {spouse.TilePoint}");
                    }
                }
            }

            foreach (NPC spouse in allSpouses) 
            {
                if (spouse is null)
                    continue;
                SMonitor.Log("placing " + spouse.Name);

                Point spouseRoomSpot = new Point(-1, -1); 
                
                if(customSpouseRoomsAPI != null)
                {
                    SMonitor.Log($"Getting spouse spot from Custom Spouse Rooms");

                    spouseRoomSpot = customSpouseRoomsAPI.GetSpouseTile(spouse);
                    if(spouseRoomSpot.X >= 0)
                        SMonitor.Log($"Got custom spouse spot {spouseRoomSpot}");
                }
                if(spouseRoomSpot.X < 0 && farmer.spouse == spouse.Name)
                {
                    spouseRoomSpot = farmHouse.GetSpouseRoomSpot();
                    SMonitor.Log($"Using default spouse spot {spouseRoomSpot}");
                }

                if (!farmHouse.Equals(spouse.currentLocation))
                {
                    SMonitor.Log($"{spouse.Name} is not in farm house ({spouse.currentLocation.Name})");
                    continue;
                }

                SMonitor.Log("in farm house");
                spouse.shouldPlaySpousePatioAnimation.Value = false;

                Vector2 bedPos = GetSpouseBedPosition(farmHouse, spouse.Name);

                if (bedSpouses.Count > 0 && bedSpouses.Contains(spouse.Name) && bedPos != Vector2.Zero)
                {
                    SMonitor.Log($"putting {spouse.Name} in bed");
                    spouse.position.Value = GetSpouseBedPosition(farmHouse, spouse.Name);
                }
                else if (kitchenSpouse == spouse.Name && !IsTileOccupied(farmHouse, farmHouse.getKitchenStandingSpot(), spouse.Name))
                {
                    SMonitor.Log($"{spouse.Name} is in kitchen");

                    spouse.setTilePosition(farmHouse.getKitchenStandingSpot());
                    spouse.setRandomAfternoonMarriageDialogue(Game1.timeOfDay, farmHouse, false);
                }
                else if (spouseRoomSpot.X > -1 && !IsTileOccupied(farmHouse, spouseRoomSpot, spouse.Name))
                {
                    SMonitor.Log($"{spouse.Name} is in spouse room");
                    spouse.setTilePosition(spouseRoomSpot);
                    spouse.setSpouseRoomMarriageDialogue();
                }
                else 
                { 
                    spouse.setTilePosition(farmHouse.getRandomOpenPointInHouse(myRand));
                    spouse.faceDirection(myRand.Next(0, 4));
                    SMonitor.Log($"{spouse.Name} spouse random loc {spouse.TilePoint}");
                    spouse.setRandomAfternoonMarriageDialogue(Game1.timeOfDay, farmHouse, false);
                }
            }
        }

        private static bool IsTileOccupied(GameLocation location, Point tileLocation, string characterToIgnore)
        {
            Rectangle tileLocationRect = new Rectangle(tileLocation.X * 64 + 1, tileLocation.Y * 64 + 1, 62, 62);

            for (int i = 0; i < location.characters.Count; i++)
            {
                if (location.characters[i] != null && !location.characters[i].Name.Equals(characterToIgnore) && location.characters[i].GetBoundingBox().Intersects(tileLocationRect))
                {
                    SMonitor.Log($"Tile {tileLocation} is occupied by {location.characters[i].Name}");

                    return true;
                }
            }
            return false;
        }

        public static Point GetSpouseBedEndPoint(FarmHouse fh, string name)
        {
            var bedSpouses = GetBedSpouses(fh);

            Point bedStart = fh.GetSpouseBed().GetBedSpot();
            int bedWidth = GetBedWidth();
            bool up = fh.upgradeLevel > 1;

            int x = (int)(bedSpouses.IndexOf(name) / (float)(bedSpouses.Count) * (bedWidth - 1));
            if (x < 0)
                return Point.Zero;
            return new Point(bedStart.X + x, bedStart.Y);
        }
        public static Vector2 GetSpouseBedPosition(FarmHouse fh, string name)
        {
            var allBedmates = GetBedSpouses(fh);

            Point bedStart = GetBedStart(fh);
            int x = 64 + (int)((allBedmates.IndexOf(name) + 1) / (float)(allBedmates.Count + 1) * (GetBedWidth() - 1) * 64);
            return new Vector2(bedStart.X * 64 + x, bedStart.Y * 64 + bedSleepOffset - (GetTopOfHeadSleepOffset(name) * 4));
        }

        public static Point GetBedStart(FarmHouse fh)
        {
            if (fh?.GetSpouseBed()?.GetBedSpot() == null)
                return Point.Zero;
            return new Point(fh.GetSpouseBed().GetBedSpot().X - 1, fh.GetSpouseBed().GetBedSpot().Y - 1);
        }

        public static bool IsInBed(FarmHouse fh, Rectangle box)
        {
            int bedWidth = GetBedWidth();
            Point bedStart = GetBedStart(fh);
            Rectangle bed = new Rectangle(bedStart.X * 64, bedStart.Y * 64, bedWidth * 64, 3 * 64);

            if (box.Intersects(bed))
            {
                return true;
            }
            return false;
        }
        public static int GetBedWidth()
        {
            if (bedTweaksAPI != null)
            {
                return bedTweaksAPI.GetBedWidth();
            }
            else
            {
                return 3;
            }
        }
        public static List<string> GetBedSpouses(FarmHouse fh)
        {
            if (Config.RoommateRomance)
                return GetSpouses(fh.owner, true).Keys.ToList();

            return GetSpouses(fh.owner, true).Keys.ToList().FindAll(s => !fh.owner.friendshipData[s].RoommateMarriage);
        }

        public static List<string> ReorderSpousesForSleeping(List<string> sleepSpouses)
        {
            List<string> configSpouses = Config.SpouseSleepOrder.Split(',').Where(s => s.Length > 0).ToList();
            List<string> spouses = new List<string>();
            foreach (string s in configSpouses)
            {
                if (sleepSpouses.Contains(s))
                    spouses.Add(s);
            }

            foreach (string s in sleepSpouses)
            {
                if (!spouses.Contains(s))
                {
                    spouses.Add(s);
                    configSpouses.Add(s);
                }
            }
            string configString = string.Join(",", configSpouses);
            if (configString != Config.SpouseSleepOrder)
            {
                Config.SpouseSleepOrder = configString;
                SHelper.WriteConfig(Config);
            }

            return spouses;
        }


        public static int GetTopOfHeadSleepOffset(string name)
        {
            if (topOfHeadOffsets.ContainsKey(name))
            {
                return topOfHeadOffsets[name];
            }
            //SMonitor.Log($"dont yet have offset for {name}");
            int top = 0;

            if (name == "Krobus")
                return 8;

            Texture2D tex = Game1.content.Load<Texture2D>($"Characters\\{name}");

            int sleepidx;
            string sleepAnim = SleepAnimation(name);
            if (sleepAnim == null || !int.TryParse(sleepAnim.Split('/')[0], out sleepidx))
                sleepidx = 8;

            if ((sleepidx * 16) / 64 * 32 >= tex.Height)
            {
                sleepidx = 8;
            }


            Color[] colors = new Color[tex.Width * tex.Height];
            tex.GetData(colors);

            //SMonitor.Log($"sleep index for {name} {sleepidx}");

            int startx = (sleepidx * 16) % 64;
            int starty = (sleepidx * 16) / 64 * 32;

            //SMonitor.Log($"start {startx},{starty}");

            for (int i = 0; i < 16 * 32; i++)
            {
                int idx = startx + (i % 16) + (starty + i / 16) * 64;
                if (idx >= colors.Length)
                {
                    SMonitor.Log($"Sleep pos couldn't get pixel at {startx + i % 16},{starty + i / 16} ");
                    break;
                }
                Color c = colors[idx];
                if (c != Color.Transparent)
                {
                    top = i / 16;
                    break;
                }
            }
            topOfHeadOffsets.Add(name, top);
            return top;
        }


        public static bool HasSleepingAnimation(string name)
        {
            string sleepAnim = SleepAnimation(name);
            if (sleepAnim == null || !sleepAnim.Contains("/"))
                return false;

            if (!int.TryParse(sleepAnim.Split('/')[0], out int sleepidx))
                return false;

            Texture2D tex = SHelper.GameContent.Load<Texture2D>($"Characters/{name}");
            //SMonitor.Log($"tex height for {name}: {tex.Height}");

            if (sleepidx / 4 * 32 >= tex.Height)
            {
                return false;
            }
            return true;
        }

        private static string SleepAnimation(string name)
        {
            string anim = null;
            if (Game1.content.Load<Dictionary<string, string>>("Data\\animationDescriptions").ContainsKey(name.ToLower() + "_sleep"))
            {
                anim = Game1.content.Load<Dictionary<string, string>>("Data\\animationDescriptions")[name.ToLower() + "_sleep"];
            }
            else if (Game1.content.Load<Dictionary<string, string>>("Data\\animationDescriptions").ContainsKey(name + "_Sleep"))
            {
                anim = Game1.content.Load<Dictionary<string, string>>("Data\\animationDescriptions")[name + "_Sleep"];
            }
            return anim;
        }


        internal static void NPCDoAnimation(NPC npc, string npcAnimation)
        {
            Dictionary<string, string> animationDescriptions = SHelper.GameContent.Load<Dictionary<string, string>>("Data\\animationDescriptions");
            if (!animationDescriptions.ContainsKey(npcAnimation))
                return;

            string[] rawData = animationDescriptions[npcAnimation].Split('/');
            var animFrames = Utility.parseStringToIntArray(rawData[1], ' ');
 
            List<FarmerSprite.AnimationFrame> anim = new List<FarmerSprite.AnimationFrame>();
            for (int i = 0; i < animFrames.Length; i++)
            {
                    anim.Add(new FarmerSprite.AnimationFrame(animFrames[i], 100, 0, false, false, null, false, 0));
            }
            SMonitor.Log($"playing animation {npcAnimation} for {npc.Name}");
            npc.Sprite.setCurrentAnimation(anim);
        }

        public static void ResetSpouses(Farmer f, bool force = false)
        {
            if (force)
            {
                currentSpouses.Remove(f.UniqueMultiplayerID);
                currentUnofficialSpouses.Remove(f.UniqueMultiplayerID);
            }
            Dictionary<string, NPC> spouses = GetSpouses(f,true);
            if (f.spouse == null)
            {
                if(spouses.Count > 0)
                {
                    SMonitor.Log("No official spouse, setting official spouse to: " + spouses.First().Key);
                    f.spouse = spouses.First().Key;
                }
            }

            foreach (string name in f.friendshipData.Keys)
            {
                if (f.friendshipData[name].IsEngaged())
                {
                    SMonitor.Log($"{f.Name} is engaged to: {name} {f.friendshipData[name].CountdownToWedding} days until wedding");
                    if (f.friendshipData[name].WeddingDate.TotalDays < new WorldDate(Game1.Date).TotalDays)
                    {
                        SMonitor.Log("invalid engagement: " + name);
                        f.friendshipData[name].WeddingDate.TotalDays = new WorldDate(Game1.Date).TotalDays + 1;
                    }
                    if(f.spouse != name)
                    {
                        SMonitor.Log("setting spouse to engagee: " + name);
                        f.spouse = name;
                    }
                }
                if (f.friendshipData[name].IsMarried() && f.spouse != name)
                {
                    //SMonitor.Log($"{f.Name} is married to: {name}");
                    if (f.spouse != null && f.friendshipData[f.spouse] != null && !f.friendshipData[f.spouse].IsMarried() && !f.friendshipData[f.spouse].IsEngaged())
                    {
                        SMonitor.Log("invalid ospouse, setting ospouse to " + name);
                        f.spouse = name;
                    }
                    if (f.spouse == null)
                    {
                        SMonitor.Log("null ospouse, setting ospouse to " + name);
                        f.spouse = name;
                    }
                }
            }
            ReloadSpouses(f);
        }
        public static void SetAllNPCsDatable()
        {
            if (!Config.RomanceAllVillagers)
                return;
            Farmer f = Game1.player;
            if (f == null)
            {
                return;
            }
            foreach (string friend in f.friendshipData.Keys)
            {
                NPC npc = Game1.getCharacterFromName(friend);
                if (npc != null && !npc.datable.Value && npc is NPC && !(npc is Child) && (npc.Age == 0 || npc.Age == 1))
                {
                    SMonitor.Log($"Making {npc.Name} datable.");
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
                int k = myRand.Next(n + 1);
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
                int k = myRand.Next(n + 1);
                var value = list[list.Keys.ToArray()[k]];
                list[list.Keys.ToArray()[k]] = list[list.Keys.ToArray()[n]];
                list[list.Keys.ToArray()[n]] = value;
            }
        }
    }
}