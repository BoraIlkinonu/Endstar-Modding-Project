using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class Projectile : EndlessBehaviour, IPoolableT, NetClock.ISimulateFrameEnvironmentSubscriber, NetClock.IRollbackSubscriber, NetClock.IPostFixedUpdateSubscriber
{
	public struct HitScanResult : INetworkSerializable
	{
		public NetworkObjectReference Target;

		public UnityEngine.Vector3 Position;

		public Transform baseTypeTransform;

		public WorldObject WorldObject;

		public UnityEngine.Vector3 WorldPosition
		{
			get
			{
				_ = (NetworkObject)Target;
				if ((bool)baseTypeTransform)
				{
					return baseTypeTransform.position + Position;
				}
				return Position;
			}
		}

		public void NetworkSerialize<AutoHitData>(BufferSerializer<AutoHitData> serializer) where AutoHitData : IReaderWriter
		{
			serializer.SerializeValue(ref Target, default(FastBufferWriter.ForNetworkSerializable));
			serializer.SerializeValue(ref Position);
			if (serializer.IsReader)
			{
				NetworkObject networkObject = Target;
				if ((bool)networkObject)
				{
					WorldObject = networkObject.GetComponent<WorldObject>();
					baseTypeTransform = WorldObject.BaseTypeComponent.transform;
				}
			}
		}
	}

	private const uint PREDICTION_EXPIRE_FRAMES = 6u;

	private const uint SIMULATION_DELAY_FRAMES = 1u;

	private const float MAX_HIT_SCAN_DISTANCE = 8f;

	[SerializeField]
	private ProjectileAppearance projectileAppearancePrefab;

	[SerializeField]
	private SmallOfflinePhysicsObject magazinePrefab;

	[SerializeField]
	private SmallOfflinePhysicsObject casingPrefab;

	[Header("Collision")]
	[SerializeField]
	private SphereCollider sphereCollider;

	[Header("Projectile")]
	[SerializeField]
	[Min(0f)]
	private int damage = 1;

	[SerializeField]
	[Min(1f)]
	private float projectileSpeedMetersPerSecond = 30f;

	[SerializeField]
	[Min(1f)]
	private float projectileMaxDistanceMeters = 60f;

	[Header("Knockback")]
	[SerializeField]
	private float knockbackForce = 0.5f;

	[Header("OnHit")]
	[SerializeField]
	private List<OnHitModule> onHitModules;

	[NonSerialized]
	public bool pooled;

	private uint startFrame;

	private UnityEngine.Vector3 startPosition;

	private UnityEngine.Vector3 startAngle;

	private uint expireFrame;

	private uint deleteFrame;

	private NetworkProjectile linkedNetworkProjectile;

	private ProjectileAppearance runtimeAppearance;

	private ProjectileShooter runtimeSpawner;

	private Action<HealthChangeResult, Context> onChangedHealth;

	private static RaycastHit[] rayhitCache = new RaycastHit[12];

	private uint hitFrame;

	private bool simulate = true;

	private uint runtimeStartDelayFrames;

	private Action<Projectile> destroyedCallback;

	private bool destroyed;

	public float Speed => projectileSpeedMetersPerSecond;

	public uint StartFrame => startFrame;

	public UnityEngine.Vector3 StartPosition => startPosition;

	public UnityEngine.Vector3 StartAngle => startAngle;

	public SmallOfflinePhysicsObject MagazinePrefab => magazinePrefab;

	public SmallOfflinePhysicsObject CasingPrefab => casingPrefab;

	public WorldObject WorldObject { get; private set; }

	private uint LifetimeFrameCount => (uint)Mathf.RoundToInt(projectileMaxDistanceMeters / projectileSpeedMetersPerSecond / NetClock.FixedDeltaTime);

	public float DistanceTravelledPerFrame => projectileSpeedMetersPerSecond * NetClock.FixedDeltaTime;

	private uint LifetimeStartFrame => StartFrame + 1 + runtimeStartDelayFrames;

	private uint LifetimeEndFrame => LifetimeStartFrame + LifetimeFrameCount;

	public List<OnHitModule> OnHitModules => onHitModules;

	public MonoBehaviour Prefab { get; set; }

	private void OnEnable()
	{
		ProjectileManager.AddProjectile(this);
	}

	private void OnDisable()
	{
		ProjectileManager.RemoveProjectile(this);
		NetClock.Unregister(this);
		if (destroyedCallback != null)
		{
			destroyedCallback(this);
		}
		if (NetworkBehaviourSingleton<NetClock>.Instance.IsServer && (bool)linkedNetworkProjectile)
		{
			linkedNetworkProjectile.ServerDelayedDestroy();
			linkedNetworkProjectile = null;
		}
		runtimeAppearance = null;
		linkedNetworkProjectile = null;
	}

	private void ProjectileHit(UnityEngine.Vector3 position, uint frame)
	{
		runtimeAppearance.LifetimeEndFrame = frame;
		runtimeAppearance.SetState(frame, position, base.transform.eulerAngles, visible: true);
		hitFrame = frame;
		if (runtimeSpawner != null)
		{
			runtimeSpawner.PlayOnHitEffect(position, base.transform.rotation, (frame >= NetClock.CurrentFrame) ? 1u : (NetClock.CurrentFrame - frame + 1));
		}
	}

	private bool DamageCheck(TeamComponent teamSource)
	{
		if (runtimeSpawner.Team != Team.Neutral)
		{
			return runtimeSpawner.Team.IsHostileTo(teamSource.Team);
		}
		return true;
	}

	public void LocalInit(ProjectileShooter spawner, UnityEngine.Vector3 position, UnityEngine.Vector3 angle, uint frame, Action<HealthChangeResult, Context> callback, WorldObject worldObject, Action<Projectile> destroyedCallback, uint startDelayFrames = 0u)
	{
		runtimeSpawner = spawner;
		WorldObject = worldObject;
		onChangedHealth = callback;
		startPosition = position;
		startAngle = angle;
		startFrame = frame;
		base.transform.SetPositionAndRotation(position, Quaternion.Euler(angle));
		SpawnAppearance(position);
		NetClock.Register(this);
		runtimeStartDelayFrames = startDelayFrames;
		runtimeAppearance.RegisterSpawnInfo(frame, frame, position, angle);
		runtimeAppearance.LifetimeEndFrame = LifetimeEndFrame;
		this.destroyedCallback = destroyedCallback;
		if (NetworkManager.Singleton.IsServer)
		{
			if (ProjectileManager.PoolNetwork)
			{
				linkedNetworkProjectile = MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn(ProjectileManager.NetworkProjectilePrefab, position, base.transform.rotation);
			}
			else
			{
				linkedNetworkProjectile = UnityEngine.Object.Instantiate(ProjectileManager.NetworkProjectilePrefab);
			}
			linkedNetworkProjectile.ServerInit(position, angle, frame, spawner);
			linkedNetworkProjectile.NetworkObject.Spawn();
		}
	}

	public void LocalInitAutoHit(UnityEngine.Vector3 position, HitScanResult data, uint autoHitFrames)
	{
		SpawnAppearance(position);
		runtimeAppearance.SetupAutoHit(position, data, autoHitFrames);
		DestroySelf();
	}

	private void SpawnAppearance(UnityEngine.Vector3 position)
	{
		if (ProjectileManager.UsePooling)
		{
			runtimeAppearance = MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn(projectileAppearancePrefab, position, base.transform.rotation);
		}
		else
		{
			runtimeAppearance = UnityEngine.Object.Instantiate(projectileAppearancePrefab, position, base.transform.rotation);
		}
		runtimeAppearance.OwnerInstanceID = GetInstanceID();
		runtimeAppearance.Play();
	}

	public void LinkToNetworkProjectile(NetworkProjectile networkProjectile, bool predicted)
	{
		startPosition = networkProjectile.StartPosition;
		startAngle = networkProjectile.StartAngle;
		startFrame = networkProjectile.StartFrame;
		linkedNetworkProjectile = networkProjectile;
		linkedNetworkProjectile.runtimeGameProjectile = this;
	}

	public void ProcessDamage(NetworkObject networkObject)
	{
		HittableComponent hittableComponent = null;
		if ((object)networkObject != null)
		{
			hittableComponent = networkObject.GetComponentInChildren<HittableComponent>();
		}
		if (hittableComponent == null)
		{
			return;
		}
		HealthChangeResult arg = hittableComponent.ModifyHealth(new HealthModificationArgs(-damage, WorldObject));
		if (!NetworkManager.Singleton.IsServer)
		{
			return;
		}
		onChangedHealth?.Invoke(arg, hittableComponent.WorldObject.Context);
		if (hittableComponent.WorldObject.TryGetUserComponent<NpcEntity>(out var component))
		{
			if (component.DamageMode != DamageMode.IgnoreDamage)
			{
				component.Components.Parameters.FlinchTrigger = true;
			}
			if (component.Components.PathFollower.IsJumping && component.PhysicsMode != PhysicsMode.IgnorePhysics)
			{
				component.Components.PathFollower.StopPath(forceStop: true);
				component.Components.VelocityTracker.GetVelocity();
				component.WorldObject.TryGetNetworkObjectId(out var _);
			}
		}
	}

	public void ProcessHitModules(uint frame, WorldObject shooter, WorldObject target, UnityEngine.Vector3 position, UnityEngine.Vector3 travelDirection)
	{
		if (knockbackForce > 0f && (object)target != null && target?.GetComponentInChildren(typeof(IPhysicsTaker)) is IPhysicsTaker physicsTaker)
		{
			ulong source = 0uL;
			if ((object)shooter != null)
			{
				source = shooter.NetworkObject.NetworkObjectId;
			}
			physicsTaker.TakePhysicsForce(knockbackForce, travelDirection, frame - 1, source, forceFreeFall: true);
		}
		foreach (OnHitModule onHitModule in onHitModules)
		{
			onHitModule.Hit(frame, shooter, target, position, travelDirection);
		}
	}

	public bool HitScan(uint frames, UnityEngine.Vector3 startPos, UnityEngine.Vector3 direction, out HitScanResult result, ProjectileShooter fromSpawner = null, bool respectMaxHitScanDistance = false)
	{
		float num = DistanceTravelledPerFrame * (float)frames;
		if (respectMaxHitScanDistance)
		{
			num = Mathf.Min(num, 8f);
		}
		int num2 = Physics.SphereCastNonAlloc(startPos + sphereCollider.center, sphereCollider.radius, direction, rayhitCache, num, ProjectileManager.AttackableAndWallCollisionMask);
		NetworkObject networkObject = null;
		float num3 = float.MaxValue;
		UnityEngine.Vector3 position = UnityEngine.Vector3.zero;
		float num4 = float.MaxValue;
		bool flag = false;
		UnityEngine.Vector3 position2 = UnityEngine.Vector3.zero;
		ProjectileShooter projectileShooter = runtimeSpawner ?? fromSpawner;
		for (int i = 0; i < num2; i++)
		{
			HittableComponent hittableFromMap = MonoBehaviourSingleton<HittableMap>.Instance.GetHittableFromMap(rayhitCache[i].collider);
			if (!hittableFromMap)
			{
				if (ProjectileManager.IsWallLayer(rayhitCache[i].collider.gameObject.layer) && rayhitCache[i].distance < num4)
				{
					num4 = rayhitCache[i].distance;
					position2 = ((num4 > 0f) ? rayhitCache[i].point : (startPos + sphereCollider.center));
					flag = true;
				}
				continue;
			}
			NetworkObject networkObject2 = hittableFromMap.WorldObject.NetworkObject;
			if (((bool)hittableFromMap && (bool)projectileShooter && projectileShooter.NetworkObject == networkObject2) || !networkObject2 || !(rayhitCache[i].distance < num3))
			{
				continue;
			}
			if (hittableFromMap.WorldObject.TryGetUserComponent<TeamComponent>(out var component))
			{
				if (projectileShooter.Team.Damages(component.Team))
				{
					networkObject = networkObject2;
					num3 = rayhitCache[i].distance;
					position = ((num3 > 0f) ? rayhitCache[i].point : (startPos + sphereCollider.center));
					flag = true;
				}
			}
			else
			{
				networkObject = networkObject2;
				num3 = rayhitCache[i].distance;
				position = ((num3 > 0f) ? rayhitCache[i].point : (startPos + sphereCollider.center));
				flag = true;
			}
		}
		result = default(HitScanResult);
		if (flag)
		{
			bool flag2 = num3 < num4;
			if (flag2)
			{
				result.Target = networkObject;
				result.WorldObject = (flag2 ? networkObject.GetComponent<WorldObject>() : null);
				result.baseTypeTransform = result.WorldObject.BaseTypeComponent.transform;
				result.Position = result.baseTypeTransform.InverseTransformPoint(position);
				return true;
			}
			result.Position = position2;
			return true;
		}
		return false;
	}

	private void ServerDelayedDestroy()
	{
		Invoke("DestroySelf", 1f);
	}

	public void DestroySelf()
	{
		if (!(base.gameObject != null))
		{
			return;
		}
		if (!destroyed)
		{
			destroyed = true;
			if (ProjectileManager.UsePooling)
			{
				MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn(this);
			}
			else
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
		simulate = false;
	}

	public void OnSpawn()
	{
		base.transform.SetParent(null, worldPositionStays: true);
		simulate = true;
		destroyed = false;
		hitFrame = 0u;
	}

	public void SimulateFrameEnvironment(uint frame)
	{
		if (!simulate || frame <= LifetimeStartFrame)
		{
			return;
		}
		UnityEngine.Vector3 position = base.transform.position;
		UnityEngine.Vector3 forward = base.transform.forward;
		UnityEngine.Vector3 position2 = position + forward * DistanceTravelledPerFrame;
		if (HitScan(1u, position, forward, out var result))
		{
			NetworkObject networkObject = result.Target;
			ProjectileHit(result.WorldPosition, frame);
			if ((bool)networkObject)
			{
				ProcessDamage(networkObject);
			}
			ProcessHitModules(frame, runtimeSpawner.ShootingWorldObject, result.WorldObject, result.WorldPosition, forward);
			if (NetworkManager.Singleton.IsServer)
			{
				ServerDelayedDestroy();
			}
			simulate = false;
		}
		base.transform.position = position2;
	}

	public void Rollback(uint frame)
	{
		if (frame > LifetimeStartFrame && frame <= LifetimeEndFrame)
		{
			base.transform.position = startPosition + base.transform.forward * ((float)(frame - LifetimeStartFrame) * DistanceTravelledPerFrame);
		}
		else if (frame <= LifetimeStartFrame)
		{
			base.transform.position = startPosition;
		}
	}

	public void PostFixedUpdate(uint frame)
	{
		if (!NetworkManager.Singleton.IsServer && linkedNetworkProjectile == null && frame > startFrame + 6)
		{
			if ((bool)runtimeSpawner)
			{
				runtimeSpawner.ProjectileMispredicted(this);
			}
			DestroySelf();
			return;
		}
		if (runtimeAppearance != null && runtimeAppearance.OwnerInstanceID == GetInstanceID() && frame != hitFrame)
		{
			runtimeAppearance.SetState(frame, base.transform.position, base.transform.eulerAngles, simulate && frame >= LifetimeStartFrame);
		}
		if (simulate && frame > LifetimeEndFrame)
		{
			if (NetworkBehaviourSingleton<NetClock>.Instance.IsServer)
			{
				DestroySelf();
			}
			else
			{
				simulate = false;
			}
		}
	}
}
