using RibbonFileManager.Views;
using System;
using System.Collections;
using System.Linq;

namespace RibbonFileManager
{
    public class MessageEntry : IMessageEntry
    {
        public MessageEntry(Type type, String name)
        {
            MessageObjectType = type;
            FriendlyName = name;
        }

        public Type MessageObjectType { get; }

        public String FriendlyName { get; }

        public event EventHandler<MessageReceivedEventArgs> MessageSent;
    }

    public class ConfigurationEntry : IConfigurationEntry
    {
        public ConfigurationEntry(Object obj, String name)
        {
            Object = obj;
            FriendlyName = name;
        }

        public Object Object { get; }

        public String FriendlyName { get; }
    }
}