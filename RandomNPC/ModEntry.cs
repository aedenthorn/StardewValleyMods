using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using xTile.Layers;
using xTile.Tiles;

namespace RandomNPC
{
	/// <summary>The mod entry point.</summary>
	public class ModEntry : Mod, IAssetEditor, IAssetLoader
	{
		internal static ModConfig Config { get; private set; }
		internal DialogueData RNPCdialogueData { get; private set; }
		internal ModData RNPCengagementDialogueStrings { get; private set; }
		internal ModData RNPCgiftDialogueStrings { get; private set; }
		internal ModData RNPCscheduleStrings { get; private set; }
		internal ModData RNPCfemaleNameStrings { get; private set; }
		internal ModData RNPCmaleNameStrings { get; private set; }
		internal ModData RNPCbodyTypes { get; private set; }
		internal ModData RNPCdarkSkinColours { get; private set; }
		internal ModData RNPClightSkinColours { get; private set; }
		internal ModData RNPChairStyles { get; private set; }
		internal ModData RNPCnaturalHairColours { get; private set; }
		internal ModData RNPCdarkHairColours { get; private set; }
		internal ModData RNPCexoticHairColours { get; private set; }
		internal ModData RNPCclothes { get; private set; }
		internal ModData RNPCskinColours { get; private set; }
		internal ModData RNPCsavedNPCs { get; private set; }

		private List<RNPCSchedule> RNPCSchedules = new List<RNPCSchedule>();
		public int RNPCMaxVisitors { get; private set; }

		private bool droveOff = false;
		public List<RNPC> RNPCs = new List<RNPC>();
		private bool drivingOff;



		/*********
        ** Public methods
        *********/
		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			Config = this.Helper.ReadConfig<ModConfig>();

			this.RNPCdialogueData = this.Helper.Data.ReadJsonFile<DialogueData>("assets/dialogue.json") ?? new DialogueData();
			this.RNPCengagementDialogueStrings = this.Helper.Data.ReadJsonFile<ModData>("assets/engagement_dialogues.json") ?? new ModData();
			this.RNPCgiftDialogueStrings = this.Helper.Data.ReadJsonFile<ModData>("assets/gift_dialogues.json") ?? new ModData();

			this.RNPCscheduleStrings = this.Helper.Data.ReadJsonFile<ModData>("assets/schedules.json") ?? new ModData();

			this.RNPCfemaleNameStrings = this.Helper.Data.ReadJsonFile<ModData>("assets/female_names.json") ?? new ModData();
			this.RNPCmaleNameStrings = this.Helper.Data.ReadJsonFile<ModData>("assets/male_names.json") ?? new ModData();

			this.RNPCbodyTypes = this.Helper.Data.ReadJsonFile<ModData>("assets/body_types.json") ?? new ModData();
			this.RNPCdarkSkinColours = this.Helper.Data.ReadJsonFile<ModData>("assets/dark_skin_colours.json") ?? new ModData();
			this.RNPClightSkinColours = this.Helper.Data.ReadJsonFile<ModData>("assets/light_skin_colours.json") ?? new ModData();

			this.RNPChairStyles = this.Helper.Data.ReadJsonFile<ModData>("assets/hair_styles.json") ?? new ModData();
			this.RNPCnaturalHairColours = this.Helper.Data.ReadJsonFile<ModData>("assets/natural_hair_sets.json") ?? new ModData();
			this.RNPCdarkHairColours = this.Helper.Data.ReadJsonFile<ModData>("assets/dark_hair_sets.json") ?? new ModData();
			this.RNPCexoticHairColours = this.Helper.Data.ReadJsonFile<ModData>("assets/exotic_hair_sets.json") ?? new ModData();

			this.RNPCclothes = this.Helper.Data.ReadJsonFile<ModData>("assets/clothes.json") ?? new ModData();

			this.RNPCMaxVisitors = Math.Min(24, Math.Min(Config.RNPCTotal, Config.RNPCMaxVisitors));

			this.RNPCsavedNPCs = this.Helper.Data.ReadJsonFile<ModData>("saved_npcs.json") ?? new ModData();
			while (RNPCsavedNPCs.data.Count < Config.RNPCTotal)
			{
				RNPCsavedNPCs.data.Add(GenerateNPCString());
			}
			RNPCsavedNPCs.data = RNPCsavedNPCs.data.Take(Config.RNPCTotal).ToList();

			this.Helper.Data.WriteJsonFile<ModData>("saved_npcs.json", RNPCsavedNPCs);

			for (int i = 0; i <RNPCsavedNPCs.data.Count; i++)
			{
				string npc = RNPCsavedNPCs.data[i];
				this.RNPCs.Add(new RNPC(npc, "RNPC" + i));
			}

			// shuffle for visitors

			RNPCs = RNPCs.OrderBy(n => Guid.NewGuid()).ToList();

			for (int i = 0; i < RNPCs.Count; i++)
			{
				RNPCs[i].startLoc = "BusStop " + (13 + (i % 6)) + " " + (11 + i / 6);
				if (i < RNPCMaxVisitors)
				{
					RNPCs[i].visiting = true;
				}
				else
				{
					RNPCs[i].visiting = false;
				}
			}

