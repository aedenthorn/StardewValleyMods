using System.Collections.Generic;

namespace EventCreator
{
    public enum ParamType
    {
        String,
        Number,
        MapCoord,
        Object,
        Recipe,
        Bool

    }
    public class EventParam
    {
        public ParamType type;
        public List<object> options;
        public bool optional;
        public object value;
    }
}