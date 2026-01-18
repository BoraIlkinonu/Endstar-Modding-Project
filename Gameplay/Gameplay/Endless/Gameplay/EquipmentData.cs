using System;
using Endless.Shared.SoVariables;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000255 RID: 597
	[Serializable]
	public struct EquipmentData
	{
		// Token: 0x04000B61 RID: 2913
		public NpcEnum.Equipment Equipment;

		// Token: 0x04000B62 RID: 2914
		public GameObject GameObject;

		// Token: 0x04000B63 RID: 2915
		public StringReference BoneName;
	}
}
