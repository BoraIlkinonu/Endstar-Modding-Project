using System;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;

namespace Endless.Gameplay.Serialization;

internal class CharacterVisualsReferenceJsonConverter : JsonConverter<CharacterVisualsReference>
{
	public override void WriteJson(JsonWriter writer, CharacterVisualsReference value, JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}

	public override CharacterVisualsReference ReadJson(JsonReader reader, Type objectType, CharacterVisualsReference existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		CharacterVisualsReference characterVisualsReference = new CharacterVisualsReference();
		while (reader.Read() && reader.TokenType != JsonToken.EndObject)
		{
			string text = (string)reader.Value;
			SerializableGuid newAssetId = SerializableGuid.Empty;
			if (text == "AssetId")
			{
				newAssetId = reader.ReadAsString();
			}
			else if (text == "Id")
			{
				newAssetId = reader.ReadAsString();
			}
			InspectorReferenceUtility.SetId(characterVisualsReference, newAssetId);
		}
		return characterVisualsReference;
	}
}