			helper.Events.GameLoop.ReturnedToTitle += this.ReturnedToTitle;
			helper.Events.GameLoop.DayEnding += this.DayEnding;
			helper.Events.GameLoop.DayStarted += this.DayStarted;
			//helper.Events.GameLoop.TimeChanged += this.TimeChanged;
			helper.Events.GameLoop.OneSecondUpdateTicked += this.OneSecondUpdateTicked;
			helper.Events.GameLoop.UpdateTicked += this.UpdateTicked;


		}

		private void UpdateTicked(object sender, UpdateTickedEventArgs e)
		{
			if (drivingOff && !droveOff) {
				FieldInfo pos = typeof(BusStop).GetField("busPosition", BindingFlags.NonPublic | BindingFlags.Instance);
				BusStop bs = (BusStop)Game1.getLocationFromName("BusStop");
				if (((Vector2)pos.GetValue(bs)).X + 512f >= 0f)
				{
					FieldInfo mot = typeof(BusStop).GetField("busMotion", BindingFlags.NonPublic | BindingFlags.Instance);
					mot.SetValue(bs, new Vector2(((Vector2)mot.GetValue(bs)).X - 0.075f, ((Vector2)mot.GetValue(bs)).Y));
				}
				else
				{
					droveOff = true;
					drivingOff = false;
				}
			}
		}

		private void TimeChanged(object sender, TimeChangedEventArgs e)
		{
			if(Game1.timeOfDay < 700)
			{
				foreach (RNPCSchedule schedule in RNPCSchedules)
				{
					RNPC rnpc = schedule.npc;
					if (rnpc.visiting && !rnpc.inTown && schedule.startM == (Game1.timeOfDay-600)/10)
					{

					}
				}
			}
		}

