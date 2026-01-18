using System;
using System.Collections;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020001A9 RID: 425
	public abstract class Motion
	{
		// Token: 0x06000988 RID: 2440 RVA: 0x0002BDEE File Offset: 0x00029FEE
		protected Motion(NpcEntity entity, NavPath.Segment segment, Transform lookRotationOverride)
		{
			this.entity = entity;
			this.segment = segment;
			this.lookRotationOverride = lookRotationOverride;
		}

		// Token: 0x06000989 RID: 2441
		public abstract bool CanRun();

		// Token: 0x0600098A RID: 2442
		public abstract bool IsComplete();

		// Token: 0x0600098B RID: 2443
		public abstract bool HasFailed();

		// Token: 0x0600098C RID: 2444
		public abstract void WriteState(ref NpcState state);

		// Token: 0x0600098D RID: 2445
		public abstract IEnumerator Execute();

		// Token: 0x0600098E RID: 2446
		public abstract void Stop();

		// Token: 0x040007BB RID: 1979
		protected readonly NpcEntity entity;

		// Token: 0x040007BC RID: 1980
		protected readonly NavPath.Segment segment;

		// Token: 0x040007BD RID: 1981
		protected readonly Transform lookRotationOverride;

		// Token: 0x020001AA RID: 426
		public static class Factory
		{
			// Token: 0x0600098F RID: 2447 RVA: 0x0002BE0C File Offset: 0x0002A00C
			public static Motion Build(NpcEntity entity, NavPath.Segment segment, Transform lookRotationOverride = null)
			{
				Motion motion;
				switch (segment.ConnectionKind)
				{
				case ConnectionKind.Walk:
					motion = new Motion.Factory.WalkProxy(entity, segment, lookRotationOverride);
					break;
				case ConnectionKind.Threshold:
					motion = new Motion.Factory.ThresholdProxy(entity, segment, lookRotationOverride);
					break;
				case ConnectionKind.Jump:
					motion = new Motion.Factory.JumpProxy(entity, segment, lookRotationOverride);
					break;
				case ConnectionKind.Dropdown:
					motion = new Motion.Factory.JumpProxy(entity, segment, lookRotationOverride);
					break;
				case ConnectionKind.Swim:
					throw new NotImplementedException();
				default:
					throw new ArgumentOutOfRangeException();
				}
				return motion;
			}

			// Token: 0x020001AB RID: 427
			private class WalkProxy : Walk
			{
				// Token: 0x06000990 RID: 2448 RVA: 0x0002BE75 File Offset: 0x0002A075
				public WalkProxy(NpcEntity entity, NavPath.Segment segment, Transform lookRotationOverride)
					: base(entity, segment, lookRotationOverride)
				{
				}
			}

			// Token: 0x020001AC RID: 428
			private class JumpProxy : Jump
			{
				// Token: 0x06000991 RID: 2449 RVA: 0x0002BE80 File Offset: 0x0002A080
				public JumpProxy(NpcEntity entity, NavPath.Segment segment, Transform lookRotationOverride)
					: base(entity, segment, lookRotationOverride)
				{
				}
			}

			// Token: 0x020001AD RID: 429
			private class ThresholdProxy : Threshold
			{
				// Token: 0x06000992 RID: 2450 RVA: 0x0002BE8B File Offset: 0x0002A08B
				public ThresholdProxy(NpcEntity entity, NavPath.Segment segment, Transform lookRotationOverride)
					: base(entity, segment, lookRotationOverride)
				{
				}
			}
		}
	}
}
