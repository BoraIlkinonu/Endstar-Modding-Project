using System;
using Endless.Gameplay.Serialization;
using Unity.Netcode;

namespace Endless.Gameplay.LevelEditing.Level
{
	// Token: 0x0200053F RID: 1343
	[Serializable]
	public class StoredParameter : INetworkSerializable
	{
		// Token: 0x06002067 RID: 8295 RVA: 0x00092072 File Offset: 0x00090272
		public StoredParameter Copy()
		{
			return new StoredParameter
			{
				DataType = this.DataType,
				JsonData = this.JsonData
			};
		}

		// Token: 0x06002068 RID: 8296 RVA: 0x00092091 File Offset: 0x00090291
		public Type GetReferencedType()
		{
			return Type.GetType(EndlessTypeMapping.Instance.GetAssemblyQualifiedTypeName(this.DataType));
		}

		// Token: 0x06002069 RID: 8297 RVA: 0x000920A8 File Offset: 0x000902A8
		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue<int>(ref this.DataType, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue(ref this.JsonData, false);
		}

		// Token: 0x040019EB RID: 6635
		public int DataType;

		// Token: 0x040019EC RID: 6636
		public string JsonData;
	}
}
