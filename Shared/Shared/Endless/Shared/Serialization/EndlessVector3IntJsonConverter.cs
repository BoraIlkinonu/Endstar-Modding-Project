using System;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace Endless.Shared.Serialization
{
	// Token: 0x020000E5 RID: 229
	public class EndlessVector3IntJsonConverter : JsonConverter<Vector3Int>
	{
		// Token: 0x0600059B RID: 1435 RVA: 0x000181B4 File Offset: 0x000163B4
		public override Vector3Int ReadJson(JsonReader reader, Type objectType, Vector3Int existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			Vector3Int vector3Int = default(Vector3Int);
			string[] array = (reader.Value as string).Split(',', StringSplitOptions.None);
			for (int i = 0; i < array.Length; i++)
			{
				vector3Int[i] = int.Parse(array[i]);
			}
			return vector3Int;
		}

		// Token: 0x0600059C RID: 1436 RVA: 0x000181FC File Offset: 0x000163FC
		public override void WriteJson(JsonWriter writer, Vector3Int value, JsonSerializer serializer)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(value.x);
			stringBuilder.Append(",");
			stringBuilder.Append(value.y);
			stringBuilder.Append(",");
			stringBuilder.Append(value.z);
			writer.WriteValue(stringBuilder.ToString());
		}
	}
}
