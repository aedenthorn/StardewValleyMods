using System.Collections.Generic;

namespace StardewVN
{
    public class VNAction
    {
        public List<VNVariableChange> Variables { get; set; }
        public string Sound { get; set; }
        public string Scene { get; set; }
        public string Background { get; set; }
        public List<VNOption> Dialogue { get; set; }
        public List<VNOption> Objects { get; set; }
    }
}