 using System;
using System.Collections.Generic;
using System.Reflection;
using Harmony;
using static Harmony.AccessTools;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;
using System.Linq;
using StardewValley.Network;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using StardewValley.Events;
using StardewValley.Menus;

namespace MultipleSpouses
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {

		public static IMonitor PMonitor;
		public static IModHelper PHelper;
        public static ModConfig config;

        public static Dictionary<string, NPC> spouses { get; private set; } = new Dictionary<string, NPC>();
        public static string outdoorSpouse = null;
		public static string kitchenSpouse = null;
        public static string bedSpouse = null;
        public static string spouseToDivorce = null;
        public static int spouseRolesDate = -1;
		public static Multiplayer mp;
		public static List<string> kissingSpouses = new List<string>();
		public static int lastKissTime = 0;
		public static SoundEffect kissEffect = null;
		public static Random myRand;

		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			config = Helper.ReadConfig<ModConfig>();

			if (!config.EnableMod)
				return;

			PMonitor = Monitor;
			PHelper = helper;

            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
            helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;
			mp = helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
			helper.Events.Input.ButtonPressed += Input_ButtonPressed;
			myRand = new Random();



			string filePath = $"{helper.DirectoryPath}\\assets\\kiss.wav";
			PMonitor.Log("Kissing audio path: " + filePath);
			if (File.Exists(filePath))
			{
				kissEffect = SoundEffect.FromStream(new FileStream(filePath, FileMode.Open));
			}
			else
			{
				PMonitor.Log("Kissing audio not found at path: " + filePath);
			}

			NPCPatches.Initialize(Monitor);
			LocationPatches.Initialize(Monitor);
			MiscPatches.Initialize(Monitor);

			var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);


			// npc patches

			harmony.Patch(
			   original: AccessTools.Method(typeof(NPC), nameof(NPC.setUpForOutdoorPatioActivity)),
			   prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_setUpForOutdoorPatioActivity_Prefix))
			);

			harmony.Patch(
			   original: AccessTools.Method(typeof(NPC), nameof(NPC.getSpouse)),
			   prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_getSpouse_Prefix))
			);

			harmony.Patch(
			   original: AccessTools.Method(typeof(NPC), nameof(NPC.isRoommate)),
			   prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_isRoommate_Prefix))
			);

			harmony.Patch(
			   original: AccessTools.Method(typeof(NPC), nameof(NPC.isMarried)),
			   prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_isMarried_Prefix))
			);

			harmony.Patch(
			   original: AccessTools.Method(typeof(NPC), nameof(NPC.isMarriedOrEngaged)),
			   prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_isMarriedOrEngaged_Prefix))
			);

			harmony.Patch(
			   original: AccessTools.Method(typeof(NPC), nameof(NPC.tryToReceiveActiveObject)),
			   prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_tryToReceiveActiveObject_Prefix)),
			   postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_tryToReceiveActiveObject_Postfix))
			);

			harmony.Patch(
			   original: AccessTools.Method(typeof(NPC), nameof(NPC.checkAction)),
			   prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_checkAction_Prefix))
			);

			harmony.Patch(
			   original: AccessTools.Method(typeof(NPC), nameof(NPC.marriageDuties)),
			   //prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_marriageDuties_Prefix)),
			   postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_marriageDuties_Postfix))
			);

			harmony.Patch(
			   original: AccessTools.Method(typeof(NPC), nameof(NPC.spouseObstacleCheck)),
			   postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_spouseObstacleCheck_Postfix))
			);

			harmony.Patch(
			   original: AccessTools.Method(typeof(NPC), "engagementResponse"),
			   postfix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_engagementResponse_Postfix))
			);


			// location patches

			harmony.Patch(
			   original: AccessTools.Method(typeof(Farm), "addSpouseOutdoorArea"),
			   prefix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.Farm_addSpouseOutdoorArea_Prefix))
			);

			harmony.Patch(
			   original: AccessTools.Method(typeof(Beach), nameof(Beach.checkAction)),
			   prefix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.Beach_checkAction_Prefix))
			);

			harmony.Patch(
			   original: AccessTools.Method(typeof(ManorHouse), nameof(ManorHouse.performAction)),
			   prefix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.ManorHouse_performAction_Prefix))
			);

			harmony.Patch(
			   original: AccessTools.Method(typeof(GameLocation), "resetLocalState"),
			   postfix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.GameLocation_resetLocalState_Postfix))
			);

			harmony.Patch(
			   original: AccessTools.Method(typeof(GameLocation), "updateMap"),
			   prefix: new HarmonyMethod(typeof(LocationPatches), nameof(LocationPatches.GameLocation_updateMap_Prefix))
			);
			

			// pregnancy patches

			harmony.Patch(
			   original: AccessTools.Method(typeof(Utility), nameof(Utility.pickPersonalFarmEvent)),
			   prefix: new HarmonyMethod(typeof(Pregnancy), nameof(Pregnancy.Utility_pickPersonalFarmEvent_Prefix))
			);
			harmony.Patch(
			   original: AccessTools.Method(typeof(QuestionEvent), nameof(QuestionEvent.setUp)),
			   prefix: new HarmonyMethod(typeof(Pregnancy), nameof(Pregnancy.QuestionEvent_setUp_Prefix))
			);

			harmony.Patch(
			   original: AccessTools.Method(typeof(BirthingEvent), nameof(BirthingEvent.setUp)),
			   prefix: new HarmonyMethod(typeof(Pregnancy), nameof(Pregnancy.BirthingEvent_setUp_Prefix))
			);

			harmony.Patch(
			   original: AccessTools.Method(typeof(BirthingEvent), nameof(BirthingEvent.tickUpdate)),
			   prefix: new HarmonyMethod(typeof(Pregnancy), nameof(Pregnancy.BirthingEvent_tickUpdate_Prefix))
			);


			// misc patches

			harmony.Patch(
			   original: AccessTools.Method(typeof(Farmer), nameof(Farmer.doDivorce)),
			   prefix: new HarmonyMethod(typeof(MiscPatches), nameof(MiscPatches.Farmer_doDivorce_Prefix))
			);

		}

        private void GameLoop_DayEnding(object sender, DayEndingEventArgs e)
        {
			Helper.Events.GameLoop.OneSecondUpdateTicked -= GameLoop_OneSecondUpdateTicked;
		}

		private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
			Helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;
            if (config.CustomBed)
            {
				LocationPatches.ReplaceBed(Utility.getHomeOfFarmer(Game1.player));
			}
		}

		private void GameLoop_ReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
			Helper.Events.GameLoop.OneSecondUpdateTicked -= GameLoop_OneSecondUpdateTicked;
			spouses.Clear();
			outdoorSpouse = null;
			kitchenSpouse = null;
			bedSpouse = null;
			spouseToDivorce = null;
			spouseRolesDate = -1;
		}

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
		{
			if (e.Button == SButton.MouseLeft || e.Button == SButton.MouseRight)
			{
				if (Game1.currentLocation == null || Game1.currentLocation.lastQuestionKey != "divorce")
					return;
				
				IClickableMenu menu = Game1.activeClickableMenu;
				if (menu == null || menu.GetType() != typeof(DialogueBox))
					return;
				int resp = (int)typeof(DialogueBox).GetField("selectedResponse", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(menu);
				List<Response> resps = (List<Response>)typeof(DialogueBox).GetField("responses", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(menu);

				if (resp < 0 || resps == null || resp >= resps.Count || resps[resp] == null)
					return;

				string key = resps[resp].responseKey;

				foreach (NPC spouse in spouses.Values)
				{
					if (key == spouse.Name || key == "Krobus" || key == Game1.player.spouse)
					{
						if (Game1.player.Money >= 50000 || key == "Krobus")
						{
							if (!Game1.player.isRoommate(key))
							{
								Game1.player.Money -= 50000;
								ModEntry.spouseToDivorce = key;
							}
							Game1.player.divorceTonight.Value = true;
							string s = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:ManorHouse_DivorceBook_Filed_" + key);
							if (s == null)
							{
								s = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:ManorHouse_DivorceBook_Filed");
							}
							Game1.drawObjectDialogue(s);
							if (!Game1.player.isRoommate(Game1.player.spouse))
							{
								mp.globalChatInfoMessage("Divorce", new string[]
								{
									Game1.player.Name
								});
							}
						}
						else
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
						}
						break;
					}
				}
			}
		}
		private void GameLoop_OneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
			if (Game1.player == null)
				return;
			FarmHouse fh = Utility.getHomeOfFarmer(Game1.player);
			if (fh == null)
				return;

			int bedWidth = GetBedWidth(fh);
			Point bedStart = GetBedStart(fh);
			Dictionary<string,NPC> allSpouses = GetRandomSpouses(true);
			if (Game1.timeOfDay > 2000)
            {
				foreach (NPC character in fh.characters)
				{
					if (allSpouses.ContainsKey(character.Name))
					{
						//Monitor.Log($"{character.Name} bounding box: {character.GetBoundingBox().X + character.GetBoundingBox().Width/2},{character.GetBoundingBox().Y + character.GetBoundingBox().Height/2}");
						character.farmerPassesThrough = true;
						Microsoft.Xna.Framework.Rectangle bed = new Microsoft.Xna.Framework.Rectangle(bedStart.X * 64, bedStart.Y * 64 + 64, bedWidth * 64, 3 * 64);
						if (character.GetBoundingBox().Intersects(bed) && !character.isMoving())
						{
							int x =  (int)(allSpouses.Keys.ToList().IndexOf(character.Name) / (float)allSpouses.Count * (bedWidth - 1) * 64 + 32);
							Monitor.Log($"moving {character.Name} by {x} pixels");
							character.position.Value = new Vector2(bed.X + x, bed.Y + 48);
						}
						else
                        {
							character.farmerPassesThrough = false;
						}
					}
				}
			}
			if (config.AllowSpousesToKiss)
            {
				TrySpousesKiss();
			}
		}

        public static Point GetBedStart(FarmHouse fh)
        {
			bool up = fh.upgradeLevel > 1;
			return new Point(21 - (up ? (GetBedWidth(fh) / 2) - 1: 0) + 6,2 + (up?9:0));
		}

        public static int GetBedWidth(FarmHouse fh)
        {
			bool up = fh.upgradeLevel > 1;
			return Math.Min(up ? 9 : 7, Math.Max(config.BedWidth, 3));
		}

		public static void ResetSpouseRoles()
        {
			spouseRolesDate = new WorldDate().TotalDays;
			outdoorSpouse = null;
			kitchenSpouse = null;
			bedSpouse = null;
			ResetSpouses(Game1.player);
			List<NPC> allSpouses = spouses.Values.ToList();
			PMonitor.Log("num spouses: " + allSpouses.Count);
			if(Game1.player.getSpouse() != null)
            {
				PMonitor.Log("official spouse: " + Game1.player.getSpouse().Name);
				allSpouses.Add(Game1.player.getSpouse()); 
			}

			foreach (NPC npc in allSpouses)
            {
				Friendship friendship = npc.getSpouse().friendshipData[npc.Name];
				PMonitor.Log($"spouse: {npc.Name}{(friendship.DaysUntilBirthing >= 0 ? " gives birth in: " + friendship.DaysUntilBirthing : "")}");
			}



			int n = allSpouses.Count;
			while (n > 1)
			{
				n--;
				int k = myRand.Next(n + 1);
				NPC value = allSpouses[k];
				allSpouses[k] = allSpouses[n];
				allSpouses[n] = value;
			}

			Game1.getFarm().addSpouseOutdoorArea("");

			foreach (NPC spouse in allSpouses)
            {
				int maxType = 4;


				int type = myRand.Next(0, maxType);

				PMonitor.Log("spouse type: " + type);
				switch (type)
                {
					case 1:
						if (bedSpouse == null)
                        {
							PMonitor.Log("made bed spouse: " + spouse.Name);
							bedSpouse = spouse.Name;
						}
						break;
					case 2:
						if (kitchenSpouse == null)
                        {
							PMonitor.Log("made kitchen spouse: " + spouse.Name);
							kitchenSpouse = spouse.Name;
						}
						break;
					case 3:
						if (outdoorSpouse == null)
                        {
							PMonitor.Log("made outdoor spouse: " + spouse.Name);
							outdoorSpouse = spouse.Name;
							Game1.getFarm().addSpouseOutdoorArea(outdoorSpouse);
						}
						break;
					default:
						break;
                }
			}
		}

        internal static bool SpotHasSpouse(Vector2 position, GameLocation location)
        {
			foreach(NPC spouse in spouses.Values)
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
			PMonitor.Log("Resetting spouses");
			PMonitor.Log("official spouse: " + f.spouse);
			spouses.Clear();
			foreach (string name in f.friendshipData.Keys)
			{
				if (f.friendshipData[name].IsEngaged())
				{
					if(f.friendshipData[name].WeddingDate.TotalDays < new WorldDate(Game1.Date).TotalDays)
                    {
						PMonitor.Log("invalid engagement: " + name);
						f.friendshipData[name].WeddingDate.TotalDays = new WorldDate(Game1.Date).TotalDays + 1;
					}
					if(f.spouse != name)
                    {
						PMonitor.Log("setting spouse to engagee: " + name);
						f.spouse = name;
					}
					continue;
				}
				if (f.friendshipData[name].IsMarried() && f.spouse != name)
				{
					if (f.friendshipData[name].WeddingDate != null)
					{
						//PMonitor.Log($"wedding date {f.friendshipData[name].WeddingDate.TotalDays} " + name);
					}
					if (f.spouse != null && f.friendshipData[f.spouse] != null && !f.friendshipData[f.spouse].IsMarried() && !f.friendshipData[f.spouse].IsEngaged() && !f.friendshipData[f.spouse].IsRoommate())
					{
						PMonitor.Log("invalid ospouse, setting: " + name);
						f.spouse = name;
						continue;
					}
					if (f.spouse == null)
                    {
						f.spouse = name;
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
					PMonitor.Log("adding spouse: " + name);
					spouses.Add(name,npc);
				}
				Utility.getHomeOfFarmer(f).showSpouseRoom();
			}
		}

		public static Dictionary<string,NPC> GetRandomSpouses(bool all = false)
        {
			Dictionary<string, NPC> npcs = new Dictionary<string, NPC>(spouses);
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


        public static void TrySpousesKiss()
        {
			GameLocation location = Game1.currentLocation;

			lastKissTime++;

			if (location == null || location.characters == null)
				return;

			List<NPC> list = location.characters.ToList();

			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = myRand.Next(n + 1);
				NPC value = list[k];
				list[k] = list[n];
				list[n] = value;
			}


			foreach (NPC npc1 in list)
			{
				foreach (NPC npc2 in list)
				{
					if (npc1.Name == npc2.Name)
						continue;

					if (lastKissTime >= config.MinSpouseKissInterval)
						kissingSpouses.Clear();




					float distance = Vector2.Distance(npc1.position, npc2.position);
					if (
						npc1.getSpouse() != null && npc2.getSpouse() != null  
						&& npc1.getSpouse().Name == npc2.getSpouse().Name 
						&& distance < config.MaxDistanceToKiss 
						&& !kissingSpouses.Contains(npc1.Name) 
						&& !kissingSpouses.Contains(npc2.Name) 
						&& lastKissTime > config.MinSpouseKissInterval 
						&& ModEntry.myRand.NextDouble() < config.SpouseKissChance
					)
                    {
						kissingSpouses.Add(npc1.Name);
						kissingSpouses.Add(npc2.Name);
						PMonitor.Log("spouses kissing"); 
						lastKissTime = 0;
						Vector2 npc1pos = npc1.position;
						Vector2 npc2pos = npc2.position;
						int npc1face = npc1.facingDirection;
						int npc2face = npc1.facingDirection;
						Vector2 midpoint = new Vector2((npc1.position.X + npc2.position.X) / 2, (npc1.position.Y + npc2.position.Y) / 2);
						PerformKiss(npc1, midpoint, npc2.Name);
						PerformKiss(npc2, midpoint, npc1.Name);
						DelayedAction action = new DelayedAction(1000);
						var t = Task.Run(async delegate
						{
							await Task.Delay(TimeSpan.FromSeconds(1));
							npc1.position.Value = npc1pos;
							npc2.position.Value = npc2pos;
							npc1.FacingDirection = npc1face;
							npc2.FacingDirection = npc2face;
							return;
						});
					}
				}
			}
		}

        private static void PerformKiss(NPC npc, Vector2 midpoint, string partner)
        {
			int spouseFrame = 28;
			bool facingRight = true;
			string name = npc.Name;
			if (name == "Sam")
			{
				spouseFrame = 36;
				facingRight = true;
			}
			else if (name == "Penny")
			{
				spouseFrame = 35;
				facingRight = true;
			}
			else if (name == "Sebastian")
			{
				spouseFrame = 40;
				facingRight = false;
			}
			else if (name == "Alex")
			{
				spouseFrame = 42;
				facingRight = true;
			}
			else if (name == "Krobus")
			{
				spouseFrame = 16;
				facingRight = true;
			}
			else if (name == "Maru")
			{
				spouseFrame = 28;
				facingRight = false;
			}
			else if (name == "Emily")
			{
				spouseFrame = 33;
				facingRight = false;
			}
			else if (name == "Harvey")
			{
				spouseFrame = 31;
				facingRight = false;
			}
			else if (name == "Shane")
			{
				spouseFrame = 34;
				facingRight = false;
			}
			else if (name == "Elliott")
			{
				spouseFrame = 35;
				facingRight = false;
			}
			else if (name == "Leah")
			{
				spouseFrame = 25;
				facingRight = true;
			}
			else if (name == "Abigail")
			{
				spouseFrame = 33;
				facingRight = false;
			}

			bool right = npc.position.X < midpoint.X;
			if(npc.position == midpoint)
            {
				right = String.Compare(npc.Name, partner) < 0;
			}
			else if(npc.position.X == midpoint.X)
            {
				right = npc.position.Y > midpoint.Y;
            }

			bool flip = (facingRight && !right) || (!facingRight && right);

			int offset = 24;
			if (right)
				offset *= -1;

			npc.position.Value = new Vector2(midpoint.X+offset,midpoint.Y);

			int delay = 1000;
			npc.movementPause = delay;
			npc.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(spouseFrame, delay, false, flip, new AnimatedSprite.endOfAnimationBehavior(npc.haltMe), true)
				});
			npc.doEmote(20, true);
            if (config.RealKissSound && kissEffect != null)
            {
				float distance = 1f / ((Vector2.Distance(midpoint, Game1.player.position) / 256) + 1);
				float pan = (float)(Math.Atan((midpoint.X - Game1.player.position.X) / Math.Abs(midpoint.Y - Game1.player.position.Y)) /(Math.PI/2));
				PMonitor.Log($"kiss distance: {distance} pan: {pan}");
				kissEffect.Play(distance, 0, pan);
			}
			else
            {
				Game1.currentLocation.playSound("dwop", NetAudio.SoundContext.NPC);
			}

			npc.Sprite.UpdateSourceRect();
		}

        public static void BuildSpouseRoom(FarmHouse farmHouse, string name, int count)
        {

			NPC spouse = Game1.getCharacterFromName(name);
			string back = "Back";
			string buildings = "Buildings";
			string front = "Front";
			if (spouse != null || name == "")
			{
				Map refurbishedMap;
				if (name == "")
                {
					refurbishedMap = PHelper.Content.Load<Map>("Maps\\" + farmHouse.Name + ((farmHouse.upgradeLevel == 0) ? "" : ((farmHouse.upgradeLevel == 3) ? "2" : string.Concat(farmHouse.upgradeLevel))) + "_marriage", ContentSource.GameContent);
				}
                else
                {
					refurbishedMap = PHelper.Content.Load<Map>("Maps\\spouseRooms", ContentSource.GameContent);
				}
				int indexInSpouseMapSheet = -1;
				if (name == "Sam")
				{
					indexInSpouseMapSheet = 9;
				}
				else if (name == "Penny")
				{
					indexInSpouseMapSheet = 1;
				}
				else if (name == "Sebastian")
				{
					indexInSpouseMapSheet = 5;
				}
				else if (name == "Alex")
				{
					indexInSpouseMapSheet = 6;
				}
				else if (name == "Krobus")
				{
					indexInSpouseMapSheet = 12;
				}
				else if (name == "Maru")
				{
					indexInSpouseMapSheet = 4;
				}
				else if (name == "Haley")
				{
					indexInSpouseMapSheet = 3;
				}
				else if (name == "Harvey")
				{
					indexInSpouseMapSheet = 7;
				}
				else if (name == "Shane")
				{
					indexInSpouseMapSheet = 10;
				}
				else if (name == "Abigail")
				{
					indexInSpouseMapSheet = 0;
				}
				else if (name == "Emily")
				{
					indexInSpouseMapSheet = 11;
				}
				else if (name == "Elliott")
				{
					indexInSpouseMapSheet = 8;
				}
				else if (name == "Leah")
				{
					indexInSpouseMapSheet = 2;
				}
				else if(name == "Victor" || name == "Olivia" || name == "Sophia")
                {
					back = "BackSpouse";
					buildings = "BuildingsSpouse";
					front = "FrontSpouse";

					refurbishedMap = PHelper.Content.Load<Map>($"../[TMX] Stardew Valley Expanded/assets/{name}sRoom.tmx", ContentSource.ModFolder);
					if(refurbishedMap == null)
                    {
						refurbishedMap = ModEntry.PHelper.Content.Load<Map>($"../../[TMX] Stardew Valley Expanded/assets/{name}sRoom.tmx", ContentSource.ModFolder);
					}
					if (refurbishedMap == null)
                    {
						ModEntry.PMonitor.Log($"Couldn't load spouse room for SVE spouse {name}", LogLevel.Error);
						return;
					}
					indexInSpouseMapSheet = 0;

				}


				PMonitor.Log($"Building {name}'s room", LogLevel.Debug);
				
				Microsoft.Xna.Framework.Rectangle areaToRefurbish = (farmHouse.upgradeLevel == 1) ? new Microsoft.Xna.Framework.Rectangle(36+(7*count), 1, 6, 9) : new Microsoft.Xna.Framework.Rectangle(42+(7 * count), 10, 6, 9);

				List<Layer> layers = FieldRefAccess<Map, List<Layer>>(farmHouse.map, "m_layers");
				for(int i = 0; i < layers.Count; i++)
                {
					Tile[,] tiles = FieldRefAccess<Layer, Tile[,]>(layers[i], "m_tiles");
					Size size = FieldRefAccess<Layer, Size>(layers[i], "m_layerSize");
					if (size.Width >= areaToRefurbish.X + 7)
						continue;
					size = new Size(size.Width + 7, size.Height);
					FieldRefAccess<Layer, Size>(layers[i], "m_layerSize") = size;
					FieldRefAccess<Map, List<Layer>>(farmHouse.map, "m_layers") = layers;

					Tile[,] newTiles = new Tile[tiles.GetLength(0) + 7, tiles.GetLength(1)];

					for (int k = 0; k < tiles.GetLength(0); k++)
					{
						for (int l = 0; l < tiles.GetLength(1); l++)
						{
							newTiles[k, l] = tiles[k, l];
						}
					}

					FieldRefAccess<Layer, Tile[,]>(layers[i], "m_tiles") = newTiles;
					FieldRefAccess<Layer, TileArray>(layers[i], "m_tileArray") = new TileArray(layers[i], newTiles);
				}
				FieldRefAccess<Map, List<Layer>>(farmHouse.map, "m_layers") = layers;


				Point mapReader;
				if(name == "")
                {
					mapReader = new Point(areaToRefurbish.X, areaToRefurbish.Y);
                }
				else
                {
					mapReader = new Point(indexInSpouseMapSheet % 5 * 6, indexInSpouseMapSheet / 5 * 9);
				}
				farmHouse.map.Properties.Remove("DayTiles");
				farmHouse.map.Properties.Remove("NightTiles");


				int untitled = 0;
				for (int i = 0; i < farmHouse.map.TileSheets.Count; i++)
				{
					if (farmHouse.map.TileSheets[i].Id == "untitled tile sheet")
						untitled = i;
				}


				int ox = 0;
				int oy = 0;
				if (farmHouse.upgradeLevel > 1)
				{
					ox = 6;
					oy = 9;
				}

				for (int i = 0; i < 7; i++)
				{
					farmHouse.setMapTileIndex(ox + 36 + i + (count*7), oy + 10, 165, "Front", 0);
					farmHouse.setMapTileIndex(ox + 36 + i + (count*7), oy + 11, 0, "Buildings", 0);
				}
				for (int i = 0; i < 3; i++)
				{
					farmHouse.setMapTileIndex(ox + 36 + (i * 2) + (count * 7), oy + 10, config.HallTileOdd, "Back", 0);
					farmHouse.setMapTileIndex(ox + 36 + (i * 2 + 1) + (count * 7), oy + 10, config.HallTileEven, "Back", 0);
				}
				farmHouse.setMapTileIndex(ox + 42 + (count * 7), oy + 10, config.HallTileOdd, "Back", 0);

				for (int i = 0; i < 6; i++)
				{
					farmHouse.setMapTileIndex(ox + 36 + i + (count * 7), oy + 0, 2, "Buildings", 0);
					farmHouse.setMapTileIndex(ox + 35 + (7 * count), oy + 1 + i, 99, "Buildings", untitled);
				}

				farmHouse.setMapTileIndex(ox + 35 + (7 * count), oy + 0, 87, "Buildings", untitled);
				farmHouse.setMapTileIndex(ox + 35 + (7 * count), oy + 7, 111, "Buildings", untitled);
				farmHouse.setMapTileIndex(ox + 35 + (7 * count), oy + 8, 123, "Buildings", untitled);
				farmHouse.setMapTileIndex(ox + 35 + (7 * count), oy + 9, 135, "Buildings", untitled);
				farmHouse.setMapTileIndex(ox + 35 + (7 * count), oy + 9, 54, "Back", untitled);


				for (int x = 0; x < areaToRefurbish.Width; x++)
				{
					for (int y = 0; y < areaToRefurbish.Height; y++)
					{
						//PMonitor.Log($"x {x}, y {y}", LogLevel.Debug);
						if (refurbishedMap.GetLayer(back).Tiles[mapReader.X + x, mapReader.Y + y] != null)
						{
							farmHouse.map.GetLayer("Back").Tiles[areaToRefurbish.X + x, areaToRefurbish.Y + y] = new StaticTile(farmHouse.map.GetLayer("Back"), farmHouse.map.GetTileSheet(refurbishedMap.GetLayer(back).Tiles[mapReader.X + x, mapReader.Y + y].TileSheet.Id), BlendMode.Alpha, refurbishedMap.GetLayer(back).Tiles[mapReader.X + x, mapReader.Y + y].TileIndex);
						}
						if (refurbishedMap.GetLayer(buildings).Tiles[mapReader.X + x, mapReader.Y + y] != null)
						{
							farmHouse.map.GetLayer("Buildings").Tiles[areaToRefurbish.X + x, areaToRefurbish.Y + y] = new StaticTile(farmHouse.map.GetLayer("Buildings"), farmHouse.map.GetTileSheet(refurbishedMap.GetLayer(buildings).Tiles[mapReader.X + x, mapReader.Y + y].TileSheet.Id), BlendMode.Alpha, refurbishedMap.GetLayer(buildings).Tiles[mapReader.X + x, mapReader.Y + y].TileIndex);

							typeof(GameLocation).GetMethod("adjustMapLightPropertiesForLamp", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(farmHouse, new object[] { refurbishedMap.GetLayer(buildings).Tiles[mapReader.X + x, mapReader.Y + y].TileIndex, areaToRefurbish.X + x, areaToRefurbish.Y + y, "Buildings" });
						}
						else
						{
							farmHouse.map.GetLayer("Buildings").Tiles[areaToRefurbish.X + x, areaToRefurbish.Y + y] = null;
						}
						if (y < areaToRefurbish.Height - 1 && refurbishedMap.GetLayer(front).Tiles[mapReader.X + x, mapReader.Y + y] != null)
						{
							farmHouse.map.GetLayer("Front").Tiles[areaToRefurbish.X + x, areaToRefurbish.Y + y] = new StaticTile(farmHouse.map.GetLayer("Front"), farmHouse.map.GetTileSheet(refurbishedMap.GetLayer(front).Tiles[mapReader.X + x, mapReader.Y + y].TileSheet.Id), BlendMode.Alpha, refurbishedMap.GetLayer(front).Tiles[mapReader.X + x, mapReader.Y + y].TileIndex);
							typeof(GameLocation).GetMethod("adjustMapLightPropertiesForLamp", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(farmHouse, new object[] { refurbishedMap.GetLayer(front).Tiles[mapReader.X + x, mapReader.Y + y].TileIndex, areaToRefurbish.X + x, areaToRefurbish.Y + y, "Front" });
						}
						else if (y < areaToRefurbish.Height - 1)
						{
							farmHouse.map.GetLayer("Front").Tiles[areaToRefurbish.X + x, areaToRefurbish.Y + y] = null;
						}
						if (x == 4 && y == 4)
						{
                            try
                            {
								farmHouse.map.GetLayer("Back").Tiles[areaToRefurbish.X + x, areaToRefurbish.Y + y].Properties["NoFurniture"] = new PropertyValue("T");
							}
                            catch(Exception ex)
                            {
								PMonitor.Log(ex.ToString());
                            }
						}
					}
				}
			}
		}
	}
}