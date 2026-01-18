using System;
using Endless.Shared;
using UnityEngine;

namespace Endless.Props
{
	// Token: 0x02000006 RID: 6
	[Serializable]
	public class ProjectileShooterEjectionSettings
	{
		// Token: 0x04000007 RID: 7
		[Header("Casing settings")]
		[Tooltip("This is the initial velocity in m/s")]
		[MinMax(0f, 5f)]
		public Vector2 casingForce = new Vector2(0.25f, 2.5f);

		// Token: 0x04000008 RID: 8
		[Tooltip("Max angle variation in degrees, relative to the prefab's rotation")]
		[Range(0f, 25f)]
		public float casingAngleVariance = 10f;

		// Token: 0x04000009 RID: 9
		[Tooltip("Random angular velocity on X axis relative to ejection transform")]
		[MinMax(-1080f, 1080f, " X", "Casing Angular Velocity Variance")]
		public Vector2 casingAngularVelocityVarianceX;

		// Token: 0x0400000A RID: 10
		[Tooltip("Random angular velocity on Y axis relative to ejection transform")]
		[MinMax(-1080f, 1080f, " Y")]
		public Vector2 casingAngularVelocityVarianceY;

		// Token: 0x0400000B RID: 11
		[Tooltip("Random angular velocity on Z axis relative to ejection transform")]
		[MinMax(-1080f, 1080f, " Z")]
		public Vector2 casingAngularVelocityVarianceZ;

		// Token: 0x0400000C RID: 12
		public float casingDrag = 0.3f;

		// Token: 0x0400000D RID: 13
		public float casingAngularDrag = 0.6f;

		// Token: 0x0400000E RID: 14
		[Tooltip("If drag values are adjusted manually on the rigidbody, don't use these values. Adding this because the default drag of 0.05 and 0 make casings roll around forever.")]
		public bool casingOverrideDrag = true;

		// Token: 0x0400000F RID: 15
		[Header("Magazine settings")]
		[Tooltip("This is the initial velocity in m/s")]
		[MinMax(0f, 5f)]
		public Vector2 magForce = new Vector2(0.25f, 2.5f);

		// Token: 0x04000010 RID: 16
		[Tooltip("Max angle variation in degrees, relative to the prefab's rotation")]
		[Range(0f, 25f)]
		public float magAngleVariance = 10f;

		// Token: 0x04000011 RID: 17
		[Tooltip("Random angular velocity on X axis relative to ejection transform")]
		[MinMax(-1080f, 1080f, " X", "Mag Angular Velocity Variance")]
		public Vector2 magAngularVelocityVarianceX;

		// Token: 0x04000012 RID: 18
		[Tooltip("Random angular velocity on Y axis relative to ejection transform")]
		[MinMax(-1080f, 1080f, " Y")]
		public Vector2 magAngularVelocityVarianceY;

		// Token: 0x04000013 RID: 19
		[Tooltip("Random angular velocity on Z axis relative to ejection transform")]
		[MinMax(-1080f, 1080f, " Z")]
		public Vector2 magAngularVelocityVarianceZ;

		// Token: 0x04000014 RID: 20
		public float magDrag;

		// Token: 0x04000015 RID: 21
		public float magAngularDrag;

		// Token: 0x04000016 RID: 22
		public bool magOverrideDrag;
	}
}
