using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HugsAndKisses
{
    /// <summary>The mod entry point.</summary>
    public class Kissing
    {
        private static IMonitor Monitor;
        private static ModConfig Config;
        private static IModHelper PHelper;
        public static int elapsedSeconds;
        public static Dictionary<string, int> lastKissed = new Dictionary<string, int>();
        public static SoundEffect kissEffect = null;
        public static SoundEffect hugEffect = null;
        public static void Initialize(IMonitor monitor, ModConfig config, IModHelper helper)
        {
            Monitor = monitor;
            Config = config;
            PHelper = ModEntry.PHelper;
        }

        public static void TrySpousesKiss(GameLocation location)
        {

            if ( location == null || Game1.eventUp || Game1.activeClickableMenu != null || Game1.player.currentLocation != location || (!Config.AllowNPCSpousesToKiss && !Config.AllowPlayerSpousesToKiss && !Config.AllowNPCRelativesToHug))
                return;

            elapsedSeconds++;

            var characters = location.characters;
            if (characters == null)
                return;

            List<NPC> list = characters.ToList();

            Misc.ShuffleList(ref list);

            foreach (NPC npc1 in list)
            {
                if (!npc1.datable.Value && !npc1.isRoommate() && !Config.AllowNonDateableNPCsToHugAndKiss)
                    continue;

                if (lastKissed.ContainsKey(npc1.Name) && elapsedSeconds - lastKissed[npc1.Name] <= Config.MinSpouseKissIntervalSeconds)
                    continue;

                foreach (NPC npc2 in list)
                {
                    if (npc1.Name == npc2.Name)
                        continue;

                    if (!npc2.datable.Value && !Config.AllowNonDateableNPCsToHugAndKiss)
                        continue;

                    if (lastKissed.ContainsKey(npc2.Name) && elapsedSeconds - lastKissed[npc2.Name] <= Config.MinSpouseKissIntervalSeconds)
                        continue;

                    bool npcRelatedHug = Misc.AreNPCsRelated(npc1.Name, npc2.Name) && Config.AllowNPCRelativesToHug;
                    bool npcRoommateHug = !Config.RoommateKisses && (npc1.isRoommate() || npc2.isRoommate());
                    
                    bool npcMarriageKiss = Misc.AreNPCsMarried(npc1.Name, npc2.Name) && Config.AllowNPCSpousesToKiss;
                    bool playerSpouseKiss = Config.AllowPlayerSpousesToKiss &&
                        npc1.getSpouse() != null && npc2.getSpouse() != null &&
                        npc1.getSpouse() == npc2.getSpouse() &&
                        npc1.getSpouse().friendshipData.ContainsKey(npc1.Name) && npc1.getSpouse().friendshipData.ContainsKey(npc2.Name) &&
                        (Config.RoommateKisses || !npc1.getSpouse().friendshipData[npc1.Name].RoommateMarriage) && (Config.RoommateKisses || !npc1.getSpouse().friendshipData[npc2.Name].RoommateMarriage) &&
                        npc1.getSpouse().getFriendshipHeartLevelForNPC(npc1.Name) >= Config.MinHeartsForMarriageKiss && npc1.getSpouse().getFriendshipHeartLevelForNPC(npc2.Name) >= Config.MinHeartsForMarriageKiss &&
                        (Config.AllowRelativesToKiss || !Misc.AreNPCsRelated(npc1.Name, npc2.Name));

                    // check if spouses
                    if (!npcMarriageKiss && !npcRelatedHug && !playerSpouseKiss && !npcRoommateHug)
                        continue;

                    float distance = Vector2.Distance(npc1.position, npc2.position);
                    if (
                        distance < Config.MaxDistanceToKiss
                        && !npc1.isSleeping.Value
                        && !npc2.isSleeping.Value
                        && ModEntry.myRand.NextDouble() < Config.SpouseKissChance0to1
                    )
                    {
                        Monitor.Log($"{npc1.Name} and {npc2.Name} are marriage kissing: {npcMarriageKiss}, related hugging: {npcRelatedHug}, player spouse kissing: {playerSpouseKiss}");

                        lastKissed[npc1.Name] = elapsedSeconds;
                        lastKissed[npc2.Name] = elapsedSeconds;

                        Vector2 npc1pos = npc1.position;
                        Vector2 npc2pos = npc2.position;
                        int npc1face = npc1.facingDirection;
                        int npc2face = npc1.facingDirection;
                        Vector2 midpoint = new Vector2((npc1.position.X + npc2.position.X) / 2, (npc1.position.Y + npc2.position.Y) / 2);

                        PerformEmbrace(npc1, midpoint, npc2.Name);
                        PerformEmbrace(npc2, midpoint, npc1.Name);

                        if (playerSpouseKiss || npcMarriageKiss)
                        {
                            if (Config.CustomKissSound.Length > 0 && kissEffect != null)
                            {
                                float playerDistance = 1f / ((Vector2.Distance(midpoint, Game1.player.position) / 256) + 1);
                                kissEffect.Play(playerDistance * Game1.options.soundVolumeLevel, 0, 0);
                            }
                            else
                            {
                                Game1.currentLocation.playSound("dwop", NetAudio.SoundContext.NPC);
                            }
                            Misc.ShowHeart(npc1);
                            Misc.ShowHeart(npc2);

                        }
                        else
                        {
                            if (Config.CustomHugSound.Length > 0 && hugEffect != null)
                            {
                                float playerDistance = 1f / ((Vector2.Distance(midpoint, Game1.player.position) / 256) + 1);
                                hugEffect.Play(playerDistance * Game1.options.soundVolumeLevel, 0, 0);
                            }
                            Misc.ShowSmiley(npc1);
                            Misc.ShowSmiley(npc2);
                        }

                        DelayedAction action = new DelayedAction(1000);
                        var t = Task.Run(async delegate
                        {
                            await Task.Delay(TimeSpan.FromSeconds(1f));
                            npc1.position.Value = npc1pos;
                            npc2.position.Value = npc2pos;
                            npc1.FacingDirection = npc1face;
                            npc2.FacingDirection = npc2face;
                            npc1.Sprite.UpdateSourceRect();
                            npc2.Sprite.UpdateSourceRect();
                            return;
                        });
                    }
                }
            }
        }

        public static void PerformEmbrace(NPC npc1, NPC npc2)
        {
            Vector2 midpoint = new Vector2((npc1.position.X + npc2.position.X) / 2, (npc1.position.Y + npc2.position.Y) / 2);
            PerformEmbrace(npc1, midpoint, npc2.Name);
            PerformEmbrace(npc2, midpoint, npc1.Name);
        }
        private static void PerformEmbrace(NPC npc, Vector2 midpoint, string partner)
        {
            string name = npc.Name;
            int spouseFrame = Misc.GetKissingFrame(name);
            bool facingRight = Misc.GetFacingRight(name);

            List<string> customFrames = Config.CustomKissFrames.Split(',').ToList();
            foreach(string nameframe in customFrames)
            {
                if (nameframe.StartsWith(name + ":"))
                {
                    int.TryParse(nameframe.Substring(name.Length + 1), out spouseFrame);
                    break;
                }
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
            npc.Sprite.UpdateSourceRect();
        }


        public static void PlayerNPCKiss(Farmer player, NPC npc)
        {
            string name = npc.Name;
            int spouseFrame = Misc.GetKissingFrame(name);
            bool facingRight = Misc.GetFacingRight(name);

            bool flip = (facingRight && npc.FacingDirection == 3) || (!facingRight && npc.FacingDirection == 1);
            if (!player.friendshipData.TryGetValue(npc.Name, out Friendship f))
            {
                ModEntry.SMonitor.Log($"{player.Name} has no friendship data for {npc.Name}");
                return;
            }

            if (npc.hasBeenKissedToday.Value && !Config.UnlimitedDailyKisses)
            {
                ModEntry.SMonitor.Log($"{player.Name} has already kissed {npc.Name} today");
                return;
            }

            bool accepted = true;
            if (f.IsMarried() || f.IsEngaged()) 
            {
                if (player.getFriendshipHeartLevelForNPC(npc.Name) < Config.MinHeartsForMarriageKiss)
                {
                    accepted = false;
                }
            }
            else if (f.IsDating())
            {
                if (player.getFriendshipHeartLevelForNPC(npc.Name) < Config.MinHeartsForDatingKiss)
                {
                    accepted = false;
                }
            }
            if (accepted)
            {
                ModEntry.SMonitor.Log($"Can kiss/hug {npc.Name}");

                int delay = Game1.IsMultiplayer ? 1000 : 10;
                npc.movementPause = delay;
                npc.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
                    {
                        new FarmerSprite.AnimationFrame(spouseFrame, delay, false, flip, new AnimatedSprite.endOfAnimationBehavior(npc.haltMe), true)
                    }
                );
                if (!npc.hasBeenKissedToday.Value)
                {
                    player.changeFriendship(10, npc);
                }

                if (!Config.RoommateKisses && player.friendshipData[npc.Name].RoommateMarriage)
                {
                    ModEntry.SMonitor.Log($"Hugging {npc.Name}");
                    Misc.ShowSmiley(npc);
                }
                else
                {
                    ModEntry.SMonitor.Log($"Kissing {npc.Name}");
                    Misc.ShowHeart(npc);
                }
                if (Config.RoommateKisses || !player.friendshipData[npc.Name].RoommateMarriage)
                {
                    if (Config.CustomKissSound.Length > 0 && kissEffect != null)
                    {
                        kissEffect.Play();
                    }
                    else
                    {
                        npc.currentLocation.playSound("dwop", NetAudio.SoundContext.NPC);
                    }
                }
                player.exhausted.Value = false;
                npc.hasBeenKissedToday.Value = true;
                npc.Sprite.UpdateSourceRect();
            }
            else
            {
                ModEntry.SMonitor.Log($"Kiss/hug rejected by {npc.Name}");

                npc.faceDirection((ModEntry.myRand.NextDouble() < 0.5) ? 2 : 0);
                npc.doEmote(12, true);
            }
            int playerFaceDirection = 1;
            if ((facingRight && !flip) || (!facingRight && flip))
            {
                playerFaceDirection = 3;
            }
            player.PerformKiss(playerFaceDirection);
        }

        public static void PlayerNPCHug(Farmer player, NPC npc)
        {
            string name = npc.Name;
            int spouseFrame = Misc.GetKissingFrame(name);
            bool facingRight = Misc.GetFacingRight(name);

            bool flip = (facingRight && npc.FacingDirection == 3) || (!facingRight && npc.FacingDirection == 1);

            ModEntry.SMonitor.Log($"Can hug {npc.Name}");

            int delay = Game1.IsMultiplayer ? 1000 : 10;
            npc.movementPause = delay;
            npc.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
                    {
                        new FarmerSprite.AnimationFrame(spouseFrame, delay, false, flip, new AnimatedSprite.endOfAnimationBehavior(npc.haltMe), true)
                    }
            );

            ModEntry.SMonitor.Log($"Hugging {npc.Name}");
            Misc.ShowSmiley(npc);
            if (Config.CustomHugSound.Length > 0 && hugEffect != null)
            {
                hugEffect.Play();
            }
            player.exhausted.Value = false;
            npc.hasBeenKissedToday.Value = true;
            npc.Sprite.UpdateSourceRect();

            int playerFaceDirection = 1;
            if ((facingRight && !flip) || (!facingRight && flip))
            {
                playerFaceDirection = 3;
            }
            player.PerformKiss(playerFaceDirection);
        }
    }
}