using System;
using System.Collections.Generic;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020002EB RID: 747
	public abstract class UsableDefinition : ScriptableObject
	{
		// Token: 0x17000351 RID: 849
		// (get) Token: 0x060010CB RID: 4299 RVA: 0x00055011 File Offset: 0x00053211
		// (set) Token: 0x060010CC RID: 4300 RVA: 0x00055019 File Offset: 0x00053219
		public SerializableGuid Guid
		{
			get
			{
				return this.guid;
			}
			set
			{
				this.guid = value;
			}
		}

		// Token: 0x17000352 RID: 850
		// (get) Token: 0x060010CD RID: 4301 RVA: 0x00055022 File Offset: 0x00053222
		public string DisplayName
		{
			get
			{
				return this.displayName;
			}
		}

		// Token: 0x17000353 RID: 851
		// (get) Token: 0x060010CE RID: 4302 RVA: 0x0005502A File Offset: 0x0005322A
		public Sprite Sprite
		{
			get
			{
				return this.sprite;
			}
		}

		// Token: 0x060010CF RID: 4303
		public abstract UsableDefinition.UseState GetNewUseState();

		// Token: 0x060010D0 RID: 4304
		public abstract UsableDefinition.UseState ProcessUseStart(ref NetState state, NetInput input, SerializableGuid guid, PlayerReferenceManager playerReference, Item item);

		// Token: 0x060010D1 RID: 4305
		public abstract bool ProcessUseFrame(ref NetState state, NetInput input, ref UsableDefinition.UseState useState, PlayerReferenceManager playerReference, bool equipped, bool pressed);

		// Token: 0x060010D2 RID: 4306 RVA: 0x00055034 File Offset: 0x00053234
		public static void SerializeEquipmentUseState<T>(BufferSerializer<T> serializer, ref UsableDefinition.UseState useState) where T : IReaderWriter
		{
			if (serializer.IsWriter)
			{
				serializer.SerializeValue<SerializableGuid>(ref useState.EquipmentGuid, default(FastBufferWriter.ForNetworkSerializable));
				Item.NetworkWrite<T>(useState.Item, serializer);
				useState.Serialize<T>(serializer);
				return;
			}
			SerializableGuid empty = SerializableGuid.Empty;
			serializer.SerializeValue<SerializableGuid>(ref empty, default(FastBufferWriter.ForNetworkSerializable));
			useState = UsableDefinition.UseState.GetStateFromPool(empty, null);
			useState.Item = Item.NetworkRead<T>(serializer);
			useState.Serialize<T>(serializer);
		}

		// Token: 0x060010D3 RID: 4307 RVA: 0x0001965C File Offset: 0x0001785C
		public virtual uint GetItemSwapDelayFrames(UsableDefinition.UseState currentUseState)
		{
			return 0U;
		}

		// Token: 0x04000E95 RID: 3733
		[SerializeField]
		private SerializableGuid guid;

		// Token: 0x04000E96 RID: 3734
		[SerializeField]
		private string displayName = string.Empty;

		// Token: 0x04000E97 RID: 3735
		[SerializeField]
		private Sprite sprite;

		// Token: 0x020002EC RID: 748
		public class BlankUseState : UsableDefinition.UseState
		{
		}

		// Token: 0x020002ED RID: 749
		public abstract class UseState
		{
			// Token: 0x060010D6 RID: 4310 RVA: 0x00002DB0 File Offset: 0x00000FB0
			public virtual void Serialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
			{
			}

			// Token: 0x060010D7 RID: 4311 RVA: 0x000550C0 File Offset: 0x000532C0
			public static void PoolState(UsableDefinition.UseState eus)
			{
				if (!UsableDefinition.UseState.statePoolDictionary.ContainsKey(eus.EquipmentGuid))
				{
					UsableDefinition.UseState.statePoolDictionary.Add(eus.EquipmentGuid, new List<UsableDefinition.UseState>());
				}
				UsableDefinition.UseState.statePoolDictionary[eus.EquipmentGuid].Add(eus);
			}

			// Token: 0x060010D8 RID: 4312 RVA: 0x00055100 File Offset: 0x00053300
			public static UsableDefinition.UseState GetStateFromPool(SerializableGuid guid, Item item)
			{
				if (!UsableDefinition.UseState.statePoolDictionary.ContainsKey(guid))
				{
					UsableDefinition.UseState.statePoolDictionary.Add(guid, new List<UsableDefinition.UseState>());
				}
				UsableDefinition.UseState useState;
				if (UsableDefinition.UseState.statePoolDictionary[guid].Count > 0)
				{
					useState = UsableDefinition.UseState.statePoolDictionary[guid][0];
					UsableDefinition.UseState.statePoolDictionary[guid].RemoveAt(0);
				}
				else
				{
					useState = RuntimeDatabase.GetUsableDefinition(guid).GetNewUseState();
					useState.EquipmentGuid = guid;
				}
				useState.Item = item;
				return useState;
			}

			// Token: 0x04000E98 RID: 3736
			public SerializableGuid EquipmentGuid;

			// Token: 0x04000E99 RID: 3737
			public Item Item;

			// Token: 0x04000E9A RID: 3738
			protected static Dictionary<SerializableGuid, List<UsableDefinition.UseState>> statePoolDictionary = new Dictionary<SerializableGuid, List<UsableDefinition.UseState>>();
		}
	}
}
