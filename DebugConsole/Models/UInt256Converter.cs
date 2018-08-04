namespace UChainDB.BingChain.Contracts.Json
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Linq;
    using UChainDB.Example.Chain.Entity;

    public class UInt256Converter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(UInt256);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            if (token.Type == JTokenType.Array)
            {
                return token.ToObject<string[]>()
                    .Select(_ => UInt256.Parse(_))
                    .ToArray();
            }
            var value = token.ToObject<string>();

            return value == null ? null : UInt256.Parse(value);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            switch (value)
            {
                case UInt256 single:
                    writer.WriteValue(single.ToHex());
                    break;

                case UInt256[] multiple:
                    writer.WriteStartArray();
                    foreach (var item in multiple)
                    {
                        serializer.Serialize(writer, item.ToHex());
                    }
                    writer.WriteEndArray();
                    break;

                default:
                    break;
            }
        }
    }
}