		private void OneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
		{
			if (Game1.timeOfDay >= Config.LeaveTime && !droveOff && !drivingOff)
			{
				int gone = 0;
				foreach (NPC npc in Game1.getLocationFromName("BusStop").characters)
				{
					if (npc.getTileLocation().X == 0 && npc.getTileLocation().Y == 0)
					{
						gone++;
					}
					if (npc.getTileLocation().X == 12 && npc.getTileLocation().Y == 9)
					{
						foreach (RNPC rnpc in RNPCs)
						{
							if (npc.name.Equals(rnpc.nameID))
							{
								npc.IsInvisible = true;
								Game1.warpCharacter(npc, "BusStop", new Vector2(0, 0));
							}
						}
					}
				}

				if (!drivingOff && gone == RNPCs.Count)
				{
					//Alert("Driving off");
					drivingOff = true;
					FieldInfo door = typeof(BusStop).GetField("busDoor", BindingFlags.NonPublic | BindingFlags.Instance);
					FieldInfo pos = typeof(BusStop).GetField("busPosition", BindingFlags.NonPublic | BindingFlags.Instance);
					FieldInfo mot = typeof(BusStop).GetField("busMotion", BindingFlags.NonPublic | BindingFlags.Instance);
					BusStop bs = (BusStop)Game1.getLocationFromName("BusStop");
					door.SetValue(bs, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(288, 1311, 16, 38), (Vector2)pos.GetValue(bs) + new Vector2(16f, 26f) * 4f, false, 0f, Color.White)
					{
						interval = 999999f,
						animationLength = 6,
						holdLastFrame = true,
						layerDepth = (((Vector2)pos.GetValue(bs)).Y + 192f) / 10000f + 1E-05f,
						scale = 4f
					});
					((TemporaryAnimatedSprite)door.GetValue(bs)).timer = 0f;
					((TemporaryAnimatedSprite)door.GetValue(bs)).interval = 70f;
					((TemporaryAnimatedSprite)door.GetValue(bs)).endFunction = new TemporaryAnimatedSprite.endBehavior(delegate
					{
						bs.localSound("batFlap");
						bs.localSound("busDriveOff");
					});
					bs.localSound("trashcanlid");
					((TemporaryAnimatedSprite)door.GetValue(bs)).paused = false;

					for (int i = 11; i < 19; i++)
					{
						for (int j = 7; j < 10; j++)
						{
							if (i == 12 && j == 9)
								continue;
							bs.removeTile(i, j, "Buildings");
							//bs.setTileProperty(i, j, "Buildings", "Passable", "T");
						}
					}

				}
			}
		}

		private void ReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
		{
			// shuffle for visitors

			RNPCs = RNPCs.OrderBy(n => Guid.NewGuid()).ToList();

			for (int i = 0; i < RNPCs.Count; i++)
			{
				RNPCs[i].startLoc = "BusStop " + (13 + (i % 6)) + " " + (11 + i / 6);
				if (i < RNPCMaxVisitors)
				{
					RNPCs[i].visiting = true;
				}
				else
				{
					RNPCs[i].visiting = false;
				}
				this.Helper.Content.InvalidateCache("Characters/schedules/" + RNPCs[i].nameID);
			}
			this.Helper.Content.InvalidateCache("Data/NPCDispositions");
		}

		private void DayEnding(object sender, DayEndingEventArgs e)
		{
			//reset bus

			drivingOff = false;
			droveOff = false;
			BusStop bs = (BusStop)Game1.getLocationFromName("BusStop");
			Layer layer = bs.map.GetLayer("Buildings");
			for (int i = 11; i < 19; i++)
			{
				for (int j = 8; j < 10; j++)
				{
					if (i == 12 && j == 9)
						continue;

					TileSheet tilesheet = bs.map.GetTileSheet("outdoors");
					int index = j == 8 ? 1056 : 1054;
					layer.Tiles[i, j] = new StaticTile(layer, tilesheet, BlendMode.Alpha, tileIndex: index);
				}
			}

			// shuffle for visitors
			RNPCs = RNPCs.OrderBy(n => Guid.NewGuid()).ToList();

			for (int i = 0; i < RNPCs.Count; i++)
			{
				RNPCs[i].startLoc = "BusStop " + (13 + (i % 6)) + " " + (11 + i / 6);
				if (i < RNPCMaxVisitors)
				{
					RNPCs[i].visiting = true;
				}
				else
				{
					RNPCs[i].visiting = false;
				}
				this.Helper.Content.InvalidateCache("Characters/schedules/" + RNPCs[i].nameID);
			}
			this.Helper.Content.InvalidateCache("Data/NPCDispositions");
		}

		private void DayStarted(object sender, DayStartedEventArgs e)
		{
			foreach (GameLocation l in Game1.locations)
			{
				if (l.GetType() == typeof(BusStop))
				{
					foreach (NPC npc in l.getCharacters())
					{
						for(int i = 0; i < RNPCs.Count; i++)
						{
							RNPC rnpc = RNPCs[i];
							if(rnpc.nameID == npc.name && !rnpc.visiting) 
							{
								Game1.warpCharacter(npc, "BusStop", new Vector2(0, 0));
								npc.IsInvisible = true;
								
							}
							else
							{
								l.getCharacterFromName(npc.name).faceDirection(2);
								npc.IsInvisible = false;
							}
						}
					}
				}
			}
		}


		/// <summary>Get whether this instance can edit the given asset.</summary>
		/// <param name="asset">Basic metadata about the asset being loaded.</param>
		public bool CanEdit<T>(IAssetInfo asset)
		{
			if (asset.AssetNameEquals("Data/NPCDispositions") || asset.AssetNameEquals("Data/NPCGiftTastes") || asset.AssetNameEquals("Characters/EngagementDialogue"))
			{
				//base.Monitor.Log("Can load: " + asset.AssetName, LogLevel.Alert);
				return true;
			}

			return false;
		}

		/// <summary>Edit a matched asset.</summary>
		/// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it.</param>
		public void Edit<T>(IAssetData asset)
		{
			if (asset.AssetNameEquals("Data/NPCDispositions"))
			{

				IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;

				foreach(RNPC npc in RNPCs)
				{
					data[npc.nameID] = npc.MakeDisposition();
				}
			}
			else if (asset.AssetNameEquals("Data/NPCGiftTastes"))
			{

				IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;

				foreach(RNPC npc in RNPCs)
				{
					data[npc.nameID] = MakeGiftDialogue(npc);
				}
			}
			else if (asset.AssetNameEquals("Characters/EngagementDialogue"))
			{
				IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;

				foreach(RNPC npc in RNPCs)
				{
					string[] str = MakeEngagementDialogue(npc);
					data[npc.nameID + "0"] = str[0];
					data[npc.nameID + "1"] = str[1];
				}
			}
		}

		private string[] MakeEngagementDialogue(RNPC npc)
		{
			List<string[]> potentialDialogues = new List<string[]>();
			List<string> potentialDialogue = new List<string>();
			potentialDialogue = GetHighestRankedStrings(npc.npcString, RNPCengagementDialogueStrings.data,7);
			for (int j = 0; j < potentialDialogue.Count; j++)
			{
				potentialDialogues.Add(potentialDialogue[j].Split('/'));
			}

			string[] output = potentialDialogues[Game1.random.Next(0,potentialDialogues.Count)];
			return output;
		}

		private string MakeGiftDialogue(RNPC npc)
		{
			List<string> potentialDialogue = new List<string>();
			potentialDialogue = GetHighestRankedStrings(npc.npcString,RNPCgiftDialogueStrings.data,7);
			for(int j = 0; j < potentialDialogue.Count; j++)
			{
				string str = potentialDialogue[j];
				string[] tastes = str.Split('^');
				for (int i = 0; i < tastes.Length; i++)
				{
					tastes[i] += "/" + npc.giftTaste[i];
				}
				potentialDialogue[j] = String.Join("/", tastes);
			}

			return potentialDialogue[Game1.random.Next(0,potentialDialogue.Count)];
		}


		/// <summary>Get whether this instance can load the initial version of the given asset.</summary>
		/// <param name="asset">Basic metadata about the asset being loaded.</param>
		public bool CanLoad<T>(IAssetInfo asset)
		{
			foreach (RNPC npc in RNPCs)
			{
				if (asset.AssetNameEquals("Portraits/"+ npc.nameID) || asset.AssetNameEquals("Characters/" + npc.nameID) || asset.AssetNameEquals("Characters/Dialogue/" + npc.nameID) || asset.AssetNameEquals("Characters/schedules/" + npc.nameID))
				{
					return true;
				}

			}

			return false;
		}

		/// <summary>Load a matched asset.</summary>
		/// <param name="asset">Basic metadata about the asset being loaded.</param>
		public T Load<T>(IAssetInfo asset)
		{

			foreach (RNPC npc in RNPCs)
			{
				if (asset.AssetNameEquals("Portraits/" + npc.nameID))
				{
					Texture2D texture = CreateCustomCharacter(npc, "portrait");
						
					return (T)(object) texture;
				}
				else if (asset.AssetNameEquals("Characters/" + npc.nameID))
				{
					Texture2D texture = CreateCustomCharacter(npc, "character");

					return (T)(object) texture;
				}
				else if (asset.AssetNameEquals("Characters/schedules/" + npc.nameID))
				{
					return (T)(object) MakeSchedule(npc);
				}
				else if (asset.AssetNameEquals("Characters/Dialogue/" + npc.nameID))
				{
					return (T)(object) MakeDialogue(npc);
				}

			}

			throw new InvalidOperationException($"Unexpected asset '{asset.AssetName}'.");
		}

		private Dictionary<string,string> MakeDialogue(RNPC rnpc)
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			List<string> intros = GetHighestRankedStrings(rnpc.npcString,RNPCdialogueData.introductions,7);
			data.Add("Introduction", intros[Game1.random.Next(0, intros.Count)]);

			// make dialogue

			string[] dow = { "Mon","Tue","Wed","Thu","Fri","Sat","Sun" };

			NPC npc = Game1.getCharacterFromName(rnpc.nameID);
			int hearts = Game1.player.getFriendshipHeartLevelForNPC(rnpc.nameID);

			if (!Config.RequireHeartsForDialogue)
			{
				hearts = 10;
			}

			string question = GetRandomDialogue(rnpc, RNPCdialogueData.questions);
			List<string> farmerQuestions = RNPCdialogueData.farmer_questions;
			List<string> rejections = GetHighestRankedStrings(rnpc.npcString, RNPCdialogueData.rejections,7); 

			List<string> questa = new List<string>();

			string questionString = "$q 4242 question_asked#" + question;

			int fqi = 0;

			questionString += "#$r " + (Game1.Date.TotalDays - 1) + " 0 fquest_" + (fqi) + "#" + farmerQuestions[fqi];
			if (hearts >=2) // allow asking about personality
			{
				string manner = GetRandomDialogue(rnpc,RNPCdialogueData.manner);
				string anxiety = GetRandomDialogue(rnpc,RNPCdialogueData.anxiety);
				string optimism = GetRandomDialogue(rnpc, RNPCdialogueData.optimism);

				string infoResponse = GetRandomDialogue(rnpc, RNPCdialogueData.info_responses);

				data.Add("fquest_"+(fqi), infoResponse.Replace("$name",rnpc.name).Replace("$manner",manner).Replace("$anxiety",anxiety).Replace("$optimism",optimism));
			}
			else
			{
				data.Add("fquest_" + (fqi),rejections[Game1.random.Next(0, rejections.Count)]);
			}
			fqi++;

			questionString += "#$r 4343 0 fquest_" + (fqi) + "#" + farmerQuestions[fqi];
			if (hearts >=4) // allow asking about plans
			{
				string morning = "THIS IS AN ERROR";
				string afternoon = "THIS IS AN ERROR";
				foreach (RNPCSchedule schedule in RNPCSchedules)
				{
					if(schedule.npc.nameID == rnpc.nameID)
					{
						morning = GetRandomDialogue(rnpc, RNPCdialogueData.places[schedule.morningLoc.Split(' ')[0]]);
						afternoon = GetRandomDialogue(rnpc, RNPCdialogueData.places[schedule.afternoonLoc.Split(' ')[0]]);
						break;
					}
				}
				string scheduleDialogue = GetRandomDialogue(rnpc, RNPCdialogueData.schedules);
				data.Add("fquest_" + (fqi), scheduleDialogue.Replace("@", morning).Replace("#", afternoon));
			}
			else
			{
				data.Add("fquest_" + (fqi), rejections[Game1.random.Next(0, rejections.Count)]);
			}
			fqi++;

			questionString += "#$r 4343 0 fquest_" + (fqi) + "#" + farmerQuestions[fqi];
			if (hearts >= 6) // allow asking about advice
			{
				string advice = GetRandomDialogue(rnpc, RNPCdialogueData.advice);
				data.Add("fquest_" + (fqi), advice);
			}
			else
			{
				data.Add("fquest_" + (fqi), rejections[Game1.random.Next(0, rejections.Count)]);
			}
			fqi++;

			questionString += "#$r 4343 0 fquest_" + (fqi) + "#" + farmerQuestions[fqi];
			if (hearts >=6) // allow asking about help
			{

				string quest = GetRandomDialogue(rnpc, RNPCdialogueData.quests);
				string questq = "$q 4242 questq_answered#" + quest.Split('^')[0];
				string questRight = GetRandomDialogue(rnpc, RNPCdialogueData.questRight);
				string questWrong = GetRandomDialogue(rnpc, RNPCdialogueData.questWrong);
				string questUnknown = GetRandomDialogue(rnpc, RNPCdialogueData.questUnknown);

				questa = quest.Split('^')[1].Split('|').ToList();
				questa = questa.OrderBy(n => Guid.NewGuid()).ToList();

				for(int i = 0; i < questa.Count; i++)
				{
					questq += "#$r 4343 " + questa[i];
				}

				data.Add("fquest_" + (fqi), questq);

				data.Add("quest_right", questRight);
				data.Add("quest_wrong", questWrong);
				data.Add("quest_unknown", questUnknown);
			}
			else
			{
				data.Add("fquest_" + (fqi), rejections[Game1.random.Next(0, rejections.Count)]);
			}
			fqi++;

			if (!npc.datingFarmer)
			{
				questionString += "#$r 4343 0 fquest_" + (fqi) + "#" + farmerQuestions[fqi];
				if (hearts >= 8) // allow asking about datability
				{
					string datable = GetRandomDialogue(rnpc, RNPCdialogueData.datable);

					data.Add("fquest_" + (fqi), npc.datable ? datable.Split('^')[0] : datable.Split('^')[1]);
				}
				else
				{
					data.Add("fquest_" + (fqi), rejections[Game1.random.Next(0, rejections.Count)]);
				}
			}

			//base.Monitor.Log(questionString, LogLevel.Alert);

			foreach(string d in dow)
			{
				data.Add(d, questionString);
			}

			return data;
		}

		private void Alert(string alert)
		{
			base.Monitor.Log(alert, LogLevel.Alert);	
		}

		private string GetRandomDialogue(RNPC rnpc, List<string> dialogues)
		{
			List<string> potStrings = GetHighestRankedStrings(rnpc.npcString, dialogues,7);
			return potStrings[Game1.random.Next(0,potStrings.Count)];
		}

		private Dictionary<string, string> MakeSchedule(RNPC npc)
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			RNPCSchedule schedule = new RNPCSchedule(npc);

			if (!npc.visiting)
			{
				//base.Monitor.Log(npc.nameID + " at "+ npc.startLoc+" is not visiting", LogLevel.Alert);

				data.Add("spring", "");
				return data;
			}

			string[] morning = MakeRandomAppointment(npc, "morning");
			schedule.morningEarliest = morning[0];
			schedule.morningLoc = morning[1];
			string[] afternoon = MakeRandomAppointment(npc, schedule.morningLoc);
			schedule.afternoonEarliest = afternoon[0];
			schedule.afternoonLoc = afternoon[1];

			RNPCSchedules.Add(schedule);

			string sstr = schedule.MakeString();

			Alert(npc.nameID + " at " + npc.startLoc + " " + sstr);

			data.Add("spring", sstr);
			return data;
		}

		private string[] MakeRandomAppointment(RNPC npc, string morning)
		{
			List<string[]> potentialApps = new List<string[]>();
			List<string> fitApps = GetHighestRankedStrings(npc.npcString,RNPCscheduleStrings.data,7);

			foreach (string appset in fitApps)
			{
				string time = appset.Split('^')[0];
				string place = appset.Split('^')[1];
				string[] locs = appset.Split('^')[2].Split('#');
				if (time == "any" || morning != "morning" || int.Parse(time) < 1100)
				{
					foreach (string loc in locs)
					{
						string thisApp = place + " " + loc;
						if (thisApp != morning)
						{
							bool taken = false;
							foreach (RNPCSchedule schedule in RNPCSchedules)
							{
								if ((morning == "morning" && schedule.morningLoc == thisApp) || (morning != "morning" && schedule.afternoonLoc == thisApp))
								{
									taken = true;
									break;
								}
							}
							if(!taken)
							{
								potentialApps.Add(new string[]{time,thisApp});
							}
						}
					}
				}
			}

			return potentialApps[Game1.random.Next(0, potentialApps.Count)];

		}

		private Texture2D CreateCustomCharacter(RNPC npc, string type)
		{
			Texture2D sprite = this.Helper.Content.Load<Texture2D>("assets/body/" + npc.bodyType + "_"+type+".png", ContentSource.ModFolder);
			Texture2D hairT = this.Helper.Content.Load<Texture2D>("assets/hair/" + npc.hairStyle + "_" + type + ".png", ContentSource.ModFolder);
			Texture2D eyeT = this.Helper.Content.Load<Texture2D>("assets/body/" + npc.gender + "_eyes_" + type + ".png", ContentSource.ModFolder);
			
			Texture2D eyeBackT = null;
			Texture2D noseT = null;
			Texture2D mouthT = null;
			if (type == "portrait")
			{
				eyeBackT = this.Helper.Content.Load<Texture2D>("assets/body/" + npc.gender + "_eyes_back.png", ContentSource.ModFolder);
				noseT = this.Helper.Content.Load<Texture2D>("assets/body/" + npc.gender + "_nose.png", ContentSource.ModFolder);
				mouthT = this.Helper.Content.Load<Texture2D>("assets/body/" + npc.gender + "_mouth.png", ContentSource.ModFolder);
			}
			Texture2D topT = this.Helper.Content.Load<Texture2D>("assets/transparent_" + type + ".png", ContentSource.ModFolder);
			Texture2D bottomT = topT;
			Texture2D shoesT = topT;

			// clothes

			// try and share with other type (char/portrait)
			string[] clothes;
			if (npc.clothes != null)
			{
				clothes = npc.clothes;
			}
			else
			{
				string npcString = string.Join("/", npc.npcString.Split('/').Take(7)) + "/" + npc.bodyType;
				List<string> potentialClothes = GetHighestRankedStrings(npcString,RNPCclothes.data,8);

				clothes = potentialClothes[Game1.random.Next(0, potentialClothes.Count)].Split('^');
				npc.clothes = clothes;
				npc.topRandomColour = new string[] { Game1.random.Next(0, 255).ToString(), Game1.random.Next(0, 255).ToString(), Game1.random.Next(0, 255).ToString() };
			}

			if (clothes[0] != "")
			{
				topT = this.Helper.Content.Load<Texture2D>("assets/clothes/" + clothes[0]+ "_" + type + ".png", ContentSource.ModFolder);
			}
			if(clothes[1] != "" && type == "character")
			{
				bottomT = this.Helper.Content.Load<Texture2D>("assets/clothes/" + clothes[1] + ".png", ContentSource.ModFolder);
			}
			if(clothes[2] != "" && type == "character")
			{
				shoesT = this.Helper.Content.Load<Texture2D>("assets/clothes/" + clothes[2]+ ".png", ContentSource.ModFolder);
			}

			Color[] data = new Color[sprite.Width * sprite.Height];
			Color[] dataH = new Color[sprite.Width * sprite.Height];
			Color[] dataE = new Color[sprite.Width * sprite.Height];
			Color[] dataEB = new Color[sprite.Width * sprite.Height];
			Color[] dataN = new Color[sprite.Width * sprite.Height];
			Color[] dataM = new Color[sprite.Width * sprite.Height];
			Color[] dataT = new Color[sprite.Width * sprite.Height];
			Color[] dataB = new Color[sprite.Width * sprite.Height];
			Color[] dataS = new Color[sprite.Width * sprite.Height];
			sprite.GetData(data);
			hairT.GetData(dataH);
			eyeT.GetData(dataE);
			if (type == "portrait")
			{
				eyeBackT.GetData(dataEB);
				noseT.GetData(dataN);
				mouthT.GetData(dataM);
			}
			topT.GetData(dataT);
			bottomT.GetData(dataB);
			shoesT.GetData(dataS);

			string[] skinRBG = npc.skinColour.Split(' ');
			string[] eyeRBG = npc.eyeColour.Split(' ');
			List<string> hairRBGs = npc.hairColour.Split('^').ToList();
			
			string[] baseColourT = clothes[3] == "any" ? npc.topRandomColour : null;
			
			string[] baseColourB;
			switch (clothes[4])
			{
				case "any":
					baseColourB = new string[] { Game1.random.Next(0, 255).ToString(), Game1.random.Next(0, 255).ToString(), Game1.random.Next(0, 255).ToString() };
					break;
				case "top":
					baseColourB = baseColourT;
					break;
				default:
					baseColourB = null;
					break;
			}
			string[] baseColourS;
			switch (clothes[5])
			{
				case "any":
					baseColourS = new string[] { Game1.random.Next(0, 255).ToString(), Game1.random.Next(0, 255).ToString(), Game1.random.Next(0, 255).ToString() };
					break;
				case "top":
					baseColourS = baseColourT;
					break;
				case "bottom":
					baseColourS = baseColourB;
					break;
				default:
					baseColourS = null;
					break;
			}

			// make hair gradient

			List<byte> hairGreys = new List<byte>();
			for (int i = 0; i < data.Length; i++)
			{
				if (dataH[i].R == dataH[i].G && dataH[i].R == dataH[i].B && dataH[i].G == dataH[i].B) // greyscale
				{
					if(!hairGreys.Contains(dataH[i].R)) { // only add one of each grey
						hairGreys.Add(dataH[i].R);
					}
				}
			}

			hairGreys.Sort();
			hairGreys.Reverse(); // lightest to darkest

			// make same number of greys as colours in gradient

			if (hairRBGs.Count > hairGreys.Count) // ex 9 and 6
			{
				hairGreys = LengthenToMatch<byte,string>(hairGreys, hairRBGs);
			}
			else if (hairRBGs.Count < hairGreys.Count)
			{
				hairRBGs = LengthenToMatch<string,byte>(hairRBGs, hairGreys);

			}

			// start putting it together

			for (int i = 0; i < data.Length; i++)
			{
				if (dataH[i] != Color.Transparent)
				{
					if (dataH[i].R == dataH[i].G && dataH[i].R == dataH[i].B && dataH[i].G == dataH[i].B) // greyscale
					{
						// hair gradient

						// for cases where more greys than colours
						List<int> greyMatches = new List<int>();
						for(int j = 0; j < hairGreys.Count; j++)
						{
							if(hairGreys[j] == dataH[i].R)
							{
								greyMatches.Add(j);
							}

						}

						string[] hairRBG;
						hairRBG = hairRBGs[greyMatches[Game1.random.Next(0,greyMatches.Count)]].Split(' '); // turns single grey into set of colours

						data[i] = new Color(byte.Parse(hairRBG[0]), byte.Parse(hairRBG[1]), byte.Parse(hairRBG[2]), 255);
					}
					else // ignore already coloured parts
					{
						data[i] = dataH[i];
					}
				}
				else if(dataT[i] != Color.Transparent)
				{
					data[i] = baseColourT != null ? ColorizeGrey(baseColourT, dataT[i]) : dataT[i];
				}
				else if(dataB[i] != Color.Transparent)
				{
					data[i] = baseColourB != null ? ColorizeGrey(baseColourB, dataB[i]) : dataB[i];
				}
				else if(dataS[i] != Color.Transparent)
				{
					data[i] = baseColourS != null ? ColorizeGrey(baseColourS, dataS[i]) : dataS[i];
				}
				else if (dataE[i] != Color.Transparent)
				{
					if (dataE[i] != Color.White)
					{
						data[i] = ColorizeGrey(eyeRBG, dataE[i]);
					}
					else
					{
						data[i] = Color.White;
					}
				}
				else if (dataEB[i] != Color.Transparent)
				{
					data[i] = ColorizeGrey(skinRBG, dataEB[i]);
				}
				else if (dataN[i] != Color.Transparent)
				{
					data[i] = ColorizeGrey(skinRBG, dataN[i]);
				}
				else if (dataM[i] != Color.Transparent)
				{
					data[i] = ColorizeGrey(skinRBG, dataM[i]);
				}
				else if (data[i] != Color.Transparent)
				{
					data[i] = ColorizeGrey(skinRBG, data[i]);
				}
			}
			sprite.SetData<Color>(data);
			return sprite;
		}

		private Color ColorizeGrey(string[] baseColour, Color greyMap)
		{
			if(greyMap.R == greyMap.G && greyMap.R == greyMap.B && greyMap.G == greyMap.B) // not greyscale
			{
				return greyMap;
			}
			//base.Monitor.Log(string.Join("", baseColour), LogLevel.Alert);
			Color outColour = new Color();
			outColour.R = (byte)(greyMap.R - Math.Round((255 - double.Parse(baseColour[0])) * greyMap.R / 255));
			outColour.G = (byte)(greyMap.G - Math.Round((255 - double.Parse(baseColour[1])) * greyMap.G / 255));
			outColour.B = (byte)(greyMap.B - Math.Round((255 - double.Parse(baseColour[2])) * greyMap.B / 255));
			outColour.A = 255;
			return outColour;
		}

		private string GenerateNPCString() 
		{
			string npcstring = "";

			// age
			//string[] ages = { "child", "teen", "adult" };
			string[] ages = { "teen", "adult" };
			string age = GetRandomFromDist(ages,Config.AgeDist);

			// manners
			string[] manners = { "polite", "rude", "neutral" };
			string manner = manners[Game1.random.Next(0, manners.Length)];

			// social anxiety
			string[] anxieties = { "outgoing", "shy", "neutral" };
			string anxiety = anxieties[Game1.random.Next(0, anxieties.Length)];

			// optimism
			string[] optimisms = { "positive", "negative", "neutral" };
			string optimism = optimisms[Game1.random.Next(0, optimisms.Length)];

			// gender
			double female = Config.FemaleChance;

			string gender = Game1.random.NextDouble() < female ? "female" : "male";

			// datable
			double datableChance = Config.DatableChance;
			string datable = Game1.random.NextDouble() < datableChance ? "datable" : "non-datable";

			// traits
			string traits = "none"; // not used yet

			// birthday
			string[] seasons = { "spring", "summer", "fall", "winter" };
			string season = seasons[Game1.random.Next(0, seasons.Length)];
			int day = Game1.random.Next(1, 29);
			string birthday = season + " " + day;

			//name

			string name = "";
			bool freename = false;
			while (freename == false)
			{
				string firstName = (gender == "female" ? RNPCfemaleNameStrings.data[Game1.random.Next(0, RNPCfemaleNameStrings.data.Count)] : RNPCmaleNameStrings.data[Game1.random.Next(0, RNPCmaleNameStrings.data.Count)]);

				name = firstName;

				if (RNPCsavedNPCs.data.Count == 0)
					freename = true;
				bool thisfreename = true;
				for (int i = 0; i < RNPCsavedNPCs.data.Count; i++)
				{
					if (RNPCsavedNPCs.data[i].Split('/')[11] == name)
					{
						thisfreename = false;
						break;
					}
				}
				if (thisfreename)
					freename = true;
			}
			TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
			name = textInfo.ToTitleCase(name.ToLower());

			// gift taste

			string giftTaste = "^^^^";

			// body type

			List<string> potentialBodyTypes = new List<string>();
			foreach(string body in RNPCbodyTypes.data)
			{
				string[] ba = body.Split('/');
				if ((ba[0] == "any" || ba[0] == age || ba[0].Split('|').Contains(age)) && (ba[1] == "any" || ba[1] == gender || ba[1].Split('|').Contains(gender)))
				{
					potentialBodyTypes.Add(ba[2]);
				}
			}
			string bodyType = potentialBodyTypes[Game1.random.Next(0, potentialBodyTypes.Count)];

			// skin colour

			string skinColour = (Game1.random.NextDouble() < Config.LightSkinChance ? RNPClightSkinColours.data[Game1.random.Next(0, RNPClightSkinColours.data.Count)] : RNPCdarkSkinColours.data[Game1.random.Next(0, RNPCdarkSkinColours.data.Count)]);

			// hair style

			List<string> potentialHairStyles = new List<string>();
			foreach (string style in RNPChairStyles.data)
			{
				string[] ba = style.Split('/');
				if ((ba[0] == "any" || ba[0] == age || ba[0].Split('|').Contains(age))&& (ba[1] == "any" || ba[1] == gender) && (ba[2] == "any" || ba[2] == manner || ba[2].Split('|').Contains(manner)) && (ba[3] == "any" || SharesItems(traits.Split('|'),ba[3].Split('|'))))
				{
					potentialHairStyles.Add(ba[4]);
				}
			}
			string hairStyle = potentialHairStyles[Game1.random.Next(0, potentialHairStyles.Count)];

			// hair colour

			string hairColour;
			if(int.Parse(skinColour.Split(' ')[0]) < 150 && Config.DarkSkinDarkHair)
			{
				hairColour = RNPCdarkHairColours.data[Game1.random.Next(0, RNPCdarkHairColours.data.Count)];
			}
			else if (Game1.random.NextDouble() < Config.NaturalHairChance)
			{
				hairColour = RNPCnaturalHairColours.data[Game1.random.Next(0, RNPCnaturalHairColours.data.Count)];
			}
			else
			{
				hairColour = RNPCexoticHairColours.data[Game1.random.Next(0, RNPCexoticHairColours.data.Count)];
			}

			string eyeColour;
			string[] eyeColours = { "green", "blue", "brown" };
			string eyeColourRange = eyeColours[Game1.random.Next(0, eyeColours.Length)];
			int r = 0;
			int g = 255;
			int b = 0;
			switch (eyeColourRange)
			{
				case "green":
					g = 255;
					b = Game1.random.Next(0, 200);
					r = Game1.random.Next(0, b);
					break;
				case "blue":
					b = 255;
					g = Game1.random.Next(0, 200);
					r = Game1.random.Next(0, g);
					break;
				case "brown":
					r = 255;
					g = Game1.random.Next(100, 150);
					b = g - 100;
					break;
				default:
					break;

			};
			eyeColour = r + " " + g + " " + b;

			npcstring = age + "/" + manner + "/" + anxiety + "/" + optimism + "/" + gender + "/" + datable + "/"+traits+"/" + birthday + "/" + name + "/" + giftTaste + "/" + bodyType + "/" + skinColour + "/" + hairStyle + "/" + hairColour + "/" + eyeColour;

			return npcstring;
		}

		private bool SharesItems(string[] sharing, string[] shares)
		{
			foreach(string s in shares)
			{
				if (!sharing.Contains(s))
				{
					return false;
				}
			}
			return true;
		}

		private List<T1> LengthenToMatch<T1, T2>(List<T1> smallerL, List<T2> largerL)
		{
			int multMax = (int)Math.Ceiling((double)largerL.Count / (double)smallerL.Count); // 10/6 = 2
			int multMin = (int)Math.Floor((double)largerL.Count / (double)smallerL.Count);  // 10/6 = 1
			int multDiff = largerL.Count - smallerL.Count * multMin;  // 10 - 6*1 = 4 remainder, number of entries that get extra

			List<T1> outList = new List<T1>();

			for (int i = 0; i < smallerL.Count; i++)
			{
				int no;
				if (i == 0 || i > multDiff) // first gets repeated fewer, also those after multDiff (4)
				{
					no = multMin;
				}
				else
				{
					no = multMax;
				}
				for (int j = 0; j < no; j++)
				{
					outList.Add(smallerL[i]);
				}
			}
			return outList;
		}


		private List<string> GetHighestRankedStrings(string npcString, List<string> data, int checks)
		{
			List<string> outStrings = new List<string>();
			int rank = 0;
			foreach (string str in data)
			{
				int aRank = RankStringForNPC(npcString, str, checks);
				if (aRank > rank)
				{
					outStrings = new List<string>(); // reset on higher rank
					rank = aRank;
				}
				if (aRank == rank)
				{
					outStrings.Add(string.Join("/",str.Split('/').Skip(checks)));
				}

			}
			return outStrings;
		}


		private int RankStringForNPC(string npcString, string str, int checks)
		{
			int rank = 0;

			IEnumerable<string> stra = str.Split('/').Take(checks);
			IEnumerable<string> npca = npcString.Split('/').Take(checks);
			for (int i = 0; i < checks; i++)
			{
				if(stra.Count() == i) 
				{
					break;
				}
				string strai = stra.ElementAt(i);
				string npcai = npca.ElementAt(i);
				if (strai != "any")
				{
					List<string> straia = strai.Split('|').ToList();
					if (strai != "" && strai != npcai && !straia.Contains(npcai))
					{
						return -1;
					}
					rank++;
				}
			}
			return rank;
		}
		private string GetRandomFromDist(string[] strings, double[] dists) 
		{
			double rnd = Game1.random.NextDouble();
			double x = 0;
			for(int i = 0; i < strings.Length; i++)
			{
				if(rnd < x + dists[i])
				{
					return strings[i];
				}
				else
				{
					x += dists[i];
				}
			}
			return "";
		}
	}
}