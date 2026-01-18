using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000264 RID: 612
	public struct TargeterDatum
	{
		// Token: 0x04000BB2 RID: 2994
		public Vector3 Position;

		// Token: 0x04000BB3 RID: 2995
		public Vector3 LookVector;

		// Token: 0x04000BB4 RID: 2996
		public float ProximityDistance;

		// Token: 0x04000BB5 RID: 2997
		public float MaxDistance;

		// Token: 0x04000BB6 RID: 2998
		public float VerticalViewAngle;

		// Token: 0x04000BB7 RID: 2999
		public float HorizontalViewAngle;

		// Token: 0x04000BB8 RID: 3000
		public bool UseXray;
	}
}
