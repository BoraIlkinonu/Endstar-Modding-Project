using System;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000152 RID: 338
	public class TriggerDetector : NpcComponent, IStartSubscriber
	{
		// Token: 0x060007F5 RID: 2037 RVA: 0x0002563A File Offset: 0x0002383A
		private void HandleOnCheckWorldTriggers(uint frame)
		{
			this.HandleOnCheckWorldTriggers(frame, false);
		}

		// Token: 0x060007F6 RID: 2038 RVA: 0x00025644 File Offset: 0x00023844
		private void HandleOnCheckWorldTriggers(uint frame, bool onDestroy)
		{
			Vector3 vector = base.transform.position + Vector3.down * 0.5f;
			Vector3 vector2 = vector + Vector3.up;
			this.hitCount = Physics.OverlapCapsuleNonAlloc(vector, vector2, this.detectionRadius, this.hits, this.worldEffectMask, QueryTriggerInteraction.Collide);
			for (int i = 0; i < this.hitCount; i++)
			{
				WorldTriggerCollider component = this.hits[i].GetComponent<WorldTriggerCollider>();
				WorldTrigger worldTrigger = ((component != null) ? component.WorldTrigger : null);
				if (worldTrigger)
				{
					if (onDestroy)
					{
						worldTrigger.DestroyOverlap(base.Components.WorldCollidable, frame);
					}
					else
					{
						worldTrigger.Overlapped(base.Components.WorldCollidable, frame, true);
					}
				}
			}
		}

		// Token: 0x060007F7 RID: 2039 RVA: 0x000256FF File Offset: 0x000238FF
		protected override void OnDestroy()
		{
			if (MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying)
			{
				this.HandleOnCheckWorldTriggers(NetClock.CurrentFrame, true);
			}
			base.OnDestroy();
		}

		// Token: 0x060007F8 RID: 2040 RVA: 0x0002571F File Offset: 0x0002391F
		public void EndlessStart()
		{
			base.Components.IndividualStateUpdater.OnCheckWorldTriggers += this.HandleOnCheckWorldTriggers;
		}

		// Token: 0x0400064D RID: 1613
		[SerializeField]
		private LayerMask worldEffectMask;

		// Token: 0x0400064E RID: 1614
		[SerializeField]
		private float detectionRadius;

		// Token: 0x0400064F RID: 1615
		private int hitCount;

		// Token: 0x04000650 RID: 1616
		private readonly Collider[] hits = new Collider[5];
	}
}
