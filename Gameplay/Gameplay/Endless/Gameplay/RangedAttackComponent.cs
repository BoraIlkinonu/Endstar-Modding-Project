using System;
using Endless.Matchmaking;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200014D RID: 333
	public class RangedAttackComponent : AttackComponent
	{
		// Token: 0x060007E6 RID: 2022 RVA: 0x0002532C File Offset: 0x0002352C
		protected override void HandleOnUpdateAiState(uint frame)
		{
			if (!this.nextAttackTiming.IsValid)
			{
				return;
			}
			ref NpcState currentState = ref base.NpcEntity.Components.IndividualStateUpdater.GetCurrentState();
			if (this.nextAttackTiming.StartFrame == frame + 10U)
			{
				base.NpcEntity.Components.AttackAlert.ImminentlyAttacking(false);
				currentState.ImminentlyAttacking = true;
			}
			if (frame >= this.nextAttackTiming.StartFrame && frame <= this.nextAttackTiming.EndFrame && (this.nextAttackTiming.StartFrame - frame) % 2U == 0U)
			{
				base.NpcEntity.Components.Animator.SetTrigger(NpcAnimator.Attack);
				currentState.attack = 1;
				if (MatchSession.Instance.IsHost || MatchSession.Instance.IsServer)
				{
					this.emitter.SpawnProjectile(true, this.minDeflectionScalar, this.maxDeflectionScalar);
				}
			}
		}

		// Token: 0x060007E7 RID: 2023 RVA: 0x00025409 File Offset: 0x00023609
		public override void ClearAttackQueue()
		{
			this.nextAttackTiming = default(RangedAttackComponent.EnqueuedRangedAttack);
		}

		// Token: 0x060007E8 RID: 2024 RVA: 0x00025417 File Offset: 0x00023617
		public void EnqueueRangedAttack(uint nextAttackFrame, uint duration)
		{
			this.nextAttackTiming = new RangedAttackComponent.EnqueuedRangedAttack(nextAttackFrame, duration);
		}

		// Token: 0x0400063E RID: 1598
		[SerializeField]
		private uint attackLength;

		// Token: 0x0400063F RID: 1599
		[SerializeField]
		private ProjectileEmitter emitter;

		// Token: 0x04000640 RID: 1600
		[SerializeField]
		[Range(0f, 10f)]
		private float minDeflectionScalar;

		// Token: 0x04000641 RID: 1601
		[SerializeField]
		[Range(0f, 10f)]
		private float maxDeflectionScalar;

		// Token: 0x04000642 RID: 1602
		private RangedAttackComponent.EnqueuedRangedAttack nextAttackTiming;

		// Token: 0x0200014E RID: 334
		private struct EnqueuedRangedAttack
		{
			// Token: 0x1700017B RID: 379
			// (get) Token: 0x060007EA RID: 2026 RVA: 0x0002542E File Offset: 0x0002362E
			public readonly uint StartFrame { get; }

			// Token: 0x1700017C RID: 380
			// (get) Token: 0x060007EB RID: 2027 RVA: 0x00025436 File Offset: 0x00023636
			public readonly uint EndFrame { get; }

			// Token: 0x1700017D RID: 381
			// (get) Token: 0x060007EC RID: 2028 RVA: 0x0002543E File Offset: 0x0002363E
			public readonly bool IsValid { get; }

			// Token: 0x060007ED RID: 2029 RVA: 0x00025446 File Offset: 0x00023646
			public EnqueuedRangedAttack(uint startFrame, uint duration)
			{
				this.StartFrame = startFrame;
				this.EndFrame = startFrame + duration;
				this.IsValid = true;
			}
		}
	}
}
