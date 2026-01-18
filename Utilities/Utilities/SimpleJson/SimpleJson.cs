using System;
using TinyJson;

namespace SimpleJson
{
	// Token: 0x02000002 RID: 2
	public static class SimpleJson
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		public static string SerializeObject(object serializableObject)
		{
			return serializableObject.ToJson();
		}

		// Token: 0x06000002 RID: 2 RVA: 0x00002068 File Offset: 0x00000268
		public static T DeserializeObject<T>(string jsonText)
		{
			return jsonText.FromJson<T>();
		}
	}
}
