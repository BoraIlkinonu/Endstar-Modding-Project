using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Endless.Gameplay.Serialization
{
	// Token: 0x020004C0 RID: 1216
	internal class CellReferenceJsonConverter : JsonConverter<CellReference>
	{
		// Token: 0x06001E51 RID: 7761 RVA: 0x0008452B File Offset: 0x0008272B
		public override void WriteJson(JsonWriter writer, CellReference value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001E52 RID: 7762 RVA: 0x00084534 File Offset: 0x00082734
		public static CellReference ReadSingleCellReference(JsonReader reader)
		{
			Vector3 vector = default(Vector3);
			bool flag = true;
			float? num = null;
			while (reader.Read() && reader.TokenType != JsonToken.EndObject)
			{
				string text = (string)reader.Value;
				if (text == "Cell")
				{
					while (reader.Read())
					{
						if (reader.TokenType == JsonToken.EndObject)
						{
							break;
						}
						string text2 = null;
						if (reader.TokenType == JsonToken.PropertyName)
						{
							text2 = (string)reader.Value;
						}
						if (text2 == "x")
						{
							flag = false;
							vector.x = (float)reader.ReadAsInt32().Value;
						}
						else if (text2 == "y")
						{
							vector.y = (float)reader.ReadAsInt32().Value;
						}
						else if (text2 == "z")
						{
							vector.z = (float)reader.ReadAsInt32().Value;
						}
					}
				}
				else if (text == "Rot")
				{
					double? num2 = reader.ReadAsDouble();
					if (num2 != null)
					{
						num = new float?((float)num2.Value);
					}
				}
			}
			CellReference cellReference = new CellReference();
			cellReference.SetCell(flag ? null : new Vector3?(vector), num);
			return cellReference;
		}

		// Token: 0x06001E53 RID: 7763 RVA: 0x00084681 File Offset: 0x00082881
		public override CellReference ReadJson(JsonReader reader, Type objectType, CellReference existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			return CellReferenceJsonConverter.ReadSingleCellReference(reader);
		}
	}
}
