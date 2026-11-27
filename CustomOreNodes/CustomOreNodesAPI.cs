using StardewValley;
using System.Collections.Generic;
using System.Linq;

namespace CustomOreNodes
{
    public interface ICustomOreNodeAPI
    {
        public ICustomOreNode GetCustomOreNode(string itemId);
        public List<ICustomOreNode> GetCustomOreNodes();

    }
    public class CustomOreNodesAPI : ICustomOreNodeAPI
    {
        public ICustomOreNode GetCustomOreNode(string itemId)
        {
            return ModEntry.customOreNodesList.Find(n => n.itemId == itemId);
        }
        public void AddCustomOreNode(ICustomOreNode node)
        {
            ModEntry.customOreNodesList.Add(node);
        }
        public List<ICustomOreNode> GetCustomOreNodes()
        {
            return new List<ICustomOreNode>(ModEntry.customOreNodesList);
        }
    }
}