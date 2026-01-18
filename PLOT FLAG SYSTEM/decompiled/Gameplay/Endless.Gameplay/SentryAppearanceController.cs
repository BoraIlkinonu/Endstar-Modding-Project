using System.Collections;
using System.Collections.Generic;
using Endless.Props.ReferenceComponents;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class SentryAppearanceController : MonoBehaviour
{
	private readonly InterpolationRingBuffer<Sentry.SentryState> appearanceStateRingBuffer = new InterpolationRingBuffer<Sentry.SentryState>(15);

	[SerializeField]
	[HideInInspector]
	private SentryReferences sentryReferences;

	[SerializeField]
	[HideInInspector]
	private List<ParticleSystem> shootFlashParticles = new List<ParticleSystem>();

	[SerializeField]
	[HideInInspector]
	private List<ParticleSystem> shootHitParticles = new List<ParticleSystem>();

	[SerializeField]
	private SentryTurretShootLaserAppearance shootLaser;

	[SerializeField]
	private SentryTurretTrackLaserAppearance trackLaser;

	[SerializeField]
	private LayerMask scanHitLayerMask;

	private int currentShootIndex;

	public void ComponentInitialize(ReferenceBase referenceBase)
	{
		sentryReferences = (SentryReferences)referenceBase;
		if (sentryReferences.ShootLaser != null)
		{
			shootLaser = sentryReferences.ShootLaser.gameObject.AddComponent<SentryTurretShootLaserAppearance>();
			shootLaser.transform.parent = sentryReferences.LaserPoint;
			shootLaser.transform.localPosition = Vector3.zero;
			shootLaser.Init(sentryReferences.ShootLaser);
		}
		if (sentryReferences.TrackLaser != null)
		{
			trackLaser = sentryReferences.TrackLaser.gameObject.AddComponent<SentryTurretTrackLaserAppearance>();
			trackLaser.transform.parent = sentryReferences.LaserPoint;
			trackLaser.transform.localPosition = Vector3.zero;
			trackLaser.Init(sentryReferences.TrackLaser, scanHitLayerMask);
		}
		for (int i = 0; i < sentryReferences.ShootPointTransformList.Length; i++)
		{
			if (sentryReferences.ShootFlashEffect != null)
			{
				sentryReferences.ShootFlashEffect.RuntimeParticleSystem.Stop();
				ParticleSystem item = Object.Instantiate(sentryReferences.ShootFlashEffect.RuntimeParticleSystem, sentryReferences.ShootPointTransformList[i]);
				shootFlashParticles.Add(item);
			}
			if (sentryReferences.ShootHitEffect != null)
			{
				sentryReferences.ShootHitEffect.RuntimeParticleSystem.Stop();
				ParticleSystem item2 = Object.Instantiate(sentryReferences.ShootHitEffect.RuntimeParticleSystem, sentryReferences.ShootPointTransformList[i]);
				shootHitParticles.Add(item2);
			}
		}
	}

	public void SetState(Sentry.SentryState state)
	{
		appearanceStateRingBuffer.GetAtPosition(state.NetFrame).NetFrame = state.NetFrame;
		appearanceStateRingBuffer.GetAtPosition(state.NetFrame).CurrentPitch = state.CurrentPitch;
		appearanceStateRingBuffer.GetAtPosition(state.NetFrame).CurrentYaw = state.CurrentYaw;
		appearanceStateRingBuffer.GetAtPosition(state.NetFrame).TrackingLaserEnabled = state.TrackingLaserEnabled;
		appearanceStateRingBuffer.GetAtPosition(state.NetFrame).ShootDistance = state.ShootDistance;
		appearanceStateRingBuffer.FrameUpdated(state.NetFrame);
	}

	public void Update()
	{
		appearanceStateRingBuffer.ActiveInterpolationTime = (NetworkManager.Singleton.IsServer ? NetClock.ServerAppearanceTime : (NetClock.ClientExtrapolatedAppearanceTime + (double)NetClock.FixedDeltaTime));
		appearanceStateRingBuffer.ActiveInterpolatedState.CurrentPitch = Mathf.LerpAngle(appearanceStateRingBuffer.PastInterpolationState.CurrentPitch, appearanceStateRingBuffer.NextInterpolationState.CurrentPitch, appearanceStateRingBuffer.ActiveStateLerpTime);
		appearanceStateRingBuffer.ActiveInterpolatedState.CurrentYaw = Mathf.LerpAngle(appearanceStateRingBuffer.PastInterpolationState.CurrentYaw, appearanceStateRingBuffer.NextInterpolationState.CurrentYaw, appearanceStateRingBuffer.ActiveStateLerpTime);
		appearanceStateRingBuffer.ActiveInterpolatedState.TrackingLaserEnabled = appearanceStateRingBuffer.NextInterpolationState.TrackingLaserEnabled;
		appearanceStateRingBuffer.ActiveInterpolatedState.ShootDistance = appearanceStateRingBuffer.NextInterpolationState.ShootDistance;
		if ((bool)trackLaser)
		{
			trackLaser.SetState(appearanceStateRingBuffer.ActiveInterpolatedState.TrackingLaserEnabled, appearanceStateRingBuffer.ActiveInterpolatedState.ShootDistance);
		}
		Vector3 eulerAngles = sentryReferences.SwivelTransform.eulerAngles;
		sentryReferences.SwivelTransform.rotation = Quaternion.Euler(new Vector3(Mathf.LerpAngle(appearanceStateRingBuffer.ActiveInterpolatedState.CurrentPitch, eulerAngles.x, 0.5f), Mathf.LerpAngle(appearanceStateRingBuffer.ActiveInterpolatedState.CurrentYaw, eulerAngles.y, 0.5f), 0f));
	}

	public void PlayShootVisuals(Vector3 lookRotation, float distance, NetworkObjectReference trackingTarget)
	{
		if (trackingTarget.TryGet(out var networkObject) && networkObject.IsOwner)
		{
			StartCoroutine(DelayedPlayShootVisuals(sentryReferences.SwivelTransform.forward, distance, 0f));
		}
		else
		{
			StartCoroutine(DelayedPlayShootVisuals(lookRotation, distance, (float)(NetClock.GetFrameTime(NetClock.CurrentFrame) - (NetworkManager.Singleton.IsServer ? NetClock.ServerAppearanceTime : NetClock.ClientInterpolatedAppearanceTime))));
		}
	}

	private IEnumerator DelayedPlayShootVisuals(Vector3 lookRotation, float distance, float delay, bool hitTrackingTarget = false)
	{
		_ = sentryReferences.LaserPoint.position;
		bool hitSomething = false;
		float hitDistance;
		if (Physics.Raycast(sentryReferences.LaserPoint.position, lookRotation, out var hit, distance, scanHitLayerMask))
		{
			_ = hit.point;
			hitSomething = true;
			hitDistance = hit.distance;
		}
		else
		{
			hitDistance = distance;
			_ = lookRotation * hitDistance + sentryReferences.LaserPoint.position;
		}
		yield return new WaitForSeconds(delay);
		Vector3 vector = lookRotation * hitDistance + sentryReferences.LaserPoint.position;
		if (hitSomething)
		{
			vector += Random.insideUnitSphere * 0.1f;
		}
		currentShootIndex = (int)Mathf.Repeat(currentShootIndex + 1, sentryReferences.ShootPointTransformList.Length);
		if ((bool)shootLaser)
		{
			shootLaser.Play(vector, sentryReferences.ShootLaserFlashDuration);
		}
		if ((bool)sentryReferences.ShootFlashEffect)
		{
			shootFlashParticles[currentShootIndex].Play();
		}
		if (hitSomething && (bool)sentryReferences.ShootHitEffect)
		{
			shootHitParticles[currentShootIndex].transform.position = vector;
			shootHitParticles[currentShootIndex].transform.eulerAngles = hit.normal;
			shootHitParticles[currentShootIndex].Play();
		}
	}
}
