using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MultipleSpouses
{
    /// <summary>The mod entry point.</summary>
    public class HelperEvents
    {
        private static IMonitor Monitor;
        private static IModHelper Helper;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor, IModHelper helper)
        {
            Monitor = monitor;
            Helper = helper;
        }


		public static void SaveGame_Load_prefix(string filename)
        {
			Monitor.Log($"loading save {filename}");
			ModEntry.outdoorAreaData = Helper.Data.ReadJsonFile<OutdoorAreaData>($"assets/outdoor-area-{filename}.json") ?? new OutdoorAreaData();
			if (ModEntry.outdoorAreaData.areas.Count == 0)
			{
				Helper.Data.WriteJsonFile($"assets/outdoor-area-{filename}.json", new OutdoorAreaData()); 
			}
		}

		public static void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            ModEntry.spouseToDivorce = null;
            Misc.SetAllNPCsDatable();
            FileIO.LoadTMXSpouseRooms();
            Misc.ResetSpouses(Game1.player);
			Misc.SetNPCRelations();
		}


        public static void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            FileIO.LoadKissAudio();
        }

        public static void GameLoop_DayEnding(object sender, DayEndingEventArgs e)
        {
            Helper.Events.GameLoop.OneSecondUpdateTicked -= GameLoop_OneSecondUpdateTicked;
        }

        public static void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            Helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;
            Misc.ResetSpouses(Game1.player);

			foreach(GameLocation location in Game1.locations)
            {
				if(ReferenceEquals(location.GetType(),typeof(FarmHouse)))
                {
					(location as FarmHouse).showSpouseRoom();
					Maps.BuildSpouseRooms((location as FarmHouse));
					Misc.PlaceSpousesInFarmhouse((location as FarmHouse));
				}
			}
            if (Game1.IsMasterGame)
            {
				Game1.getFarm().addSpouseOutdoorArea(Game1.player.spouse == null ? "" : Game1.player.spouse);
				ModEntry.farmHelperSpouse = Misc.GetRandomSpouse(Game1.MasterPlayer);
			}
		}

		public static void GameLoop_ReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            Helper.Events.GameLoop.OneSecondUpdateTicked -= GameLoop_OneSecondUpdateTicked;
            ModEntry.spouseToDivorce = null;
        }

		public static string complexDivorceSpouse;
		public static void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
		{
			if ((e.Button != SButton.MouseLeft && e.Button != SButton.MouseRight) || Game1.currentLocation == null || !(Game1.currentLocation is ManorHouse) || Game1.currentLocation.lastQuestionKey == null || !Game1.currentLocation.lastQuestionKey.StartsWith("divorce"))
				return;

			IClickableMenu menu = Game1.activeClickableMenu;
			if (menu == null || !ReferenceEquals(menu.GetType(), typeof(DialogueBox)))
				return;
			int resp = (int)typeof(DialogueBox).GetField("selectedResponse", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(menu);
			List<Response> resps = (List<Response>)typeof(DialogueBox).GetField("responses", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(menu);

			if (resp < 0 || resps == null || resp >= resps.Count || resps[resp] == null)
				return;


			Game1.currentLocation.lastQuestionKey = "";
			string whichAnswer = resps[resp].responseKey;

			Monitor.Log("answer " + whichAnswer);

            if (Misc.GetSpouses(Game1.player,1).ContainsKey(whichAnswer))
            {
				string s2 = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:ManorHouse_DivorceBook_Question_" + Game1.player.spouse);
				if (s2 == null)
				{
					s2 = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:ManorHouse_DivorceBook_Question");
				}
				List<Response> responses = new List<Response>();
				responses.Add(new Response($"Yes_{whichAnswer}", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes")));
                if (ModEntry.config.ComplexDivorce)
                {
					responses.Add(new Response($"divorce_complex_{whichAnswer}", Helper.Translation.Get("divorce_complex")));
				}
				responses.Add(new Response("No", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")));
				Game1.currentLocation.createQuestionDialogue(s2, responses.ToArray(), $"divorce_{whichAnswer}");
			}
			else if (whichAnswer.StartsWith("Yes_"))
			{
				string spouse = whichAnswer.Substring(4);
				ModEntry.spouseToDivorce = spouse;
				if (Game1.player.Money >= 50000 || spouse == "Krobus")
				{
					if (!Game1.player.isRoommate(spouse))
					{
						Game1.player.Money -= 50000;
						ModEntry.divorceHeartsLost = ModEntry.config.FriendlyDivorce ? 0 : -1;
					}
					else
                    {
						ModEntry.divorceHeartsLost = 0;
					}
					Game1.player.divorceTonight.Value = true;
					string s = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:ManorHouse_DivorceBook_Filed_" + spouse);
					if (s == null)
					{
						s = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:ManorHouse_DivorceBook_Filed");
					}
					Game1.drawObjectDialogue(s);
					if (!Game1.player.isRoommate(spouse))
					{
						ModEntry.mp.globalChatInfoMessage("Divorce", new string[]
						{
							Game1.player.Name
						});
					}
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
				}
			}
			else if (whichAnswer.StartsWith("divorce_complex_"))
			{
				complexDivorceSpouse = whichAnswer.Replace("divorce_complex_", "");
				ModEntry.divorceHeartsLost = 1;
				ShowNextDialogue("divorce_fault_", Game1.currentLocation);
			}
			else if (whichAnswer.StartsWith("divorce_fault_"))
			{
				Monitor.Log("divorce fault");
				string r = Helper.Translation.Get(whichAnswer);
				if (r != null)
				{
					if (int.TryParse(r.Split('#')[r.Split('#').Length - 1], out int lost))
					{
						ModEntry.divorceHeartsLost += lost;
					}
				}
				string nextKey = $"divorce_{r.Split('#')[r.Split('#').Length - 2]}reason_";
				Translation test = Helper.Translation.Get(nextKey + "q");
				if (!test.HasValue())
				{
					ShowNextDialogue($"divorce_method_", Game1.currentLocation);
					return;
				}
				ShowNextDialogue($"divorce_{r.Split('#')[r.Split('#').Length - 2]}reason_", Game1.currentLocation);
			}
			else if (whichAnswer.Contains("reason_"))
			{
				Monitor.Log("divorce reason");
				string r = Helper.Translation.Get(whichAnswer);
				if (r != null)
				{
					if (int.TryParse(r.Split('#')[r.Split('#').Length - 1], out int lost))
					{
						ModEntry.divorceHeartsLost += lost;
					}
				}

				ShowNextDialogue($"divorce_method_", Game1.currentLocation);
			}
			else if (whichAnswer.StartsWith("divorce_method_"))
			{
				Monitor.Log("divorce method");
				ModEntry.spouseToDivorce = complexDivorceSpouse;
				string r = Helper.Translation.Get(whichAnswer);
				if (r != null)
				{
					if (int.TryParse(r.Split('#')[r.Split('#').Length - 1], out int lost))
					{
						ModEntry.divorceHeartsLost += lost;
					}
				}

				if (Game1.player.Money >= 50000 || complexDivorceSpouse == "Krobus")
				{
					if (!Game1.player.isRoommate(complexDivorceSpouse))
					{
						int money = 50000;
						if (int.TryParse(r.Split('#')[r.Split('#').Length - 2], out int mult))
						{
							money = (int)Math.Round(money * mult / 100f);
						}
						Monitor.Log($"money cost {money}");
						Game1.player.Money -= money;
					}
					Game1.player.divorceTonight.Value = true;
					string s = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:ManorHouse_DivorceBook_Filed_" + complexDivorceSpouse);
					if (s == null)
					{
						s = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:ManorHouse_DivorceBook_Filed");
					}
					Game1.drawObjectDialogue(s);
					if (!Game1.player.isRoommate(complexDivorceSpouse))
					{
						ModEntry.mp.globalChatInfoMessage("Divorce", new string[]
						{
									Game1.player.Name
						});
					}
					Monitor.Log($"hearts lost {ModEntry.divorceHeartsLost}");
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney1"));
				}
			}
		}
		private static void ShowNextDialogue(string key, GameLocation l)
		{
			Translation s2 = Helper.Translation.Get($"{key}q");
			if (!s2.HasValue())
			{
				Monitor.Log("no dialogue: " + s2.ToString(), LogLevel.Error);
				return;
			}
			Monitor.Log("has dialogue: " + s2.ToString());
			List<Response> responses = new List<Response>();
			int i = 1;
			while (true)
			{
				Translation r = Helper.Translation.Get($"{key}{i}");
				if (!r.HasValue())
					break;
				string str = r.ToString().Split('#')[0];
				Monitor.Log(str);

				responses.Add(new Response(key + i, str));
				i++;
			}
			Monitor.Log("next question: " + s2.ToString());
			l.createQuestionDialogue(s2, responses.ToArray(), key);
		}

		public static void GameLoop_OneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
			foreach(GameLocation location in Game1.locations)
            {

				if(ReferenceEquals(location.GetType(), typeof(FarmHouse)))
                {
					FarmHouse fh = location as FarmHouse;
					if (fh.owner == null)
						continue;

					List<string> allSpouses = Misc.GetSpouses(fh.owner, 1).Keys.ToList();
					List<string> bedSpouses = allSpouses.FindAll((s) => ModEntry.config.RoommateRomance || !fh.owner.friendshipData[s].RoommateMarriage);

					foreach (NPC character in fh.characters)
					{
						if (allSpouses.Contains(character.Name))
						{

							if (Misc.IsInBed(fh, character.GetBoundingBox()))
							{
								character.farmerPassesThrough = true;
								if (!character.isMoving())
								{
									Vector2 bedPos = Misc.GetSpouseBedPosition(fh, bedSpouses, character.name);
									character.position.Value = bedPos;
									if(Game1.timeOfDay >= 2000 || Game1.timeOfDay <= 630)
                                    {
										if (!character.isSleeping)
                                        {
											character.isSleeping.Value = true;
											if (!Misc.HasSleepingAnimation(character.name.Value))
											{
												character.Sprite.StopAnimation();
												character.faceDirection(0);
											}
											else
											{
												character.playSleepingAnimation();
											}
										}
									}
									else
                                    {
										character.isSleeping.Value = false;
									}
								}
								else
                                {
									character.isSleeping.Value = false;
								}
								character.HideShadow = true;
							}
							else
							{
								character.farmerPassesThrough = false;
								character.HideShadow = false;
								character.isSleeping.Value = false;
							}
						}
					}
					if (location == Game1.player.currentLocation && ModEntry.config.AllowSpousesToKiss)
					{
						Kissing.TrySpousesKiss();
					}
				}
			}


        }
    }
}