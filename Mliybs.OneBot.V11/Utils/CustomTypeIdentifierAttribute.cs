using System;
using System.Collections.Generic;
using System.Text;

namespace Mliybs.OneBot.V11.Utils
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CustomTypeIdentifierAttribute(string name) : Attribute
    {
        public string Name => name;
    }
}
