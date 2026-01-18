using System;
using Endless.Gameplay.LuaEnums;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000253 RID: 595
	[Serializable]
	public struct ClassData
	{
		// Token: 0x04000B56 RID: 2902
		public NpcClass Class;

		// Token: 0x04000B57 RID: 2903
		public GameObject DefaultWeapon;

		// Token: 0x04000B58 RID: 2904
		public string WeaponBone;

		// Token: 0x04000B59 RID: 2905
		public GameObject DefaultEquipment;

		// Token: 0x04000B5A RID: 2906
		public string EquipmentBone;

		// Token: 0x04000B5B RID: 2907
		public GoalsList Goals;

		// Token: 0x04000B5C RID: 2908
		public GameObject AttackComponent;

		// Token: 0x04000B5D RID: 2909
		public int EquippedItemParameter;
	}
}
