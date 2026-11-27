using StardewValley.Monsters;

namespace CustomMonsters
{
    public class CustomBreathProjectile : DinoMonster.BreathProjectile
    {
        public MonsterData data;

        public CustomBreathProjectile(MonsterData data)
        {
            this.data = data;
            
        }
    }
}