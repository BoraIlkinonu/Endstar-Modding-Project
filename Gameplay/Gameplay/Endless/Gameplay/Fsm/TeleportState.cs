using System;
using Endless.Gameplay.LuaEnums;
using UnityEngine;

namespace Endless.Gameplay.Fsm
{
	// Token: 0x0200043E RID: 1086
	public class TeleportState : FsmState
	{
		// Token: 0x06001B2E RID: 6958 RVA: 0x0007A9F5 File Offset: 0x00078BF5
		public TeleportState(NpcEntity entity)
			: base(entity)
		{
		}

		// Token: 0x06001B2F RID: 6959 RVA: 0x0007B8BC File Offset: 0x00079ABC
		public override void Enter()
		{
			base.Enter();
			base.Components.IndividualStateUpdater.OnWriteState += this.HandleOnWriteState;
			base.Components.HittableComponent.IsDamageable = false;
			base.Entity.BasePhysicsMode = PhysicsMode.IgnorePhysics;
			int num;
			if (!base.Entity.NpcBlackboard.TryGet<int>(NpcBlackboard.Key.TeleportType, out num) || !base.Entity.NpcBlackboard.TryGet<Vector3>(NpcBlackboard.Key.TeleportPosition, out this.teleportPosition) || !base.Entity.NpcBlackboard.TryGet<float>(NpcBlackboard.Key.TeleportRotation, out this.teleportRotation))
			{
				Debug.LogError("Incomplete teleport data");
				base.Components.Parameters.TeleportCompleteTrigger = true;
				return;
			}
			this.teleportType = (TeleportType)num;
			TeleportInfo teleportInfo = RuntimeDatabase.GetTeleportInfo(this.teleportType);
			this.teleportEndFrame = NetClock.CurrentFrame + teleportInfo.FramesToTeleport;
		}

		// Token: 0x06001B30 RID: 6960 RVA: 0x0007B99C File Offset: 0x00079B9C
		private void HandleOnWriteState(ref NpcState npcState)
		{
			if (!this.teleporting)
			{
				RuntimeDatabase.GetTeleportInfo(this.teleportType).TeleportStart(base.Entity.WorldObject.EndlessVisuals, base.Entity.Components.Animator, base.Entity.Position);
				base.Entity.StartTeleportClientRpc(this.teleportType);
				this.teleporting = true;
			}
			if (NetClock.CurrentFrame == this.teleportEndFrame)
			{
				base.Entity.Position = this.teleportPosition;
				base.Entity.Rotation = this.teleportRotation;
				RuntimeDatabase.GetTeleportInfo(this.teleportType).TeleportEnd(base.Entity.WorldObject.EndlessVisuals, base.Entity.Components.Animator, base.Entity.Position);
				base.Entity.EndTeleportClientRpc(this.teleportType);
				base.Components.Parameters.TeleportCompleteTrigger = true;
			}
		}

		// Token: 0x06001B31 RID: 6961 RVA: 0x0007BA90 File Offset: 0x00079C90
		protected override void Exit()
		{
			base.Exit();
			base.Components.IndividualStateUpdater.OnWriteState -= this.HandleOnWriteState;
			base.Components.HittableComponent.IsDamageable = true;
			this.teleporting = false;
			base.Entity.BasePhysicsMode = this.oldPhysicsMode;
		}

		// Token: 0x1700057C RID: 1404
		// (get) Token: 0x06001B32 RID: 6962 RVA: 0x0007BAE8 File Offset: 0x00079CE8
		public override NpcEnum.FsmState State
		{
			get
			{
				return NpcEnum.FsmState.Teleport;
			}
		}

		// Token: 0x04001592 RID: 5522
		private uint teleportEndFrame;

		// Token: 0x04001593 RID: 5523
		private Vector3 teleportPosition;

		// Token: 0x04001594 RID: 5524
		private float teleportRotation;

		// Token: 0x04001595 RID: 5525
		private bool teleporting;

		// Token: 0x04001596 RID: 5526
		private PhysicsMode oldPhysicsMode;

		// Token: 0x04001597 RID: 5527
		private TeleportType teleportType;
	}
}
