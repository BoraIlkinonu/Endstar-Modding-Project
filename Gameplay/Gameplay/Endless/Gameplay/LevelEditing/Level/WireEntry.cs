using System;
using System.Linq;
using Endless.Gameplay.Serialization;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;
using Unity.Netcode;

namespace Endless.Gameplay.LevelEditing.Level
{
	// Token: 0x02000555 RID: 1365
	[Serializable]
	public class WireEntry : INetworkSerializable
	{
		// Token: 0x17000647 RID: 1607
		// (get) Token: 0x060020E6 RID: 8422 RVA: 0x0009446F File Offset: 0x0009266F
		[JsonIgnore]
		public string EmitterComponentAssemblyQualifiedTypeName
		{
			get
			{
				if (this.EmitterComponentTypeId != -1)
				{
					return EndlessTypeMapping.Instance.GetAssemblyQualifiedTypeName(this.EmitterComponentTypeId);
				}
				return null;
			}
		}

		// Token: 0x17000648 RID: 1608
		// (get) Token: 0x060020E7 RID: 8423 RVA: 0x0009448C File Offset: 0x0009268C
		[JsonIgnore]
		public string ReceiverComponentAssemblyQualifiedTypeName
		{
			get
			{
				if (this.ReceiverComponentTypeId != -1)
				{
					return EndlessTypeMapping.Instance.GetAssemblyQualifiedTypeName(this.ReceiverComponentTypeId);
				}
				return null;
			}
		}

		// Token: 0x060020E8 RID: 8424 RVA: 0x000944AC File Offset: 0x000926AC
		public WireEntry Copy(bool generateNewUniqueIds)
		{
			WireEntry wireEntry = new WireEntry();
			wireEntry.WireId = (generateNewUniqueIds ? SerializableGuid.NewGuid() : this.WireId);
			wireEntry.EmitterComponentTypeId = this.EmitterComponentTypeId;
			wireEntry.ReceiverComponentTypeId = this.ReceiverComponentTypeId;
			wireEntry.EmitterMemberName = this.EmitterMemberName;
			wireEntry.ReceiverMemberName = this.ReceiverMemberName;
			wireEntry.StaticParameters = this.StaticParameters.Select((StoredParameter storedParam) => storedParam.Copy()).ToArray<StoredParameter>();
			wireEntry.WireColor = this.WireColor;
			return wireEntry;
		}

		// Token: 0x060020E9 RID: 8425 RVA: 0x00094548 File Offset: 0x00092748
		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue<SerializableGuid>(ref this.WireId, default(FastBufferWriter.ForNetworkSerializable));
			serializer.SerializeValue<int>(ref this.EmitterComponentTypeId, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<int>(ref this.ReceiverComponentTypeId, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue(ref this.EmitterMemberName, false);
			serializer.SerializeValue(ref this.ReceiverMemberName, false);
			serializer.SerializeValue<int>(ref this.WireColor, default(FastBufferWriter.ForPrimitives));
			int num = 0;
			if (serializer.IsWriter)
			{
				StoredParameter[] staticParameters = this.StaticParameters;
				num = ((staticParameters != null) ? staticParameters.Length : 0);
			}
			serializer.SerializeValue<int>(ref num, default(FastBufferWriter.ForPrimitives));
			if (serializer.IsReader)
			{
				this.StaticParameters = new StoredParameter[num];
				for (int i = 0; i < this.StaticParameters.Length; i++)
				{
					this.StaticParameters[i] = new StoredParameter();
				}
			}
			for (int j = 0; j < num; j++)
			{
				serializer.SerializeValue<StoredParameter>(ref this.StaticParameters[j], default(FastBufferWriter.ForNetworkSerializable));
			}
		}

		// Token: 0x060020EA RID: 8426 RVA: 0x00094658 File Offset: 0x00092858
		public override string ToString()
		{
			return string.Format("{0} -> {1}, {2} static params.", this.EmitterMemberName, this.ReceiverMemberName, this.StaticParameters.Length);
		}

		// Token: 0x04001A32 RID: 6706
		public SerializableGuid WireId = SerializableGuid.Empty;

		// Token: 0x04001A33 RID: 6707
		public int EmitterComponentTypeId;

		// Token: 0x04001A34 RID: 6708
		public int ReceiverComponentTypeId;

		// Token: 0x04001A35 RID: 6709
		public string EmitterMemberName;

		// Token: 0x04001A36 RID: 6710
		public string ReceiverMemberName;

		// Token: 0x04001A37 RID: 6711
		public StoredParameter[] StaticParameters = Array.Empty<StoredParameter>();

		// Token: 0x04001A38 RID: 6712
		public int WireColor;
	}
}
