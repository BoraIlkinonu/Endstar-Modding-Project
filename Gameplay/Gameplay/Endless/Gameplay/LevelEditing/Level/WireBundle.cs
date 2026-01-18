using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;
using Unity.Netcode;

namespace Endless.Gameplay.LevelEditing.Level
{
	// Token: 0x02000554 RID: 1364
	[Serializable]
	public class WireBundle : INetworkSerializable
	{
		// Token: 0x17000645 RID: 1605
		// (get) Token: 0x060020DE RID: 8414 RVA: 0x000941E1 File Offset: 0x000923E1
		// (set) Token: 0x060020DF RID: 8415 RVA: 0x000941E9 File Offset: 0x000923E9
		public List<WireEntry> Wires { get; set; } = new List<WireEntry>();

		// Token: 0x17000646 RID: 1606
		// (get) Token: 0x060020E0 RID: 8416 RVA: 0x000941F2 File Offset: 0x000923F2
		[JsonIgnore]
		public int WiresInBundle
		{
			get
			{
				return this.Wires.Count;
			}
		}

		// Token: 0x060020E1 RID: 8417 RVA: 0x00094200 File Offset: 0x00092400
		public WireBundle Copy(bool generateNewUniqueIds)
		{
			WireBundle wireBundle = new WireBundle();
			wireBundle.BundleId = (generateNewUniqueIds ? SerializableGuid.NewGuid() : this.BundleId);
			wireBundle.EmitterInstanceId = this.EmitterInstanceId;
			wireBundle.ReceiverInstanceId = this.ReceiverInstanceId;
			wireBundle.WireColor = this.WireColor;
			foreach (WireEntry wireEntry in this.Wires)
			{
				wireBundle.Wires.Add(wireEntry.Copy(generateNewUniqueIds));
			}
			return wireBundle;
		}

		// Token: 0x060020E2 RID: 8418 RVA: 0x000942A0 File Offset: 0x000924A0
		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue<SerializableGuid>(ref this.BundleId, default(FastBufferWriter.ForNetworkSerializable));
			serializer.SerializeValue<SerializableGuid>(ref this.EmitterInstanceId, default(FastBufferWriter.ForNetworkSerializable));
			serializer.SerializeValue<SerializableGuid>(ref this.ReceiverInstanceId, default(FastBufferWriter.ForNetworkSerializable));
			serializer.SerializeValue<WireColor>(ref this.WireColor, default(FastBufferWriter.ForEnums));
			this.SerializeRerouteNodes<T>(serializer);
			this.SerializeWires<T>(serializer);
		}

		// Token: 0x060020E3 RID: 8419 RVA: 0x00094314 File Offset: 0x00092514
		private void SerializeWires<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			int num = 0;
			if (serializer.IsWriter)
			{
				num = this.Wires.Count;
			}
			serializer.SerializeValue<int>(ref num, default(FastBufferWriter.ForPrimitives));
			WireEntry[] array;
			if (serializer.IsWriter)
			{
				array = this.Wires.ToArray();
				for (int i = 0; i < num; i++)
				{
					serializer.SerializeValue<WireEntry>(ref array[i], default(FastBufferWriter.ForNetworkSerializable));
				}
			}
			else
			{
				array = new WireEntry[num];
				for (int j = 0; j < num; j++)
				{
					array[j] = new WireEntry();
					array[j].NetworkSerialize<T>(serializer);
				}
			}
			if (serializer.IsReader)
			{
				this.Wires = array.ToList<WireEntry>();
			}
		}

		// Token: 0x060020E4 RID: 8420 RVA: 0x000943C4 File Offset: 0x000925C4
		private void SerializeRerouteNodes<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			int num = 0;
			if (serializer.IsWriter)
			{
				num = this.RerouteNodeIds.Count;
			}
			serializer.SerializeValue<int>(ref num, default(FastBufferWriter.ForPrimitives));
			SerializableGuid[] array;
			if (serializer.IsWriter)
			{
				array = this.RerouteNodeIds.ToArray();
			}
			else
			{
				array = new SerializableGuid[num];
			}
			for (int i = 0; i < num; i++)
			{
				serializer.SerializeValue<SerializableGuid>(ref array[i], default(FastBufferWriter.ForNetworkSerializable));
			}
			if (serializer.IsReader)
			{
				this.RerouteNodeIds = array.ToList<SerializableGuid>();
			}
		}

		// Token: 0x04001A2C RID: 6700
		public SerializableGuid BundleId;

		// Token: 0x04001A2D RID: 6701
		public SerializableGuid EmitterInstanceId;

		// Token: 0x04001A2E RID: 6702
		public SerializableGuid ReceiverInstanceId;

		// Token: 0x04001A2F RID: 6703
		public WireColor WireColor;

		// Token: 0x04001A30 RID: 6704
		public List<SerializableGuid> RerouteNodeIds = new List<SerializableGuid>();
	}
}
