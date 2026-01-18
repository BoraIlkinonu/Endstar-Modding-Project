using System;
using Endless.Shared.DataTypes;
using Unity.Netcode;

namespace Endless.Gameplay;

public struct InventorySlot : INetworkSerializable, IEquatable<InventorySlot>
{
	public bool Locked;

	public NetworkObjectReference ItemObjectReference;

	public Item Item
	{
		get
		{
			if (!ItemObjectReference.TryGet(out var networkObject))
			{
				return null;
			}
			return networkObject.GetComponentInChildren<Item>();
		}
	}

	public SerializableGuid DefinitionGuid
	{
		get
		{
			if (!Item)
			{
				return SerializableGuid.Empty;
			}
			return Item.InventoryUsableDefinition.Guid;
		}
	}

	public SerializableGuid AssetID
	{
		get
		{
			if (!Item)
			{
				return SerializableGuid.Empty;
			}
			return Item.AssetID;
		}
	}

	public int Count
	{
		get
		{
			if (!Item)
			{
				return 1;
			}
			return Item.StackCount;
		}
	}

	public InventorySlot(Item item, bool locked = false)
	{
		if ((bool)item)
		{
			ItemObjectReference = new NetworkObjectReference(item.NetworkObject);
		}
		else
		{
			ItemObjectReference = default(NetworkObjectReference);
		}
		Locked = locked;
	}

	public bool Equals(InventorySlot other)
	{
		if (other.DefinitionGuid.Equals(DefinitionGuid))
		{
			return other.Count == Count;
		}
		return false;
	}

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref Locked, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref ItemObjectReference, default(FastBufferWriter.ForNetworkSerializable));
	}
}
