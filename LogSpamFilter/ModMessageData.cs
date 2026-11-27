using System;
using System.Collections.Generic;

namespace LogSpamFilter
{
    public class ModMessageData
    {
        public string lastMessage;
        public DateTime lastMessageTime;
        public int lastMessageType;
        public bool throttled;
        public DateTime throttledTime;
    }
}