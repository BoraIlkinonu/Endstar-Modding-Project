using System.Linq;
using Newtonsoft.Json;
using Unity.Netcode;

namespace Endless.Gameplay.Stats;

public class PerPlayerStat : NumericPlayerStat, INetworkSerializable
{
	internal const int ORDER = 100;

	public string DefaultValue = string.Empty;

	internal int[] GetUserIds()
	{
		return statMap.Keys.ToArray();
	}

	internal bool TryGetValue(int userId, out float value)
	{
		return statMap.TryGetValue(userId, out value);
	}

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref Identifier);
		serializer.SerializeValue(ref Message, default(FastBufferWriter.ForNetworkSerializable));
		serializer.SerializeValue(ref Order, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref DefaultValue);
		int value = 0;
		if (serializer.IsWriter)
		{
			value = statMap.Count;
		}
		serializer.SerializeValue(ref value, default(FastBufferWriter.ForPrimitives));
		int[] array;
		float[] array2;
		if (serializer.IsWriter)
		{
			array = statMap.Keys.ToArray();
			array2 = statMap.Values.ToArray();
		}
		else
		{
			array = new int[value];
			array2 = new float[value];
		}
		for (int i = 0; i < value; i++)
		{
			serializer.SerializeValue(ref array[i], default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue(ref array2[i], default(FastBufferWriter.ForPrimitives));
			if (serializer.IsReader)
			{
				statMap.Add(array[i], array2[i]);
			}
		}
	}

	public override string ToString()
	{
		return JsonConvert.SerializeObject(this);
	}

	public void LoadFromString(string stringData)
	{
		PerPlayerStat perPlayerStat = JsonConvert.DeserializeObject<PerPlayerStat>(stringData);
		CopyFrom(perPlayerStat);
		statMap = perPlayerStat.statMap;
		DefaultValue = perPlayerStat.DefaultValue;
	}
}
