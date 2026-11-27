using Microsoft.Xna.Framework.Audio;
using StardewValley;

namespace HugsAndKisses
{
    public interface IKissingAPI
    {
        public void PlayerNPCKiss(Farmer farmer, NPC npc);
        public void NPCKiss(NPC kisser, NPC kissee);
        public SoundEffect GetKissSound();
        public SoundEffect GetHugSound();
        public int LastKissed(string name);
    }
}