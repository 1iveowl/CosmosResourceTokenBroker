using System;
using System.IO;
using System.Text;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;


namespace CosmosResourceTokenClient.JsonSerialize
{
    // https://stackoverflow.com/a/57066220/4140832
    [Preserve(AllMembers = true)]
    public class CosmosJsonNetSerializer : CosmosSerializer
    {
        private readonly Encoding _defaultEncoding;

        private readonly JsonSerializer _serializer;


        public CosmosJsonNetSerializer() //: this(new JsonSerializerSettings())
        {
            throw new Exception("Must be constructed with serializer settings");
        }

        public CosmosJsonNetSerializer(JsonSerializerSettings serializerSettings)
        {
            _serializer = JsonSerializer.Create(serializerSettings);

            _defaultEncoding = new UTF8Encoding(false, true);
        }

        public override T FromStream<T>(Stream stream)
        {
            if (typeof(Stream).IsAssignableFrom(typeof(T)))
            {
                return (T)(object)stream;
            }

            using var streamReader = new StreamReader(stream);
            using var jsonTextReader = new JsonTextReader(streamReader);
            
            return _serializer.Deserialize<T>(jsonTextReader);
        }

        public override Stream ToStream<T>(T input)
        {
            var streamPayload = new MemoryStream();
            using var streamWriter = new StreamWriter(streamPayload, encoding: _defaultEncoding, bufferSize: 1024, leaveOpen: true);
            using JsonWriter writer = new JsonTextWriter(streamWriter);
            
            writer.Formatting = Formatting.None;
            
            _serializer.Serialize(writer, input);
            
            writer.Flush();
            
            streamWriter.Flush();

            streamPayload.Position = 0;
            
            return streamPayload;
        }
    }
}