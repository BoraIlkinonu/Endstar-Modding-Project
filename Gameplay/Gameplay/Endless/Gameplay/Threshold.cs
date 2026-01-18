using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Mathematics;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020001B0 RID: 432
	public class Threshold : Motion
	{
		// Token: 0x060009A0 RID: 2464 RVA: 0x0002C14C File Offset: 0x0002A34C
		protected Threshold(NpcEntity entity, NavPath.Segment segment, Transform lookRotationOverride)
			: base(entity, segment, lookRotationOverride)
		{
			this.Door = MonoBehaviourSingleton<Pathfinding>.Instance.GetDoorFromThresholdSection(segment.StartSection);
			this.canRun = this.Door && this.Door.CurrentNpcDoorInteraction > Door.NpcDoorInteraction.NotOpenable;
		}

		// Token: 0x060009A1 RID: 2465 RVA: 0x0002C19C File Offset: 0x0002A39C
		public override bool CanRun()
		{
			return this.canRun;
		}

		// Token: 0x060009A2 RID: 2466 RVA: 0x0002C1A4 File Offset: 0x0002A3A4
		public override bool IsComplete()
		{
			return this.thresholdPassed;
		}

		// Token: 0x060009A3 RID: 2467 RVA: 0x0002C1AC File Offset: 0x0002A3AC
		public override bool HasFailed()
		{
			return this.failedToWalkThrough;
		}

		// Token: 0x060009A4 RID: 2468 RVA: 0x00002DB0 File Offset: 0x00000FB0
		public override void WriteState(ref NpcState state)
		{
		}

		// Token: 0x060009A5 RID: 2469 RVA: 0x0002C1B4 File Offset: 0x0002A3B4
		public override IEnumerator Execute()
		{
			yield return new WaitForSeconds(0.25f);
			if (this.Door.IsOpenOrOpening)
			{
				if (this.entity.Components.Agent.SetDestination(this.segment.EndPosition))
				{
					this.failedToWalkThrough = true;
				}
			}
			else
			{
				this.entity.Components.Animator.SetTrigger(NpcAnimator.Interact);
				if (this.Door.IsLocked)
				{
					Lockable lockable = this.Door.WorldObject.GetUserComponent<Lockable>();
					foreach (NpcEntity npcEntity in MonoBehaviourSingleton<NpcManager>.Instance.Npcs)
					{
						if (!(npcEntity == this.entity) && npcEntity.Group == this.entity.Group && npcEntity.Components.TargeterComponent.KnownHittables.Contains(this.entity.Components.HittableComponent))
						{
							npcEntity.Components.Pathing.ExcludeEdge(lockable, this.segment);
						}
					}
					yield return new WaitForSeconds(0.75f);
					this.entity.Components.Pathing.ExcludeEdge(lockable, this.segment);
					this.failedToWalkThrough = true;
					yield break;
				}
				this.Door.Open(this.entity.Context, true);
				MonoBehaviourSingleton<Pathfinding>.Instance.OnPathfindingUpdated -= this.HandleNavigationUpdated;
				MonoBehaviourSingleton<Pathfinding>.Instance.OnPathfindingUpdated += this.HandleNavigationUpdated;
				yield return new WaitUntil(() => this.navigationUpdated);
				MonoBehaviourSingleton<Pathfinding>.Instance.OnPathfindingUpdated -= this.HandleNavigationUpdated;
				if (!this.entity.Components.Agent.SetDestination(this.segment.EndPosition))
				{
					this.failedToWalkThrough = true;
					yield break;
				}
				if (!this.entity.Components.GoapController.HasCombatPlan && this.Door.CurrentNpcDoorInteraction == Door.NpcDoorInteraction.OpenAndCloseBehind)
				{
					while (this.entity.Components.Agent.pathPending || this.entity.Components.Agent.remainingDistance > this.entity.Components.Agent.stoppingDistance)
					{
						yield return null;
					}
					float3 @float = this.segment.StartPosition - this.segment.EndPosition;
					@float = new float3(@float.x, 0f, @float.z);
					Quaternion rotation = quaternion.LookRotation(@float, math.up());
					while (Quaternion.Angle(this.entity.transform.rotation, rotation) > 5f)
					{
						this.entity.transform.rotation = Quaternion.RotateTowards(this.entity.transform.rotation, rotation, this.entity.Settings.RotationSpeed * Time.deltaTime);
						yield return null;
					}
					this.Door.Close(this.entity.Context);
					this.entity.Components.Animator.SetTrigger(NpcAnimator.Interact);
					yield return new WaitForSeconds(0.25f);
					rotation = default(Quaternion);
				}
			}
			while (this.entity.Components.Agent.remainingDistance > 0.05f)
			{
				yield return null;
			}
			this.thresholdPassed = true;
			yield break;
		}

		// Token: 0x060009A6 RID: 2470 RVA: 0x0002C1C3 File Offset: 0x0002A3C3
		private void HandleNavigationUpdated(HashSet<SerializableGuid> updatedProps)
		{
			if (updatedProps.Contains(this.Door.WorldObject.InstanceId))
			{
				this.navigationUpdated = true;
			}
		}

		// Token: 0x060009A7 RID: 2471 RVA: 0x0002C1E4 File Offset: 0x0002A3E4
		public override void Stop()
		{
			if (this.entity.Components.Agent.isOnNavMesh)
			{
				this.entity.Components.Agent.ResetPath();
			}
			MonoBehaviourSingleton<Pathfinding>.Instance.OnPathfindingUpdated -= this.HandleNavigationUpdated;
		}

		// Token: 0x040007C3 RID: 1987
		public readonly Door Door;

		// Token: 0x040007C4 RID: 1988
		private readonly bool canRun;

		// Token: 0x040007C5 RID: 1989
		private bool thresholdPassed;

		// Token: 0x040007C6 RID: 1990
		private bool navigationUpdated;

		// Token: 0x040007C7 RID: 1991
		private bool failedToWalkThrough;
	}
}
