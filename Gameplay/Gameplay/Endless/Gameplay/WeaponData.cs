using System;
using System.Collections.Generic;
using Endless.Shared.SoVariables;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000257 RID: 599
	[Serializable]
	public struct WeaponData
	{
		// Token: 0x04000B69 RID: 2921
		public NpcEnum.Weapon Weapon;

		// Token: 0x04000B6A RID: 2922
		public StringReference BoneName;

		// Token: 0x04000B6B RID: 2923
		public List<GameObject> Variants;
	}
}
