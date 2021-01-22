using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CustomSpousePatioWizard
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
	{

		private static ModConfig Config;
		private static List<string> customAreaSpouses;
		private static List<string> noCustomAreaSpouses;
        public static ICustomSpousePatioApi customSpousePatioApi;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			if (!Config.Enabled)
				return;

            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
		}

		private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
		{
			customSpousePatioApi = Helper.ModRegistry.GetApi<ICustomSpousePatioApi>("aedenthorn.CustomSpousePatio");
			if (customSpousePatioApi != null)
			{
				Monitor.Log($"loaded CustomSpousePatio API", LogLevel.Debug);
			}
		}

		private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
		{
			//Monitor.Log($"last question key: {Game1.player?.currentLocation?.lastQuestionKey}");

			if (Context.CanPlayerMove && e.Button == Config.PatioWizardKey && Game1.player?.currentLocation == Game1.getFarm())
				StartWizard();
			else if (Game1.activeClickableMenu != null && Game1.player?.currentLocation?.lastQuestionKey?.StartsWith("CSP_Wizard_Questions") == true && Game1.player?.currentLocation == Game1.getFarm())
			{

				IClickableMenu menu = Game1.activeClickableMenu;
				if (menu == null || menu.GetType() != typeof(DialogueBox))
					return;

				DialogueBox db = menu as DialogueBox;
				int resp = db.selectedResponse;
				List<Response> resps = db.responses;

				if (resp < 0 || resps == null || resp >= resps.Count || resps[resp] == null)
					return;
				Monitor.Log($"Answered {Game1.player.currentLocation.lastQuestionKey} with {resps[resp].responseKey}");

				CSPWizardDialogue(Game1.player.currentLocation.lastQuestionKey, resps[resp].responseKey);
				return;
			}
		}

		public void StartWizard()
		{
			customAreaSpouses = new List<string>();
			noCustomAreaSpouses = new List<string>();
			foreach (KeyValuePair<string, Friendship> spouse in Game1.player.friendshipData.Pairs.Where(s => s.Value.IsMarried()))
			{
				if (customSpousePatioApi.GetCurrentSpouseAreas().ContainsKey(spouse.Key))
					customAreaSpouses.Add(spouse.Key);
				else
					noCustomAreaSpouses.Add(spouse.Key);
			}

			List<Response> responses = new List<Response>();
			if (noCustomAreaSpouses.Any())
				responses.Add(new Response("CSP_Wizard_Questions_AddPatio", Helper.Translation.Get("add-new-patio")));
			if (customAreaSpouses.Any())
			{
				responses.Add(new Response("CSP_Wizard_Questions_RemovePatio", Helper.Translation.Get("remove-patio")));
				responses.Add(new Response("CSP_Wizard_Questions_ListPatios", Helper.Translation.Get("list-patios")));
			}
			responses.Add(new Response("CSP_Wizard_Questions_ReloadPatios", Helper.Translation.Get("reload-patios")));
			responses.Add(new Response("cancel", Helper.Translation.Get("cancel")));
			Game1.player.currentLocation.createQuestionDialogue(Helper.Translation.Get("welcome"), responses.ToArray(), "CSP_Wizard_Questions");
		}


		public void CSPWizardDialogue(string whichQuestion, string whichAnswer)
		{
			Monitor.Log($"question: {whichQuestion}, answer: {whichAnswer}");
			if (whichAnswer == "cancel")
				return;

			List<Response> responses = new List<Response>();
			string header = "";
			string newQuestion = whichAnswer;
			switch (whichQuestion)
			{
				case "CSP_Wizard_Questions":
					switch (whichAnswer)
					{
						case "CSP_Wizard_Questions_AddPatio":
							header = Helper.Translation.Get("new-patio-which");
							foreach (string spouse in noCustomAreaSpouses)
							{
								responses.Add(new Response(spouse, spouse));
							}
							break;
						case "CSP_Wizard_Questions_RemovePatio":
							header = Helper.Translation.Get("remove-patio-which");
							foreach (string spouse in customAreaSpouses)
							{
								responses.Add(new Response(spouse, spouse));
							}
							break;
						case "CSP_Wizard_Questions_ListPatios":
							Game1.drawObjectDialogue(string.Format(Helper.Translation.Get("patios-exist-for"), string.Join(", ", customAreaSpouses)));
							return;
						case "CSP_Wizard_Questions_ReloadPatios":
							customSpousePatioApi.ReloadPatios();
							Game1.drawObjectDialogue(string.Format(Helper.Translation.Get("reloaded-patios"), customSpousePatioApi.GetCurrentSpouseAreas().Count));
							return;
					}
					break;
				case "CSP_Wizard_Questions_AddPatio":
					header = Helper.Translation.Get("new-patio-use-tiles");
					foreach (string spouse in customSpousePatioApi.GetCurrentSpouseAreas().Keys)
					{
						responses.Add(new Response($"{whichAnswer}_{spouse}", spouse));
					}
					newQuestion = "CSP_Wizard_Questions_AddPatio_2";
					break;
				case "CSP_Wizard_Questions_AddPatio_2":
					customSpousePatioApi.AddSpousePatioHere(whichAnswer);
					Game1.drawObjectDialogue(Helper.Translation.Get("created-patio"));
					return;
				case "CSP_Wizard_Questions_RemovePatio":
					if (customSpousePatioApi.RemoveSpousePatio(whichAnswer))
						Game1.drawObjectDialogue(string.Format(Helper.Translation.Get("removed-patio"), whichAnswer));
					else
						Game1.drawObjectDialogue(string.Format(Helper.Translation.Get("not-removed-patio"), whichAnswer));
					return;
				default:
					return;
			}
			responses.Add(new Response("cancel", Helper.Translation.Get("cancel")));
			Game1.player.currentLocation.createQuestionDialogue($"{header}", responses.ToArray(), newQuestion);
		}


	}
}