using System;
using Endless.Gameplay.Serialization;
using Newtonsoft.Json;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

[Serializable]
public class MemberChange : INetworkSerializable
{
	public string MemberName;

	public int DataType;

	public string JsonData;

	public MemberChange()
	{
	}

	public MemberChange(string memberName, int dataType, string jsonData)
	{
		MemberName = memberName;
		DataType = dataType;
		JsonData = jsonData;
	}

	public MemberChange Copy()
	{
		return new MemberChange(MemberName, DataType, JsonData);
	}

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref MemberName);
		serializer.SerializeValue(ref DataType, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref JsonData);
	}

	public override string ToString()
	{
		return string.Format("{{ {0}: {1}, {2}: {3}, {4}: {5} }}", "MemberName", MemberName, "DataType", DataType, "JsonData", JsonData);
	}

	public object ToObject()
	{
		string text = string.Empty;
		Type type = null;
		try
		{
			text = EndlessTypeMapping.Instance.GetAssemblyQualifiedTypeName(DataType);
			type = Type.GetType(text);
			object obj = JsonConvert.DeserializeObject(JsonData, type);
			return obj.GetType().IsEnum ? ((object)(int)obj) : obj;
		}
		catch (Exception)
		{
			Debug.LogError($"Error Converting memberChange to object {MemberName}, DataType: {DataType} assemblyQualifiedTypeName: {text} dataType {type} JsonData {JsonData}");
			throw;
		}
	}
}
