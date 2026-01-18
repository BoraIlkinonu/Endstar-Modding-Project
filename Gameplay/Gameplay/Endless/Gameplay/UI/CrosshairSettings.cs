using System;
using UnityEngine;

namespace Endless.Gameplay.UI
{
	// Token: 0x02000390 RID: 912
	[Serializable]
	public struct CrosshairSettings
	{
		// Token: 0x040012A6 RID: 4774
		public float maxSpread;

		// Token: 0x040012A7 RID: 4775
		public float resetSpeed;

		// Token: 0x040012A8 RID: 4776
		public float weaponStrength;

		// Token: 0x040012A9 RID: 4777
		public float weaponAccuracy;

		// Token: 0x040012AA RID: 4778
		public float movementPenalty;

		// Token: 0x040012AB RID: 4779
		public AnimationCurve recoilSettleCurve;
	}
}
