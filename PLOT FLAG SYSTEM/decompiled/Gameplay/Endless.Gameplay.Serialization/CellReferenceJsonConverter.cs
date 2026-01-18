using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Endless.Gameplay.Serialization;

internal class CellReferenceJsonConverter : JsonConverter<CellReference>
{
	public override void WriteJson(JsonWriter writer, CellReference value, JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}

	public static CellReference ReadSingleCellReference(JsonReader reader)
	{
		Vector3 value = default(Vector3);
		bool flag = true;
		float? rotation = null;
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = (string)reader.Value;
			if (text == "Cell")
			{
				while (reader.Read() && reader.TokenType != JsonToken.EndObject)
				{
					string text2 = null;
					if (reader.TokenType == JsonToken.PropertyName)
					{
						text2 = (string)reader.Value;
					}
					switch (text2)
					{
					case "x":
						flag = false;
						value.x = reader.ReadAsInt32().Value;
						break;
					case "y":
						value.y = reader.ReadAsInt32().Value;
						break;
					case "z":
						value.z = reader.ReadAsInt32().Value;
						break;
					}
				}
			}
			else if (text == "Rot")
			{
				double? num = reader.ReadAsDouble();
				if (num.HasValue)
				{
					rotation = (float)num.Value;
				}
			}
		}
		CellReference cellReference = new CellReference();
		cellReference.SetCell(flag ? ((Vector3?)null) : new Vector3?(value), rotation);
		return cellReference;
	}

	public override CellReference ReadJson(JsonReader reader, Type objectType, CellReference existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		return ReadSingleCellReference(reader);
	}
}
