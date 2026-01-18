using System;
using Endless.Gameplay.LuaEnums;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000218 RID: 536
	public struct NativePathfindingRequest
	{
		// Token: 0x04000A83 RID: 2691
		public Vector3 StartPosition;

		// Token: 0x04000A84 RID: 2692
		public Vector3 EndPosition;

		// Token: 0x04000A85 RID: 2693
		public int StartNodeKey;

		// Token: 0x04000A86 RID: 2694
		public int EndNodeKey;

		// Token: 0x04000A87 RID: 2695
		public PathfindingRange PathfindingRange;

		// Token: 0x04000A88 RID: 2696
		public float OnMeshPathDistance;
	}
}
