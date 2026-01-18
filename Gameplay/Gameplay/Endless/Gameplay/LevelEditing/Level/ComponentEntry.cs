using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.Serialization;
using Newtonsoft.Json;
using Unity.Netcode;

namespace Endless.Gameplay.LevelEditing.Level
{
	// Token: 0x02000540 RID: 1344
	[Serializable]
	public class ComponentEntry : INetworkSerializable
	{
		// Token: 0x17000636 RID: 1590
		// (get) Token: 0x0600206B RID: 8299 RVA: 0x000920D9 File Offset: 0x000902D9
		[JsonIgnore]
		public string AssemblyQualifiedName
		{
			get
			{
				return EndlessTypeMapping.Instance.GetAssemblyQualifiedTypeName(this.TypeId);
			}
		}

		// Token: 0x0600206C RID: 8300 RVA: 0x000920EC File Offset: 0x000902EC
		public ComponentEntry Copy()
		{
			ComponentEntry componentEntry = new ComponentEntry();
			componentEntry.TypeId = this.TypeId;
			foreach (MemberChange memberChange in this.Changes)
			{
				componentEntry.Changes.Add(memberChange.Copy());
			}
			return componentEntry;
		}

		// Token: 0x0600206D RID: 8301 RVA: 0x0009215C File Offset: 0x0009035C
		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue<int>(ref this.TypeId, default(FastBufferWriter.ForPrimitives));
			int num = 0;
			if (!serializer.IsReader)
			{
				num = this.Changes.Count;
			}
			serializer.SerializeValue<int>(ref num, default(FastBufferWriter.ForPrimitives));
			MemberChange[] array;
			if (!serializer.IsReader)
			{
				array = this.Changes.ToArray();
			}
			else
			{
				array = new MemberChange[num];
				for (int i = 0; i < num; i++)
				{
					array[i] = new MemberChange();
				}
			}
			for (int j = 0; j < num; j++)
			{
				serializer.SerializeValue<MemberChange>(ref array[j], default(FastBufferWriter.ForNetworkSerializable));
			}
			if (serializer.IsReader)
			{
				this.Changes = array.ToList<MemberChange>();
			}
		}

		// Token: 0x040019ED RID: 6637
		public int TypeId;

		// Token: 0x040019EE RID: 6638
		public List<MemberChange> Changes = new List<MemberChange>();
	}
}
