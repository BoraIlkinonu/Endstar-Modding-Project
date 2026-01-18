using System;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class NetworkProjectile : NetworkBehaviour, IPoolableT
{
	private NetworkVariable<Vector3> startPosition = new NetworkVariable<Vector3>();

	private NetworkVariable<Vector3> startAngle = new NetworkVariable<Vector3>();

	private NetworkVariable<uint> startFrame = new NetworkVariable<uint>(0u);

	private NetworkVariable<NetworkObjectReference> parentObject = new NetworkVariable<NetworkObjectReference>();

	[NonSerialized]
	public Projectile runtimeGameProjectile;

	public uint StartFrame => startFrame.Value;

	public Vector3 StartPosition => startPosition.Value;

	public Vector3 StartAngle => startAngle.Value;

	public NetworkObjectReference ParentObject => parentObject.Value;

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

	private void OnEnable()
	{
		ProjectileManager.AddNetworkProjectile(this);
	}

	private void OnDisable()
	{
		ProjectileManager.RemoveNetworkProjectile(this);
	}

	public void ServerDelayedDestroy()
	{
		Invoke("DestroySelf", 1f);
	}

	private void DestroySelf()
	{
		if (base.IsServer)
		{
			if (base.IsSpawned && base.NetworkManager.SpawnManager != null)
			{
				base.NetworkObject.Despawn();
			}
			if (!ProjectileManager.PoolNetwork)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}

	public void ServerInit(Vector3 position, Vector3 angle, uint frame, ProjectileShooter shooter)
	{
		if (base.NetworkManager.IsServer)
		{
			startPosition.Reset(position);
			startAngle.Reset(angle);
			startFrame.Reset(frame);
			parentObject.Reset(shooter.NetworkObject);
		}
	}

	public void OnSpawn()
	{
		base.transform.SetParent(null, worldPositionStays: true);
	}

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		if (!base.NetworkManager.IsServer)
		{
			ProjectileShooter projectileShooter = ((NetworkObject)parentObject.Value)?.GetComponentInChildren<ProjectileShooter>();
			if (projectileShooter != null)
			{
				projectileShooter.GotNetworkProjectile(this);
			}
			else if (MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying)
			{
				Debug.LogWarning("Invalid projectile spawner for network projectile.");
			}
		}
	}

	public override void OnNetworkDespawn()
	{
		base.OnNetworkDespawn();
		if (runtimeGameProjectile != null)
		{
			runtimeGameProjectile.DestroySelf();
			runtimeGameProjectile = null;
		}
	}

	protected override void __initializeVariables()
	{
		if (startPosition == null)
		{
			throw new Exception("NetworkProjectile.startPosition cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		startPosition.Initialize(this);
		__nameNetworkVariable(startPosition, "startPosition");
		NetworkVariableFields.Add(startPosition);
		if (startAngle == null)
		{
			throw new Exception("NetworkProjectile.startAngle cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		startAngle.Initialize(this);
		__nameNetworkVariable(startAngle, "startAngle");
		NetworkVariableFields.Add(startAngle);
		if (startFrame == null)
		{
			throw new Exception("NetworkProjectile.startFrame cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		startFrame.Initialize(this);
		__nameNetworkVariable(startFrame, "startFrame");
		NetworkVariableFields.Add(startFrame);
		if (parentObject == null)
		{
			throw new Exception("NetworkProjectile.parentObject cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		parentObject.Initialize(this);
		__nameNetworkVariable(parentObject, "parentObject");
		NetworkVariableFields.Add(parentObject);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected internal override string __getTypeName()
	{
		return "NetworkProjectile";
	}
}
