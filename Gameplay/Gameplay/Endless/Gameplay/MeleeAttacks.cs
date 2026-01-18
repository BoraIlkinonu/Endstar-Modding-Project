using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200023F RID: 575
	[CreateAssetMenu(menuName = "ScriptableObject/AiMeleeAttacks", fileName = "AiMeleeAttacks")]
	public class MeleeAttacks : ScriptableObject
	{
		// Token: 0x06000BD2 RID: 3026 RVA: 0x00040D8C File Offset: 0x0003EF8C
		public MeleeAttackData GetMeleeAttackDataForIndex(int index)
		{
			return this.MeleeAttackPrefabs[index].GetComponent<MeleeAttackData>();
		}

		// Token: 0x04000B07 RID: 2823
		[SerializeField]
		public List<GameObject> MeleeAttackPrefabs;
	}
}
