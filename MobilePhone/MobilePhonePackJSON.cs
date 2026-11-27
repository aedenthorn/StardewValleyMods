using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MobilePhone
{
    public class MobilePhonePackJSON
    {
        public List<AppJSON> apps;
        public List<EventInvite> invites;
        public string id;
        public string name;
        public string iconPath;
        public string keyPress;
        public bool closePhone;
    }

    public class AppJSON
    {
        public string id;
        public string name;
        public string iconPath;
        public string dllName;
        public string className;
        public string methodName;
        public string keyPress;
        public bool closePhone;
    }
    public class EventInvite
    {
        public string name;
        public string location;
        public bool date;
        public bool allowMarried = true;
        public bool requireMarried = false;
        public string allowedNPCs;
        public int minPoints;
        public int maxPoints = -1;
        public string season;
        public int dayOfWeek;
        public int minTimeOfDay;
        public int maxTimeOfDay = -1;
        public List<string> incompatibleMods;
        public List<string> requiredMods;
        public List<EventNode> nodes;
        public List<EventFork> forks;
        public bool CanInvite(NPC npc)
        {
            if (npc is null || Game1.player is null)
                return false;
            if(incompatibleMods != null)
            {
                foreach(string mod in incompatibleMods)
                {
                    if (ModEntry.SHelper.ModRegistry.IsLoaded(mod))
                        return false;
                }
            }
            if(requiredMods != null)
            {
                foreach(string mod in requiredMods)
                {
                    if (!ModEntry.SHelper.ModRegistry.IsLoaded(mod))
                        return false;
                }
            }
            if (!Game1.player.friendshipData.TryGetValue(npc.Name, out Friendship f))
                return false;
            if (date &&  !f.IsDating() && !f.IsEngaged() && !f.IsMarried())
                return false;
            if (!allowMarried && (f.IsMarried() || f.IsEngaged()))
                return false;
            if (requireMarried && !f.IsMarried() && !f.IsEngaged())
                return false;
            if (allowedNPCs != null && !allowedNPCs.Split(',').Contains(npc.Name))
                return false;
            if (f.Points < minPoints)
                return false;
            if (maxPoints != -1 && f.Points >= maxPoints)
                return false;
            if (season != null && Game1.currentSeason != season)
                return false;
            if (dayOfWeek != 0 && Game1.dayOfMonth % 7 != dayOfWeek)
                return false;
            if (Game1.timeOfDay < minTimeOfDay)
                return false;
            if (maxTimeOfDay != -1 && Game1.timeOfDay >= maxTimeOfDay)
                return false;
            return true;

        }
    }
    public class EventNode
    {
        public string defaultNode;
        public Dictionary<string, string> customNodes;
        public string GetCustomNode(string name)
        {
            string outNode;
            if (customNodes != null && customNodes.ContainsKey(name))
                outNode = customNodes[name];
            else
                outNode = defaultNode;
            return FormatNode(name, outNode);
        }

        private string FormatNode(string name, string outNode)
        {
            return outNode.Replace("{name}", name);
        }
    }
    public class EventFork
    {
        public string key;
        public List<EventNode> nodes;
    }
}