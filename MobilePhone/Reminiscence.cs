using StardewValley;
using System.Collections.Generic;

namespace MobilePhone
{
    public class Reminiscence
    {
        public List<Reminisce> events = new List<Reminisce>();

        internal void WeedOutUnseen()
        {
            if (events.Count == 0)
                return;
            for (int i = events.Count - 1; i >= 0; i--)
            {
                string ids = events[i].eventId;
                if(!int.TryParse(ids, out int id))
                {
                    if (!int.TryParse(ids.Split('/')[0], out id))
                        continue;
                }

                if (!Game1.player.eventsSeen.Contains(id))
                    events.RemoveAt(i);
            }
        }
    }

    public class Reminisce
    {
        public string name;
        public string location;
        public string eventId;
    }
}