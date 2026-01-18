using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay;

public static class MeleeAttackDataPool
{
	private static Dictionary<MeleeAttackData, MeleeAttackData> pool = new Dictionary<MeleeAttackData, MeleeAttackData>();

	public static MeleeAttackData GetRuntimeInstance(MeleeAttackData prefab)
	{
		if (pool.ContainsKey(prefab))
		{
			return pool[prefab];
		}
		MeleeAttackData meleeAttackData = CreateInstance(prefab);
		meleeAttackData.RuntimeSetup();
		pool.Add(prefab, meleeAttackData);
		return meleeAttackData;
	}

	private static MeleeAttackData CreateInstance(MeleeAttackData prefab)
	{
		return Object.Instantiate(prefab);
	}
}
