using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000254 RID: 596
	[Serializable]
	public struct ComboStep
	{
		// Token: 0x1700023A RID: 570
		// (get) Token: 0x06000C55 RID: 3157 RVA: 0x00042C3C File Offset: 0x00040E3C
		public MeleeAttackData MeleeAttackData
		{
			get
			{
				return this.MeleeAttackPrefab.GetComponent<MeleeAttackData>();
			}
		}

		// Token: 0x04000B5E RID: 2910
		public GameObject MeleeAttackPrefab;

		// Token: 0x04000B5F RID: 2911
		public int MeleeAttackIndex;

		// Token: 0x04000B60 RID: 2912
		public uint pauseFramesAfterAttack;
	}
}
