using RibbonFileManager.View;
using System;
using System.Collections;
using System.Linq;

namespace RibbonFileManager
{
    struct Message : IMessage
    {
        public Message(String message = "", Object o = null)
        {
            Text = message;
            Object = o;
        }

        public static Message Empty { get; } = new Message();

        public String Text { get; }
        public Object Object { get; }
    }

    class RibbonFileManagerConfiguration : IConfiguration
    {
        public IDictionary Entries => GetType().GetFields().Select(f => (f.Name, f.GetValue(this))).ToDictionary(k => k.Name, v => v.Item2);
    }
}