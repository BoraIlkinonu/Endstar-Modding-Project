using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.Serialization;
using Newtonsoft.Json;
using Unity.Netcode;

namespace Endless.Gameplay.LevelEditing.Level;

[Serializable]
public class ComponentEntry : INetworkSerializable
{
	public int TypeId;

	public List<MemberChange> Changes = new List<MemberChange>();

	[JsonIgnore]
	public string AssemblyQualifiedName => EndlessTypeMapping.Instance.GetAssemblyQualifiedTypeName(TypeId);

	public ComponentEntry Copy()
	{
		ComponentEntry componentEntry = new ComponentEntry();
		componentEntry.TypeId = TypeId;
		foreach (MemberChange change in Changes)
		{
			componentEntry.Changes.Add(change.Copy());
		}
		return componentEntry;
	}

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref TypeId, default(FastBufferWriter.ForPrimitives));
		int value = 0;
		if (!serializer.IsReader)
		{
			value = Changes.Count;
		}
		serializer.SerializeValue(ref value, default(FastBufferWriter.ForPrimitives));
		MemberChange[] array;
		if (!serializer.IsReader)
		{
			array = Changes.ToArray();
		}
		else
		{
			array = new MemberChange[value];
			for (int i = 0; i < value; i++)
			{
				array[i] = new MemberChange();
			}
		}
		for (int j = 0; j < value; j++)
		{
			serializer.SerializeValue(ref array[j], default(FastBufferWriter.ForNetworkSerializable));
		}
		if (serializer.IsReader)
		{
			Changes = array.ToList();
		}
	}
}
