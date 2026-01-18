using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000287 RID: 647
	public class Projectile : EndlessBehaviour, IPoolableT, NetClock.ISimulateFrameEnvironmentSubscriber, NetClock.IRollbackSubscriber, NetClock.IPostFixedUpdateSubscriber
	{
		// Token: 0x17000298 RID: 664
		// (get) Token: 0x06000DEF RID: 3567 RVA: 0x0004AFD1 File Offset: 0x000491D1
		public float Speed
		{
			get
			{
				return this.projectileSpeedMetersPerSecond;
			}
		}

		// Token: 0x17000299 RID: 665
		// (get) Token: 0x06000DF0 RID: 3568 RVA: 0x0004AFD9 File Offset: 0x000491D9
		public uint StartFrame
		{
			get
			{
				return this.startFrame;
			}
		}

		// Token: 0x1700029A RID: 666
		// (get) Token: 0x06000DF1 RID: 3569 RVA: 0x0004AFE1 File Offset: 0x000491E1
		public global::UnityEngine.Vector3 StartPosition
		{
			get
			{
				return this.startPosition;
			}
		}

		// Token: 0x1700029B RID: 667
		// (get) Token: 0x06000DF2 RID: 3570 RVA: 0x0004AFE9 File Offset: 0x000491E9
		public global::UnityEngine.Vector3 StartAngle
		{
			get
			{
				return this.startAngle;
			}
		}

		// Token: 0x1700029C RID: 668
		// (get) Token: 0x06000DF3 RID: 3571 RVA: 0x0004AFF1 File Offset: 0x000491F1
		public SmallOfflinePhysicsObject MagazinePrefab
		{
			get
			{
				return this.magazinePrefab;
			}
		}

		// Token: 0x1700029D RID: 669
		// (get) Token: 0x06000DF4 RID: 3572 RVA: 0x0004AFF9 File Offset: 0x000491F9
		public SmallOfflinePhysicsObject CasingPrefab
		{
			get
			{
				return this.casingPrefab;
			}
		}

		// Token: 0x1700029E RID: 670
		// (get) Token: 0x06000DF5 RID: 3573 RVA: 0x0004B001 File Offset: 0x00049201
		// (set) Token: 0x06000DF6 RID: 3574 RVA: 0x0004B009 File Offset: 0x00049209
		public WorldObject WorldObject { get; private set; }

		// Token: 0x1700029F RID: 671
		// (get) Token: 0x06000DF7 RID: 3575 RVA: 0x0004B012 File Offset: 0x00049212
		private uint LifetimeFrameCount
		{
			get
			{
				return (uint)Mathf.RoundToInt(this.projectileMaxDistanceMeters / this.projectileSpeedMetersPerSecond / NetClock.FixedDeltaTime);
			}
		}

		// Token: 0x170002A0 RID: 672
		// (get) Token: 0x06000DF8 RID: 3576 RVA: 0x0004B02C File Offset: 0x0004922C
		public float DistanceTravelledPerFrame
		{
			get
			{
				return this.projectileSpeedMetersPerSecond * NetClock.FixedDeltaTime;
			}
		}

		// Token: 0x170002A1 RID: 673
		// (get) Token: 0x06000DF9 RID: 3577 RVA: 0x0004B03A File Offset: 0x0004923A
		private uint LifetimeStartFrame
		{
			get
			{
				return this.StartFrame + 1U + this.runtimeStartDelayFrames;
			}
		}

		// Token: 0x170002A2 RID: 674
		// (get) Token: 0x06000DFA RID: 3578 RVA: 0x0004B04B File Offset: 0x0004924B
		private uint LifetimeEndFrame
		{
			get
			{
				return this.LifetimeStartFrame + this.LifetimeFrameCount;
			}
		}

		// Token: 0x170002A3 RID: 675
		// (get) Token: 0x06000DFB RID: 3579 RVA: 0x0004B05A File Offset: 0x0004925A
		public List<OnHitModule> OnHitModules
		{
			get
			{
				return this.onHitModules;
			}
		}

		// Token: 0x170002A4 RID: 676
		// (get) Token: 0x06000DFC RID: 3580 RVA: 0x0004B062 File Offset: 0x00049262
		// (set) Token: 0x06000DFD RID: 3581 RVA: 0x0004B06A File Offset: 0x0004926A
		public MonoBehaviour Prefab { get; set; }

		// Token: 0x06000DFE RID: 3582 RVA: 0x0004B073 File Offset: 0x00049273
		private void OnEnable()
		{
			ProjectileManager.AddProjectile(this);
		}

		// Token: 0x06000DFF RID: 3583 RVA: 0x0004B07C File Offset: 0x0004927C
		private void OnDisable()
		{
			ProjectileManager.RemoveProjectile(this);
			NetClock.Unregister(this);
			if (this.destroyedCallback != null)
			{
				this.destroyedCallback(this);
			}
			if (NetworkBehaviourSingleton<NetClock>.Instance.IsServer && this.linkedNetworkProjectile)
			{
				this.linkedNetworkProjectile.ServerDelayedDestroy();
				this.linkedNetworkProjectile = null;
			}
			this.runtimeAppearance = null;
			this.linkedNetworkProjectile = null;
		}

		// Token: 0x06000E00 RID: 3584 RVA: 0x0004B0E4 File Offset: 0x000492E4
		private void ProjectileHit(global::UnityEngine.Vector3 position, uint frame)
		{
			this.runtimeAppearance.LifetimeEndFrame = frame;
			this.runtimeAppearance.SetState(frame, position, base.transform.eulerAngles, true, null);
			this.hitFrame = frame;
			if (this.runtimeSpawner != null)
			{
				this.runtimeSpawner.PlayOnHitEffect(position, base.transform.rotation, (frame < NetClock.CurrentFrame) ? (NetClock.CurrentFrame - frame + 1U) : 1U);
			}
		}

		// Token: 0x06000E01 RID: 3585 RVA: 0x0004B15F File Offset: 0x0004935F
		private bool DamageCheck(TeamComponent teamSource)
		{
			return this.runtimeSpawner.Team == Team.Neutral || this.runtimeSpawner.Team.IsHostileTo(teamSource.Team);
		}

		// Token: 0x06000E02 RID: 3586 RVA: 0x0004B188 File Offset: 0x00049388
		public void LocalInit(ProjectileShooter spawner, global::UnityEngine.Vector3 position, global::UnityEngine.Vector3 angle, uint frame, Action<HealthChangeResult, Context> callback, WorldObject worldObject, Action<Projectile> destroyedCallback, uint startDelayFrames = 0U)
		{
			this.runtimeSpawner = spawner;
			this.WorldObject = worldObject;
			this.onChangedHealth = callback;
			this.startPosition = position;
			this.startAngle = angle;
			this.startFrame = frame;
			base.transform.SetPositionAndRotation(position, Quaternion.Euler(angle));
			this.SpawnAppearance(position);
			NetClock.Register(this);
			this.runtimeStartDelayFrames = startDelayFrames;
			this.runtimeAppearance.RegisterSpawnInfo(frame, frame, position, angle);
			this.runtimeAppearance.LifetimeEndFrame = this.LifetimeEndFrame;
			this.destroyedCallback = destroyedCallback;
			if (NetworkManager.Singleton.IsServer)
			{
				if (ProjectileManager.PoolNetwork)
				{
					this.linkedNetworkProjectile = MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn<NetworkProjectile>(ProjectileManager.NetworkProjectilePrefab, position, base.transform.rotation, null);
				}
				else
				{
					this.linkedNetworkProjectile = global::UnityEngine.Object.Instantiate<NetworkProjectile>(ProjectileManager.NetworkProjectilePrefab);
				}
				this.linkedNetworkProjectile.ServerInit(position, angle, frame, spawner);
				this.linkedNetworkProjectile.NetworkObject.Spawn(false);
			}
		}

		// Token: 0x06000E03 RID: 3587 RVA: 0x0004B27B File Offset: 0x0004947B
		public void LocalInitAutoHit(global::UnityEngine.Vector3 position, Projectile.HitScanResult data, uint autoHitFrames)
		{
			this.SpawnAppearance(position);
			this.runtimeAppearance.SetupAutoHit(position, data, autoHitFrames);
			this.DestroySelf();
		}

		// Token: 0x06000E04 RID: 3588 RVA: 0x0004B298 File Offset: 0x00049498
		private void SpawnAppearance(global::UnityEngine.Vector3 position)
		{
			if (ProjectileManager.UsePooling)
			{
				this.runtimeAppearance = MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn<ProjectileAppearance>(this.projectileAppearancePrefab, position, base.transform.rotation, null);
			}
			else
			{
				this.runtimeAppearance = global::UnityEngine.Object.Instantiate<ProjectileAppearance>(this.projectileAppearancePrefab, position, base.transform.rotation);
			}
			this.runtimeAppearance.OwnerInstanceID = base.GetInstanceID();
			this.runtimeAppearance.Play();
		}

		// Token: 0x06000E05 RID: 3589 RVA: 0x0004B30A File Offset: 0x0004950A
		public void LinkToNetworkProjectile(NetworkProjectile networkProjectile, bool predicted)
		{
			this.startPosition = networkProjectile.StartPosition;
			this.startAngle = networkProjectile.StartAngle;
			this.startFrame = networkProjectile.StartFrame;
			this.linkedNetworkProjectile = networkProjectile;
			this.linkedNetworkProjectile.runtimeGameProjectile = this;
		}

		// Token: 0x06000E06 RID: 3590 RVA: 0x0004B344 File Offset: 0x00049544
		public void ProcessDamage(NetworkObject networkObject)
		{
			HittableComponent hittableComponent = null;
			if (networkObject != null)
			{
				hittableComponent = networkObject.GetComponentInChildren<HittableComponent>();
			}
			if (hittableComponent == null)
			{
				return;
			}
			HealthChangeResult healthChangeResult = hittableComponent.ModifyHealth(new HealthModificationArgs(-this.damage, this.WorldObject, DamageType.Normal, HealthChangeType.Damage));
			if (NetworkManager.Singleton.IsServer)
			{
				Action<HealthChangeResult, Context> action = this.onChangedHealth;
				if (action != null)
				{
					action(healthChangeResult, hittableComponent.WorldObject.Context);
				}
				NpcEntity npcEntity;
				if (hittableComponent.WorldObject.TryGetUserComponent<NpcEntity>(out npcEntity))
				{
					if (npcEntity.DamageMode != DamageMode.IgnoreDamage)
					{
						npcEntity.Components.Parameters.FlinchTrigger = true;
					}
					if (npcEntity.Components.PathFollower.IsJumping && npcEntity.PhysicsMode != PhysicsMode.IgnorePhysics)
					{
						npcEntity.Components.PathFollower.StopPath(true);
						npcEntity.Components.VelocityTracker.GetVelocity();
						uint num;
						npcEntity.WorldObject.TryGetNetworkObjectId(out num);
					}
				}
			}
		}

		// Token: 0x06000E07 RID: 3591 RVA: 0x0004B424 File Offset: 0x00049624
		public void ProcessHitModules(uint frame, WorldObject shooter, WorldObject target, global::UnityEngine.Vector3 position, global::UnityEngine.Vector3 travelDirection)
		{
			if (this.knockbackForce > 0f && target != null)
			{
				IPhysicsTaker physicsTaker = ((target != null) ? target.GetComponentInChildren(typeof(IPhysicsTaker)) : null) as IPhysicsTaker;
				if (physicsTaker != null)
				{
					ulong num = 0UL;
					if (shooter != null)
					{
						num = shooter.NetworkObject.NetworkObjectId;
					}
					physicsTaker.TakePhysicsForce(this.knockbackForce, travelDirection, frame - 1U, num, true, false, false);
				}
			}
			foreach (OnHitModule onHitModule in this.onHitModules)
			{
				onHitModule.Hit(frame, shooter, target, position, travelDirection);
			}
		}

		// Token: 0x06000E08 RID: 3592 RVA: 0x0004B4D4 File Offset: 0x000496D4
		public bool HitScan(uint frames, global::UnityEngine.Vector3 startPos, global::UnityEngine.Vector3 direction, out Projectile.HitScanResult result, ProjectileShooter fromSpawner = null, bool respectMaxHitScanDistance = false)
		{
			float num = this.DistanceTravelledPerFrame * frames;
			if (respectMaxHitScanDistance)
			{
				num = Mathf.Min(num, 8f);
			}
			int num2 = Physics.SphereCastNonAlloc(startPos + this.sphereCollider.center, this.sphereCollider.radius, direction, Projectile.rayhitCache, num, ProjectileManager.AttackableAndWallCollisionMask);
			NetworkObject networkObject = null;
			float num3 = float.MaxValue;
			global::UnityEngine.Vector3 vector = global::UnityEngine.Vector3.zero;
			float num4 = float.MaxValue;
			bool flag = false;
			global::UnityEngine.Vector3 vector2 = global::UnityEngine.Vector3.zero;
			ProjectileShooter projectileShooter = this.runtimeSpawner ?? fromSpawner;
			for (int i = 0; i < num2; i++)
			{
				HittableComponent hittableFromMap = MonoBehaviourSingleton<HittableMap>.Instance.GetHittableFromMap(Projectile.rayhitCache[i].collider);
				if (!hittableFromMap)
				{
					if (ProjectileManager.IsWallLayer(Projectile.rayhitCache[i].collider.gameObject.layer) && Projectile.rayhitCache[i].distance < num4)
					{
						num4 = Projectile.rayhitCache[i].distance;
						vector2 = ((num4 > 0f) ? Projectile.rayhitCache[i].point : (startPos + this.sphereCollider.center));
						flag = true;
					}
				}
				else
				{
					NetworkObject networkObject2 = hittableFromMap.WorldObject.NetworkObject;
					if ((!hittableFromMap || !projectileShooter || !(projectileShooter.NetworkObject == networkObject2)) && networkObject2 && Projectile.rayhitCache[i].distance < num3)
					{
						TeamComponent teamComponent;
						if (hittableFromMap.WorldObject.TryGetUserComponent<TeamComponent>(out teamComponent))
						{
							if (projectileShooter.Team.Damages(teamComponent.Team))
							{
								networkObject = networkObject2;
								num3 = Projectile.rayhitCache[i].distance;
								vector = ((num3 > 0f) ? Projectile.rayhitCache[i].point : (startPos + this.sphereCollider.center));
								flag = true;
							}
						}
						else
						{
							networkObject = networkObject2;
							num3 = Projectile.rayhitCache[i].distance;
							vector = ((num3 > 0f) ? Projectile.rayhitCache[i].point : (startPos + this.sphereCollider.center));
							flag = true;
						}
					}
				}
			}
			result = default(Projectile.HitScanResult);
			if (!flag)
			{
				return false;
			}
			bool flag2 = num3 < num4;
			if (flag2)
			{
				result.Target = networkObject;
				result.WorldObject = (flag2 ? networkObject.GetComponent<WorldObject>() : null);
				result.baseTypeTransform = result.WorldObject.BaseTypeComponent.transform;
				result.Position = result.baseTypeTransform.InverseTransformPoint(vector);
				return true;
			}
			result.Position = vector2;
			return true;
		}

		// Token: 0x06000E09 RID: 3593 RVA: 0x0004ACD9 File Offset: 0x00048ED9
		private void ServerDelayedDestroy()
		{
			base.Invoke("DestroySelf", 1f);
		}

		// Token: 0x06000E0A RID: 3594 RVA: 0x0004B7A8 File Offset: 0x000499A8
		public void DestroySelf()
		{
			if (base.gameObject != null)
			{
				if (!this.destroyed)
				{
					this.destroyed = true;
					if (ProjectileManager.UsePooling)
					{
						MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<Projectile>(this);
					}
					else
					{
						global::UnityEngine.Object.Destroy(base.gameObject);
					}
				}
				this.simulate = false;
			}
		}

		// Token: 0x06000E0B RID: 3595 RVA: 0x0004B7F8 File Offset: 0x000499F8
		public void OnSpawn()
		{
			base.transform.SetParent(null, true);
			this.simulate = true;
			this.destroyed = false;
			this.hitFrame = 0U;
		}

		// Token: 0x06000E0C RID: 3596 RVA: 0x0004B81C File Offset: 0x00049A1C
		public void SimulateFrameEnvironment(uint frame)
		{
			if (!this.simulate || frame <= this.LifetimeStartFrame)
			{
				return;
			}
			global::UnityEngine.Vector3 position = base.transform.position;
			global::UnityEngine.Vector3 forward = base.transform.forward;
			global::UnityEngine.Vector3 vector = position + forward * this.DistanceTravelledPerFrame;
			Projectile.HitScanResult hitScanResult;
			if (this.HitScan(1U, position, forward, out hitScanResult, null, false))
			{
				NetworkObject networkObject = hitScanResult.Target;
				this.ProjectileHit(hitScanResult.WorldPosition, frame);
				if (networkObject)
				{
					this.ProcessDamage(networkObject);
				}
				this.ProcessHitModules(frame, this.runtimeSpawner.ShootingWorldObject, hitScanResult.WorldObject, hitScanResult.WorldPosition, forward);
				if (NetworkManager.Singleton.IsServer)
				{
					this.ServerDelayedDestroy();
				}
				this.simulate = false;
			}
			base.transform.position = vector;
		}

		// Token: 0x06000E0D RID: 3597 RVA: 0x0004B8E8 File Offset: 0x00049AE8
		public void Rollback(uint frame)
		{
			if (frame > this.LifetimeStartFrame && frame <= this.LifetimeEndFrame)
			{
				base.transform.position = this.startPosition + base.transform.forward * ((frame - this.LifetimeStartFrame) * this.DistanceTravelledPerFrame);
				return;
			}
			if (frame <= this.LifetimeStartFrame)
			{
				base.transform.position = this.startPosition;
			}
		}

		// Token: 0x06000E0E RID: 3598 RVA: 0x0004B95C File Offset: 0x00049B5C
		public void PostFixedUpdate(uint frame)
		{
			if (!NetworkManager.Singleton.IsServer && this.linkedNetworkProjectile == null && frame > this.startFrame + 6U)
			{
				if (this.runtimeSpawner)
				{
					this.runtimeSpawner.ProjectileMispredicted(this);
				}
				this.DestroySelf();
				return;
			}
			if (this.runtimeAppearance != null && this.runtimeAppearance.OwnerInstanceID == base.GetInstanceID() && frame != this.hitFrame)
			{
				this.runtimeAppearance.SetState(frame, base.transform.position, base.transform.eulerAngles, this.simulate && frame >= this.LifetimeStartFrame, null);
			}
			if (this.simulate && frame > this.LifetimeEndFrame)
			{
				if (NetworkBehaviourSingleton<NetClock>.Instance.IsServer)
				{
					this.DestroySelf();
					return;
				}
				this.simulate = false;
			}
		}

		// Token: 0x04000CB8 RID: 3256
		private const uint PREDICTION_EXPIRE_FRAMES = 6U;

		// Token: 0x04000CB9 RID: 3257
		private const uint SIMULATION_DELAY_FRAMES = 1U;

		// Token: 0x04000CBA RID: 3258
		private const float MAX_HIT_SCAN_DISTANCE = 8f;

		// Token: 0x04000CBB RID: 3259
		[SerializeField]
		private ProjectileAppearance projectileAppearancePrefab;

		// Token: 0x04000CBC RID: 3260
		[SerializeField]
		private SmallOfflinePhysicsObject magazinePrefab;

		// Token: 0x04000CBD RID: 3261
		[SerializeField]
		private SmallOfflinePhysicsObject casingPrefab;

		// Token: 0x04000CBE RID: 3262
		[Header("Collision")]
		[SerializeField]
		private SphereCollider sphereCollider;

		// Token: 0x04000CBF RID: 3263
		[Header("Projectile")]
		[SerializeField]
		[Min(0f)]
		private int damage = 1;

		// Token: 0x04000CC0 RID: 3264
		[SerializeField]
		[Min(1f)]
		private float projectileSpeedMetersPerSecond = 30f;

		// Token: 0x04000CC1 RID: 3265
		[SerializeField]
		[Min(1f)]
		private float projectileMaxDistanceMeters = 60f;

		// Token: 0x04000CC2 RID: 3266
		[Header("Knockback")]
		[SerializeField]
		private float knockbackForce = 0.5f;

		// Token: 0x04000CC3 RID: 3267
		[Header("OnHit")]
		[SerializeField]
		private List<OnHitModule> onHitModules;

		// Token: 0x04000CC4 RID: 3268
		[NonSerialized]
		public bool pooled;

		// Token: 0x04000CC5 RID: 3269
		private uint startFrame;

		// Token: 0x04000CC6 RID: 3270
		private global::UnityEngine.Vector3 startPosition;

		// Token: 0x04000CC7 RID: 3271
		private global::UnityEngine.Vector3 startAngle;

		// Token: 0x04000CC8 RID: 3272
		private uint expireFrame;

		// Token: 0x04000CC9 RID: 3273
		private uint deleteFrame;

		// Token: 0x04000CCA RID: 3274
		private NetworkProjectile linkedNetworkProjectile;

		// Token: 0x04000CCB RID: 3275
		private ProjectileAppearance runtimeAppearance;

		// Token: 0x04000CCC RID: 3276
		private ProjectileShooter runtimeSpawner;

		// Token: 0x04000CCD RID: 3277
		private Action<HealthChangeResult, Context> onChangedHealth;

		// Token: 0x04000CD0 RID: 3280
		private static RaycastHit[] rayhitCache = new RaycastHit[12];

		// Token: 0x04000CD1 RID: 3281
		private uint hitFrame;

		// Token: 0x04000CD2 RID: 3282
		private bool simulate = true;

		// Token: 0x04000CD3 RID: 3283
		private uint runtimeStartDelayFrames;

		// Token: 0x04000CD4 RID: 3284
		private Action<Projectile> destroyedCallback;

		// Token: 0x04000CD5 RID: 3285
		private bool destroyed;

		// Token: 0x02000288 RID: 648
		public struct HitScanResult : INetworkSerializable
		{
			// Token: 0x170002A5 RID: 677
			// (get) Token: 0x06000E11 RID: 3601 RVA: 0x0004BA8A File Offset: 0x00049C8A
			public global::UnityEngine.Vector3 WorldPosition
			{
				get
				{
					this.Target;
					if (this.baseTypeTransform)
					{
						return this.baseTypeTransform.position + this.Position;
					}
					return this.Position;
				}
			}

			// Token: 0x06000E12 RID: 3602 RVA: 0x0004BAC4 File Offset: 0x00049CC4
			public void NetworkSerialize<AutoHitData>(BufferSerializer<AutoHitData> serializer) where AutoHitData : IReaderWriter
			{
				serializer.SerializeValue<NetworkObjectReference>(ref this.Target, default(FastBufferWriter.ForNetworkSerializable));
				serializer.SerializeValue(ref this.Position);
				if (serializer.IsReader)
				{
					NetworkObject networkObject = this.Target;
					if (networkObject)
					{
						this.WorldObject = networkObject.GetComponent<WorldObject>();
						this.baseTypeTransform = this.WorldObject.BaseTypeComponent.transform;
					}
				}
			}

			// Token: 0x04000CD6 RID: 3286
			public NetworkObjectReference Target;

			// Token: 0x04000CD7 RID: 3287
			public global::UnityEngine.Vector3 Position;

			// Token: 0x04000CD8 RID: 3288
			public Transform baseTypeTransform;

			// Token: 0x04000CD9 RID: 3289
			public WorldObject WorldObject;
		}
	}
}
