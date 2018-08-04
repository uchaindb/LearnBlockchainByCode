using System;
using Newtonsoft.Json;
using UChainDB.Example.Chain.Entity;

namespace UChainDB.Example.Chain.DebugConsole.Models
{
    public class ByteArrayConverter<T> : JsonConverter where T : ByteArrayDef, new()
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(ByteArray<T>).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var valEntity = reader.Value;
            if (valEntity == null) return null;
            var val = valEntity.ToString();
            if (typeof(T) == typeof(AddressDef)) return Address.ParseBase58(val);
            if (typeof(T) == typeof(SignatureDef)) return Signature.ParseBase58(val);
            if (typeof(T) == typeof(PrivateKeyDef)) return PrivateKey.ParseBase58(val);
            if (typeof(T) == typeof(PublicKeyDef)) return PublicKey.ParseBase58(val);
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var bytes = value as ByteArray<T>;
            writer.WriteValue(bytes.ToBase58());
        }
    }
}