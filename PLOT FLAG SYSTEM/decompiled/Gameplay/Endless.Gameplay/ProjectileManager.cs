using System.Collections.Generic;
using Endless.Shared;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

[DefaultExecutionOrder(2000)]
public class ProjectileManager : MonoBehaviourSingleton<ProjectileManager>
{
	[Header("Prefab References")]
	[SerializeField]
	private NetworkProjectile networkProjectilePrefab;

	[Header("LayerMasks")]
	[SerializeField]
	protected LayerMask attackableCollisionMask;

	[SerializeField]
	protected LayerMask wallCollisionMask;

	[SerializeField]
	protected LayerMask attackableAndWallCollisionMask;

	[Header("Pooling")]
	[SerializeField]
	protected bool usePooling;

	private List<Projectile> projectiles;

	private List<ProjectileAppearance> projectileAppearances;

	private List<NetworkProjectile> networkProjectiles;

	private bool blockRemove;

	public static NetworkProjectile NetworkProjectilePrefab => MonoBehaviourSingleton<ProjectileManager>.Instance.networkProjectilePrefab;

	public static LayerMask AttackableCollisionMask => MonoBehaviourSingleton<ProjectileManager>.Instance.attackableCollisionMask;

	public static LayerMask AttackableAndWallCollisionMask => MonoBehaviourSingleton<ProjectileManager>.Instance.attackableAndWallCollisionMask;

	public static LayerMask WallCollisionMask => MonoBehaviourSingleton<ProjectileManager>.Instance.wallCollisionMask;

	public static Transform ProjectileParent { get; private set; }

	public static bool UsePooling => MonoBehaviourSingleton<ProjectileManager>.Instance.usePooling;

	public static bool PoolNetwork => UsePooling;

	public static bool IsWallLayer(int layer)
	{
		return (int)WallCollisionMask == ((int)WallCollisionMask | (1 << layer));
	}

	private void OnEnable()
	{
		NetworkManager.Singleton.OnClientStopped += HandleNetworkDisconnect;
		NetworkManager.Singleton.OnServerStopped += HandleNetworkDisconnect;
	}

	private void OnDisable()
	{
		if (NetworkManager.Singleton != null)
		{
			NetworkManager.Singleton.OnClientStopped -= HandleNetworkDisconnect;
			NetworkManager.Singleton.OnServerStopped -= HandleNetworkDisconnect;
		}
	}

	private void Start()
	{
		projectiles = new List<Projectile>(100);
		projectileAppearances = new List<ProjectileAppearance>(100);
		networkProjectiles = new List<NetworkProjectile>(100);
		NetworkPooledPrefabHandler<NetworkProjectile>.RegisterNetworkPrefab(networkProjectilePrefab, 20);
	}

	private void HandleNetworkDisconnect(bool host)
	{
		DestroyAllProjectiles();
	}

	private void DestroyAllProjectiles()
	{
		blockRemove = true;
		for (int num = projectiles.Count - 1; num >= 0; num--)
		{
			projectiles[num].DestroySelf();
		}
		projectiles.Clear();
		for (int num2 = projectileAppearances.Count - 1; num2 >= 0; num2--)
		{
			projectileAppearances[num2].DestroySelf();
		}
		projectileAppearances.Clear();
		for (int num3 = networkProjectiles.Count - 1; num3 >= 0; num3--)
		{
			if (usePooling)
			{
				MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn(networkProjectiles[num3]);
			}
			else
			{
				Object.Destroy(networkProjectiles[num3].gameObject);
			}
		}
		networkProjectiles.Clear();
		blockRemove = false;
	}

	public static void AddProjectile(Projectile projectile)
	{
		MonoBehaviourSingleton<ProjectileManager>.Instance.projectiles.Add(projectile);
	}

	public static void RemoveProjectile(Projectile projectile)
	{
		if (!MonoBehaviourSingleton<ProjectileManager>.Instance.blockRemove)
		{
			MonoBehaviourSingleton<ProjectileManager>.Instance.projectiles.RemoveSwapBack(projectile);
		}
	}

	public static void AddProjectileAppearance(ProjectileAppearance appearance)
	{
		MonoBehaviourSingleton<ProjectileManager>.Instance.projectileAppearances.Add(appearance);
	}

	public static void RemoveProjectileAppearance(ProjectileAppearance appearance)
	{
		if (!MonoBehaviourSingleton<ProjectileManager>.Instance.blockRemove)
		{
			MonoBehaviourSingleton<ProjectileManager>.Instance.projectileAppearances.RemoveSwapBack(appearance);
		}
	}

	public static void AddNetworkProjectile(NetworkProjectile projectile)
	{
		MonoBehaviourSingleton<ProjectileManager>.Instance.networkProjectiles.Add(projectile);
	}

	public static void RemoveNetworkProjectile(NetworkProjectile projectile)
	{
		if (!MonoBehaviourSingleton<ProjectileManager>.Instance.blockRemove)
		{
			MonoBehaviourSingleton<ProjectileManager>.Instance.networkProjectiles.RemoveSwapBack(projectile);
		}
	}
}
