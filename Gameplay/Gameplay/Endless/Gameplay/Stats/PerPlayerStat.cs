using System;
using System.Linq;
using Endless.Shared;
using Newtonsoft.Json;
using Unity.Netcode;

namespace Endless.Gameplay.Stats
{
	// Token: 0x02000385 RID: 901
	public class PerPlayerStat : NumericPlayerStat, INetworkSerializable
	{
		// Token: 0x06001706 RID: 5894 RVA: 0x0006B8C8 File Offset: 0x00069AC8
		internal int[] GetUserIds()
		{
			return this.statMap.Keys.ToArray<int>();
		}

		// Token: 0x06001707 RID: 5895 RVA: 0x0006B8DA File Offset: 0x00069ADA
		internal bool TryGetValue(int userId, out float value)
		{
			return this.statMap.TryGetValue(userId, out value);
		}

		// Token: 0x06001708 RID: 5896 RVA: 0x0006B8EC File Offset: 0x00069AEC
		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref this.Identifier, false);
			serializer.SerializeValue<LocalizedString>(ref this.Message, default(FastBufferWriter.ForNetworkSerializable));
			serializer.SerializeValue<int>(ref this.Order, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue(ref this.DefaultValue, false);
			int num = 0;
			if (serializer.IsWriter)
			{
				num = this.statMap.Count;
			}
			serializer.SerializeValue<int>(ref num, default(FastBufferWriter.ForPrimitives));
			int[] array;
			float[] array2;
			if (serializer.IsWriter)
			{
				array = this.statMap.Keys.ToArray<int>();
				array2 = this.statMap.Values.ToArray<float>();
			}
			else
			{
				array = new int[num];
				array2 = new float[num];
			}
			for (int i = 0; i < num; i++)
			{
				serializer.SerializeValue<int>(ref array[i], default(FastBufferWriter.ForPrimitives));
				serializer.SerializeValue<float>(ref array2[i], default(FastBufferWriter.ForPrimitives));
				if (serializer.IsReader)
				{
					this.statMap.Add(array[i], array2[i]);
				}
			}
		}

		// Token: 0x06001709 RID: 5897 RVA: 0x0006B81F File Offset: 0x00069A1F
		public override string ToString()
		{
			return JsonConvert.SerializeObject(this);
		}

		// Token: 0x0600170A RID: 5898 RVA: 0x0006BA08 File Offset: 0x00069C08
		public void LoadFromString(string stringData)
		{
			PerPlayerStat perPlayerStat = JsonConvert.DeserializeObject<PerPlayerStat>(stringData);
			base.CopyFrom(perPlayerStat);
			this.statMap = perPlayerStat.statMap;
			this.DefaultValue = perPlayerStat.DefaultValue;
		}

		// Token: 0x04001276 RID: 4726
		internal const int ORDER = 100;

		// Token: 0x04001277 RID: 4727
		public string DefaultValue = string.Empty;
	}
}
