using System;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;

namespace Endless.Gameplay.Serialization
{
	// Token: 0x020004C4 RID: 1220
	internal class CharacterVisualsReferenceJsonConverter : JsonConverter<CharacterVisualsReference>
	{
		// Token: 0x06001E5C RID: 7772 RVA: 0x0008452B File Offset: 0x0008272B
		public override void WriteJson(JsonWriter writer, CharacterVisualsReference value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001E5D RID: 7773 RVA: 0x00084700 File Offset: 0x00082900
		public override CharacterVisualsReference ReadJson(JsonReader reader, Type objectType, CharacterVisualsReference existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			CharacterVisualsReference characterVisualsReference = new CharacterVisualsReference();
			while (reader.Read() && reader.TokenType != JsonToken.EndObject)
			{
				string text = (string)reader.Value;
				SerializableGuid serializableGuid = SerializableGuid.Empty;
				if (text == "AssetId")
				{
					serializableGuid = reader.ReadAsString();
				}
				else if (text == "Id")
				{
					serializableGuid = reader.ReadAsString();
				}
				InspectorReferenceUtility.SetId(characterVisualsReference, serializableGuid);
			}
			return characterVisualsReference;
		}
	}
}
