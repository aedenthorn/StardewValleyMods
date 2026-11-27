namespace StardewVN
{
    public enum VNVariableType
    {
        String,
        Integer,
        Decimal,
        Boolean
    }
    public class VNVariable
    {
        public VNVariableType Type;
        public object Value;
    }
}