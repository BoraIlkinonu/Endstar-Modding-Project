using System;
using System.Data;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using Newtonsoft.Json;
using Unity.Netcode;

namespace Endless.Gameplay.Stats;

public class BasicStat : StatBase, INetworkSerializable
{
	[JsonProperty]
	internal int UserId = -1;

	public string Value = string.Empty;

	public void SetPlayerContext(Context playerContext)
	{
		if (playerContext.IsPlayer())
		{
			if (!NetworkBehaviourSingleton<UserIdManager>.Instance.TryGetUserId(playerContext.WorldObject.NetworkObject.OwnerClientId, out UserId))
			{
				throw new DataException("Provided player context to SetPlayerContext was not registered yet.");
			}
			return;
		}
		throw new ArgumentException("Context provided to SetPlayerContext was not a player");
	}

	public void SetNumericValue(Context instigator, float value, int displayFormat)
	{
		Value = StatBase.GetFormattedString(value, (NumericDisplayFormat)displayFormat);
	}

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref Identifier);
		serializer.SerializeValue(ref Message, default(FastBufferWriter.ForNetworkSerializable));
		serializer.SerializeValue(ref Order, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref UserId, default(FastBufferWriter.ForPrimitives));
		if (serializer.IsWriter && Value == null)
		{
			Value = string.Empty;
		}
		serializer.SerializeValue(ref Value);
	}

	public override string ToString()
	{
		return JsonConvert.SerializeObject(this);
	}

	public void LoadFromString(string stringData)
	{
		BasicStat basicStat = JsonConvert.DeserializeObject<BasicStat>(stringData);
		CopyFrom(basicStat);
		UserId = basicStat.UserId;
	}
}
