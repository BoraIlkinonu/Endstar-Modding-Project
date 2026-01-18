using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Level;

[Serializable]
public class PropEntry : INetworkSerializable
{
	[JsonProperty("AssetId")]
	public SerializableGuid AssetId;

	public SerializableGuid InstanceId;

	public string Label = "Prop";

	[JsonProperty("Pos")]
	public Vector3 Position;

	[JsonProperty("Rot")]
	public Quaternion Rotation;

	public List<ComponentEntry> ComponentEntries = new List<ComponentEntry>();

	public List<MemberChange> LuaMemberChanges = new List<MemberChange>();

	public PropEntry CopyWithNewInstanceId(SerializableGuid newInstanceId)
	{
		PropEntry propEntry = Copy();
		propEntry.InstanceId = newInstanceId;
		return propEntry;
	}

	public PropEntry Copy()
	{
		PropEntry propEntry = new PropEntry();
		propEntry.AssetId = AssetId;
		propEntry.InstanceId = InstanceId;
		propEntry.Label = Label;
		propEntry.Position = Position;
		propEntry.Rotation = Rotation;
		foreach (ComponentEntry componentEntry in ComponentEntries)
		{
			propEntry.ComponentEntries.Add(componentEntry.Copy());
		}
		foreach (MemberChange luaMemberChange in LuaMemberChanges)
		{
			propEntry.LuaMemberChanges.Add(luaMemberChange.Copy());
		}
		return propEntry;
	}

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref AssetId, default(FastBufferWriter.ForNetworkSerializable));
		serializer.SerializeValue(ref InstanceId, default(FastBufferWriter.ForNetworkSerializable));
		serializer.SerializeValue(ref Label);
		serializer.SerializeValue(ref Position);
		serializer.SerializeValue(ref Rotation);
		int value = 0;
		if (!serializer.IsReader)
		{
			value = ComponentEntries.Count;
		}
		int value2 = 0;
		if (!serializer.IsReader)
		{
			value2 = LuaMemberChanges.Count;
		}
		serializer.SerializeValue(ref value, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref value2, default(FastBufferWriter.ForPrimitives));
		ComponentEntry[] array;
		if (!serializer.IsReader)
		{
			array = ComponentEntries.ToArray();
		}
		else
		{
			array = new ComponentEntry[value];
			for (int i = 0; i < value; i++)
			{
				array[i] = new ComponentEntry();
			}
		}
		MemberChange[] array2;
		if (!serializer.IsReader)
		{
			array2 = LuaMemberChanges.ToArray();
		}
		else
		{
			array2 = new MemberChange[value2];
			for (int j = 0; j < value2; j++)
			{
				array2[j] = new MemberChange();
			}
		}
		for (int k = 0; k < value; k++)
		{
			serializer.SerializeValue(ref array[k], default(FastBufferWriter.ForNetworkSerializable));
		}
		for (int l = 0; l < value2; l++)
		{
			serializer.SerializeValue(ref array2[l], default(FastBufferWriter.ForNetworkSerializable));
		}
		if (serializer.IsReader)
		{
			ComponentEntries = array.ToList();
		}
		if (serializer.IsReader)
		{
			LuaMemberChanges = array2.ToList();
		}
	}
}
