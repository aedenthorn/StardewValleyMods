using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RandomNPC
{
	/// <summary>The mod entry point.</summary>
	public class ModEntry : Mod, IAssetEditor, IAssetLoader
	{
		internal ModConfig Config { get; private set; }
		internal ModData RNPCdialogueStrings { get; private set; }
		internal ModData RNPCengagementDialogueStrings { get; private set; }
		internal ModData RNPCgiftDialogueStrings { get; private set; }
		internal ModData RNPCscheduleStrings { get; private set; }
		internal ModData RNPCgiftResponseStrings { get; private set; }
		internal ModData RNPCfemaleNameStrings { get; private set; }
		internal ModData RNPCmaleNameStrings { get; private set; }
		internal ModData RNPCskinColours { get; private set; }
		internal ModData RNPCsavedNPCs { get; private set; }

		private List<RNPCSchedule> RNPCSchedules = new List<RNPCSchedule>();
		public int RNPCMax { get; private set; }
		public List<RNPC> RNPCs = new List<RNPC>();

		/// <summary>Get whether this instance can edit the given asset.</summary>
		/// <param name="asset">Basic metadata about the asset being loaded.</param>
		public bool CanEdit<T>(IAssetInfo asset)
		{
			if (asset.AssetNameEquals("Data/NPCDispositions") || asset.AssetNameEquals("Data/NPCGiftTastes") || asset.AssetNameEquals("Characters/EngagementDialogue"))
			{
				base.Monitor.Log("Can load: " + asset.AssetName, LogLevel.Alert);
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
					data[npc.nameID] = String.Join("/",npc.npc.Split('/').Take(12));
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
			foreach (string dialog in RNPCengagementDialogueStrings.data)
			{
				if (FitsNPC(npc, dialog))
				{
					potentialDialogues.Add(dialog.Split('/').Skip(7).ToArray());
				}

			}
			string[] output = potentialDialogues[Game1.random.Next(0,potentialDialogues.Count)];
			return output;
		}

		private string MakeGiftDialogue(RNPC npc)
		{
			List<string> potentialDialogue = new List<string>();
			foreach(string taste in RNPCgiftDialogueStrings.data)
			{
				if(FitsNPC(npc,taste))
				{
					string[] tastea = taste.Split('/')[7].Split('^');
					for(int i = 0; i < tastea.Length; i++)
					{
						tastea[i] += "/" + npc.giftTaste[i];
					}
					potentialDialogue.Add(String.Join("/",tastea));
				}

			}
			return potentialDialogue[Game1.random.Next(0,potentialDialogue.Count)];
		}

		private bool FitsNPC(RNPC npc, string str)
		{
			string[] stra = str.Split('/');
			if(
					(stra[0] == npc.age || stra[0] == "any")
					&& (stra[1] == npc.manner || stra[1] == "any")
					&& (stra[2] == npc.anxiety || stra[2] == "any")
					&& (stra[3] == npc.optimism || stra[3] == "any")
					&& (stra[4] == npc.gender || stra[4] == "any")
					&& (stra[5] == npc.datable || stra[5] == "any")
					&& (stra[6] == npc.refinement || stra[6] == "any")
				)
			{
				return true;
			}
			return false;
		}

		/// <summary>Get whether this instance can load the initial version of the given asset.</summary>
		/// <param name="asset">Basic metadata about the asset being loaded.</param>
		public bool CanLoad<T>(IAssetInfo asset)
		{
			foreach (RNPC npc in RNPCs)
			{
				if (asset.AssetNameEquals("Portraits/"+ npc.nameID) || asset.AssetNameEquals("Characters/" + npc.nameID) || asset.AssetNameEquals("Characters/Dialogue/" + npc.nameID) || asset.AssetNameEquals("Characters/schedules/" + npc.nameID))
				{
					base.Monitor.Log("Can load: " + asset.AssetName, LogLevel.Alert);
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
					Texture2D texture = this.Helper.Content.Load<Texture2D>("assets/"+npc.gender+"_portrait.png", ContentSource.ModFolder);
					texture = tintTexture(texture, npc.skin);
						
					return (T)(object) texture;
				}
				else if (asset.AssetNameEquals("Characters/" + npc.nameID))
				{
					Texture2D texture = this.Helper.Content.Load<Texture2D>("assets/" + npc.gender + "_character.png", ContentSource.ModFolder);
					texture = tintTexture(texture, npc.skin);

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

		private Dictionary<string,string> MakeDialogue(RNPC npc)
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			Dictionary<string, List<string>> potentialDialogues = new Dictionary<string, List<string>>();
			foreach(string dialogue in RNPCdialogueStrings.data)
			{
				if(FitsNPC(npc,dialogue))
				{
					string which = dialogue.Split('/')[7];
					if (!potentialDialogues.ContainsKey(which)) 
						potentialDialogues[which] = new List<string>();
					potentialDialogues[which].Add(dialogue.Split('/')[8]);
				}
			}
			foreach(KeyValuePair<string,List<string>> pot in potentialDialogues)
			{
				data.Add(pot.Key, pot.Value[Game1.random.Next(0, pot.Value.Count)]);
			}
			return data;
		}

		private Dictionary<string, string> MakeSchedule(RNPC npc)
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			RNPCSchedule schedule = new RNPCSchedule(npc);

			string[] morning = MakeRandomAppointment(npc, "morning");
			schedule.morningEarliest = morning[0];
			schedule.morningLoc = morning[1];
			string[] afternoon = MakeRandomAppointment(npc, schedule.morningLoc);
			schedule.afternoonEarliest = afternoon[0];
			schedule.afternoonLoc = afternoon[1];

			RNPCSchedules.Add(schedule);

			string sstr = schedule.MakeString();

			base.Monitor.Log("Schedule for "+npc.name+": "+sstr,LogLevel.Alert);

			data.Add("spring", sstr);
			data.Add("summer", sstr);
			data.Add("fall", sstr);
			data.Add("winter", sstr);
			return data;
		}

		private string[] MakeRandomAppointment(RNPC npc, string morning)
		{
			List<string[]> potentialApps = new List<string[]>();
			foreach (string appset in RNPCscheduleStrings.data)
			{
				if (!FitsNPC(npc, appset))
				{
					continue;
				}
				string time = appset.Split('/')[7].Split('^')[0];
				string place = appset.Split('/')[7].Split('^')[1];
				string[] locs = appset.Split('/')[7].Split('^')[2].Split('#');
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


		private Texture2D tintTexture(Texture2D sprite, string tint)
		{
			Color[] data = new Color[sprite.Width * sprite.Height];
			sprite.GetData(data);

			string[] rgb = tint.Split(' ');

			for (int i = 0; i < data.Length; i++)
			{
				if(data[i] != Color.Transparent)
				{
					data[i].R = (byte)(data[i].R - ((255-int.Parse(rgb[0]))*data[i].R/255));
					data[i].G = (byte)(data[i].G - ((255 - int.Parse(rgb[1])) * data[i].G / 255));
					data[i].B = (byte)(data[i].B - ((255 - int.Parse(rgb[2])) * data[i].B / 255));
				}
			}
			sprite.SetData<Color>(data);
			return sprite;
		}

		/*********
        ** Public methods
        *********/
		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			this.Config = this.Helper.ReadConfig<ModConfig>();
			
			this.RNPCdialogueStrings = this.Helper.Data.ReadJsonFile<ModData>("assets/dialogues.json") ?? new ModData();
			this.RNPCengagementDialogueStrings = this.Helper.Data.ReadJsonFile<ModData>("assets/engagement_dialogues.json") ?? new ModData();
			this.RNPCgiftDialogueStrings = this.Helper.Data.ReadJsonFile<ModData>("assets/gift_dialogues.json") ?? new ModData();
			this.RNPCscheduleStrings = this.Helper.Data.ReadJsonFile<ModData>("assets/schedules.json") ?? new ModData();
			this.RNPCgiftResponseStrings = this.Helper.Data.ReadJsonFile<ModData>("assets/gift_responses.json") ?? new ModData();
			this.RNPCfemaleNameStrings = this.Helper.Data.ReadJsonFile<ModData>("assets/female_names.json") ?? new ModData();
			this.RNPCmaleNameStrings = this.Helper.Data.ReadJsonFile<ModData>("assets/male_names.json") ?? new ModData();
			this.RNPCskinColours = this.Helper.Data.ReadJsonFile<ModData>("assets/skin_colours.json") ?? new ModData();

			this.RNPCMax = Math.Min(24, Config.RNPCMax);

			this.RNPCsavedNPCs = this.Helper.Data.ReadJsonFile<ModData>("assets/saved_npcs.json") ?? new ModData();
			while (RNPCsavedNPCs.data.Count < RNPCMax)
			{
				RNPCsavedNPCs.data.Add(generateNPCString());
			}
			this.Helper.Data.WriteJsonFile<ModData>("assets/saved_npcs.json", RNPCsavedNPCs);


			foreach(string npc in RNPCsavedNPCs.data)
			{
				this.RNPCs.Add(new RNPC(npc));
			}


			base.Monitor.Log("loaded", LogLevel.Alert);

		}

		private string generateNPCString()
		{
			string npcstring = "";

			// age
			string[] ages = { "child", "teen", "adult" };
			string age = ages[Game1.random.Next(0, ages.Length)];

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
			double female = Config.femaleChance;

			string gender = Game1.random.NextDouble() < female ? "female" : "male";

			// datable
			double datableChance = Config.datableChance;
			string datable = Game1.random.NextDouble() < datableChance ? "datable" : "non-datable";

			// birthday
			string[] seasons = { "spring", "summer", "fall", "winter" };
			string season = seasons[Game1.random.Next(0, seasons.Length)];
			int day = Game1.random.Next(1, 29);
			string birthday = season + " " + day;

			// Start Location
			string startLoc = "BusStop 13 10";
			bool freespot = false;
			int locX = 0;
			int locY = 0;
			while(freespot == false)
			{
				if (RNPCsavedNPCs.data.Count == 0)
					freespot = true;
				bool thisfreespot = true;
				for (int i = 0; i < RNPCsavedNPCs.data.Count; i++)
				{
					if (RNPCsavedNPCs.data[i].Split('/')[10] == startLoc)
					{
						thisfreespot = false;
						break;
					}
				}
				if (thisfreespot)
					freespot = true;
				else
				{
					locX++;
					if(locX > 5)
					{
						locX = 0;
						locY++;
					}
					startLoc = "BusStop " + (13 + locX) + " " + (10 + locY);
				}
			}

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

			// skin colour

			string skin = RNPCskinColours.data[Game1.random.Next(0, RNPCskinColours.data.Count)];

			// refinement
			string[] refinements = { "refined", "ordinary", "coarse" };
			string refinement = refinements[Game1.random.Next(0, refinements.Length)];

			// gift taste

			string giftTaste = "^^^^";

			npcstring = age + "/" + manner + "/" + anxiety + "/" + optimism + "/" + gender + "/" + datable + "/" + "" + "/" + "Town" + "/" + birthday + "/" + "" + "/" + startLoc + "/" + name + "/" + skin + "/" + refinement + "/" + giftTaste;

			return npcstring;
		}
	}
}