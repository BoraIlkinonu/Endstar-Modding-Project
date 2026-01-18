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

namespace Endless.Gameplay;

public class ProjectileShooter : NetworkBehaviour, IGameEndSubscriber
{
	private const uint AUTO_HIT_SCAN_AHEAD_FRAMES = 1u;

	private const float SERVER_AUTO_HIT_DISTANCE_LENIENCE = 3.5f;

	[SerializeField]
	[HideInInspector]
	private WorldObject _worldObject;

	[Header("Team")]
	[SerializeField]
	private Team baseTeam;

	[Header("Simulation Delay")]
	[SerializeField]
	private uint simulationDelayFrames;

	[SerializeField]
	private ParticleSystem muzzleFlashEffectInstance;

	[SerializeField]
	private ParticleSystem ejectionEffectInstance;

	[Header("Player Character Equipment Only")]
	[SerializeField]
	private RangedWeaponItem associatedItem;

	private List<Projectile> predictedProjectiles = new List<Projectile>();

	private int magHash;

	private int casingHash;

	[SerializeReference]
	private ProjectileShooterEjectionSettings ejectionSettings;

	[field: SerializeField]
	public Projectile ProjectilePrefab { get; private set; }

	[field: SerializeField]
	public Transform FirePoint { get; private set; }

	[field: SerializeField]
	public ParticleSystem MuzzleFlashEffect { get; private set; }

	[field: SerializeField]
	public ParticleSystem EjectionEffect { get; private set; }

	[field: SerializeField]
	public ParticleSystem HitEffect { get; private set; }

	[field: SerializeField]
	public HitEffect HitEffectPrefab { get; private set; }

	[field: SerializeField]
	public Transform EjectionEffectPoint { get; private set; }

	[field: SerializeField]
	public Transform Magazine { get; private set; }

	[field: SerializeField]
	public Transform MagazineEjectionPoint { get; private set; }

	public WorldObject ShootingWorldObject
	{
		get
		{
			if (!associatedItem)
			{
				return _worldObject;
			}
			return associatedItem.Carrier.WorldObject;
		}
	}

	public WorldObject WorldObject
	{
		get
		{
			if (_worldObject == null)
			{
				_worldObject = GetComponentInParent<WorldObject>();
			}
			return _worldObject;
		}
	}

	public Transform ShootPointTransform
	{
		get
		{
			if (!FirePoint)
			{
				return base.transform;
			}
			return FirePoint;
		}
	}

	public Team Team
	{
		get
		{
			if (!WorldObject)
			{
				return baseTeam;
			}
			if (!WorldObject.TryGetUserComponent<TeamComponent>(out var component))
			{
				return baseTeam;
			}
			return component.Team;
		}
	}

	public void SetupProjectileShooterReferences(ProjectileShooterReferences references)
	{
		SetupProjectileShooterReferences(references.FirePoint, references.EjectionEffectPoint, references.Magazine, null, null);
	}

	public void SetupProjectileShooterReferences(Transform firePoint, Transform ejectionPoint, Transform magazine, Transform magazineEjectionPoint, ProjectileShooterEjectionSettings ejectionSettings)
	{
		FirePoint = firePoint;
		EjectionEffectPoint = ejectionPoint;
		Magazine = magazine;
		MagazineEjectionPoint = ((magazineEjectionPoint != null) ? MagazineEjectionPoint : magazine);
		this.ejectionSettings = ((ejectionSettings != null) ? ejectionSettings : new ProjectileShooterEjectionSettings());
		if (HitEffectPrefab == null)
		{
			Debug.LogWarning("Please update and assign HitEffectPrefab instead of HitEffect on " + base.name);
		}
	}

