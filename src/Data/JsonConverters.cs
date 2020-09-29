using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Neuralia.Blockchains.Tools.Data.Arrays;

namespace Neuralia.Blockchains.Tools.Data {
	public class SafeArrayHandleConverter : JsonConverter<SafeArrayHandle> {
		
		public override SafeArrayHandle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => SafeArrayHandle.FromBase64(reader.GetString());
		public override void Write(Utf8JsonWriter writer, SafeArrayHandle array, JsonSerializerOptions options) => array.ToBase64();
	}
	
	public class ByteArrayConverter : JsonConverter<ByteArray> {
		
		public override ByteArray Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => ByteArray.FromBase64(reader.GetString());
		public override void Write(Utf8JsonWriter writer, ByteArray array, JsonSerializerOptions options) => array.ToBase64();
		public override bool CanConvert(Type typeToConvert) => typeof(ByteArray).IsAssignableFrom(typeToConvert);
	}
}