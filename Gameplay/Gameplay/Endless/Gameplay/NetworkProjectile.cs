using System;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000285 RID: 645
	public class NetworkProjectile : NetworkBehaviour, IPoolableT
	{
		// Token: 0x17000293 RID: 659
		// (get) Token: 0x06000DDB RID: 3547 RVA: 0x0004AC8E File Offset: 0x00048E8E
		public uint StartFrame
		{
			get
			{
				return this.startFrame.Value;
			}
		}

		// Token: 0x17000294 RID: 660
		// (get) Token: 0x06000DDC RID: 3548 RVA: 0x0004AC9B File Offset: 0x00048E9B
		public Vector3 StartPosition
		{
			get
			{
				return this.startPosition.Value;
			}
		}

		// Token: 0x17000295 RID: 661
		// (get) Token: 0x06000DDD RID: 3549 RVA: 0x0004ACA8 File Offset: 0x00048EA8
		public Vector3 StartAngle
		{
			get
			{
				return this.startAngle.Value;
			}
		}

		// Token: 0x17000296 RID: 662
		// (get) Token: 0x06000DDE RID: 3550 RVA: 0x0004ACB5 File Offset: 0x00048EB5
		public NetworkObjectReference ParentObject
		{
			get
			{
				return this.parentObject.Value;
			}
		}

		// Token: 0x17000297 RID: 663
		// (get) Token: 0x06000DDF RID: 3551 RVA: 0x0004ACC2 File Offset: 0x00048EC2
		// (set) Token: 0x06000DE0 RID: 3552 RVA: 0x00002DB0 File Offset: 0x00000FB0
		public MonoBehaviour Prefab
		{
			get
			{
				return ProjectileManager.NetworkProjectilePrefab;
			}
			set
			{
			}
		}

		// Token: 0x06000DE1 RID: 3553 RVA: 0x0004ACC9 File Offset: 0x00048EC9
		private void OnEnable()
		{
			ProjectileManager.AddNetworkProjectile(this);
		}

		// Token: 0x06000DE2 RID: 3554 RVA: 0x0004ACD1 File Offset: 0x00048ED1
		private void OnDisable()
		{
			ProjectileManager.RemoveNetworkProjectile(this);
		}

		// Token: 0x06000DE3 RID: 3555 RVA: 0x0004ACD9 File Offset: 0x00048ED9
		public void ServerDelayedDestroy()
		{
			base.Invoke("DestroySelf", 1f);
		}

		// Token: 0x06000DE4 RID: 3556 RVA: 0x0004ACEB File Offset: 0x00048EEB
		private void DestroySelf()
		{
			if (base.IsServer)
			{
				if (base.IsSpawned && base.NetworkManager.SpawnManager != null)
				{
					base.NetworkObject.Despawn(true);
				}
				if (!ProjectileManager.PoolNetwork)
				{
					global::UnityEngine.Object.Destroy(base.gameObject);
				}
			}
		}

		// Token: 0x06000DE5 RID: 3557 RVA: 0x0004AD28 File Offset: 0x00048F28
		public void ServerInit(Vector3 position, Vector3 angle, uint frame, ProjectileShooter shooter)
		{
			if (!base.NetworkManager.IsServer)
			{
				return;
			}
			this.startPosition.Reset(position);
			this.startAngle.Reset(angle);
			this.startFrame.Reset(frame);
			this.parentObject.Reset(shooter.NetworkObject);
		}

		// Token: 0x06000DE6 RID: 3558 RVA: 0x0004AD7E File Offset: 0x00048F7E
		public void OnSpawn()
		{
			base.transform.SetParent(null, true);
		}

		// Token: 0x06000DE7 RID: 3559 RVA: 0x0004AD90 File Offset: 0x00048F90
		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			if (!base.NetworkManager.IsServer)
			{
				NetworkObject networkObject = this.parentObject.Value;
				ProjectileShooter projectileShooter = ((networkObject != null) ? networkObject.GetComponentInChildren<ProjectileShooter>() : null);
				if (projectileShooter != null)
				{
					projectileShooter.GotNetworkProjectile(this);
					return;
				}
				if (MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying)
				{
					Debug.LogWarning("Invalid projectile spawner for network projectile.");
				}
			}
		}

		// Token: 0x06000DE8 RID: 3560 RVA: 0x0004ADF4 File Offset: 0x00048FF4
		public override void OnNetworkDespawn()
		{
			base.OnNetworkDespawn();
			if (this.runtimeGameProjectile != null)
			{
				this.runtimeGameProjectile.DestroySelf();
				this.runtimeGameProjectile = null;
			}
		}

		// Token: 0x06000DEA RID: 3562 RVA: 0x0004AE80 File Offset: 0x00049080
		protected override void __initializeVariables()
		{
			bool flag = this.startPosition == null;
			if (flag)
			{
				throw new Exception("NetworkProjectile.startPosition cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.startPosition.Initialize(this);
			base.__nameNetworkVariable(this.startPosition, "startPosition");
			this.NetworkVariableFields.Add(this.startPosition);
			flag = this.startAngle == null;
			if (flag)
			{
				throw new Exception("NetworkProjectile.startAngle cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.startAngle.Initialize(this);
			base.__nameNetworkVariable(this.startAngle, "startAngle");
			this.NetworkVariableFields.Add(this.startAngle);
			flag = this.startFrame == null;
			if (flag)
			{
				throw new Exception("NetworkProjectile.startFrame cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.startFrame.Initialize(this);
			base.__nameNetworkVariable(this.startFrame, "startFrame");
			this.NetworkVariableFields.Add(this.startFrame);
			flag = this.parentObject == null;
			if (flag)
			{
				throw new Exception("NetworkProjectile.parentObject cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.parentObject.Initialize(this);
			base.__nameNetworkVariable(this.parentObject, "parentObject");
			this.NetworkVariableFields.Add(this.parentObject);
			base.__initializeVariables();
		}

		// Token: 0x06000DEB RID: 3563 RVA: 0x0000E74E File Offset: 0x0000C94E
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x06000DEC RID: 3564 RVA: 0x0004AFCA File Offset: 0x000491CA
		protected internal override string __getTypeName()
		{
			return "NetworkProjectile";
		}

		// Token: 0x04000CB3 RID: 3251
		private NetworkVariable<Vector3> startPosition = new NetworkVariable<Vector3>(default(Vector3), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04000CB4 RID: 3252
		private NetworkVariable<Vector3> startAngle = new NetworkVariable<Vector3>(default(Vector3), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04000CB5 RID: 3253
		private NetworkVariable<uint> startFrame = new NetworkVariable<uint>(0U, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04000CB6 RID: 3254
		private NetworkVariable<NetworkObjectReference> parentObject = new NetworkVariable<NetworkObjectReference>(default(NetworkObjectReference), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04000CB7 RID: 3255
		[NonSerialized]
		public Projectile runtimeGameProjectile;
	}
}
