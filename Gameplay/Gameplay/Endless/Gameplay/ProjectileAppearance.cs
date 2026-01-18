using System;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000289 RID: 649
	public class ProjectileAppearance : MonoBehaviour, IPoolableT
	{
		// Token: 0x170002A6 RID: 678
		// (get) Token: 0x06000E13 RID: 3603 RVA: 0x0004BB33 File Offset: 0x00049D33
		// (set) Token: 0x06000E14 RID: 3604 RVA: 0x0004BB3B File Offset: 0x00049D3B
		public MonoBehaviour Prefab { get; set; }

		// Token: 0x170002A7 RID: 679
		// (get) Token: 0x06000E15 RID: 3605 RVA: 0x0004BB44 File Offset: 0x00049D44
		// (set) Token: 0x06000E16 RID: 3606 RVA: 0x0004BB4C File Offset: 0x00049D4C
		public uint LifetimeEndFrame { get; set; } = uint.MaxValue;

		// Token: 0x170002A8 RID: 680
		// (get) Token: 0x06000E17 RID: 3607 RVA: 0x0004BB55 File Offset: 0x00049D55
		// (set) Token: 0x06000E18 RID: 3608 RVA: 0x0004BB5D File Offset: 0x00049D5D
		public int OwnerInstanceID { get; set; }

		// Token: 0x06000E19 RID: 3609 RVA: 0x0004BB66 File Offset: 0x00049D66
		private void Awake()
		{
			this.DisableVisuals();
		}

		// Token: 0x06000E1A RID: 3610 RVA: 0x0004BB6E File Offset: 0x00049D6E
		private void OnEnable()
		{
			ProjectileManager.AddProjectileAppearance(this);
		}

		// Token: 0x06000E1B RID: 3611 RVA: 0x0004BB76 File Offset: 0x00049D76
		private void OnDisable()
		{
			ProjectileManager.RemoveProjectileAppearance(this);
		}

		// Token: 0x06000E1C RID: 3612 RVA: 0x0004BB80 File Offset: 0x00049D80
		public void SetState(uint frame, Vector3 position, Vector3 eulerAngles, bool visible, uint? catchUpFrames = null)
		{
			uint num = ((catchUpFrames != null) ? (frame + catchUpFrames.Value) : ((this.localSpawnFrame > frame) ? this.localSpawnFrame : frame));
			if (catchUpFrames == null)
			{
				catchUpFrames = new uint?(6U);
			}
			position = Vector3.Lerp(this.spawnPosition, position, (num - this.localSpawnFrame) / catchUpFrames.Value);
			ref ProjectileAppearance.ProjectileState atPosition = ref this.stateRingBuffer.GetAtPosition(frame);
			atPosition.position = position;
			atPosition.eulerAngles = eulerAngles;
			atPosition.visible = visible;
			atPosition.NetFrame = ((frame < this.localSpawnFrame) ? frame : num);
			this.stateRingBuffer.FrameUpdated(frame);
			this.lastStateFrame = ((NetClock.CurrentFrame > num) ? NetClock.CurrentFrame : num);
		}

		// Token: 0x06000E1D RID: 3613 RVA: 0x0004BC40 File Offset: 0x00049E40
		public void SetupAutoHit(Vector3 startPosition, Projectile.HitScanResult autohitData, uint autoHitFrames)
		{
			Vector3 eulerAngles = Quaternion.LookRotation(autohitData.WorldPosition - startPosition, Vector3.up).eulerAngles;
			uint num = NetClock.CurrentSimulationFrame - 1U;
			this.RegisterSpawnInfo(num, num, startPosition, eulerAngles);
			this.SetState(num + 1U, autohitData.WorldPosition, eulerAngles, true, new uint?(autoHitFrames));
			this.SetState(num + 2U, autohitData.WorldPosition, eulerAngles, false, new uint?(autoHitFrames));
			this.LifetimeEndFrame = num + autoHitFrames;
		}

		// Token: 0x06000E1E RID: 3614 RVA: 0x0004BCB8 File Offset: 0x00049EB8
		public void RegisterSpawnInfo(uint serverSpawnFrame, uint localSpawnFrame, Vector3 spawnPosition, Vector3 spawnAngle)
		{
			this.serverSpawnFrame = serverSpawnFrame;
			this.localSpawnFrame = localSpawnFrame;
			this.spawnPosition = spawnPosition;
			this.SetState(localSpawnFrame - 1U, spawnPosition, spawnAngle, false, null);
			this.SetState(localSpawnFrame, spawnPosition, spawnAngle, true, null);
			this.stateRingBuffer.InitPastAndNext(localSpawnFrame - 1U);
		}

		// Token: 0x06000E1F RID: 3615 RVA: 0x0004BD14 File Offset: 0x00049F14
		private void Update()
		{
			if (this.stateRingBuffer.PastInterpolationState.NetFrame > this.LifetimeEndFrame || NetworkManager.Singleton == null || (NetClock.CurrentFrame > this.lastStateFrame && NetClock.CurrentFrame - this.lastStateFrame > 3U))
			{
				this.DestroySelf();
				return;
			}
			this.stateRingBuffer.ActiveInterpolationTime = (NetworkManager.Singleton.IsServer ? NetClock.ServerAppearanceTime : NetClock.ClientExtrapolatedAppearanceTime);
			this.stateRingBuffer.ActiveInterpolatedState.position = Vector3.Lerp(this.stateRingBuffer.PastInterpolationState.position, this.stateRingBuffer.NextInterpolationState.position, this.stateRingBuffer.ActiveStateLerpTime);
			this.stateRingBuffer.ActiveInterpolatedState.eulerAngles = Vector3.Lerp(this.stateRingBuffer.PastInterpolationState.eulerAngles, this.stateRingBuffer.NextInterpolationState.eulerAngles, this.stateRingBuffer.ActiveStateLerpTime);
			this.stateRingBuffer.ActiveInterpolatedState.visible = this.stateRingBuffer.NextInterpolationState.visible;
			base.transform.position = this.stateRingBuffer.ActiveInterpolatedState.position;
			base.transform.eulerAngles = this.stateRingBuffer.ActiveInterpolatedState.eulerAngles;
			if (this.stateRingBuffer.ActiveInterpolatedState.visible)
			{
				this.EnableVisuals();
				return;
			}
			this.DisableVisuals();
		}

		// Token: 0x06000E20 RID: 3616 RVA: 0x0004BE84 File Offset: 0x0004A084
		private void EnableVisuals()
		{
			if (!this.visualsActive)
			{
				this.visualsActive = true;
				this.visualsObject.SetActive(true);
			}
			if (this.particles != null)
			{
				this.particles.Play();
			}
		}

		// Token: 0x06000E21 RID: 3617 RVA: 0x0004BEBA File Offset: 0x0004A0BA
		private void DisableVisuals()
		{
			if (this.visualsActive)
			{
				this.visualsActive = false;
				this.visualsObject.SetActive(false);
			}
		}

		// Token: 0x06000E22 RID: 3618 RVA: 0x0004BED7 File Offset: 0x0004A0D7
		public void DestroySelf()
		{
			if (base.gameObject != null && !this.destroyed)
			{
				this.destroyed = true;
				if (ProjectileManager.UsePooling)
				{
					MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<ProjectileAppearance>(this);
					return;
				}
				global::UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		// Token: 0x06000E23 RID: 3619 RVA: 0x0004BF14 File Offset: 0x0004A114
		public void OnSpawn()
		{
			base.transform.SetParent(null, true);
			this.LifetimeEndFrame = uint.MaxValue;
			this.lastStateFrame = 0U;
			this.destroyed = false;
			this.stateRingBuffer.Clear();
		}

		// Token: 0x06000E24 RID: 3620 RVA: 0x0004BF43 File Offset: 0x0004A143
		public void OnDespawn()
		{
			this.DisableVisuals();
			if (this.particles != null)
			{
				this.particles.Stop();
			}
		}

		// Token: 0x06000E25 RID: 3621 RVA: 0x0004BF64 File Offset: 0x0004A164
		public void Play()
		{
			if (this.particles != null)
			{
				this.particles.Play();
			}
		}

		// Token: 0x04000CDA RID: 3290
		private const uint CATCH_UP_FRAME_COUNT = 6U;

		// Token: 0x04000CDB RID: 3291
		[SerializeField]
		private GameObject visualsObject;

		// Token: 0x04000CDC RID: 3292
		[SerializeField]
		private ParticleSystem particles;

		// Token: 0x04000CDD RID: 3293
		private uint serverSpawnFrame;

		// Token: 0x04000CDE RID: 3294
		private uint localSpawnFrame;

		// Token: 0x04000CDF RID: 3295
		private Vector3 spawnPosition;

		// Token: 0x04000CE0 RID: 3296
		private InterpolationRingBuffer<ProjectileAppearance.ProjectileState> stateRingBuffer = new InterpolationRingBuffer<ProjectileAppearance.ProjectileState>(15);

		// Token: 0x04000CE4 RID: 3300
		private bool visualsActive = true;

		// Token: 0x04000CE5 RID: 3301
		private uint lastStateFrame;

		// Token: 0x04000CE6 RID: 3302
		private bool destroyed;

		// Token: 0x0200028A RID: 650
		public struct ProjectileState : IFrameInfo
		{
			// Token: 0x170002A9 RID: 681
			// (get) Token: 0x06000E27 RID: 3623 RVA: 0x0004BFA2 File Offset: 0x0004A1A2
			// (set) Token: 0x06000E28 RID: 3624 RVA: 0x0004BFAA File Offset: 0x0004A1AA
			public uint NetFrame { readonly get; set; }

			// Token: 0x06000E29 RID: 3625 RVA: 0x00002DB0 File Offset: 0x00000FB0
			public void Clear()
			{
			}

			// Token: 0x06000E2A RID: 3626 RVA: 0x00002DB0 File Offset: 0x00000FB0
			public void Initialize()
			{
			}

			// Token: 0x04000CE8 RID: 3304
			public Vector3 position;

			// Token: 0x04000CE9 RID: 3305
			public Vector3 eulerAngles;

			// Token: 0x04000CEA RID: 3306
			public bool visible;
		}
	}
}
