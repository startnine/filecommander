using RibbonFileManager.Views;
using System;
using System.Collections;
using System.Linq;

namespace RibbonFileManager
{
    public class ReceiverEntry : IReceiverEntry
    {
        public ReceiverEntry(String name)
        {
            FriendlyName = name;
        }

        public String FriendlyName { get; }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
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