using System;
using Endless.Gameplay.Serialization;
using Newtonsoft.Json;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020000CD RID: 205
	[Serializable]
	public class MemberChange : INetworkSerializable
	{
		// Token: 0x0600041B RID: 1051 RVA: 0x000030D2 File Offset: 0x000012D2
		public MemberChange()
		{
		}

		// Token: 0x0600041C RID: 1052 RVA: 0x00016842 File Offset: 0x00014A42
		public MemberChange(string memberName, int dataType, string jsonData)
		{
			this.MemberName = memberName;
			this.DataType = dataType;
			this.JsonData = jsonData;
		}

		// Token: 0x0600041D RID: 1053 RVA: 0x0001685F File Offset: 0x00014A5F
		public MemberChange Copy()
		{
			return new MemberChange(this.MemberName, this.DataType, this.JsonData);
		}

		// Token: 0x0600041E RID: 1054 RVA: 0x00016878 File Offset: 0x00014A78
		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref this.MemberName, false);
			serializer.SerializeValue<int>(ref this.DataType, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue(ref this.JsonData, false);
		}

		// Token: 0x0600041F RID: 1055 RVA: 0x000168B8 File Offset: 0x00014AB8
		public override string ToString()
		{
			return string.Format("{{ {0}: {1}, {2}: {3}, {4}: {5} }}", new object[] { "MemberName", this.MemberName, "DataType", this.DataType, "JsonData", this.JsonData });
		}

		// Token: 0x06000420 RID: 1056 RVA: 0x00016910 File Offset: 0x00014B10
		public object ToObject()
		{
			string text = string.Empty;
			Type type = null;
			object obj2;
			try
			{
				text = EndlessTypeMapping.Instance.GetAssemblyQualifiedTypeName(this.DataType);
				type = Type.GetType(text);
				object obj = JsonConvert.DeserializeObject(this.JsonData, type);
				obj2 = (obj.GetType().IsEnum ? ((int)obj) : obj);
			}
			catch (Exception)
			{
				Debug.LogError(string.Format("Error Converting memberChange to object {0}, DataType: {1} assemblyQualifiedTypeName: {2} dataType {3} JsonData {4}", new object[] { this.MemberName, this.DataType, text, type, this.JsonData }));
				throw;
			}
			return obj2;
		}

		// Token: 0x040003A2 RID: 930
		public string MemberName;

		// Token: 0x040003A3 RID: 931
		public int DataType;

		// Token: 0x040003A4 RID: 932
		public string JsonData;
	}
}
