using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Creator.LevelEditing.Runtime;

public static class PropEraseDataNetworkExtensions
{
	public static void ReadValueSafe(this FastBufferReader reader, out PropEraseData[] propEraseDatas)
	{
		reader.ReadValueSafe(out int value, default(FastBufferWriter.ForPrimitives));
		propEraseDatas = new PropEraseData[value];
		for (int i = 0; i < value; i++)
		{
			PropEraseData propEraseData = new PropEraseData();
			reader.ReadValueSafe(out Vector3 value2);
			reader.ReadValueSafe(out Quaternion value3);
			reader.ReadValueSafe(out SerializableGuid value4, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out SerializableGuid value5, default(FastBufferWriter.ForNetworkSerializable));
			propEraseData.Position = value2;
			propEraseData.Rotation = value3;
			propEraseData.InstanceId = value4;
			propEraseData.AssetId = value5;
			propEraseDatas[i] = propEraseData;
		}
	}

	public static void WriteValueSafe(this FastBufferWriter writer, in PropEraseData[] propEraseDatas)
	{
		writer.WriteValueSafe<int>(propEraseDatas.Length, default(FastBufferWriter.ForPrimitives));
		for (int i = 0; i < propEraseDatas.Length; i++)
		{
			writer.WriteValueSafe(propEraseDatas[i].Position);
			writer.WriteValueSafe(propEraseDatas[i].Rotation);
			writer.WriteValueSafe<SerializableGuid>(propEraseDatas[i].InstanceId, default(FastBufferWriter.ForNetworkSerializable));
			writer.WriteValueSafe<SerializableGuid>(propEraseDatas[i].AssetId, default(FastBufferWriter.ForNetworkSerializable));
		}
	}
}
