using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay.LuaInterfaces;

public class Sentry
{
	private Endless.Gameplay.Sentry sentry;

	internal Sentry(Endless.Gameplay.Sentry sentry)
	{
		this.sentry = sentry;
	}

	public void Shoot(Context instigator)
	{
		if (NetClock.CurrentFrame > sentry.localState.ShootFrame)
		{
			sentry.localState.ShootFrame = NetClock.CurrentFrame + 1;
			sentry.SyncState();
		}
	}

	public void EnableTrackingLaser(Context instigator)
	{
		if (!sentry.localState.TrackingLaserEnabled)
		{
			sentry.localState.TrackingLaserEnabled = true;
			sentry.SyncState();
		}
	}

	public void DisableTrackingLaser(Context instigator)
	{
		if (sentry.localState.TrackingLaserEnabled)
		{
			sentry.localState.TrackingLaserEnabled = false;
			sentry.SyncState();
		}
	}

	public void SetDamagedState(Context instigator, int value)
	{
		sentry.damageLevel.Value = value;
	}

	public void SetFollowTarget(Context instigator, Context target)
	{
		sentry.SetFollowTarget(target);
	}

	public void SetLookDirection(Context instigator, float x, float y)
	{
		sentry.SetLookDirection(x, y);
	}

	public float GetLookPitch()
	{
		return sentry.localState.CurrentPitch;
	}

	public float GetLookYaw()
	{
		return Mathf.DeltaAngle(0f, sentry.localState.CurrentYaw - sentry.WorldObject.transform.eulerAngles.y);
	}

	public void SetRotationSpeed(Context instigator, float value)
	{
		sentry.RotationSpeed = value;
	}

	public float GetRotationSpeed()
	{
		return sentry.RotationSpeed;
	}

	public void SetShootDistance(Context instigator, float value)
	{
		sentry.ShootDistance = value;
		sentry.localState.ShootDistance = value;
		sentry.SyncState();
	}

	public float GetShootDistance()
	{
		return sentry.ShootDistance;
	}

	public void LookForward(Context instigator)
	{
		SetLookDirection(instigator, 0f, 0f);
	}

	public void LookBackward(Context instigator)
	{
		SetLookDirection(instigator, 0f, 180f);
	}

	public void LookUp(Context instigator)
	{
		SetLookDirection(instigator, -90f, GetLookYaw());
	}

	public void LookDown(Context instigator)
	{
		SetLookDirection(instigator, 90f, GetLookYaw());
	}

	public void LookLeft(Context instigator)
	{
		SetLookDirection(instigator, 0f, -90f);
	}

	public void LookRight(Context instigator)
	{
		SetLookDirection(instigator, 0f, 90f);
	}
}
