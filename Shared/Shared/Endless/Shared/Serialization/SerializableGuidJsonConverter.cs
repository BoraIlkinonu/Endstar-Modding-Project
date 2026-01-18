using System;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;

namespace Endless.Shared.Serialization
{
	// Token: 0x020000E4 RID: 228
	public class SerializableGuidJsonConverter : JsonConverter<SerializableGuid>
	{
		// Token: 0x06000598 RID: 1432 RVA: 0x00018189 File Offset: 0x00016389
		public override SerializableGuid ReadJson(JsonReader reader, Type objectType, SerializableGuid existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			return new SerializableGuid(reader.Value as string);
		}

		// Token: 0x06000599 RID: 1433 RVA: 0x0001819B File Offset: 0x0001639B
		public override void WriteJson(JsonWriter writer, SerializableGuid value, JsonSerializer serializer)
		{
			writer.WriteValue(value.Guid);
		}
	}
}
