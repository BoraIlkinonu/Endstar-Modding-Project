using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Endless.Gameplay.Serialization
{
	// Token: 0x020004C3 RID: 1219
	internal class CellReferenceArrayJsonConverter : JsonConverter<CellReference[]>
	{
		// Token: 0x06001E59 RID: 7769 RVA: 0x0008452B File Offset: 0x0008272B
		public override void WriteJson(JsonWriter writer, CellReference[] value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001E5A RID: 7770 RVA: 0x000846A8 File Offset: 0x000828A8
		public override CellReference[] ReadJson(JsonReader reader, Type objectType, CellReference[] existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			List<CellReference> list = new List<CellReference>();
			if (reader.TokenType != JsonToken.StartArray)
			{
				throw new JsonSerializationException("Expected start of array");
			}
			while (reader.Read() && reader.TokenType != JsonToken.EndArray)
			{
				CellReference cellReference = CellReferenceJsonConverter.ReadSingleCellReference(reader);
				list.Add(cellReference);
			}
			return list.ToArray();
		}
	}
}
