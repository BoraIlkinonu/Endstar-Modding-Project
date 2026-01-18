using System;
using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay.LuaInterfaces
{
	// Token: 0x02000463 RID: 1123
	public class Sentry
	{
		// Token: 0x06001C15 RID: 7189 RVA: 0x0007D3C9 File Offset: 0x0007B5C9
		internal Sentry(Sentry sentry)
		{
			this.sentry = sentry;
		}

		// Token: 0x06001C16 RID: 7190 RVA: 0x0007D3D8 File Offset: 0x0007B5D8
		public void Shoot(Context instigator)
		{
			if (NetClock.CurrentFrame > this.sentry.localState.ShootFrame)
			{
				this.sentry.localState.ShootFrame = NetClock.CurrentFrame + 1U;
				this.sentry.SyncState();
			}
		}

		// Token: 0x06001C17 RID: 7191 RVA: 0x0007D413 File Offset: 0x0007B613
		public void EnableTrackingLaser(Context instigator)
		{
			if (!this.sentry.localState.TrackingLaserEnabled)
			{
				this.sentry.localState.TrackingLaserEnabled = true;
				this.sentry.SyncState();
			}
		}

		// Token: 0x06001C18 RID: 7192 RVA: 0x0007D443 File Offset: 0x0007B643
		public void DisableTrackingLaser(Context instigator)
		{
			if (this.sentry.localState.TrackingLaserEnabled)
			{
				this.sentry.localState.TrackingLaserEnabled = false;
				this.sentry.SyncState();
			}
		}

		// Token: 0x06001C19 RID: 7193 RVA: 0x0007D473 File Offset: 0x0007B673
		public void SetDamagedState(Context instigator, int value)
		{
			this.sentry.damageLevel.Value = value;
		}

		// Token: 0x06001C1A RID: 7194 RVA: 0x0007D486 File Offset: 0x0007B686
		public void SetFollowTarget(Context instigator, Context target)
		{
			this.sentry.SetFollowTarget(target);
		}

		// Token: 0x06001C1B RID: 7195 RVA: 0x0007D494 File Offset: 0x0007B694
		public void SetLookDirection(Context instigator, float x, float y)
		{
			this.sentry.SetLookDirection(x, y);
		}

		// Token: 0x06001C1C RID: 7196 RVA: 0x0007D4A3 File Offset: 0x0007B6A3
		public float GetLookPitch()
		{
			return this.sentry.localState.CurrentPitch;
		}

		// Token: 0x06001C1D RID: 7197 RVA: 0x0007D4B5 File Offset: 0x0007B6B5
		public float GetLookYaw()
		{
			return Mathf.DeltaAngle(0f, this.sentry.localState.CurrentYaw - this.sentry.WorldObject.transform.eulerAngles.y);
		}

		// Token: 0x06001C1E RID: 7198 RVA: 0x0007D4EC File Offset: 0x0007B6EC
		public void SetRotationSpeed(Context instigator, float value)
		{
			this.sentry.RotationSpeed = value;
		}

		// Token: 0x06001C1F RID: 7199 RVA: 0x0007D4FA File Offset: 0x0007B6FA
		public float GetRotationSpeed()
		{
			return this.sentry.RotationSpeed;
		}

		// Token: 0x06001C20 RID: 7200 RVA: 0x0007D507 File Offset: 0x0007B707
		public void SetShootDistance(Context instigator, float value)
		{
			this.sentry.ShootDistance = value;
			this.sentry.localState.ShootDistance = value;
			this.sentry.SyncState();
		}

		// Token: 0x06001C21 RID: 7201 RVA: 0x0007D531 File Offset: 0x0007B731
		public float GetShootDistance()
		{
			return this.sentry.ShootDistance;
		}

		// Token: 0x06001C22 RID: 7202 RVA: 0x0007D53E File Offset: 0x0007B73E
		public void LookForward(Context instigator)
		{
			this.SetLookDirection(instigator, 0f, 0f);
		}

		// Token: 0x06001C23 RID: 7203 RVA: 0x0007D551 File Offset: 0x0007B751
		public void LookBackward(Context instigator)
		{
			this.SetLookDirection(instigator, 0f, 180f);
		}

		// Token: 0x06001C24 RID: 7204 RVA: 0x0007D564 File Offset: 0x0007B764
		public void LookUp(Context instigator)
		{
			this.SetLookDirection(instigator, -90f, this.GetLookYaw());
		}

		// Token: 0x06001C25 RID: 7205 RVA: 0x0007D578 File Offset: 0x0007B778
		public void LookDown(Context instigator)
		{
			this.SetLookDirection(instigator, 90f, this.GetLookYaw());
		}

		// Token: 0x06001C26 RID: 7206 RVA: 0x0007D58C File Offset: 0x0007B78C
		public void LookLeft(Context instigator)
		{
			this.SetLookDirection(instigator, 0f, -90f);
		}

		// Token: 0x06001C27 RID: 7207 RVA: 0x0007D59F File Offset: 0x0007B79F
		public void LookRight(Context instigator)
		{
			this.SetLookDirection(instigator, 0f, 90f);
		}

		// Token: 0x040015C5 RID: 5573
		private Sentry sentry;
	}
}
