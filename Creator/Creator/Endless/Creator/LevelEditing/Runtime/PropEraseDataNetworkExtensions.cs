using System;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Creator.LevelEditing.Runtime
{
	// Token: 0x0200034F RID: 847
	public static class PropEraseDataNetworkExtensions
	{
		// Token: 0x06000FF8 RID: 4088 RVA: 0x0004A168 File Offset: 0x00048368
		public static void ReadValueSafe(this FastBufferReader reader, out PropEraseData[] propEraseDatas)
		{
			int num;
			reader.ReadValueSafe<int>(out num, default(FastBufferWriter.ForPrimitives));
			propEraseDatas = new PropEraseData[num];
			for (int i = 0; i < num; i++)
			{
				PropEraseData propEraseData = new PropEraseData();
				Vector3 vector;
				reader.ReadValueSafe(out vector);
				Quaternion quaternion;
				reader.ReadValueSafe(out quaternion);
				SerializableGuid serializableGuid;
				reader.ReadValueSafe<SerializableGuid>(out serializableGuid, default(FastBufferWriter.ForNetworkSerializable));
				SerializableGuid serializableGuid2;
				reader.ReadValueSafe<SerializableGuid>(out serializableGuid2, default(FastBufferWriter.ForNetworkSerializable));
				propEraseData.Position = vector;
				propEraseData.Rotation = quaternion;
				propEraseData.InstanceId = serializableGuid;
				propEraseData.AssetId = serializableGuid2;
				propEraseDatas[i] = propEraseData;
			}
		}

		// Token: 0x06000FF9 RID: 4089 RVA: 0x0004A200 File Offset: 0x00048400
		public static void WriteValueSafe(this FastBufferWriter writer, in PropEraseData[] propEraseDatas)
		{
			int num = propEraseDatas.Length;
			writer.WriteValueSafe<int>(in num, default(FastBufferWriter.ForPrimitives));
			for (int i = 0; i < propEraseDatas.Length; i++)
			{
				Vector3 position = propEraseDatas[i].Position;
				writer.WriteValueSafe(in position);
				Quaternion rotation = propEraseDatas[i].Rotation;
				writer.WriteValueSafe(in rotation);
				SerializableGuid serializableGuid = propEraseDatas[i].InstanceId;
				writer.WriteValueSafe<SerializableGuid>(in serializableGuid, default(FastBufferWriter.ForNetworkSerializable));
				serializableGuid = propEraseDatas[i].AssetId;
				writer.WriteValueSafe<SerializableGuid>(in serializableGuid, default(FastBufferWriter.ForNetworkSerializable));
			}
		}
	}
}
