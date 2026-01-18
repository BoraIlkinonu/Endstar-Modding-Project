using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020000FF RID: 255
	public static class MeleeAttackDataPool
	{
		// Token: 0x060005AE RID: 1454 RVA: 0x0001C62C File Offset: 0x0001A82C
		public static MeleeAttackData GetRuntimeInstance(MeleeAttackData prefab)
		{
			if (MeleeAttackDataPool.pool.ContainsKey(prefab))
			{
				return MeleeAttackDataPool.pool[prefab];
			}
			MeleeAttackData meleeAttackData = MeleeAttackDataPool.CreateInstance(prefab);
			meleeAttackData.RuntimeSetup();
			MeleeAttackDataPool.pool.Add(prefab, meleeAttackData);
			return meleeAttackData;
		}

		// Token: 0x060005AF RID: 1455 RVA: 0x0001C66C File Offset: 0x0001A86C
		private static MeleeAttackData CreateInstance(MeleeAttackData prefab)
		{
			return global::UnityEngine.Object.Instantiate<MeleeAttackData>(prefab);
		}

		// Token: 0x04000444 RID: 1092
		private static Dictionary<MeleeAttackData, MeleeAttackData> pool = new Dictionary<MeleeAttackData, MeleeAttackData>();
	}
}
