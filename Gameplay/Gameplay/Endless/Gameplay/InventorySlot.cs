using System;
using Endless.Shared.DataTypes;
using Unity.Netcode;

namespace Endless.Gameplay
{
	// Token: 0x020000E1 RID: 225
	public struct InventorySlot : INetworkSerializable, IEquatable<InventorySlot>
	{
		// Token: 0x170000D0 RID: 208
		// (get) Token: 0x060004EB RID: 1259 RVA: 0x000190BC File Offset: 0x000172BC
		public Item Item
		{
			get
			{
				NetworkObject networkObject;
				if (!this.ItemObjectReference.TryGet(out networkObject, null))
				{
					return null;
				}
				return networkObject.GetComponentInChildren<Item>();
			}
		}

		// Token: 0x170000D1 RID: 209
		// (get) Token: 0x060004EC RID: 1260 RVA: 0x000190E1 File Offset: 0x000172E1
		public SerializableGuid DefinitionGuid
		{
			get
			{
				if (!this.Item)
				{
					return SerializableGuid.Empty;
				}
				return this.Item.InventoryUsableDefinition.Guid;
			}
		}

		// Token: 0x170000D2 RID: 210
		// (get) Token: 0x060004ED RID: 1261 RVA: 0x00019106 File Offset: 0x00017306
		public SerializableGuid AssetID
		{
			get
			{
				if (!this.Item)
				{
					return SerializableGuid.Empty;
				}
				return this.Item.AssetID;
			}
		}

		// Token: 0x170000D3 RID: 211
		// (get) Token: 0x060004EE RID: 1262 RVA: 0x00019126 File Offset: 0x00017326
		public int Count
		{
			get
			{
				if (!this.Item)
				{
					return 1;
				}
				return this.Item.StackCount;
			}
		}

		// Token: 0x060004EF RID: 1263 RVA: 0x00019142 File Offset: 0x00017342
		public InventorySlot(Item item, bool locked = false)
		{
			if (item)
			{
				this.ItemObjectReference = new NetworkObjectReference(item.NetworkObject);
			}
			else
			{
				this.ItemObjectReference = default(NetworkObjectReference);
			}
			this.Locked = locked;
		}

		// Token: 0x060004F0 RID: 1264 RVA: 0x00019174 File Offset: 0x00017374
		public bool Equals(InventorySlot other)
		{
			return other.DefinitionGuid.Equals(this.DefinitionGuid) && other.Count == this.Count;
		}

		// Token: 0x060004F1 RID: 1265 RVA: 0x000191AC File Offset: 0x000173AC
		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue<bool>(ref this.Locked, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<NetworkObjectReference>(ref this.ItemObjectReference, default(FastBufferWriter.ForNetworkSerializable));
		}

		// Token: 0x040003EE RID: 1006
		public bool Locked;

		// Token: 0x040003EF RID: 1007
		public NetworkObjectReference ItemObjectReference;
	}
}
