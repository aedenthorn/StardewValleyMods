using StardewValley;
using System.Collections.Generic;

namespace StardewVN
{
    public enum VNTest
    {
        Equals,
        EqualsVariable,
        MoreThan,
        LessThan,
        MoreThanVariable,
        LessThanVariable
    }
    public class VNRequirement
    {
        public List<VNRequirement> AND;
        public List<VNRequirement> OR;
        public List<VNRequirement> XOR;
        public List<VNRequirement> NOT;
        public string Variable { get; set; }  
        public VNTest Test { get; set; } = VNTest.Equals;
        public object Value;
    }
}