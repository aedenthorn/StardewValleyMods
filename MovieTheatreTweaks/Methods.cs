using StardewValley;
using StardewValley.Locations;
using StardewValley.Network;
using System.Linq;

namespace MovieTheatreTweaks
{
    public partial class ModEntry
    {
        private static void EnterTheatre(GameLocation __instance)
        {
            Rumble.rumble(0.15f, 200f);
            Game1.player.completelyStopAnimatingOrDoingAction();
            __instance.playSoundAt("doorClose", Game1.player.getTileLocation(), NetAudio.SoundContext.Default);
            Game1.warpFarmer("MovieTheater", 13, 15, 0);
        }

        private static void ShowMovieDialogue(MovieTheater instance, Farmer who)
        {
            NPC invited_npc = null;
            foreach (MovieInvitation invitation in Game1.player.team.movieInvitations)
            {
                if (invitation.farmer == Game1.player && Game1.player.currentLocation.characters.Where(n => n.Name == invitation.invitedNPC.Name).Count() > 0 && MovieTheater.GetFirstInvitedPlayer(invitation.invitedNPC) == Game1.player)
                {
                    invited_npc = invitation.invitedNPC;
                    break;
                }
            }
            if (invited_npc != null && Game1.player.hasItemInInventory(809, 1, 0))
            {
                Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_WatchWithFriendPrompt", invited_npc.displayName), Game1.currentLocation.createYesNoResponses(), "WatchMovieSpendTicket");
                return;
            }
            if (Game1.player.hasItemInInventory(809, 1, 0))
            {
                Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_WatchAlonePrompt"), Game1.currentLocation.createYesNoResponses(), "WatchMovieSpendTicket");
                return;
            }
            Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Characters:MovieTheater_NoTicket")));
        }
    }
}