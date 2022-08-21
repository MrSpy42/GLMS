using System;

namespace GLMS.Networking
{
    public class Message 
    {
        public bool IsHandshake = false;
        public bool toReply = false;
        public bool IsSessionStarter = false;
        public string Username = "none";
        public string Text = "nothing";
    }

    public static class MessageSerializer
    {
        public static Message GetClassFromString(string s)
        {
            var sr = new StringReader(s);
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(Message));

            object? obj = xs.Deserialize(sr);
            if (obj is Message mm)
            {
                return mm;
            }

            throw new Exception("Failed to Deserialize");
        }
        
        public static string GetStringFromClass(Message hs)
        {
            string keyString;
            {
                var sw = new StringWriter();
                var xs = new System.Xml.Serialization.XmlSerializer(typeof(Message));
                xs.Serialize(sw, hs);
                keyString = sw.ToString();
            }

            return keyString;
        }
    }
}
