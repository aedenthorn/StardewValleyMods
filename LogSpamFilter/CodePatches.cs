using System;

namespace LogSpamFilter
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {

        public static bool LogImpl_Prefix(string source, string message, object level)
        {
            if (!Config.EnableMod || allowList.Contains(source))
                return true;
            if (messageData.ContainsKey(source))
            {
                var s = messageData[source];
                if (s.throttled)
                {
                    if (Config.MSSpawnThrottle > DateTime.Now.Subtract(s.lastMessageTime).TotalMilliseconds)
                        return false;
                    messageData[source].throttled = false;
                }
                if (Config.MSBetweenMessages > 0 && Config.MSBetweenMessages > DateTime.Now.Subtract(s.lastMessageTime).TotalMilliseconds)
                {
                    if (Config.IsDebug)
                        SMonitor.Log($"Throttling {source} for ms between messages");
                    messageData[source].throttled = true;
                    messageData[source].throttledTime = DateTime.Now;
                    return false;
                }
                if (Config.MSBetweenIdenticalMessages > 0 && s.lastMessage == message && Config.MSBetweenIdenticalMessages > DateTime.Now.Subtract(s.lastMessageTime).TotalMilliseconds)
                {
                    if (Config.IsDebug)
                        SMonitor.Log($"Throttling {source} for ms between identical messages");

                    messageData[source].throttled = true;
                    messageData[source].throttledTime = DateTime.Now;
                    return false;
                }
                if (Config.MSBetweenSimilarMessages > 0 && s.lastMessage.StartsWith(message[0]) && Config.MSBetweenSimilarMessages > DateTime.Now.Subtract(s.lastMessageTime).TotalMilliseconds)
                {
                    int count = 0;
                    for(int i = 0; i < s.lastMessage.Length; i++)
                    {
                        if (message.Length <= i)
                            break;
                        if (s.lastMessage[i] == message[i])
                            count++;
                    }
                    if(count / (float)s.lastMessage.Length >= Config.PercentSimilarity / 100f)
                    {
                        if (Config.IsDebug)
                            SMonitor.Log($"Throttling {source} for ms between similar ({(int)(count / (float)s.lastMessage.Length * 100)}) messages");
                        messageData[source].throttled = true;
                        messageData[source].throttledTime = DateTime.Now;
                        return false;
                    }
                }

            }
            messageData[source] = new ModMessageData()
            {
                lastMessage = message,
                lastMessageTime = DateTime.Now,
                lastMessageType = (int)level
            };
            return true;
        }
    }
}