using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;

namespace Endless.Creator;

public static class WiringUtilities
{
	public static WireBundle GetWireBundle(SerializableGuid emitter, SerializableGuid receiver)
	{
		SerializableGuid[] propIds = new SerializableGuid[2] { emitter, receiver };
		MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetWiresUsingProps(propIds);
		foreach (WireBundle wireBundle in MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.WireBundles)
		{
			if (wireBundle.EmitterInstanceId == emitter && wireBundle.ReceiverInstanceId == receiver)
			{
				return wireBundle;
			}
		}
		return null;
	}

	public static SerializableGuid GetWiringId(SerializableGuid emitter, string emitterMemberName, SerializableGuid receiver, string receiverMemberName)
	{
		foreach (WireBundle wireBundle in MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.WireBundles)
		{
			if (!(wireBundle.EmitterInstanceId == emitter) || !(wireBundle.ReceiverInstanceId == receiver))
			{
				continue;
			}
			foreach (WireEntry wire in wireBundle.Wires)
			{
				if (wire.EmitterMemberName == emitterMemberName && wire.ReceiverMemberName == receiverMemberName)
				{
					return wire.WireId;
				}
			}
		}
		return SerializableGuid.Empty;
	}

	public static WireEntry GetWireEntry(SerializableGuid emitter, string emitterMemberName, SerializableGuid receiver, string receiverMemberName)
	{
		WireBundle wireBundle = GetWireBundle(emitter, receiver);
		if (wireBundle != null)
		{
			foreach (WireEntry wire in wireBundle.Wires)
			{
				if (wire.EmitterMemberName == emitterMemberName && wire.ReceiverMemberName == receiverMemberName)
				{
					return wire;
				}
			}
		}
		return null;
	}

	public static IReadOnlyList<WireBundle> GetWiresEmittingFrom(SerializableGuid emitter, string emitterMemberName = null)
	{
		List<WireBundle> list = new List<WireBundle>();
		foreach (WireBundle wireBundle in MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.WireBundles)
		{
			if (!(wireBundle.EmitterInstanceId == emitter))
			{
				continue;
			}
			if (string.IsNullOrEmpty(emitterMemberName))
			{
				list.Add(wireBundle);
				continue;
			}
			foreach (WireEntry wire in wireBundle.Wires)
			{
				if (wire.EmitterMemberName == emitterMemberName)
				{
					list.Add(wireBundle);
				}
			}
		}
		return list;
	}

	public static IReadOnlyList<WireBundle> GetWiresWithAReceiverOf(SerializableGuid receiver, string receiverMemberName = null)
	{
		List<WireBundle> list = new List<WireBundle>();
		foreach (WireBundle wireBundle in MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.WireBundles)
		{
			if (!(wireBundle.ReceiverInstanceId == receiver))
			{
				continue;
			}
			if (string.IsNullOrEmpty(receiverMemberName))
			{
				list.Add(wireBundle);
				continue;
			}
			foreach (WireEntry wire in wireBundle.Wires)
			{
				if (wire.ReceiverMemberName == receiverMemberName)
				{
					list.Add(wireBundle);
				}
			}
		}
		return list;
	}
}
