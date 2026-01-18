using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Level
{
	// Token: 0x0200054E RID: 1358
	[Serializable]
	public class PropEntry : INetworkSerializable
	{
		// Token: 0x060020D2 RID: 8402 RVA: 0x00093E86 File Offset: 0x00092086
		public PropEntry CopyWithNewInstanceId(SerializableGuid newInstanceId)
		{
			PropEntry propEntry = this.Copy();
			propEntry.InstanceId = newInstanceId;
			return propEntry;
		}

		// Token: 0x060020D3 RID: 8403 RVA: 0x00093E98 File Offset: 0x00092098
		public PropEntry Copy()
		{
			PropEntry propEntry = new PropEntry();
			propEntry.AssetId = this.AssetId;
			propEntry.InstanceId = this.InstanceId;
			propEntry.Label = this.Label;
			propEntry.Position = this.Position;
			propEntry.Rotation = this.Rotation;
			foreach (ComponentEntry componentEntry in this.ComponentEntries)
			{
				propEntry.ComponentEntries.Add(componentEntry.Copy());
			}
			foreach (MemberChange memberChange in this.LuaMemberChanges)
			{
				propEntry.LuaMemberChanges.Add(memberChange.Copy());
			}
			return propEntry;
		}

		// Token: 0x060020D4 RID: 8404 RVA: 0x00093F88 File Offset: 0x00092188
		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue<SerializableGuid>(ref this.AssetId, default(FastBufferWriter.ForNetworkSerializable));
			serializer.SerializeValue<SerializableGuid>(ref this.InstanceId, default(FastBufferWriter.ForNetworkSerializable));
			serializer.SerializeValue(ref this.Label, false);
			serializer.SerializeValue(ref this.Position);
			serializer.SerializeValue(ref this.Rotation);
			int num = 0;
			if (!serializer.IsReader)
			{
				num = this.ComponentEntries.Count;
			}
			int num2 = 0;
			if (!serializer.IsReader)
			{
				num2 = this.LuaMemberChanges.Count;
			}
			serializer.SerializeValue<int>(ref num, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<int>(ref num2, default(FastBufferWriter.ForPrimitives));
			ComponentEntry[] array;
			if (!serializer.IsReader)
			{
				array = this.ComponentEntries.ToArray();
			}
			else
			{
				array = new ComponentEntry[num];
				for (int i = 0; i < num; i++)
				{
					array[i] = new ComponentEntry();
				}
			}
			MemberChange[] array2;
			if (!serializer.IsReader)
			{
				array2 = this.LuaMemberChanges.ToArray();
			}
			else
			{
				array2 = new MemberChange[num2];
				for (int j = 0; j < num2; j++)
				{
					array2[j] = new MemberChange();
				}
			}
			for (int k = 0; k < num; k++)
			{
				serializer.SerializeValue<ComponentEntry>(ref array[k], default(FastBufferWriter.ForNetworkSerializable));
			}
			for (int l = 0; l < num2; l++)
			{
				serializer.SerializeValue<MemberChange>(ref array2[l], default(FastBufferWriter.ForNetworkSerializable));
			}
			if (serializer.IsReader)
			{
				this.ComponentEntries = array.ToList<ComponentEntry>();
			}
			if (serializer.IsReader)
			{
				this.LuaMemberChanges = array2.ToList<MemberChange>();
			}
		}

		// Token: 0x04001A16 RID: 6678
		[JsonProperty("AssetId")]
		public SerializableGuid AssetId;

		// Token: 0x04001A17 RID: 6679
		public SerializableGuid InstanceId;

		// Token: 0x04001A18 RID: 6680
		public string Label = "Prop";

		// Token: 0x04001A19 RID: 6681
		[JsonProperty("Pos")]
		public Vector3 Position;

		// Token: 0x04001A1A RID: 6682
		[JsonProperty("Rot")]
		public Quaternion Rotation;

		// Token: 0x04001A1B RID: 6683
		public List<ComponentEntry> ComponentEntries = new List<ComponentEntry>();

		// Token: 0x04001A1C RID: 6684
		public List<MemberChange> LuaMemberChanges = new List<MemberChange>();
	}
}
