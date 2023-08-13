using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WSM.Client.Configuration
{
    public class TypeSelectorConverter<TInterface> : JsonConverter
    {
        private Dictionary<string, Type> _mappings;
        public TypeSelectorConverter(Dictionary<string, Type> mappings)
        {
            _mappings = mappings;
        }
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TInterface);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);
            var typeKey = obj["Type"].ToString();
            if (!_mappings.ContainsKey(typeKey))
            {
                throw new Exception($"Key {typeKey} not found, please add mapping");
            }
            Type type = _mappings[typeKey];
            object instance = Activator.CreateInstance(type);
            if (instance != null)
            {
                serializer.Populate(obj.CreateReader(), instance);
            }

            return instance;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
