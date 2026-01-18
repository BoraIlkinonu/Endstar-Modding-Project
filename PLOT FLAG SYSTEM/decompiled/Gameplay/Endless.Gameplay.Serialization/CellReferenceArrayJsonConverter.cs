using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Endless.Gameplay.Serialization;

internal class CellReferenceArrayJsonConverter : JsonConverter<CellReference[]>
{
	public override void WriteJson(JsonWriter writer, CellReference[] value, JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}

	public override CellReference[] ReadJson(JsonReader reader, Type objectType, CellReference[] existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		List<CellReference> list = new List<CellReference>();
		if (reader.TokenType != JsonToken.StartArray)
		{
			throw new JsonSerializationException("Expected start of array");
		}
		while (reader.Read() && reader.TokenType != JsonToken.EndArray)
		{
			CellReference item = CellReferenceJsonConverter.ReadSingleCellReference(reader);
			list.Add(item);
		}
		return list.ToArray();
	}
}
