using System;

namespace XSC
{
    using Newtonsoft.Json;
    
    public class HexJsonConverter : JsonConverter
    {
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(byte[])) return true;

            return false;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string s = reader.Value.ToString();

            return s.FromHex();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((byte[])value).ToHex());
        }
    }
}
