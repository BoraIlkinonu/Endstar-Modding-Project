using System;
using System.Collections.Generic;
using Endless.Shared;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200028B RID: 651
	[DefaultExecutionOrder(2000)]
	public class ProjectileManager : MonoBehaviourSingleton<ProjectileManager>
	{
		// Token: 0x170002AA RID: 682
		// (get) Token: 0x06000E2B RID: 3627 RVA: 0x0004BFB3 File Offset: 0x0004A1B3
		public static NetworkProjectile NetworkProjectilePrefab
		{
			get
			{
				return MonoBehaviourSingleton<ProjectileManager>.Instance.networkProjectilePrefab;
			}
		}

		// Token: 0x170002AB RID: 683
		// (get) Token: 0x06000E2C RID: 3628 RVA: 0x0004BFBF File Offset: 0x0004A1BF
		public static LayerMask AttackableCollisionMask
		{
			get
			{
				return MonoBehaviourSingleton<ProjectileManager>.Instance.attackableCollisionMask;
			}
		}

		// Token: 0x170002AC RID: 684
		// (get) Token: 0x06000E2D RID: 3629 RVA: 0x0004BFCB File Offset: 0x0004A1CB
		public static LayerMask AttackableAndWallCollisionMask
		{
			get
			{
				return MonoBehaviourSingleton<ProjectileManager>.Instance.attackableAndWallCollisionMask;
			}
		}

		// Token: 0x170002AD RID: 685
		// (get) Token: 0x06000E2E RID: 3630 RVA: 0x0004BFD7 File Offset: 0x0004A1D7
		public static LayerMask WallCollisionMask
		{
			get
			{
				return MonoBehaviourSingleton<ProjectileManager>.Instance.wallCollisionMask;
			}
		}

		// Token: 0x06000E2F RID: 3631 RVA: 0x0004BFE3 File Offset: 0x0004A1E3
		public static bool IsWallLayer(int layer)
		{
			return ProjectileManager.WallCollisionMask == (ProjectileManager.WallCollisionMask | (1 << layer));
		}

		// Token: 0x170002AE RID: 686
		// (get) Token: 0x06000E30 RID: 3632 RVA: 0x0004C002 File Offset: 0x0004A202
		// (set) Token: 0x06000E31 RID: 3633 RVA: 0x0004C009 File Offset: 0x0004A209
		public static Transform ProjectileParent { get; private set; }

		// Token: 0x170002AF RID: 687
		// (get) Token: 0x06000E32 RID: 3634 RVA: 0x0004C011 File Offset: 0x0004A211
		public static bool UsePooling
		{
			get
			{
				return MonoBehaviourSingleton<ProjectileManager>.Instance.usePooling;
			}
		}

		// Token: 0x170002B0 RID: 688
		// (get) Token: 0x06000E33 RID: 3635 RVA: 0x0004C01D File Offset: 0x0004A21D
		public static bool PoolNetwork
		{
			get
			{
				return ProjectileManager.UsePooling;
			}
		}

		// Token: 0x06000E34 RID: 3636 RVA: 0x0004C024 File Offset: 0x0004A224
		private void OnEnable()
		{
			NetworkManager.Singleton.OnClientStopped += this.HandleNetworkDisconnect;
			NetworkManager.Singleton.OnServerStopped += this.HandleNetworkDisconnect;
		}

		// Token: 0x06000E35 RID: 3637 RVA: 0x0004C052 File Offset: 0x0004A252
		private void OnDisable()
		{
			if (NetworkManager.Singleton != null)
			{
				NetworkManager.Singleton.OnClientStopped -= this.HandleNetworkDisconnect;
				NetworkManager.Singleton.OnServerStopped -= this.HandleNetworkDisconnect;
			}
		}

		// Token: 0x06000E36 RID: 3638 RVA: 0x0004C08D File Offset: 0x0004A28D
		private void Start()
		{
			this.projectiles = new List<Projectile>(100);
			this.projectileAppearances = new List<ProjectileAppearance>(100);
			this.networkProjectiles = new List<NetworkProjectile>(100);
			NetworkPooledPrefabHandler<NetworkProjectile>.RegisterNetworkPrefab<NetworkProjectile>(this.networkProjectilePrefab, new int?(20));
		}

		// Token: 0x06000E37 RID: 3639 RVA: 0x0004C0C8 File Offset: 0x0004A2C8
		private void HandleNetworkDisconnect(bool host)
		{
			this.DestroyAllProjectiles();
		}

		// Token: 0x06000E38 RID: 3640 RVA: 0x0004C0D0 File Offset: 0x0004A2D0
		private void DestroyAllProjectiles()
		{
			this.blockRemove = true;
			for (int i = this.projectiles.Count - 1; i >= 0; i--)
			{
				this.projectiles[i].DestroySelf();
			}
			this.projectiles.Clear();
			for (int j = this.projectileAppearances.Count - 1; j >= 0; j--)
			{
				this.projectileAppearances[j].DestroySelf();
			}
			this.projectileAppearances.Clear();
			for (int k = this.networkProjectiles.Count - 1; k >= 0; k--)
			{
				if (this.usePooling)
				{
					MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<NetworkProjectile>(this.networkProjectiles[k]);
				}
				else
				{
					global::UnityEngine.Object.Destroy(this.networkProjectiles[k].gameObject);
				}
			}
			this.networkProjectiles.Clear();
			this.blockRemove = false;
		}

		// Token: 0x06000E39 RID: 3641 RVA: 0x0004C1AC File Offset: 0x0004A3AC
		public static void AddProjectile(Projectile projectile)
		{
			MonoBehaviourSingleton<ProjectileManager>.Instance.projectiles.Add(projectile);
		}

		// Token: 0x06000E3A RID: 3642 RVA: 0x0004C1BE File Offset: 0x0004A3BE
		public static void RemoveProjectile(Projectile projectile)
		{
			if (!MonoBehaviourSingleton<ProjectileManager>.Instance.blockRemove)
			{
				MonoBehaviourSingleton<ProjectileManager>.Instance.projectiles.RemoveSwapBack(projectile);
			}
		}

		// Token: 0x06000E3B RID: 3643 RVA: 0x0004C1DD File Offset: 0x0004A3DD
		public static void AddProjectileAppearance(ProjectileAppearance appearance)
		{
			MonoBehaviourSingleton<ProjectileManager>.Instance.projectileAppearances.Add(appearance);
		}

		// Token: 0x06000E3C RID: 3644 RVA: 0x0004C1EF File Offset: 0x0004A3EF
		public static void RemoveProjectileAppearance(ProjectileAppearance appearance)
		{
			if (!MonoBehaviourSingleton<ProjectileManager>.Instance.blockRemove)
			{
				MonoBehaviourSingleton<ProjectileManager>.Instance.projectileAppearances.RemoveSwapBack(appearance);
			}
		}

		// Token: 0x06000E3D RID: 3645 RVA: 0x0004C20E File Offset: 0x0004A40E
		public static void AddNetworkProjectile(NetworkProjectile projectile)
		{
			MonoBehaviourSingleton<ProjectileManager>.Instance.networkProjectiles.Add(projectile);
		}

		// Token: 0x06000E3E RID: 3646 RVA: 0x0004C220 File Offset: 0x0004A420
		public static void RemoveNetworkProjectile(NetworkProjectile projectile)
		{
			if (!MonoBehaviourSingleton<ProjectileManager>.Instance.blockRemove)
			{
				MonoBehaviourSingleton<ProjectileManager>.Instance.networkProjectiles.RemoveSwapBack(projectile);
			}
		}

		// Token: 0x04000CEB RID: 3307
		[Header("Prefab References")]
		[SerializeField]
		private NetworkProjectile networkProjectilePrefab;

		// Token: 0x04000CEC RID: 3308
		[Header("LayerMasks")]
		[SerializeField]
		protected LayerMask attackableCollisionMask;

		// Token: 0x04000CED RID: 3309
		[SerializeField]
		protected LayerMask wallCollisionMask;

		// Token: 0x04000CEE RID: 3310
		[SerializeField]
		protected LayerMask attackableAndWallCollisionMask;

		// Token: 0x04000CEF RID: 3311
		[Header("Pooling")]
		[SerializeField]
		protected bool usePooling;

		// Token: 0x04000CF1 RID: 3313
		private List<Projectile> projectiles;

		// Token: 0x04000CF2 RID: 3314
		private List<ProjectileAppearance> projectileAppearances;

		// Token: 0x04000CF3 RID: 3315
		private List<NetworkProjectile> networkProjectiles;

		// Token: 0x04000CF4 RID: 3316
		private bool blockRemove;
	}
}
