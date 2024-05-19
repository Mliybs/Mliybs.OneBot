using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

namespace Mliybs.OneBot.V11.Data.Messages
{
    public abstract class MessageBase
    {
        [JsonPropertyName("type")]
        public abstract string Type { get; }

        public virtual string GetCQCode()
        {
            var builder = new StringBuilder();
            builder.Append($"[CQ:{Type}");
            var props = GetType().GetProperty("Data")?.PropertyType.GetProperties();
            if (props != null)
                foreach (var prop in props)
                {
                    var obj = GetType().GetProperty("Data")?.GetValue(this);
                    var name = (prop.GetCustomAttribute(typeof(JsonPropertyNameAttribute)) as JsonPropertyNameAttribute)?.Name;
                    if (obj is null || name is null) continue;
                    var value = prop.GetValue(obj);
                    if (value != null) builder.Append($@",{name}={value.ToString()?
                        .Replace("&", "&amp;")
                        .Replace("[", "&#91;")
                        .Replace("]", "&#93;")
                        .Replace(",", "&#44;")}");
                }
            builder.Append(']');
            return builder.ToString();
        }
    }
}
