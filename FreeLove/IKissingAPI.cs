using Microsoft.Xna.Framework.Audio;
using StardewValley;

namespace FreeLove
{
    public interface IKissingAPI
    {
        void FarmerKiss(Farmer farmer, NPC npc);
        void NPCKiss(NPC kisser, NPC kissee);
        SoundEffect GetKissSound();
        bool IsKissing(string name);
    }
}