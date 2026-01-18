using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;
using Endless.Props;
using Endless.Props.Assets;
using Endless.Props.ReferenceComponents;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200028C RID: 652
	public class ProjectileShooter : NetworkBehaviour, IGameEndSubscriber
	{
		// Token: 0x170002B1 RID: 689
		// (get) Token: 0x06000E40 RID: 3648 RVA: 0x0004C247 File Offset: 0x0004A447
		// (set) Token: 0x06000E41 RID: 3649 RVA: 0x0004C24F File Offset: 0x0004A44F
		public Projectile ProjectilePrefab { get; private set; }

		// Token: 0x170002B2 RID: 690
		// (get) Token: 0x06000E42 RID: 3650 RVA: 0x0004C258 File Offset: 0x0004A458
		// (set) Token: 0x06000E43 RID: 3651 RVA: 0x0004C260 File Offset: 0x0004A460
		public Transform FirePoint { get; private set; }

		// Token: 0x170002B3 RID: 691
		// (get) Token: 0x06000E44 RID: 3652 RVA: 0x0004C269 File Offset: 0x0004A469
		// (set) Token: 0x06000E45 RID: 3653 RVA: 0x0004C271 File Offset: 0x0004A471
		public ParticleSystem MuzzleFlashEffect { get; private set; }

		// Token: 0x170002B4 RID: 692
		// (get) Token: 0x06000E46 RID: 3654 RVA: 0x0004C27A File Offset: 0x0004A47A
		// (set) Token: 0x06000E47 RID: 3655 RVA: 0x0004C282 File Offset: 0x0004A482
		public ParticleSystem EjectionEffect { get; private set; }

		// Token: 0x170002B5 RID: 693
		// (get) Token: 0x06000E48 RID: 3656 RVA: 0x0004C28B File Offset: 0x0004A48B
		// (set) Token: 0x06000E49 RID: 3657 RVA: 0x0004C293 File Offset: 0x0004A493
		public ParticleSystem HitEffect { get; private set; }

		// Token: 0x170002B6 RID: 694
		// (get) Token: 0x06000E4A RID: 3658 RVA: 0x0004C29C File Offset: 0x0004A49C
		// (set) Token: 0x06000E4B RID: 3659 RVA: 0x0004C2A4 File Offset: 0x0004A4A4
		public HitEffect HitEffectPrefab { get; private set; }

		// Token: 0x170002B7 RID: 695
		// (get) Token: 0x06000E4C RID: 3660 RVA: 0x0004C2AD File Offset: 0x0004A4AD
		// (set) Token: 0x06000E4D RID: 3661 RVA: 0x0004C2B5 File Offset: 0x0004A4B5
		public Transform EjectionEffectPoint { get; private set; }

		// Token: 0x170002B8 RID: 696
		// (get) Token: 0x06000E4E RID: 3662 RVA: 0x0004C2BE File Offset: 0x0004A4BE
		// (set) Token: 0x06000E4F RID: 3663 RVA: 0x0004C2C6 File Offset: 0x0004A4C6
		public Transform Magazine { get; private set; }

		// Token: 0x170002B9 RID: 697
		// (get) Token: 0x06000E50 RID: 3664 RVA: 0x0004C2CF File Offset: 0x0004A4CF
		// (set) Token: 0x06000E51 RID: 3665 RVA: 0x0004C2D7 File Offset: 0x0004A4D7
		public Transform MagazineEjectionPoint { get; private set; }

		// Token: 0x170002BA RID: 698
		// (get) Token: 0x06000E52 RID: 3666 RVA: 0x0004C2E0 File Offset: 0x0004A4E0
		public WorldObject ShootingWorldObject
		{
			get
			{
				if (!this.associatedItem)
				{
					return this._worldObject;
				}
				return this.associatedItem.Carrier.WorldObject;
			}
		}

		// Token: 0x170002BB RID: 699
		// (get) Token: 0x06000E53 RID: 3667 RVA: 0x0004C306 File Offset: 0x0004A506
		public WorldObject WorldObject
		{
			get
			{
				if (this._worldObject == null)
				{
					this._worldObject = base.GetComponentInParent<WorldObject>();
				}
				return this._worldObject;
			}
		}

		// Token: 0x170002BC RID: 700
		// (get) Token: 0x06000E54 RID: 3668 RVA: 0x0004C328 File Offset: 0x0004A528
		public Transform ShootPointTransform
		{
			get
			{
				if (!this.FirePoint)
				{
					return base.transform;
				}
				return this.FirePoint;
			}
		}

		// Token: 0x170002BD RID: 701
		// (get) Token: 0x06000E55 RID: 3669 RVA: 0x0004C344 File Offset: 0x0004A544
		public Team Team
		{
			get
			{
				if (!this.WorldObject)
				{
					return this.baseTeam;
				}
				TeamComponent teamComponent;
				if (!this.WorldObject.TryGetUserComponent<TeamComponent>(out teamComponent))
				{
					return this.baseTeam;
				}
				return teamComponent.Team;
			}
		}

		// Token: 0x06000E56 RID: 3670 RVA: 0x0004C381 File Offset: 0x0004A581
		public void SetupProjectileShooterReferences(ProjectileShooterReferences references)
		{
			this.SetupProjectileShooterReferences(references.FirePoint, references.EjectionEffectPoint, references.Magazine, null, null);
		}

		// Token: 0x06000E57 RID: 3671 RVA: 0x0004C3A0 File Offset: 0x0004A5A0
		public void SetupProjectileShooterReferences(Transform firePoint, Transform ejectionPoint, Transform magazine, Transform magazineEjectionPoint, ProjectileShooterEjectionSettings ejectionSettings)
		{
			this.FirePoint = firePoint;
			this.EjectionEffectPoint = ejectionPoint;
			this.Magazine = magazine;
			this.MagazineEjectionPoint = ((magazineEjectionPoint != null) ? this.MagazineEjectionPoint : magazine);
			this.ejectionSettings = ((ejectionSettings != null) ? ejectionSettings : new ProjectileShooterEjectionSettings());
			if (this.HitEffectPrefab == null)
			{
				Debug.LogWarning("Please update and assign HitEffectPrefab instead of HitEffect on " + base.name);
			}
		}

		// Token: 0x06000E58 RID: 3672 RVA: 0x0004C414 File Offset: 0x0004A614
		public void ClientAuthoritativeShoot(global::UnityEngine.Vector3 position, global::UnityEngine.Vector3 angle, uint frame, Action<HealthChangeResult, Context> callback = null, WorldObject worldObject = null)
		{
			if (this.associatedItem == null || this.associatedItem.Carrier.OwnerClientId != base.NetworkManager.LocalClientId)
			{
				return;
			}
			global::UnityEngine.Vector3 vector = Quaternion.Euler(angle) * global::UnityEngine.Vector3.forward;
			Projectile.HitScanResult hitScanResult;
			if (this.ProjectilePrefab.HitScan(1U, position, vector, out hitScanResult, this, true))
			{
				this.ClientAuthoritativeAutoHit_ServerRpc(hitScanResult, frame, vector, default(ServerRpcParams));
				this.PlayShootEffects();
				this.VisualizeAutoHit(hitScanResult);
				global::UnityEngine.Vector3 vector2 = position - hitScanResult.WorldPosition;
				uint autoHitFrames = this.GetAutoHitFrames(vector2.sqrMagnitude);
				this.PlayOnHitEffect(hitScanResult.WorldPosition, Quaternion.LookRotation(vector2.normalized), autoHitFrames);
				NetworkObject networkObject = hitScanResult.Target;
				if (networkObject)
				{
					this.ProjectilePrefab.ProcessDamage(networkObject);
				}
				this.ProjectilePrefab.ProcessHitModules(frame, this.ShootingWorldObject, hitScanResult.WorldObject, hitScanResult.WorldPosition, vector);
				return;
			}
			if (!base.NetworkManager.IsServer)
			{
				this.ShootProjectileLocal(position, angle, frame, callback, worldObject);
			}
			this.ClientAuthoritativeShoot_ServerRpc(position, angle, frame, default(ServerRpcParams));
		}

		// Token: 0x06000E59 RID: 3673 RVA: 0x0004C53C File Offset: 0x0004A73C
		[ServerRpc(RequireOwnership = false)]
		private void ClientAuthoritativeShoot_ServerRpc(global::UnityEngine.Vector3 position, global::UnityEngine.Vector3 angle, uint frame, ServerRpcParams serverRpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(586193946U, serverRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe(in position);
				fastBufferWriter.WriteValueSafe(in angle);
				BytePacker.WriteValueBitPacked(fastBufferWriter, frame);
				base.__endSendServerRpc(ref fastBufferWriter, 586193946U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (this.associatedItem == null || this.associatedItem.Carrier.OwnerClientId != serverRpcParams.Receive.SenderClientId)
			{
				return;
			}
			if (this.associatedItem.AmmoCount > 0)
			{
				this.associatedItem.AmmoCount = Mathf.Max(this.associatedItem.AmmoCount - 1, 0);
			}
			this.ShootProjectileLocal(position, angle, frame, null, this.associatedItem.Carrier.WorldObject);
		}

		// Token: 0x06000E5A RID: 3674 RVA: 0x0004C6AC File Offset: 0x0004A8AC
		[ServerRpc(RequireOwnership = false)]
		private void ClientAuthoritativeAutoHit_ServerRpc(Projectile.HitScanResult data, uint frame, global::UnityEngine.Vector3 angle, ServerRpcParams serverRpcParams = default(ServerRpcParams))
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(3555919450U, serverRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<Projectile.HitScanResult>(in data, default(FastBufferWriter.ForNetworkSerializable));
				BytePacker.WriteValueBitPacked(fastBufferWriter, frame);
				fastBufferWriter.WriteValueSafe(in angle);
				base.__endSendServerRpc(ref fastBufferWriter, 3555919450U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (this.associatedItem == null || this.associatedItem.Carrier.OwnerClientId != serverRpcParams.Receive.SenderClientId)
			{
				return;
			}
			if (this.associatedItem.AmmoCount > 0)
			{
				this.associatedItem.AmmoCount = Mathf.Max(this.associatedItem.AmmoCount - 1, 0);
			}
			NetworkObject networkObject = data.Target;
			if (networkObject && networkObject && 3.5f + 1f * this.ProjectilePrefab.DistanceTravelledPerFrame > global::UnityEngine.Vector3.Distance(this.associatedItem.Carrier.transform.position, data.WorldPosition))
			{
				this.ProjectilePrefab.ProcessDamage(networkObject);
			}
			this.ProjectilePrefab.ProcessHitModules(frame, this.ShootingWorldObject, data.WorldObject, data.WorldPosition, angle);
			this.PlayAutoHitEvent_ClientRpc(data, frame);
		}

		// Token: 0x06000E5B RID: 3675 RVA: 0x0004C897 File Offset: 0x0004AA97
		public void UpdateBaseTeam(Team team)
		{
			this.baseTeam = team;
		}

		// Token: 0x06000E5C RID: 3676 RVA: 0x0004C8A0 File Offset: 0x0004AAA0
		public void GotNetworkProjectile(NetworkProjectile networkProjectile)
		{
			Projectile associatedProjectile = null;
			this.predictedProjectiles.RemoveAll(delegate(Projectile predicted)
			{
				if (predicted.StartFrame == networkProjectile.StartFrame)
				{
					associatedProjectile = predicted;
					return true;
				}
				return false;
			});
			if (associatedProjectile == null)
			{
				associatedProjectile = this.SpawnProjectileObject(networkProjectile.StartPosition, networkProjectile.StartAngle, networkProjectile.StartFrame, null, null);
				associatedProjectile.LinkToNetworkProjectile(networkProjectile, false);
				return;
			}
			associatedProjectile.LinkToNetworkProjectile(networkProjectile, true);
		}

		// Token: 0x06000E5D RID: 3677 RVA: 0x0004C93C File Offset: 0x0004AB3C
		private void VisualizeAutoHit(Projectile.HitScanResult autohitData)
		{
			global::UnityEngine.Vector3 vector = (this.FirePoint ? this.FirePoint.position : base.transform.position);
			Projectile projectile;
			if (ProjectileManager.UsePooling)
			{
				projectile = MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn<Projectile>(this.ProjectilePrefab, vector, Quaternion.LookRotation(autohitData.WorldPosition - vector, global::UnityEngine.Vector3.up), null);
			}
			else
			{
				projectile = global::UnityEngine.Object.Instantiate<Projectile>(this.ProjectilePrefab, vector, Quaternion.LookRotation(autohitData.WorldPosition - vector, global::UnityEngine.Vector3.up));
			}
			projectile.LocalInitAutoHit(vector, autohitData, this.GetAutoHitFrames((autohitData.WorldPosition - vector).sqrMagnitude));
		}

		// Token: 0x06000E5E RID: 3678 RVA: 0x0004C9EC File Offset: 0x0004ABEC
		public void ShootProjectileLocal(global::UnityEngine.Vector3 position, global::UnityEngine.Vector3 angle, uint frame, Action<HealthChangeResult, Context> callback = null, WorldObject worldObject = null)
		{
			if (worldObject == null)
			{
				worldObject = this.WorldObject;
			}
			Projectile projectile = this.SpawnProjectileObject(position, angle, frame, callback, worldObject);
			if (!base.NetworkManager.IsServer)
			{
				this.predictedProjectiles.Add(projectile);
				this.PlayLocalShootEvent(frame);
				return;
			}
			this.PlayShootEvent_ClientRpc(frame);
		}

		// Token: 0x06000E5F RID: 3679 RVA: 0x0004CA44 File Offset: 0x0004AC44
		private Projectile SpawnProjectileObject(global::UnityEngine.Vector3 position, global::UnityEngine.Vector3 angle, uint frame, Action<HealthChangeResult, Context> callback = null, WorldObject worldObject = null)
		{
			Projectile projectile;
			if (ProjectileManager.UsePooling)
			{
				projectile = MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn<Projectile>(this.ProjectilePrefab, position, Quaternion.Euler(angle), null);
			}
			else
			{
				projectile = global::UnityEngine.Object.Instantiate<Projectile>(this.ProjectilePrefab, position, Quaternion.Euler(angle));
			}
			projectile.LocalInit(this, position, angle, frame, callback, worldObject, null, this.simulationDelayFrames);
			return projectile;
		}

		// Token: 0x06000E60 RID: 3680 RVA: 0x0004CA9C File Offset: 0x0004AC9C
		public void ProjectileMispredicted(Projectile projectile)
		{
			this.predictedProjectiles.Remove(projectile);
		}

		// Token: 0x06000E61 RID: 3681 RVA: 0x0004CAAC File Offset: 0x0004ACAC
		public void PlayLocalShootEvent(uint frame)
		{
			uint currentFrame = NetClock.CurrentFrame;
			this.UpdateShootingStateOnFire(frame);
			if (frame > currentFrame)
			{
				base.Invoke("PlayShootEffects", (frame - currentFrame) * NetClock.FixedDeltaTime + NetClock.FixedDeltaTime * 1.1f);
				return;
			}
			this.PlayShootEffects();
		}

		// Token: 0x06000E62 RID: 3682 RVA: 0x0004CAF4 File Offset: 0x0004ACF4
		private void PlayShootEffects()
		{
			if (this.muzzleFlashEffectInstance == null && this.MuzzleFlashEffect != null && this.FirePoint != null)
			{
				this.muzzleFlashEffectInstance = global::UnityEngine.Object.Instantiate<ParticleSystem>(this.MuzzleFlashEffect, this.FirePoint.position, this.FirePoint.rotation, this.FirePoint);
			}
			if (this.muzzleFlashEffectInstance != null)
			{
				this.muzzleFlashEffectInstance.Play();
			}
			if (this.ejectionEffectInstance == null && this.EjectionEffect != null && this.EjectionEffectPoint != null)
			{
				this.ejectionEffectInstance = global::UnityEngine.Object.Instantiate<ParticleSystem>(this.EjectionEffect, this.EjectionEffectPoint.position, this.EjectionEffectPoint.rotation, this.EjectionEffectPoint);
			}
			if (this.ejectionEffectInstance != null)
			{
				this.ejectionEffectInstance.Play();
			}
			if (this.ProjectilePrefab.CasingPrefab != null && this.EjectionEffectPoint != null)
			{
				if (this.casingHash == 0)
				{
					this.casingHash = this.MakeHash(this.ProjectilePrefab.CasingPrefab.name);
				}
				SmallOfflinePhysicsObject smallOfflinePhysicsObject = SmallOfflinePhysicsObjectManager.Spawn(this.casingHash, this.ProjectilePrefab.CasingPrefab, this.EjectionEffectPoint.position, this.ProjectilePrefab.CasingPrefab.transform.rotation, this.ProjectilePrefab.CasingPrefab.transform.localScale, SmallOfflinePhysicsObjectManager.Transform);
				if (smallOfflinePhysicsObject != null)
				{
					float value = global::UnityEngine.Random.value;
					float num = Mathf.Sqrt(1f - value * value);
					float num2 = global::UnityEngine.Random.Range(-this.ejectionSettings.casingAngleVariance, this.ejectionSettings.casingAngleVariance);
					Quaternion quaternion = this.EjectionEffectPoint.rotation * Quaternion.Euler(value * num2, num * num2, 0f);
					smallOfflinePhysicsObject.transform.rotation = Quaternion.FromToRotation(this.EjectionEffectPoint.forward, quaternion * global::UnityEngine.Vector3.forward) * this.ProjectilePrefab.CasingPrefab.transform.rotation;
					smallOfflinePhysicsObject.Rigidbody.velocity = quaternion * global::UnityEngine.Vector3.forward * global::UnityEngine.Random.Range(this.ejectionSettings.casingForce.x, this.ejectionSettings.casingForce.y);
					global::UnityEngine.Vector3 vector = new global::UnityEngine.Vector3(global::UnityEngine.Random.Range(this.ejectionSettings.casingAngularVelocityVarianceX.x, this.ejectionSettings.casingAngularVelocityVarianceX.y), global::UnityEngine.Random.Range(this.ejectionSettings.casingAngularVelocityVarianceY.x, this.ejectionSettings.casingAngularVelocityVarianceY.y), global::UnityEngine.Random.Range(this.ejectionSettings.casingAngularVelocityVarianceZ.x, this.ejectionSettings.casingAngularVelocityVarianceZ.y)) * 0.017453292f;
					float magnitude = vector.magnitude;
					smallOfflinePhysicsObject.Rigidbody.maxAngularVelocity = magnitude;
					smallOfflinePhysicsObject.Rigidbody.angularVelocity = this.EjectionEffectPoint.TransformDirection(vector.normalized) * magnitude;
					if (this.ejectionSettings.casingOverrideDrag)
					{
						smallOfflinePhysicsObject.Rigidbody.drag = this.ejectionSettings.casingDrag;
						smallOfflinePhysicsObject.Rigidbody.angularDrag = this.ejectionSettings.casingAngularDrag;
					}
				}
			}
			if (this.associatedItem != null)
			{
				this.associatedItem.ShotAnimTriggered();
			}
		}

		// Token: 0x06000E63 RID: 3683 RVA: 0x0004CE6C File Offset: 0x0004B06C
		public void SpawnMagazine()
		{
			if (this.ProjectilePrefab.MagazinePrefab != null)
			{
				if (this.magHash == 0)
				{
					this.magHash = this.MakeHash(this.ProjectilePrefab.MagazinePrefab.name);
				}
				Transform transform = ((this.MagazineEjectionPoint != null) ? this.MagazineEjectionPoint : this.Magazine);
				SmallOfflinePhysicsObject smallOfflinePhysicsObject = SmallOfflinePhysicsObjectManager.Spawn(this.magHash, this.ProjectilePrefab.MagazinePrefab, transform.position, transform.rotation, transform.localScale, SmallOfflinePhysicsObjectManager.Transform);
				if (smallOfflinePhysicsObject != null)
				{
					float value = global::UnityEngine.Random.value;
					float num = Mathf.Sqrt(1f - value * value);
					float num2 = global::UnityEngine.Random.Range(-this.ejectionSettings.magAngleVariance, this.ejectionSettings.magAngleVariance);
					Quaternion quaternion = transform.rotation * Quaternion.Euler(value * num2, num * num2, 0f);
					smallOfflinePhysicsObject.transform.rotation = Quaternion.FromToRotation(transform.forward, quaternion * global::UnityEngine.Vector3.forward) * this.ProjectilePrefab.MagazinePrefab.transform.rotation;
					smallOfflinePhysicsObject.Rigidbody.velocity = quaternion * global::UnityEngine.Vector3.forward * global::UnityEngine.Random.Range(this.ejectionSettings.magForce.x, this.ejectionSettings.magForce.y);
					global::UnityEngine.Vector3 vector = new global::UnityEngine.Vector3(global::UnityEngine.Random.Range(this.ejectionSettings.magAngularVelocityVarianceX.x, this.ejectionSettings.magAngularVelocityVarianceX.y), global::UnityEngine.Random.Range(this.ejectionSettings.magAngularVelocityVarianceY.x, this.ejectionSettings.magAngularVelocityVarianceY.y), global::UnityEngine.Random.Range(this.ejectionSettings.magAngularVelocityVarianceZ.x, this.ejectionSettings.magAngularVelocityVarianceZ.y)) * 0.017453292f;
					float magnitude = vector.magnitude;
					smallOfflinePhysicsObject.Rigidbody.maxAngularVelocity = magnitude;
					smallOfflinePhysicsObject.Rigidbody.angularVelocity = transform.TransformDirection(vector.normalized) * magnitude;
					if (this.ejectionSettings.magOverrideDrag)
					{
						smallOfflinePhysicsObject.Rigidbody.drag = this.ejectionSettings.magDrag;
						smallOfflinePhysicsObject.Rigidbody.angularDrag = this.ejectionSettings.magAngularDrag;
					}
				}
			}
		}

		// Token: 0x06000E64 RID: 3684 RVA: 0x0004D0C8 File Offset: 0x0004B2C8
		[ClientRpc]
		private void PlayShootEvent_ClientRpc(uint frame)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(3569284547U, clientRpcParams, RpcDelivery.Reliable);
				BytePacker.WriteValueBitPacked(fastBufferWriter, frame);
				base.__endSendClientRpc(ref fastBufferWriter, 3569284547U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (!base.NetworkManager.IsServer && this.associatedItem != null && this.associatedItem.Carrier != null && this.associatedItem.Carrier.IsOwner)
			{
				return;
			}
			this.PlayLocalShootEvent(frame);
		}

		// Token: 0x06000E65 RID: 3685 RVA: 0x0004D1F0 File Offset: 0x0004B3F0
		[ClientRpc]
		private void PlayAutoHitEvent_ClientRpc(Projectile.HitScanResult data, uint frame)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(110267456U, clientRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<Projectile.HitScanResult>(in data, default(FastBufferWriter.ForNetworkSerializable));
				BytePacker.WriteValueBitPacked(fastBufferWriter, frame);
				base.__endSendClientRpc(ref fastBufferWriter, 110267456U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (this.associatedItem != null && this.associatedItem.Carrier != null && this.associatedItem.Carrier.IsOwner)
			{
				return;
			}
			this.PlayShootEffects();
			global::UnityEngine.Vector3 vector = base.transform.position - data.WorldPosition;
			global::UnityEngine.Vector3 vector2 = (this.FirePoint ? this.FirePoint.position : base.transform.position);
			uint autoHitFrames = this.GetAutoHitFrames((data.WorldPosition - vector2).sqrMagnitude);
			this.PlayOnHitEffect(data.WorldPosition, Quaternion.LookRotation(vector.normalized), autoHitFrames);
			this.VisualizeAutoHit(data);
		}

		// Token: 0x06000E66 RID: 3686 RVA: 0x0004D3A0 File Offset: 0x0004B5A0
		public void AISetShooterReferences(AIProjectileShooterReferencePointer references)
		{
			this.FirePoint = references.FirePoint;
			this.MuzzleFlashEffect = references.MuzzleFlashEffect;
			this.EjectionEffect = references.EjectionEffect;
			this.HitEffect = references.HitEffect;
			if (references.HitEffectPrefab != null)
			{
				this.HitEffectPrefab = references.HitEffectPrefab;
				return;
			}
			this.HitEffectPrefab = this.HitEffect.GetComponent<HitEffect>();
		}

		// Token: 0x06000E67 RID: 3687 RVA: 0x0004D40C File Offset: 0x0004B60C
		public void EndlessGameEnd()
		{
			foreach (Projectile projectile in new List<Projectile>(this.predictedProjectiles))
			{
				projectile.DestroySelf();
			}
			this.predictedProjectiles.Clear();
		}

		// Token: 0x06000E68 RID: 3688 RVA: 0x0004D46C File Offset: 0x0004B66C
		public void PlayOnHitEffect(global::UnityEngine.Vector3 position, Quaternion rotation, uint frameDelay = 0U)
		{
			if (frameDelay > 0U)
			{
				base.StartCoroutine(this.DelayHitEffect(position, rotation, frameDelay * NetClock.FixedDeltaTime));
				return;
			}
			if (this.HitEffectPrefab)
			{
				HitEffect hitEffect;
				if (ProjectileManager.UsePooling)
				{
					hitEffect = MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn<HitEffect>(this.HitEffectPrefab, position, rotation, null);
					hitEffect.Particles.main.stopAction = ParticleSystemStopAction.Callback;
				}
				else
				{
					hitEffect = global::UnityEngine.Object.Instantiate<HitEffect>(this.HitEffectPrefab, position, rotation);
					hitEffect.Particles.main.stopAction = ParticleSystemStopAction.Destroy;
				}
				hitEffect.PlayEffect();
				return;
			}
			ParticleSystem particleSystem = global::UnityEngine.Object.Instantiate<ParticleSystem>(this.HitEffect, position, rotation);
			particleSystem.main.stopAction = ParticleSystemStopAction.Destroy;
			particleSystem.Play();
		}

		// Token: 0x06000E69 RID: 3689 RVA: 0x0004D520 File Offset: 0x0004B720
		public uint GetAutoHitFrames(float squaredDistance)
		{
			float distanceTravelledPerFrame = this.ProjectilePrefab.DistanceTravelledPerFrame;
			return (uint)Mathf.RoundToInt(squaredDistance / (distanceTravelledPerFrame * distanceTravelledPerFrame) + 1f);
		}

		// Token: 0x06000E6A RID: 3690 RVA: 0x0004D549 File Offset: 0x0004B749
		private IEnumerator DelayHitEffect(global::UnityEngine.Vector3 position, Quaternion rotation, float time)
		{
			yield return new WaitForSeconds(time);
			this.PlayOnHitEffect(position, rotation, 0U);
			yield break;
		}

		// Token: 0x06000E6B RID: 3691 RVA: 0x0004D570 File Offset: 0x0004B770
		private void UpdateShootingStateOnFire(uint frame)
		{
			if (this.associatedItem == null || this.associatedItem.IsLocal())
			{
				return;
			}
			ShootingState shootingState = this.associatedItem.GetShootingState(frame);
			if (frame > shootingState.lastShotFrame)
			{
				shootingState.lastShotFrame = frame;
				shootingState.waitingForShot = false;
				shootingState.NetFrame = frame;
				this.associatedItem.ShootingStateUpdated(frame, ref shootingState);
			}
		}

		// Token: 0x06000E6C RID: 3692 RVA: 0x0004D5D8 File Offset: 0x0004B7D8
		private int MakeHash(string prefabName)
		{
			if (this.associatedItem != null)
			{
				return Animator.StringToHash(this.associatedItem.InventoryUsableDefinition.DisplayName + "." + prefabName);
			}
			SerializableGuid instanceIdFromGameObject = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetInstanceIdFromGameObject(this.WorldObject.gameObject);
			SerializableGuid assetIdFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(instanceIdFromGameObject);
			PropLibrary.RuntimePropInfo runtimePropInfo;
			Prop prop = (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetIdFromInstanceId, out runtimePropInfo) ? runtimePropInfo.PropData : null);
			if (!string.IsNullOrEmpty((prop != null) ? prop.Name : null))
			{
				return Animator.StringToHash(prop.Name + "." + prefabName);
			}
			Debug.LogWarning(string.Format("{0}: couldn't get prop or prop name empty on {1}. prop null? {2}", "MakeHash", base.name, prop == null));
			Debug.LogWarning("MakeHash: No ranged item and no prop found, using instance name " + base.name + ".");
			return Animator.StringToHash(base.name + "." + prefabName);
		}

		// Token: 0x06000E6E RID: 3694 RVA: 0x0004D6F0 File Offset: 0x0004B8F0
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x06000E6F RID: 3695 RVA: 0x0004D708 File Offset: 0x0004B908
		protected override void __initializeRpcs()
		{
			base.__registerRpc(586193946U, new NetworkBehaviour.RpcReceiveHandler(ProjectileShooter.__rpc_handler_586193946), "ClientAuthoritativeShoot_ServerRpc");
			base.__registerRpc(3555919450U, new NetworkBehaviour.RpcReceiveHandler(ProjectileShooter.__rpc_handler_3555919450), "ClientAuthoritativeAutoHit_ServerRpc");
			base.__registerRpc(3569284547U, new NetworkBehaviour.RpcReceiveHandler(ProjectileShooter.__rpc_handler_3569284547), "PlayShootEvent_ClientRpc");
			base.__registerRpc(110267456U, new NetworkBehaviour.RpcReceiveHandler(ProjectileShooter.__rpc_handler_110267456), "PlayAutoHitEvent_ClientRpc");
			base.__initializeRpcs();
		}

		// Token: 0x06000E70 RID: 3696 RVA: 0x0004D790 File Offset: 0x0004B990
		private static void __rpc_handler_586193946(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			global::UnityEngine.Vector3 vector;
			reader.ReadValueSafe(out vector);
			global::UnityEngine.Vector3 vector2;
			reader.ReadValueSafe(out vector2);
			uint num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((ProjectileShooter)target).ClientAuthoritativeShoot_ServerRpc(vector, vector2, num, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06000E71 RID: 3697 RVA: 0x0004D824 File Offset: 0x0004BA24
		private static void __rpc_handler_3555919450(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			Projectile.HitScanResult hitScanResult;
			reader.ReadValueSafe<Projectile.HitScanResult>(out hitScanResult, default(FastBufferWriter.ForNetworkSerializable));
			uint num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			global::UnityEngine.Vector3 vector;
			reader.ReadValueSafe(out vector);
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((ProjectileShooter)target).ClientAuthoritativeAutoHit_ServerRpc(hitScanResult, num, vector, server);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06000E72 RID: 3698 RVA: 0x0004D8C4 File Offset: 0x0004BAC4
		private static void __rpc_handler_3569284547(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			uint num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((ProjectileShooter)target).PlayShootEvent_ClientRpc(num);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06000E73 RID: 3699 RVA: 0x0004D928 File Offset: 0x0004BB28
		private static void __rpc_handler_110267456(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			Projectile.HitScanResult hitScanResult;
			reader.ReadValueSafe<Projectile.HitScanResult>(out hitScanResult, default(FastBufferWriter.ForNetworkSerializable));
			uint num;
			ByteUnpacker.ReadValueBitPacked(reader, out num);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((ProjectileShooter)target).PlayAutoHitEvent_ClientRpc(hitScanResult, num);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x06000E74 RID: 3700 RVA: 0x0004D9A9 File Offset: 0x0004BBA9
		protected internal override string __getTypeName()
		{
			return "ProjectileShooter";
		}

		// Token: 0x04000CF5 RID: 3317
		private const uint AUTO_HIT_SCAN_AHEAD_FRAMES = 1U;

		// Token: 0x04000CF6 RID: 3318
		private const float SERVER_AUTO_HIT_DISTANCE_LENIENCE = 3.5f;

		// Token: 0x04000D00 RID: 3328
		[SerializeField]
		[HideInInspector]
		private WorldObject _worldObject;

		// Token: 0x04000D01 RID: 3329
		[Header("Team")]
		[SerializeField]
		private Team baseTeam;

		// Token: 0x04000D02 RID: 3330
		[Header("Simulation Delay")]
		[SerializeField]
		private uint simulationDelayFrames;

		// Token: 0x04000D03 RID: 3331
		[SerializeField]
		private ParticleSystem muzzleFlashEffectInstance;

		// Token: 0x04000D04 RID: 3332
		[SerializeField]
		private ParticleSystem ejectionEffectInstance;

		// Token: 0x04000D05 RID: 3333
		[Header("Player Character Equipment Only")]
		[SerializeField]
		private RangedWeaponItem associatedItem;

		// Token: 0x04000D06 RID: 3334
		private List<Projectile> predictedProjectiles = new List<Projectile>();

		// Token: 0x04000D07 RID: 3335
		private int magHash;

		// Token: 0x04000D08 RID: 3336
		private int casingHash;

		// Token: 0x04000D09 RID: 3337
		[SerializeReference]
		private ProjectileShooterEjectionSettings ejectionSettings;
	}
}
