using System.Collections.Generic;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public abstract class UsableDefinition : ScriptableObject
{
	public class BlankUseState : UseState
	{
	}

	public abstract class UseState
	{
		public SerializableGuid EquipmentGuid;

		public Item Item;

		protected static Dictionary<SerializableGuid, List<UseState>> statePoolDictionary = new Dictionary<SerializableGuid, List<UseState>>();

		public virtual void Serialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
		}

		public static void PoolState(UseState eus)
		{
			if (!statePoolDictionary.ContainsKey(eus.EquipmentGuid))
			{
				statePoolDictionary.Add(eus.EquipmentGuid, new List<UseState>());
			}
			statePoolDictionary[eus.EquipmentGuid].Add(eus);
		}

		public static UseState GetStateFromPool(SerializableGuid guid, Item item)
		{
			if (!statePoolDictionary.ContainsKey(guid))
			{
				statePoolDictionary.Add(guid, new List<UseState>());
			}
			UseState useState = null;
			if (statePoolDictionary[guid].Count > 0)
			{
				useState = statePoolDictionary[guid][0];
				statePoolDictionary[guid].RemoveAt(0);
			}
			else
			{
				useState = RuntimeDatabase.GetUsableDefinition(guid).GetNewUseState();
				useState.EquipmentGuid = guid;
			}
			useState.Item = item;
			return useState;
		}
	}

	[SerializeField]
	private SerializableGuid guid;

	[SerializeField]
	private string displayName = string.Empty;

	[SerializeField]
	private Sprite sprite;

	public SerializableGuid Guid
	{
		get
		{
			return guid;
		}
		set
		{
			guid = value;
		}
	}

	public string DisplayName => displayName;

	public Sprite Sprite => sprite;

	public abstract UseState GetNewUseState();

	public abstract UseState ProcessUseStart(ref NetState state, NetInput input, SerializableGuid guid, PlayerReferenceManager playerReference, Item item);

	public abstract bool ProcessUseFrame(ref NetState state, NetInput input, ref UseState useState, PlayerReferenceManager playerReference, bool equipped, bool pressed);

	public static void SerializeEquipmentUseState<T>(BufferSerializer<T> serializer, ref UseState useState) where T : IReaderWriter
	{
		if (serializer.IsWriter)
		{
			serializer.SerializeValue(ref useState.EquipmentGuid, default(FastBufferWriter.ForNetworkSerializable));
			Item.NetworkWrite(useState.Item, serializer);
			useState.Serialize(serializer);
		}
		else
		{
			SerializableGuid value = SerializableGuid.Empty;
			serializer.SerializeValue(ref value, default(FastBufferWriter.ForNetworkSerializable));
			useState = UseState.GetStateFromPool(value, null);
			useState.Item = Item.NetworkRead(serializer);
			useState.Serialize(serializer);
		}
	}

	public virtual uint GetItemSwapDelayFrames(UseState currentUseState)
	{
		return 0u;
	}
}
