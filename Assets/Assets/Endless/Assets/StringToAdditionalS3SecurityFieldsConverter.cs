using System;
using Newtonsoft.Json;

namespace Endless.Assets
{
	// Token: 0x0200000E RID: 14
	public class StringToAdditionalS3SecurityFieldsConverter : JsonConverter
	{
		// Token: 0x06000034 RID: 52 RVA: 0x00002577 File Offset: 0x00000777
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, value);
		}

		// Token: 0x06000035 RID: 53 RVA: 0x00002584 File Offset: 0x00000784
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}
			if (reader.TokenType == JsonToken.String)
			{
				string text = reader.Value as string;
				if (text != null)
				{
					return JsonConvert.DeserializeObject<AdditionalS3SecurityFields>(text);
				}
			}
			throw new JsonException(string.Format("Expected string when reading AdditionalS3SecurityFields type, got '{0}' <{1}>.", reader.TokenType, reader.Value));
		}

		// Token: 0x06000036 RID: 54 RVA: 0x000025DD File Offset: 0x000007DD
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(AdditionalS3SecurityFields);
		}
	}
}
