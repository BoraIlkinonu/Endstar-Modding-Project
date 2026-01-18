using Endless.Gameplay.Stats;
using Unity.Netcode;

namespace Endless.Gameplay.UI;

public class UIGameOverWindowModel : INetworkSerializable
{
	public string Title;

	public string Description;

	public bool ShowReplay;

	public bool ShowEndMatch;

	public bool ShowNextLevel;

	public BasicStat[] BasicStats;

	public PerPlayerStat[] PerPlayerStats;

	public UIGameOverWindowModel()
	{
	}

	public UIGameOverWindowModel(string title, string description, bool showReplay, bool showEndMatch, bool showNextLevel, BasicStat[] basicStats, PerPlayerStat[] perPlayerStats)
	{
		Title = title;
		Description = description;
		ShowReplay = showReplay;
		ShowEndMatch = showEndMatch;
		ShowNextLevel = showNextLevel;
		BasicStats = basicStats;
		PerPlayerStats = perPlayerStats;
	}

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref Title);
		serializer.SerializeValue(ref Description);
		serializer.SerializeValue(ref ShowReplay, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref ShowEndMatch, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref ShowNextLevel, default(FastBufferWriter.ForPrimitives));
		int value = 0;
		if (serializer.IsWriter)
		{
			value = BasicStats.Length;
		}
		serializer.SerializeValue(ref value, default(FastBufferWriter.ForPrimitives));
		if (serializer.IsReader)
		{
			BasicStats = new BasicStat[value];
			for (int i = 0; i < value; i++)
			{
				BasicStats[i] = new BasicStat();
			}
		}
		for (int j = 0; j < value; j++)
		{
			serializer.SerializeValue(ref BasicStats[j], default(FastBufferWriter.ForNetworkSerializable));
		}
		int value2 = 0;
		if (serializer.IsWriter)
		{
			value2 = PerPlayerStats.Length;
		}
		serializer.SerializeValue(ref value2, default(FastBufferWriter.ForPrimitives));
		if (serializer.IsReader)
		{
			PerPlayerStats = new PerPlayerStat[value2];
			for (int k = 0; k < value2; k++)
			{
				PerPlayerStats[k] = new PerPlayerStat();
			}
		}
		for (int l = 0; l < value2; l++)
		{
			serializer.SerializeValue(ref PerPlayerStats[l], default(FastBufferWriter.ForNetworkSerializable));
		}
	}

	public override string ToString()
	{
		return string.Format("{{ {0}: {1}, {2}: {3}, {4}: {5}, {6}, {7}, {8}: {9}, {10}: {11} }}", "Title", Title, "Description", Description, "ShowReplay", ShowReplay, ShowEndMatch, ShowNextLevel, "BasicStats", BasicStats.Length, "PerPlayerStats", PerPlayerStats.Length);
	}
}
