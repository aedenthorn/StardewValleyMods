using Microsoft.Xna.Framework;
using StardewValley;
using System.Collections.Generic;
using System.Linq;

namespace CustomCraneGame
{
    public partial class ModEntry
    {
        public static bool GetCraneGameAtCursor()
        {
            Vector2 tile = Game1.currentCursorTile;
            bool flag = !Utility.tileWithinRadiusOfPlayer((int)tile.X, (int)tile.Y, 1, Game1.player);
            if (flag)
            {
                tile = Game1.player.GetGrabTile();
            }
            GameLocation currentLocation = Game1.currentLocation;
            var obj = currentLocation?.getObjectAtTile((int)tile.X, (int)tile.Y, false);
            return obj?.QualifiedItemId == "(BC)rokugin.cranecp_CraneGame";
        }

        public static void RedirectToOriginal(Farmer who, string whichAnswer)
        {
            Game1.activeClickableMenu = null;
            if (whichAnswer == "cancel")
            {
                lastBehavior.Value = null;
                lastQuestion.Value = null;
                lastGameChoice.Value = null;
                return;
            }
            lastGameChoice.Value = whichAnswer;
            SHelper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
        }

        private static void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            skip = true;
            SHelper.Events.GameLoop.UpdateTicked -= GameLoop_UpdateTicked;
            if (lastGameChoice.Value == normalKey)
            {
                lastGameChoice.Value = null;
                Game1.currentLocation.createQuestionDialogue(lastQuestion.Value, Game1.currentLocation.createYesNoResponses(), lastBehavior.Value);
            }
            else if (GameDataDict.TryGetValue(lastGameChoice.Value, out var data))
            {
                Game1.currentLocation.createQuestionDialogue(data.Price > 0 ? string.Format(SHelper.Translation.Get("play-x-y"), data.Name, data.Price) : string.Format(SHelper.Translation.Get("play-x-free"), data.Name ), Game1.currentLocation.createYesNoResponses(), lastBehavior.Value);
            }
            skip = false;

        }

        public static Response[] GetResponses()
        {
            List<Response> responses = new()
            {
                new Response(normalKey, SHelper.Translation.Get("normal-game"))
            };
            foreach (var kvp in GameDataDict)
            {
                responses.Add(new(kvp.Key, kvp.Value.Name));
            }
            responses.Add(new("cancel", Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.10993")));
            return responses.ToArray();
        }
        public static PrizeData ChoosePrize(List<PrizeData> prizes)
        {
            int totalWeight = prizes.Sum(p => p.Weight);
            int roll = Game1.random.Next(totalWeight);
            int currentWeight = 0;
            foreach(var prize in prizes)
            {
                currentWeight += prize.Weight;
                if(currentWeight > roll)
                    return prize;
            }
            return prizes.Last();
        }
    }
}