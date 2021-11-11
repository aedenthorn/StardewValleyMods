using StardewValley;
using System.Collections.Generic;
using System.Linq;

namespace CustomOreNodes
{
    public class CustomOreNodesAPI
    {
        public int GetCustomOreNodeIndex(string id)
        {
            var node = ModEntry.customOreNodesList.Find(n => n.id == id);
            if (node == null)
                return -1;
            return node.parentSheetIndex;
        }
        public List<object> GetCustomOreNodes()
        {
            return new List<object>(ModEntry.customOreNodesList);
        }
        public List<string> GetCustomOreNodeIDs() 
        {
            return ModEntry.customOreNodesList.Select(n => n.id).ToList();
        }
    }
}