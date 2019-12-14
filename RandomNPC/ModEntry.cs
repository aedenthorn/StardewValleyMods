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
		internal ModData RNPCfemaleNameStrings { get; private set; }
		internal ModData RNPCmaleNameStrings { get; private set; }
		internal ModData RNPCbodyTypes { get; private set; }
		internal ModData RNPCdarkSkinColours { get; private set; }
		internal ModData RNPClightSkinColours { get; private set; }
		internal ModData RNPChairStyles { get; private set; }
		internal ModData RNPCnaturalHairColours { get; private set; }
		internal ModData RNPCexoticHairColours { get; private set; }
		internal ModData RNPCclothes { get; private set; }
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
			foreach (string dialog in RNPCengagementDialogueStrings.data)
			{
				if (FitsNPC(npc.npcString, dialog))
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
				if(FitsNPC(npc.npcString, taste))
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

		private bool FitsNPC(string npcString, string str)
		{
			string[] stra = str.Split('/');
			string[] npca = npcString.Split('/');
			if(
					(stra[0] == npca[0] || stra[0] == "any")
					&& (stra[1] == npca[1] || stra[1] == "any")
					&& (stra[2] == npca[2] || stra[2] == "any")
					&& (stra[3] == npca[3] || stra[3] == "any")
					&& (stra[4] == npca[4] || stra[4] == "any")
					&& (stra[5] == npca[5] || stra[5] == "any")
					&& (stra[6] == npca[6] || stra[6] == "any")
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

		private Dictionary<string,string> MakeDialogue(RNPC npc)
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			Dictionary<string, List<string>> potentialDialogues = new Dictionary<string, List<string>>();
			foreach(string dialogue in RNPCdialogueStrings.data)
			{
				if(FitsNPC(npc.npcString,dialogue))
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
				if (!FitsNPC(npc.npcString, appset))
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


		private Texture2D CreateCustomCharacter(RNPC npc, string type)
		{
			Texture2D sprite = this.Helper.Content.Load<Texture2D>("assets/" + npc.bodyType + "_"+type+".png", ContentSource.ModFolder);
			Texture2D hairT = this.Helper.Content.Load<Texture2D>("assets/" + npc.hairStyle + "_" + type + ".png", ContentSource.ModFolder);
			Texture2D eyeT = this.Helper.Content.Load<Texture2D>("assets/" + npc.bodyType+ "_eyes_" + type + ".png", ContentSource.ModFolder);
			Texture2D topT = this.Helper.Content.Load<Texture2D>("assets/transparent_" + type + ".png", ContentSource.ModFolder);
			Texture2D bottomT = topT;
			Texture2D shoesT = topT;

			if(npc.clothes[0] != "")
			{
				topT = this.Helper.Content.Load<Texture2D>("assets/" + npc.clothes[0]+ "_" + type + ".png", ContentSource.ModFolder);
			}
			if(npc.clothes[1] != "")
			{
				bottomT = this.Helper.Content.Load<Texture2D>("assets/" + npc.clothes[1]+ "_" + type + ".png", ContentSource.ModFolder);
			}
			if(npc.clothes[2] != "" && type == "character")
			{
				shoesT = this.Helper.Content.Load<Texture2D>("assets/" + npc.clothes[2]+ ".png", ContentSource.ModFolder);
			}

			Color[] data = new Color[sprite.Width * sprite.Height];
			Color[] dataH = new Color[sprite.Width * sprite.Height];
			Color[] dataE = new Color[sprite.Width * sprite.Height];
			Color[] dataT = new Color[sprite.Width * sprite.Height];
			Color[] dataB = new Color[sprite.Width * sprite.Height];
			Color[] dataS = new Color[sprite.Width * sprite.Height];
			sprite.GetData(data);
			hairT.GetData(dataH);
			eyeT.GetData(dataE);
			topT.GetData(dataT);
			bottomT.GetData(dataB);
			shoesT.GetData(dataS);

			string[] skinRBG = npc.skinColour.Split(' ');
			string[] hairRBG = npc.hairColour.Split(' ');
			string[] eyeRBG = npc.eyeColour.Split(' ');

			for (int i = 0; i < data.Length; i++)
			{
				if(dataH[i] != Color.Transparent)
				{
					data[i].R = (byte)(dataH[i].R - ((255 - int.Parse(hairRBG[0])) * dataH[i].R / 255));
					data[i].G = (byte)(dataH[i].G - ((255 - int.Parse(hairRBG[1])) * dataH[i].G / 255));
					data[i].B = (byte)(dataH[i].B - ((255 - int.Parse(hairRBG[2])) * dataH[i].B / 255));
					data[i].A = 255;
				}
				else if(dataE[i] != Color.Transparent)
				{
					if(dataE[i] != Color.White)
					{
						data[i].R = (byte)(dataE[i].R - ((255 - int.Parse(eyeRBG[0])) * dataE[i].R / 255));
						data[i].G = (byte)(dataE[i].G - ((255 - int.Parse(eyeRBG[1])) * dataE[i].G / 255));
						data[i].B = (byte)(dataE[i].B - ((255 - int.Parse(eyeRBG[2])) * dataE[i].B / 255));
					}
					else
					{
						data[i] = Color.White;
					}
					data[i].A = 255;
				}
				else if(dataT[i] != Color.Transparent)
				{
					data[i] = dataT[i];
				}
				else if(dataB[i] != Color.Transparent)
				{
					data[i] = dataB[i];
				}
				else if(dataS[i] != Color.Transparent)
				{
					data[i] = dataS[i];
				}
				else if(data[i] != Color.Transparent)
				{
					data[i].R = (byte)(data[i].R - ((255-int.Parse(skinRBG[0]))*data[i].R/255));
					data[i].G = (byte)(data[i].G - ((255 - int.Parse(skinRBG[1])) * data[i].G / 255));
					data[i].B = (byte)(data[i].B - ((255 - int.Parse(skinRBG[2])) * data[i].B / 255));
					data[i].A = 255;
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
			
			this.RNPCfemaleNameStrings = this.Helper.Data.ReadJsonFile<ModData>("assets/female_names.json") ?? new ModData();
			this.RNPCmaleNameStrings = this.Helper.Data.ReadJsonFile<ModData>("assets/male_names.json") ?? new ModData();

			this.RNPCbodyTypes = this.Helper.Data.ReadJsonFile<ModData>("assets/body_types.json") ?? new ModData();
			this.RNPCdarkSkinColours = this.Helper.Data.ReadJsonFile<ModData>("assets/dark_skin_colours.json") ?? new ModData();
			this.RNPClightSkinColours = this.Helper.Data.ReadJsonFile<ModData>("assets/light_skin_colours.json") ?? new ModData();

			this.RNPChairStyles = this.Helper.Data.ReadJsonFile<ModData>("assets/hair_styles.json") ?? new ModData();
			this.RNPCnaturalHairColours = this.Helper.Data.ReadJsonFile<ModData>("assets/natural_hair.json") ?? new ModData();
			this.RNPCexoticHairColours = this.Helper.Data.ReadJsonFile<ModData>("assets/exotic_hair.json") ?? new ModData();
			
			this.RNPCclothes = this.Helper.Data.ReadJsonFile<ModData>("assets/clothes.json") ?? new ModData();

			

			this.RNPCMax = Math.Min(24, Config.RNPCMax);

			this.RNPCsavedNPCs = this.Helper.Data.ReadJsonFile<ModData>("assets/saved_npcs.json") ?? new ModData();
			while (RNPCsavedNPCs.data.Count < RNPCMax)
			{
				RNPCsavedNPCs.data.Add(GenerateNPCString());
			}
			this.Helper.Data.WriteJsonFile<ModData>("assets/saved_npcs.json", RNPCsavedNPCs);

			int npcCount = 0;
			foreach(string npc in RNPCsavedNPCs.data)
			{
				string startLoc = "BusStop " + (13 + (npcCount % 5)) + " " + (10 + (npcCount) / 5);
				this.RNPCs.Add(new RNPC(npc,"RNPC"+ (npcCount++),startLoc));
			}
			 

			base.Monitor.Log("loaded", LogLevel.Alert);

		}

		private string GenerateNPCString()
		{
			string npcstring = "";

			// age
			string[] ages = { "child", "teen", "adult" };
			string age = GetRandomFromDist(ages,Config.ageDist);

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

			// refinement
			string[] refinements = { "refined", "ordinary", "coarse" };
			string refinement = refinements[Game1.random.Next(0, refinements.Length)];

			// gift taste

			string giftTaste = "^^^^";

			// body type

			List<string> potentialBodyTypes = new List<string>();
			foreach(string body in RNPCbodyTypes.data)
			{
				string[] ba = body.Split('/');
				if ((ba[0] == "any" || ba[0] == age) && (ba[1] == "any" || ba[1] == gender))
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
				if ((ba[0] == "any" || ba[0] == age) && (ba[1] == "any" || ba[1] == gender) && (ba[2] == "any" || ba[2] == refinement))
				{
					potentialHairStyles.Add(ba[3]);
				}
			}
			string hairStyle = potentialHairStyles[Game1.random.Next(0, potentialHairStyles.Count)];

			// hair colour

			string hairColour;
			if(Game1.random.NextDouble() < Config.NaturalHairChance)
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
					g = Game1.random.Next(128, 200);
					b = g - 128;
					break;
				default:
					break;

			};
			eyeColour = r + " " + g + " " + b;

			// clothes

			List<string> potentialClothes = new List<string>();
			foreach (string cloth in RNPCclothes.data)
			{
				string[] cla = cloth.Split('/');
				if (FitsNPC(age + "/" + manner + "/" + anxiety + "/" + optimism + "/" + gender + "/" + datable + "/" + refinement, cloth ) && (cla[7] == "any" || cla[7].Split('^').Contains(bodyType))) 
				{ 
					potentialClothes.Add(cla[8]);
				}
			}
			string clothes = potentialClothes[Game1.random.Next(0, potentialClothes.Count)];


			npcstring = age + "/" + manner + "/" + anxiety + "/" + optimism + "/" + gender + "/" + datable + "/" + refinement + "/" + birthday + "/" + name + "/" + giftTaste + "/" + bodyType + "/" + skinColour + "/" + hairStyle + "/" + hairColour + "/" + eyeColour + "/" + clothes;

			return npcstring;
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