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
        private static int elapsedSeconds;
        public static Dictionary<string, int> lastKissed = new Dictionary<string, int>();
        public static SoundEffect kissEffect = null;
        public static SoundEffect hugEffect = null;
        public static void Initialize(IMonitor monitor, ModConfig config, IModHelper helper)
        {
            Monitor = monitor;
            Config = ModEntry.config;
            PHelper = ModEntry.PHelper;
        }

        public static void TrySpousesKiss(GameLocation location)
        {

            if ( location == null || Game1.eventUp || Game1.activeClickableMenu != null || (!ModEntry.config.AllowNPCSpousesToKiss && !ModEntry.config.AllowPlayerSpousesToKiss && !ModEntry.config.AllowNPCRelativesToHug))
                return;

            elapsedSeconds++;

            Farmer player = Game1.player;

            var characters = location.characters;
            if (characters == null)
                return;

            List<NPC> list = characters.ToList();

            Misc.ShuffleList(ref list);

            foreach (NPC npc1 in list)
            {
                if (!npc1.datable.Value && !Config.AllowNonDateableNPCsToHugAndKiss)
                    continue;

                foreach (NPC npc2 in list)
                {
                    if (npc1.Name == npc2.Name)
                        continue;

                    if (!npc2.datable.Value && !Config.AllowNonDateableNPCsToHugAndKiss)
                        continue;

                    if (lastKissed.ContainsKey(npc1.Name) && elapsedSeconds - lastKissed[npc1.Name] <= Config.MinSpouseKissIntervalSeconds)
                        continue;

                    if (lastKissed.ContainsKey(npc2.Name) && elapsedSeconds - lastKissed[npc2.Name] <= Config.MinSpouseKissIntervalSeconds)
                        continue;

                    bool npcMarriageKiss = Misc.AreNPCsMarried(npc1.Name, npc2.Name) && ModEntry.config.AllowNPCSpousesToKiss;
                    bool npcRelatedHug = Misc.AreNPCsRelated(npc1.Name, npc2.Name) && ModEntry.config.AllowNPCRelativesToHug;
                    bool playerSpouseKiss = ModEntry.config.AllowPlayerSpousesToKiss &&
                        npc1.getSpouse() != null && npc2.getSpouse() != null &&
                        player.friendshipData.ContainsKey(npc1.Name) && player.friendshipData.ContainsKey(npc2.Name) &&
                        (ModEntry.config.RoommateKisses || !player.friendshipData[npc1.Name].RoommateMarriage) && (ModEntry.config.RoommateKisses || !player.friendshipData[npc2.Name].RoommateMarriage) &&
                        player.getFriendshipHeartLevelForNPC(npc1.Name) >= ModEntry.config.MinHeartsForKiss && player.getFriendshipHeartLevelForNPC(npc2.Name) >= ModEntry.config.MinHeartsForKiss &&
                        (ModEntry.config.AllowRelativesToKiss || !Misc.AreNPCsRelated(npc1.Name, npc2.Name));

                    // check if spouses
                    if (!npcMarriageKiss && !npcRelatedHug && !playerSpouseKiss)
                        continue;

                    float distance = Vector2.Distance(npc1.position, npc2.position);
                    if (
                        distance < ModEntry.config.MaxDistanceToKiss
                        && !npc1.isSleeping.Value
                        && !npc2.isSleeping.Value
                        && ModEntry.myRand.NextDouble() < ModEntry.config.SpouseKissChance0to1
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

                        if (playerSpouseKiss || npcMarriageKiss)
                        {
                            if (ModEntry.config.CustomKissSound.Length > 0 && kissEffect != null)
                            {
                                float playerDistance = 1f / ((Vector2.Distance(midpoint, Game1.player.position) / 256) + 1);
                                float pan = (float)(Math.Atan((midpoint.X - Game1.player.position.X) / Math.Abs(midpoint.Y - Game1.player.position.Y)) / (Math.PI / 2));
                                //ModEntry.PMonitor.Log($"kiss distance: {distance} pan: {pan}");
                                kissEffect.Play(playerDistance * Game1.options.soundVolumeLevel, 0, pan);
                            }
                            else
                            {
                                Game1.currentLocation.playSound("dwop", NetAudio.SoundContext.NPC);
                            }
                        }
                        else if (ModEntry.config.CustomHugSound.Length > 0 && hugEffect != null)
                        PerformKiss(npc1, midpoint, npc2.Name);
                        PerformKiss(npc2, midpoint, npc1.Name);

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
        

        private static void PerformKiss(NPC npc, Vector2 midpoint, string partner)
        {
            int spouseFrame = -1;
            bool facingRight = true;
            string name = npc.Name;
            List<string> customFrames = Config.CustomKissFrames.Split(',').ToList();
            foreach(string nameframe in customFrames)
            {
                if (nameframe.StartsWith(name + ":"))
                {
                    int.TryParse(nameframe.Substring(name.Length + 1), out spouseFrame);
                    break;
                }
            }
            if(spouseFrame == -1)
            {
                switch (name)
                {
                    case "Sam":
                        spouseFrame = 36;
                        facingRight = true;
                        break;
                    case "Penny":
                        spouseFrame = 35;
                        facingRight = true;
                        break;
                    case "Sebastian":
                        spouseFrame = 40;
                        facingRight = false;
                        break;
                    case "Alex":
                        spouseFrame = 42;
                        facingRight = true;
                        break;
                    case "Krobus":
                        spouseFrame = 16;
                        facingRight = true;
                        break;
                    case "Maru":
                        spouseFrame = 28;
                        facingRight = false;
                        break;
                    case "Emily":
                        spouseFrame = 33;
                        facingRight = false;
                        break;
                    case "Harvey":
                        spouseFrame = 31;
                        facingRight = false;
                        break;
                    case "Shane":
                        spouseFrame = 34;
                        facingRight = false;
                        break;
                    case "Elliott":
                        spouseFrame = 35;
                        facingRight = false;
                        break;
                    case "Leah":
                        spouseFrame = 25;
                        facingRight = true;
                        break;
                    case "Abigail":
                        spouseFrame = 33;
                        facingRight = false;
                        break;
                    default:
                        spouseFrame = 28;
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
            npc.doEmote(20, true);
            npc.Sprite.UpdateSourceRect();
        }

    }
}