	public void ClientAuthoritativeShoot(UnityEngine.Vector3 position, UnityEngine.Vector3 angle, uint frame, Action<HealthChangeResult, Context> callback = null, WorldObject worldObject = null)
	{
		if (associatedItem == null || associatedItem.Carrier.OwnerClientId != base.NetworkManager.LocalClientId)
		{
			return;
		}
		UnityEngine.Vector3 vector = Quaternion.Euler(angle) * UnityEngine.Vector3.forward;
		if (ProjectilePrefab.HitScan(1u, position, vector, out var result, this, respectMaxHitScanDistance: true))
		{
			ClientAuthoritativeAutoHit_ServerRpc(result, frame, vector);
			PlayShootEffects();
			VisualizeAutoHit(result);
			UnityEngine.Vector3 vector2 = position - result.WorldPosition;
			uint autoHitFrames = GetAutoHitFrames(vector2.sqrMagnitude);
			PlayOnHitEffect(result.WorldPosition, Quaternion.LookRotation(vector2.normalized), autoHitFrames);
			NetworkObject networkObject = result.Target;
			if ((bool)networkObject)
			{
				ProjectilePrefab.ProcessDamage(networkObject);
			}
			ProjectilePrefab.ProcessHitModules(frame, ShootingWorldObject, result.WorldObject, result.WorldPosition, vector);
		}
		else
		{
			if (!base.NetworkManager.IsServer)
			{
				ShootProjectileLocal(position, angle, frame, callback, worldObject);
			}
			ClientAuthoritativeShoot_ServerRpc(position, angle, frame);
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void ClientAuthoritativeShoot_ServerRpc(UnityEngine.Vector3 position, UnityEngine.Vector3 angle, uint frame, ServerRpcParams serverRpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendServerRpc(586193946u, serverRpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in position);
			bufferWriter.WriteValueSafe(in angle);
			BytePacker.WriteValueBitPacked(bufferWriter, frame);
			__endSendServerRpc(ref bufferWriter, 586193946u, serverRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		if (!(associatedItem == null) && associatedItem.Carrier.OwnerClientId == serverRpcParams.Receive.SenderClientId)
		{
			if (associatedItem.AmmoCount > 0)
			{
				associatedItem.AmmoCount = Mathf.Max(associatedItem.AmmoCount - 1, 0);
			}
			ShootProjectileLocal(position, angle, frame, null, associatedItem.Carrier.WorldObject);
		}
	}

	[ServerRpc(RequireOwnership = false)]
	private void ClientAuthoritativeAutoHit_ServerRpc(Projectile.HitScanResult data, uint frame, UnityEngine.Vector3 angle, ServerRpcParams serverRpcParams = default(ServerRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendServerRpc(3555919450u, serverRpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in data, default(FastBufferWriter.ForNetworkSerializable));
			BytePacker.WriteValueBitPacked(bufferWriter, frame);
			bufferWriter.WriteValueSafe(in angle);
			__endSendServerRpc(ref bufferWriter, 3555919450u, serverRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		if (!(associatedItem == null) && associatedItem.Carrier.OwnerClientId == serverRpcParams.Receive.SenderClientId)
		{
			if (associatedItem.AmmoCount > 0)
			{
				associatedItem.AmmoCount = Mathf.Max(associatedItem.AmmoCount - 1, 0);
			}
			NetworkObject networkObject = data.Target;
			if ((bool)networkObject && (bool)networkObject && 3.5f + 1f * ProjectilePrefab.DistanceTravelledPerFrame > UnityEngine.Vector3.Distance(associatedItem.Carrier.transform.position, data.WorldPosition))
			{
				ProjectilePrefab.ProcessDamage(networkObject);
			}
			ProjectilePrefab.ProcessHitModules(frame, ShootingWorldObject, data.WorldObject, data.WorldPosition, angle);
			PlayAutoHitEvent_ClientRpc(data, frame);
		}
	}

	public void UpdateBaseTeam(Team team)
	{
		baseTeam = team;
	}

	public void GotNetworkProjectile(NetworkProjectile networkProjectile)
	{
		Projectile associatedProjectile = null;
		predictedProjectiles.RemoveAll(delegate(Projectile predicted)
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
			associatedProjectile = SpawnProjectileObject(networkProjectile.StartPosition, networkProjectile.StartAngle, networkProjectile.StartFrame);
			associatedProjectile.LinkToNetworkProjectile(networkProjectile, predicted: false);
		}
		else
		{
			associatedProjectile.LinkToNetworkProjectile(networkProjectile, predicted: true);
		}
	}

	private void VisualizeAutoHit(Projectile.HitScanResult autohitData)
	{
		UnityEngine.Vector3 vector = (FirePoint ? FirePoint.position : base.transform.position);
		Projectile projectile = ((!ProjectileManager.UsePooling) ? UnityEngine.Object.Instantiate(ProjectilePrefab, vector, Quaternion.LookRotation(autohitData.WorldPosition - vector, UnityEngine.Vector3.up)) : MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn(ProjectilePrefab, vector, Quaternion.LookRotation(autohitData.WorldPosition - vector, UnityEngine.Vector3.up)));
		projectile.LocalInitAutoHit(vector, autohitData, GetAutoHitFrames((autohitData.WorldPosition - vector).sqrMagnitude));
	}

	public void ShootProjectileLocal(UnityEngine.Vector3 position, UnityEngine.Vector3 angle, uint frame, Action<HealthChangeResult, Context> callback = null, WorldObject worldObject = null)
	{
		if (worldObject == null)
		{
			worldObject = WorldObject;
		}
		Projectile item = SpawnProjectileObject(position, angle, frame, callback, worldObject);
		if (!base.NetworkManager.IsServer)
		{
			predictedProjectiles.Add(item);
			PlayLocalShootEvent(frame);
		}
		else
		{
			PlayShootEvent_ClientRpc(frame);
		}
	}

	private Projectile SpawnProjectileObject(UnityEngine.Vector3 position, UnityEngine.Vector3 angle, uint frame, Action<HealthChangeResult, Context> callback = null, WorldObject worldObject = null)
	{
		Projectile projectile = ((!ProjectileManager.UsePooling) ? UnityEngine.Object.Instantiate(ProjectilePrefab, position, Quaternion.Euler(angle)) : MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn(ProjectilePrefab, position, Quaternion.Euler(angle)));
		projectile.LocalInit(this, position, angle, frame, callback, worldObject, null, simulationDelayFrames);
		return projectile;
	}

	public void ProjectileMispredicted(Projectile projectile)
	{
		predictedProjectiles.Remove(projectile);
	}

	public void PlayLocalShootEvent(uint frame)
	{
		uint currentFrame = NetClock.CurrentFrame;
		UpdateShootingStateOnFire(frame);
		if (frame > currentFrame)
		{
			Invoke("PlayShootEffects", (float)(frame - currentFrame) * NetClock.FixedDeltaTime + NetClock.FixedDeltaTime * 1.1f);
		}
		else
		{
			PlayShootEffects();
		}
	}

	private void PlayShootEffects()
	{
		if (muzzleFlashEffectInstance == null && MuzzleFlashEffect != null && FirePoint != null)
		{
			muzzleFlashEffectInstance = UnityEngine.Object.Instantiate(MuzzleFlashEffect, FirePoint.position, FirePoint.rotation, FirePoint);
		}
		if (muzzleFlashEffectInstance != null)
		{
			muzzleFlashEffectInstance.Play();
		}
		if (ejectionEffectInstance == null && EjectionEffect != null && EjectionEffectPoint != null)
		{
			ejectionEffectInstance = UnityEngine.Object.Instantiate(EjectionEffect, EjectionEffectPoint.position, EjectionEffectPoint.rotation, EjectionEffectPoint);
		}
		if (ejectionEffectInstance != null)
		{
			ejectionEffectInstance.Play();
		}
		if (ProjectilePrefab.CasingPrefab != null && EjectionEffectPoint != null)
		{
			if (casingHash == 0)
			{
				casingHash = MakeHash(ProjectilePrefab.CasingPrefab.name);
			}
			SmallOfflinePhysicsObject smallOfflinePhysicsObject = SmallOfflinePhysicsObjectManager.Spawn(casingHash, ProjectilePrefab.CasingPrefab, EjectionEffectPoint.position, ProjectilePrefab.CasingPrefab.transform.rotation, ProjectilePrefab.CasingPrefab.transform.localScale, SmallOfflinePhysicsObjectManager.Transform);
			if (smallOfflinePhysicsObject != null)
			{
				float value = UnityEngine.Random.value;
				float num = Mathf.Sqrt(1f - value * value);
				float num2 = UnityEngine.Random.Range(0f - ejectionSettings.casingAngleVariance, ejectionSettings.casingAngleVariance);
				Quaternion quaternion = EjectionEffectPoint.rotation * Quaternion.Euler(value * num2, num * num2, 0f);
				smallOfflinePhysicsObject.transform.rotation = Quaternion.FromToRotation(EjectionEffectPoint.forward, quaternion * UnityEngine.Vector3.forward) * ProjectilePrefab.CasingPrefab.transform.rotation;
				smallOfflinePhysicsObject.Rigidbody.velocity = quaternion * UnityEngine.Vector3.forward * UnityEngine.Random.Range(ejectionSettings.casingForce.x, ejectionSettings.casingForce.y);
				UnityEngine.Vector3 vector = new UnityEngine.Vector3(UnityEngine.Random.Range(ejectionSettings.casingAngularVelocityVarianceX.x, ejectionSettings.casingAngularVelocityVarianceX.y), UnityEngine.Random.Range(ejectionSettings.casingAngularVelocityVarianceY.x, ejectionSettings.casingAngularVelocityVarianceY.y), UnityEngine.Random.Range(ejectionSettings.casingAngularVelocityVarianceZ.x, ejectionSettings.casingAngularVelocityVarianceZ.y)) * (MathF.PI / 180f);
				float magnitude = vector.magnitude;
				smallOfflinePhysicsObject.Rigidbody.maxAngularVelocity = magnitude;
				smallOfflinePhysicsObject.Rigidbody.angularVelocity = EjectionEffectPoint.TransformDirection(vector.normalized) * magnitude;
				if (ejectionSettings.casingOverrideDrag)
				{
					smallOfflinePhysicsObject.Rigidbody.drag = ejectionSettings.casingDrag;
					smallOfflinePhysicsObject.Rigidbody.angularDrag = ejectionSettings.casingAngularDrag;
				}
			}
		}
		if (associatedItem != null)
		{
			associatedItem.ShotAnimTriggered();
		}
	}

	public void SpawnMagazine()
	{
		if (!(ProjectilePrefab.MagazinePrefab != null))
		{
			return;
		}
		if (magHash == 0)
		{
			magHash = MakeHash(ProjectilePrefab.MagazinePrefab.name);
		}
		Transform transform = ((MagazineEjectionPoint != null) ? MagazineEjectionPoint : Magazine);
		SmallOfflinePhysicsObject smallOfflinePhysicsObject = SmallOfflinePhysicsObjectManager.Spawn(magHash, ProjectilePrefab.MagazinePrefab, transform.position, transform.rotation, transform.localScale, SmallOfflinePhysicsObjectManager.Transform);
		if (smallOfflinePhysicsObject != null)
		{
			float value = UnityEngine.Random.value;
			float num = Mathf.Sqrt(1f - value * value);
			float num2 = UnityEngine.Random.Range(0f - ejectionSettings.magAngleVariance, ejectionSettings.magAngleVariance);
			Quaternion quaternion = transform.rotation * Quaternion.Euler(value * num2, num * num2, 0f);
			smallOfflinePhysicsObject.transform.rotation = Quaternion.FromToRotation(transform.forward, quaternion * UnityEngine.Vector3.forward) * ProjectilePrefab.MagazinePrefab.transform.rotation;
			smallOfflinePhysicsObject.Rigidbody.velocity = quaternion * UnityEngine.Vector3.forward * UnityEngine.Random.Range(ejectionSettings.magForce.x, ejectionSettings.magForce.y);
			UnityEngine.Vector3 vector = new UnityEngine.Vector3(UnityEngine.Random.Range(ejectionSettings.magAngularVelocityVarianceX.x, ejectionSettings.magAngularVelocityVarianceX.y), UnityEngine.Random.Range(ejectionSettings.magAngularVelocityVarianceY.x, ejectionSettings.magAngularVelocityVarianceY.y), UnityEngine.Random.Range(ejectionSettings.magAngularVelocityVarianceZ.x, ejectionSettings.magAngularVelocityVarianceZ.y)) * (MathF.PI / 180f);
			float magnitude = vector.magnitude;
			smallOfflinePhysicsObject.Rigidbody.maxAngularVelocity = magnitude;
			smallOfflinePhysicsObject.Rigidbody.angularVelocity = transform.TransformDirection(vector.normalized) * magnitude;
			if (ejectionSettings.magOverrideDrag)
			{
				smallOfflinePhysicsObject.Rigidbody.drag = ejectionSettings.magDrag;
				smallOfflinePhysicsObject.Rigidbody.angularDrag = ejectionSettings.magAngularDrag;
			}
		}
	}

	[ClientRpc]
	private void PlayShootEvent_ClientRpc(uint frame)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(3569284547u, clientRpcParams, RpcDelivery.Reliable);
			BytePacker.WriteValueBitPacked(bufferWriter, frame);
			__endSendClientRpc(ref bufferWriter, 3569284547u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (base.NetworkManager.IsServer || !(associatedItem != null) || !(associatedItem.Carrier != null) || !associatedItem.Carrier.IsOwner)
			{
				PlayLocalShootEvent(frame);
			}
		}
	}

	[ClientRpc]
	private void PlayAutoHitEvent_ClientRpc(Projectile.HitScanResult data, uint frame)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(110267456u, clientRpcParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in data, default(FastBufferWriter.ForNetworkSerializable));
			BytePacker.WriteValueBitPacked(bufferWriter, frame);
			__endSendClientRpc(ref bufferWriter, 110267456u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (!(associatedItem != null) || !(associatedItem.Carrier != null) || !associatedItem.Carrier.IsOwner)
			{
				PlayShootEffects();
				UnityEngine.Vector3 vector = base.transform.position - data.WorldPosition;
				UnityEngine.Vector3 vector2 = (FirePoint ? FirePoint.position : base.transform.position);
				uint autoHitFrames = GetAutoHitFrames((data.WorldPosition - vector2).sqrMagnitude);
				PlayOnHitEffect(data.WorldPosition, Quaternion.LookRotation(vector.normalized), autoHitFrames);
				VisualizeAutoHit(data);
			}
		}
	}

	public void AISetShooterReferences(AIProjectileShooterReferencePointer references)
	{
		FirePoint = references.FirePoint;
		MuzzleFlashEffect = references.MuzzleFlashEffect;
		EjectionEffect = references.EjectionEffect;
		HitEffect = references.HitEffect;
		if (references.HitEffectPrefab != null)
		{
			HitEffectPrefab = references.HitEffectPrefab;
		}
		else
		{
			HitEffectPrefab = HitEffect.GetComponent<HitEffect>();
		}
	}

	public void EndlessGameEnd()
	{
		foreach (Projectile item in new List<Projectile>(predictedProjectiles))
		{
			item.DestroySelf();
		}
		predictedProjectiles.Clear();
	}

	public void PlayOnHitEffect(UnityEngine.Vector3 position, Quaternion rotation, uint frameDelay = 0u)
	{
		if (frameDelay != 0)
		{
			StartCoroutine(DelayHitEffect(position, rotation, (float)frameDelay * NetClock.FixedDeltaTime));
		}
		else if ((bool)HitEffectPrefab)
		{
			HitEffect hitEffect;
			if (ProjectileManager.UsePooling)
			{
				hitEffect = MonoBehaviourSingleton<PoolManagerT>.Instance.Spawn(HitEffectPrefab, position, rotation);
				ParticleSystem.MainModule main = hitEffect.Particles.main;
				main.stopAction = ParticleSystemStopAction.Callback;
			}
			else
			{
				hitEffect = UnityEngine.Object.Instantiate(HitEffectPrefab, position, rotation);
				ParticleSystem.MainModule main2 = hitEffect.Particles.main;
				main2.stopAction = ParticleSystemStopAction.Destroy;
			}
			hitEffect.PlayEffect();
		}
		else
		{
			ParticleSystem particleSystem = UnityEngine.Object.Instantiate(HitEffect, position, rotation);
			ParticleSystem.MainModule main3 = particleSystem.main;
			main3.stopAction = ParticleSystemStopAction.Destroy;
			particleSystem.Play();
		}
	}

	public uint GetAutoHitFrames(float squaredDistance)
	{
		float distanceTravelledPerFrame = ProjectilePrefab.DistanceTravelledPerFrame;
		return (uint)Mathf.RoundToInt(squaredDistance / (distanceTravelledPerFrame * distanceTravelledPerFrame) + 1f);
	}

	private IEnumerator DelayHitEffect(UnityEngine.Vector3 position, Quaternion rotation, float time)
	{
		yield return new WaitForSeconds(time);
		PlayOnHitEffect(position, rotation);
	}

	private void UpdateShootingStateOnFire(uint frame)
	{
		if (!(associatedItem == null) && !associatedItem.IsLocal())
		{
			ShootingState state = associatedItem.GetShootingState(frame);
			if (frame > state.lastShotFrame)
			{
				state.lastShotFrame = frame;
				state.waitingForShot = false;
				state.NetFrame = frame;
				associatedItem.ShootingStateUpdated(frame, ref state);
			}
		}
	}

	private int MakeHash(string prefabName)
	{
		if (associatedItem != null)
		{
			return Animator.StringToHash(associatedItem.InventoryUsableDefinition.DisplayName + "." + prefabName);
		}
		SerializableGuid instanceIdFromGameObject = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetInstanceIdFromGameObject(WorldObject.gameObject);
		SerializableGuid assetIdFromInstanceId = MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetAssetIdFromInstanceId(instanceIdFromGameObject);
		PropLibrary.RuntimePropInfo metadata;
		Prop prop = (MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(assetIdFromInstanceId, out metadata) ? metadata.PropData : null);
		if (!string.IsNullOrEmpty(prop?.Name))
		{
			return Animator.StringToHash(prop.Name + "." + prefabName);
		}
		Debug.LogWarning(string.Format("{0}: couldn't get prop or prop name empty on {1}. prop null? {2}", "MakeHash", base.name, prop == null));
		Debug.LogWarning("MakeHash: No ranged item and no prop found, using instance name " + base.name + ".");
		return Animator.StringToHash(base.name + "." + prefabName);
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(586193946u, __rpc_handler_586193946, "ClientAuthoritativeShoot_ServerRpc");
		__registerRpc(3555919450u, __rpc_handler_3555919450, "ClientAuthoritativeAutoHit_ServerRpc");
		__registerRpc(3569284547u, __rpc_handler_3569284547, "PlayShootEvent_ClientRpc");
		__registerRpc(110267456u, __rpc_handler_110267456, "PlayAutoHitEvent_ClientRpc");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_586193946(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out UnityEngine.Vector3 value);
			reader.ReadValueSafe(out UnityEngine.Vector3 value2);
			ByteUnpacker.ReadValueBitPacked(reader, out uint value3);
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((ProjectileShooter)target).ClientAuthoritativeShoot_ServerRpc(value, value2, value3, server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_3555919450(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out Projectile.HitScanResult value, default(FastBufferWriter.ForNetworkSerializable));
			ByteUnpacker.ReadValueBitPacked(reader, out uint value2);
			reader.ReadValueSafe(out UnityEngine.Vector3 value3);
			ServerRpcParams server = rpcParams.Server;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((ProjectileShooter)target).ClientAuthoritativeAutoHit_ServerRpc(value, value2, value3, server);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_3569284547(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ByteUnpacker.ReadValueBitPacked(reader, out uint value);
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((ProjectileShooter)target).PlayShootEvent_ClientRpc(value);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_110267456(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out Projectile.HitScanResult value, default(FastBufferWriter.ForNetworkSerializable));
			ByteUnpacker.ReadValueBitPacked(reader, out uint value2);
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((ProjectileShooter)target).PlayAutoHitEvent_ClientRpc(value, value2);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "ProjectileShooter";
	}
}
