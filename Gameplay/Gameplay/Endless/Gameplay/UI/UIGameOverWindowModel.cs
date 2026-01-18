using System;
using Endless.Gameplay.Stats;
using Unity.Netcode;

namespace Endless.Gameplay.UI
{
	// Token: 0x02000419 RID: 1049
	public class UIGameOverWindowModel : INetworkSerializable
	{
		// Token: 0x06001A14 RID: 6676 RVA: 0x000030D2 File Offset: 0x000012D2
		public UIGameOverWindowModel()
		{
		}

		// Token: 0x06001A15 RID: 6677 RVA: 0x00077F52 File Offset: 0x00076152
		public UIGameOverWindowModel(string title, string description, bool showReplay, bool showEndMatch, bool showNextLevel, BasicStat[] basicStats, PerPlayerStat[] perPlayerStats)
		{
			this.Title = title;
			this.Description = description;
			this.ShowReplay = showReplay;
			this.ShowEndMatch = showEndMatch;
			this.ShowNextLevel = showNextLevel;
			this.BasicStats = basicStats;
			this.PerPlayerStats = perPlayerStats;
		}

		// Token: 0x06001A16 RID: 6678 RVA: 0x00077F90 File Offset: 0x00076190
		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
		{
			serializer.SerializeValue(ref this.Title, false);
			serializer.SerializeValue(ref this.Description, false);
			serializer.SerializeValue<bool>(ref this.ShowReplay, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<bool>(ref this.ShowEndMatch, default(FastBufferWriter.ForPrimitives));
			serializer.SerializeValue<bool>(ref this.ShowNextLevel, default(FastBufferWriter.ForPrimitives));
			int num = 0;
			if (serializer.IsWriter)
			{
				num = this.BasicStats.Length;
			}
			serializer.SerializeValue<int>(ref num, default(FastBufferWriter.ForPrimitives));
			if (serializer.IsReader)
			{
				this.BasicStats = new BasicStat[num];
				for (int i = 0; i < num; i++)
				{
					this.BasicStats[i] = new BasicStat();
				}
			}
			for (int j = 0; j < num; j++)
			{
				serializer.SerializeValue<BasicStat>(ref this.BasicStats[j], default(FastBufferWriter.ForNetworkSerializable));
			}
			int num2 = 0;
			if (serializer.IsWriter)
			{
				num2 = this.PerPlayerStats.Length;
			}
			serializer.SerializeValue<int>(ref num2, default(FastBufferWriter.ForPrimitives));
			if (serializer.IsReader)
			{
				this.PerPlayerStats = new PerPlayerStat[num2];
				for (int k = 0; k < num2; k++)
				{
					this.PerPlayerStats[k] = new PerPlayerStat();
				}
			}
			for (int l = 0; l < num2; l++)
			{
				serializer.SerializeValue<PerPlayerStat>(ref this.PerPlayerStats[l], default(FastBufferWriter.ForNetworkSerializable));
			}
		}

		// Token: 0x06001A17 RID: 6679 RVA: 0x00078104 File Offset: 0x00076304
		public override string ToString()
		{
			return string.Format("{{ {0}: {1}, {2}: {3}, {4}: {5}, {6}, {7}, {8}: {9}, {10}: {11} }}", new object[]
			{
				"Title",
				this.Title,
				"Description",
				this.Description,
				"ShowReplay",
				this.ShowReplay,
				this.ShowEndMatch,
				this.ShowNextLevel,
				"BasicStats",
				this.BasicStats.Length,
				"PerPlayerStats",
				this.PerPlayerStats.Length
			});
		}

		// Token: 0x040014CE RID: 5326
		public string Title;

		// Token: 0x040014CF RID: 5327
		public string Description;

		// Token: 0x040014D0 RID: 5328
		public bool ShowReplay;

		// Token: 0x040014D1 RID: 5329
		public bool ShowEndMatch;

		// Token: 0x040014D2 RID: 5330
		public bool ShowNextLevel;

		// Token: 0x040014D3 RID: 5331
		public BasicStat[] BasicStats;

		// Token: 0x040014D4 RID: 5332
		public PerPlayerStat[] PerPlayerStats;
	}
}
