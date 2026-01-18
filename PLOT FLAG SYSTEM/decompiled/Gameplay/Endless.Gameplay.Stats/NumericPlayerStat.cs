using System.Collections.Generic;
using Endless.Gameplay.Scripting;
using Newtonsoft.Json;

namespace Endless.Gameplay.Stats;

public class NumericPlayerStat : StatBase
{
	public NumericDisplayFormat DisplayFormat;

	[JsonProperty]
	protected Dictionary<int, float> statMap = new Dictionary<int, float>();

	public void ModifyStat(Context playerContext, float delta)
	{
		if (TryGetUserId(playerContext, out var userId) && !statMap.TryAdd(userId, delta))
		{
			statMap[userId] += delta;
		}
	}

	internal void SetStats(Dictionary<int, float> newStatMap)
	{
		statMap = newStatMap;
	}
}
