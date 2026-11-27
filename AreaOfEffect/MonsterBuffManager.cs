using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;

namespace AreaOfEffect
{
    public class MonsterBuffManager
    {
        public List<Buff> buffs = new();
        private Monster monster;
        public MonsterBuffManager(Monster m)
        {
            monster = m;
        }
        public void AddBuff(string id)
        {
            if(id == "19")
            {
                monster.stunTime.Value = 4000;
            }
            buffs.Add(new Buff(id, null, null, -1, null, -1, null, null, null, null));
        }
        public bool Update(GameTime time)
        {
            bool result = true;
            if (buffs.Count == 0)
                return result;
            for(int i = buffs.Count - 1; i >=0; i--)
            {
                var buff = buffs[i];
                buff.millisecondsDuration -= (int)Math.Ceiling(time.ElapsedGameTime.TotalMilliseconds);
                if(buff.millisecondsDuration <= 0)
                {
                    if (buff.effects.Speed.Value == 0)
                    {
                        monster.addedSpeed = 0;
                    }
                    buffs.RemoveAt(i);
                    continue;
                }
                if (buff.effects.Speed.Value != 0)
                {
                    result = false;
                    monster.addedSpeed = buff.effects.Speed.Value;
                }
            }
            return result;
        }
    }
}