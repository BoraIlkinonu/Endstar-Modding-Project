using System;
using System.Collections.Generic;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay.Fsm
{
	// Token: 0x0200043F RID: 1087
	public class WarpState : FsmState
	{
		// Token: 0x1700057D RID: 1405
		// (get) Token: 0x06001B33 RID: 6963 RVA: 0x0007BAEC File Offset: 0x00079CEC
		public override NpcEnum.FsmState State
		{
			get
			{
				return NpcEnum.FsmState.Warp;
			}
		}

		// Token: 0x06001B34 RID: 6964 RVA: 0x0007BAF0 File Offset: 0x00079CF0
		public WarpState(NpcEntity entity)
			: base(entity)
		{
		}

		// Token: 0x06001B35 RID: 6965 RVA: 0x0007BB08 File Offset: 0x00079D08
		public override void Enter()
		{
			base.Enter();
			List<Vector3> list = MonoBehaviourSingleton<Pathfinding>.Instance.FindNavigationPositionsInRange(base.Entity.transform.position, 5f);
			foreach (Vector3 vector in list)
			{
				Vector3 vector2 = vector + Vector3.up;
				if (Physics.OverlapSphereNonAlloc(vector2, 0.1f, this.colliders, base.Entity.Settings.CharacterCollisionMask) <= 0)
				{
					this.warpPosition = vector2;
					break;
				}
			}
			if (this.warpPosition.Equals(Vector3.zero))
			{
				this.warpPosition = list[0];
			}
			this.warpCompleteFrame = NetClock.CurrentFrame + 8U;
			base.Entity.Components.IndividualStateUpdater.OnTickAi += this.HandleOnTickAi;
		}

		// Token: 0x06001B36 RID: 6966 RVA: 0x0007BC00 File Offset: 0x00079E00
		protected override void Exit()
		{
			base.Exit();
			base.Entity.Components.IndividualStateUpdater.OnTickAi -= this.HandleOnTickAi;
		}

		// Token: 0x06001B37 RID: 6967 RVA: 0x0007BC2C File Offset: 0x00079E2C
		private void HandleOnTickAi()
		{
			if (NetClock.CurrentFrame >= this.warpCompleteFrame)
			{
				base.Entity.Position = this.warpPosition + Vector3.up * 0.5f;
				base.Entity.Components.Agent.Warp(this.warpPosition);
				base.Components.Parameters.WarpCompleteTrigger = true;
			}
		}

		// Token: 0x04001598 RID: 5528
		private Vector3 warpPosition;

		// Token: 0x04001599 RID: 5529
		private uint warpCompleteFrame;

		// Token: 0x0400159A RID: 5530
		private readonly Collider[] colliders = new Collider[5];
	}
}
