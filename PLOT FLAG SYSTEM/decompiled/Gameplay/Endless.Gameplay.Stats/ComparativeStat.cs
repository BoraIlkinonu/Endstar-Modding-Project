using System;
using Newtonsoft.Json;

namespace Endless.Gameplay.Stats;

public class ComparativeStat : NumericPlayerStat
{
	public enum ValueComparison
	{
		Most,
		Least
	}

	public ValueComparison Comparison;

	internal bool IsValid => statMap.Values.Count > 0;

	internal BasicStat ToBasicStat()
	{
		int num = -1;
		float num2 = ((Comparison == ValueComparison.Most) ? float.MinValue : float.MaxValue);
		foreach (int key in statMap.Keys)
		{
			if (key == -1)
			{
				num = key;
				num2 = statMap[num];
				continue;
			}
			switch (Comparison)
			{
			case ValueComparison.Most:
				if (statMap[key] > num2)
				{
					num = key;
					num2 = statMap[key];
				}
				break;
			case ValueComparison.Least:
				if (statMap[key] < num2)
				{
					num = key;
					num2 = statMap[key];
				}
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
		return new BasicStat
		{
			Message = Message,
			Identifier = Identifier,
			Order = Order,
			InventoryIcon = InventoryIcon,
			UserId = num,
			Value = StatBase.GetFormattedString(num2, DisplayFormat)
		};
	}

	public override string ToString()
	{
		return JsonConvert.SerializeObject(this);
	}

	public void LoadFromString(string stringData)
	{
		ComparativeStat comparativeStat = JsonConvert.DeserializeObject<ComparativeStat>(stringData);
		CopyFrom(comparativeStat);
		Comparison = comparativeStat.Comparison;
		DisplayFormat = comparativeStat.DisplayFormat;
		statMap = comparativeStat.statMap;
	}
}
