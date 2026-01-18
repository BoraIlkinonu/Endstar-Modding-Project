using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000182 RID: 386
	public class RangedAttackStrategy : IActionStrategy
	{
		// Token: 0x060008C8 RID: 2248 RVA: 0x000292B0 File Offset: 0x000274B0
		public RangedAttackStrategy(NpcEntity entity)
		{
			this.entity = entity;
		}

		// Token: 0x170001AB RID: 427
		// (get) Token: 0x060008C9 RID: 2249 RVA: 0x000292BF File Offset: 0x000274BF
		public Func<float> GetCost { get; }

		// Token: 0x170001AC RID: 428
		// (get) Token: 0x060008CA RID: 2250 RVA: 0x000292C7 File Offset: 0x000274C7
		// (set) Token: 0x060008CB RID: 2251 RVA: 0x000292CF File Offset: 0x000274CF
		public GoapAction.Status Status { get; private set; }

		// Token: 0x060008CC RID: 2252 RVA: 0x000292D8 File Offset: 0x000274D8
		public void Start()
		{
			if (!this.entity.Target)
			{
				this.Status = GoapAction.Status.Failed;
				return;
			}
			this.Status = GoapAction.Status.InProgress;
			this.entity.Components.PathFollower.StopPath(false);
			this.entity.Components.Animator.SetBool(NpcAnimator.ZLock, true);
			uint num = NetClock.CurrentFrame + 10U + 5U;
			((RangedAttackComponent)this.entity.Components.Attack).EnqueueRangedAttack(num, this.entity.RangedAttackFrames);
			this.lastAttackFrame = num + this.entity.RangedAttackFrames + 15U;
		}

		// Token: 0x060008CD RID: 2253 RVA: 0x0002937E File Offset: 0x0002757E
		public void Tick(uint frame)
		{
			if (!this.entity.Target)
			{
				this.Status = GoapAction.Status.Failed;
				return;
			}
			if (frame >= this.lastAttackFrame)
			{
				this.Status = GoapAction.Status.Complete;
			}
		}

		// Token: 0x060008CE RID: 2254 RVA: 0x000293AC File Offset: 0x000275AC
		public void Update(float deltaTime)
		{
			if (!this.entity.Target)
			{
				this.Status = GoapAction.Status.Failed;
				return;
			}
			Vector3 vector = this.entity.Target.transform.position - this.entity.transform.position;
			vector = new Vector3(vector.x, 0f, vector.z);
			Quaternion quaternion = Quaternion.LookRotation(vector, Vector3.up);
			if (Quaternion.Angle(this.entity.transform.rotation, quaternion) < 5f)
			{
				return;
			}
			this.entity.transform.rotation = Quaternion.RotateTowards(this.entity.transform.rotation, quaternion, this.entity.Settings.RotationSpeed * deltaTime);
		}

		// Token: 0x060008CF RID: 2255 RVA: 0x00029478 File Offset: 0x00027678
		public void Stop()
		{
			this.entity.Components.Animator.SetBool(NpcAnimator.ZLock, false);
			this.entity.HasAttackToken = false;
		}

		// Token: 0x0400073A RID: 1850
		private readonly NpcEntity entity;

		// Token: 0x0400073B RID: 1851
		private readonly CountdownTimer timer;

		// Token: 0x0400073C RID: 1852
		private bool movingRight;

		// Token: 0x0400073D RID: 1853
		private uint lastAttackFrame;

		// Token: 0x0400073E RID: 1854
		private float cachedSpeed;
	}
}
