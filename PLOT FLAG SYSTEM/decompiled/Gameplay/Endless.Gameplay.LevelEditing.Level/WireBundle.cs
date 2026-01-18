using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;
using Unity.Netcode;

namespace Endless.Gameplay.LevelEditing.Level;

[Serializable]
public class WireBundle : INetworkSerializable
{
	public SerializableGuid BundleId;

	public SerializableGuid EmitterInstanceId;

	public SerializableGuid ReceiverInstanceId;

	public WireColor WireColor;

	public List<SerializableGuid> RerouteNodeIds = new List<SerializableGuid>();

	public List<WireEntry> Wires { get; set; } = new List<WireEntry>();

	[JsonIgnore]
	public int WiresInBundle => Wires.Count;

	public WireBundle Copy(bool generateNewUniqueIds)
	{
		WireBundle wireBundle = new WireBundle();
		wireBundle.BundleId = (generateNewUniqueIds ? SerializableGuid.NewGuid() : BundleId);
		wireBundle.EmitterInstanceId = EmitterInstanceId;
		wireBundle.ReceiverInstanceId = ReceiverInstanceId;
		wireBundle.WireColor = WireColor;
		foreach (WireEntry wire in Wires)
		{
			wireBundle.Wires.Add(wire.Copy(generateNewUniqueIds));
		}
		return wireBundle;
	}

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref BundleId, default(FastBufferWriter.ForNetworkSerializable));
		serializer.SerializeValue(ref EmitterInstanceId, default(FastBufferWriter.ForNetworkSerializable));
		serializer.SerializeValue(ref ReceiverInstanceId, default(FastBufferWriter.ForNetworkSerializable));
		serializer.SerializeValue(ref WireColor, default(FastBufferWriter.ForEnums));
		SerializeRerouteNodes(serializer);
		SerializeWires(serializer);
	}

	private void SerializeWires<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		int value = 0;
		if (serializer.IsWriter)
		{
			value = Wires.Count;
		}
		serializer.SerializeValue(ref value, default(FastBufferWriter.ForPrimitives));
		WireEntry[] array;
		if (serializer.IsWriter)
		{
			array = Wires.ToArray();
			for (int i = 0; i < value; i++)
			{
				serializer.SerializeValue(ref array[i], default(FastBufferWriter.ForNetworkSerializable));
			}
		}
		else
		{
			array = new WireEntry[value];
			for (int j = 0; j < value; j++)
			{
				array[j] = new WireEntry();
				array[j].NetworkSerialize(serializer);
			}
		}
		if (serializer.IsReader)
		{
			Wires = array.ToList();
		}
	}

	private void SerializeRerouteNodes<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		int value = 0;
		if (serializer.IsWriter)
		{
			value = RerouteNodeIds.Count;
		}
		serializer.SerializeValue(ref value, default(FastBufferWriter.ForPrimitives));
		SerializableGuid[] array = ((!serializer.IsWriter) ? new SerializableGuid[value] : RerouteNodeIds.ToArray());
		for (int i = 0; i < value; i++)
		{
			serializer.SerializeValue(ref array[i], default(FastBufferWriter.ForNetworkSerializable));
		}
		if (serializer.IsReader)
		{
			RerouteNodeIds = array.ToList();
		}
	}
}
