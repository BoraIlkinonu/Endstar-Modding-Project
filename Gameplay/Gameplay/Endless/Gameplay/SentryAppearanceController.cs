using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Props.ReferenceComponents;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000339 RID: 825
	public class SentryAppearanceController : MonoBehaviour
	{
		// Token: 0x060013D4 RID: 5076 RVA: 0x0005FDC0 File Offset: 0x0005DFC0
		public void ComponentInitialize(ReferenceBase referenceBase)
		{
			this.sentryReferences = (SentryReferences)referenceBase;
			if (this.sentryReferences.ShootLaser != null)
			{
				this.shootLaser = this.sentryReferences.ShootLaser.gameObject.AddComponent<SentryTurretShootLaserAppearance>();
				this.shootLaser.transform.parent = this.sentryReferences.LaserPoint;
				this.shootLaser.transform.localPosition = Vector3.zero;
				this.shootLaser.Init(this.sentryReferences.ShootLaser);
			}
			if (this.sentryReferences.TrackLaser != null)
			{
				this.trackLaser = this.sentryReferences.TrackLaser.gameObject.AddComponent<SentryTurretTrackLaserAppearance>();
				this.trackLaser.transform.parent = this.sentryReferences.LaserPoint;
				this.trackLaser.transform.localPosition = Vector3.zero;
				this.trackLaser.Init(this.sentryReferences.TrackLaser, this.scanHitLayerMask);
			}
			for (int i = 0; i < this.sentryReferences.ShootPointTransformList.Length; i++)
			{
				if (this.sentryReferences.ShootFlashEffect != null)
				{
					this.sentryReferences.ShootFlashEffect.RuntimeParticleSystem.Stop();
					ParticleSystem particleSystem = global::UnityEngine.Object.Instantiate<ParticleSystem>(this.sentryReferences.ShootFlashEffect.RuntimeParticleSystem, this.sentryReferences.ShootPointTransformList[i]);
					this.shootFlashParticles.Add(particleSystem);
				}
				if (this.sentryReferences.ShootHitEffect != null)
				{
					this.sentryReferences.ShootHitEffect.RuntimeParticleSystem.Stop();
					ParticleSystem particleSystem2 = global::UnityEngine.Object.Instantiate<ParticleSystem>(this.sentryReferences.ShootHitEffect.RuntimeParticleSystem, this.sentryReferences.ShootPointTransformList[i]);
					this.shootHitParticles.Add(particleSystem2);
				}
			}
		}

		// Token: 0x060013D5 RID: 5077 RVA: 0x0005FF94 File Offset: 0x0005E194
		public void SetState(Sentry.SentryState state)
		{
			this.appearanceStateRingBuffer.GetAtPosition(state.NetFrame).NetFrame = state.NetFrame;
			this.appearanceStateRingBuffer.GetAtPosition(state.NetFrame).CurrentPitch = state.CurrentPitch;
			this.appearanceStateRingBuffer.GetAtPosition(state.NetFrame).CurrentYaw = state.CurrentYaw;
			this.appearanceStateRingBuffer.GetAtPosition(state.NetFrame).TrackingLaserEnabled = state.TrackingLaserEnabled;
			this.appearanceStateRingBuffer.GetAtPosition(state.NetFrame).ShootDistance = state.ShootDistance;
			this.appearanceStateRingBuffer.FrameUpdated(state.NetFrame);
		}

		// Token: 0x060013D6 RID: 5078 RVA: 0x00060048 File Offset: 0x0005E248
		public void Update()
		{
			this.appearanceStateRingBuffer.ActiveInterpolationTime = (NetworkManager.Singleton.IsServer ? NetClock.ServerAppearanceTime : (NetClock.ClientExtrapolatedAppearanceTime + (double)NetClock.FixedDeltaTime));
			this.appearanceStateRingBuffer.ActiveInterpolatedState.CurrentPitch = Mathf.LerpAngle(this.appearanceStateRingBuffer.PastInterpolationState.CurrentPitch, this.appearanceStateRingBuffer.NextInterpolationState.CurrentPitch, this.appearanceStateRingBuffer.ActiveStateLerpTime);
			this.appearanceStateRingBuffer.ActiveInterpolatedState.CurrentYaw = Mathf.LerpAngle(this.appearanceStateRingBuffer.PastInterpolationState.CurrentYaw, this.appearanceStateRingBuffer.NextInterpolationState.CurrentYaw, this.appearanceStateRingBuffer.ActiveStateLerpTime);
			this.appearanceStateRingBuffer.ActiveInterpolatedState.TrackingLaserEnabled = this.appearanceStateRingBuffer.NextInterpolationState.TrackingLaserEnabled;
			this.appearanceStateRingBuffer.ActiveInterpolatedState.ShootDistance = this.appearanceStateRingBuffer.NextInterpolationState.ShootDistance;
			if (this.trackLaser)
			{
				this.trackLaser.SetState(this.appearanceStateRingBuffer.ActiveInterpolatedState.TrackingLaserEnabled, this.appearanceStateRingBuffer.ActiveInterpolatedState.ShootDistance);
			}
			Vector3 eulerAngles = this.sentryReferences.SwivelTransform.eulerAngles;
			this.sentryReferences.SwivelTransform.rotation = Quaternion.Euler(new Vector3(Mathf.LerpAngle(this.appearanceStateRingBuffer.ActiveInterpolatedState.CurrentPitch, eulerAngles.x, 0.5f), Mathf.LerpAngle(this.appearanceStateRingBuffer.ActiveInterpolatedState.CurrentYaw, eulerAngles.y, 0.5f), 0f));
		}

		// Token: 0x060013D7 RID: 5079 RVA: 0x000601E8 File Offset: 0x0005E3E8
		public void PlayShootVisuals(Vector3 lookRotation, float distance, NetworkObjectReference trackingTarget)
		{
			NetworkObject networkObject;
			if (trackingTarget.TryGet(out networkObject, null) && networkObject.IsOwner)
			{
				base.StartCoroutine(this.DelayedPlayShootVisuals(this.sentryReferences.SwivelTransform.forward, distance, 0f, false));
				return;
			}
			base.StartCoroutine(this.DelayedPlayShootVisuals(lookRotation, distance, (float)(NetClock.GetFrameTime(NetClock.CurrentFrame) - (NetworkManager.Singleton.IsServer ? NetClock.ServerAppearanceTime : NetClock.ClientInterpolatedAppearanceTime)), false));
		}

		// Token: 0x060013D8 RID: 5080 RVA: 0x00060262 File Offset: 0x0005E462
		private IEnumerator DelayedPlayShootVisuals(Vector3 lookRotation, float distance, float delay, bool hitTrackingTarget = false)
		{
			Vector3 vector = this.sentryReferences.LaserPoint.position;
			bool hitSomething = false;
			RaycastHit hit;
			float hitDistance;
			if (Physics.Raycast(this.sentryReferences.LaserPoint.position, lookRotation, out hit, distance, this.scanHitLayerMask))
			{
				vector = hit.point;
				hitSomething = true;
				hitDistance = hit.distance;
			}
			else
			{
				hitDistance = distance;
				vector = lookRotation * hitDistance + this.sentryReferences.LaserPoint.position;
			}
			yield return new WaitForSeconds(delay);
			vector = lookRotation * hitDistance + this.sentryReferences.LaserPoint.position;
			if (hitSomething)
			{
				vector += global::UnityEngine.Random.insideUnitSphere * 0.1f;
			}
			this.currentShootIndex = (int)Mathf.Repeat((float)(this.currentShootIndex + 1), (float)this.sentryReferences.ShootPointTransformList.Length);
			if (this.shootLaser)
			{
				this.shootLaser.Play(vector, this.sentryReferences.ShootLaserFlashDuration);
			}
			if (this.sentryReferences.ShootFlashEffect)
			{
				this.shootFlashParticles[this.currentShootIndex].Play();
			}
			if (hitSomething && this.sentryReferences.ShootHitEffect)
			{
				this.shootHitParticles[this.currentShootIndex].transform.position = vector;
				this.shootHitParticles[this.currentShootIndex].transform.eulerAngles = hit.normal;
				this.shootHitParticles[this.currentShootIndex].Play();
			}
			yield break;
		}

		// Token: 0x04001099 RID: 4249
		private readonly InterpolationRingBuffer<Sentry.SentryState> appearanceStateRingBuffer = new InterpolationRingBuffer<Sentry.SentryState>(15);

		// Token: 0x0400109A RID: 4250
		[SerializeField]
		[HideInInspector]
		private SentryReferences sentryReferences;

		// Token: 0x0400109B RID: 4251
		[SerializeField]
		[HideInInspector]
		private List<ParticleSystem> shootFlashParticles = new List<ParticleSystem>();

		// Token: 0x0400109C RID: 4252
		[SerializeField]
		[HideInInspector]
		private List<ParticleSystem> shootHitParticles = new List<ParticleSystem>();

		// Token: 0x0400109D RID: 4253
		[SerializeField]
		private SentryTurretShootLaserAppearance shootLaser;

		// Token: 0x0400109E RID: 4254
		[SerializeField]
		private SentryTurretTrackLaserAppearance trackLaser;

		// Token: 0x0400109F RID: 4255
		[SerializeField]
		private LayerMask scanHitLayerMask;

		// Token: 0x040010A0 RID: 4256
		private int currentShootIndex;
	}
}
