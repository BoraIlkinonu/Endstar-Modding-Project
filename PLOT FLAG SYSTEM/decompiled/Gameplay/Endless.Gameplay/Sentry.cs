using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Props.ReferenceComponents;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class Sentry : EndlessNetworkBehaviour, NetClock.ISimulateFrameLateSubscriber, IStartSubscriber, IGameEndSubscriber, IBaseType, IComponentBase, IScriptInjector
{
	public enum TrackingTargetType : byte
	{
		Direction,
		Target
	}

	public struct SentryState : INetworkSerializable, IFrameInfo
	{
		public TrackingTargetType TrackingStatus;

		public NetworkObjectReference TrackingTarget;

		public float TrackingTargetPitch;

		public float TrackingTargetYaw;

		public float RotationSpeed;

		public float ShootDistance;

		public bool TrackingLaserEnabled;

		public float CurrentPitch;

		public float CurrentYaw;

		public uint ShootFrame;

		public WorldObject TrackingTargetWorldObject;

		public uint NetFrame { get; set; }

		void IFrameInfo.Clear()
		{
		}

		void IFrameInfo.Initialize()
		{
		}

		public SentryState(NetworkObject target, uint frame, float currentPitch, float currentYaw, uint shootFrame, float rotationSpeed, float shootDistance, bool trackingLaser)
		{
			TrackingStatus = TrackingTargetType.Target;
			TrackingTarget = new NetworkObjectReference(target);
			NetFrame = frame;
			CurrentPitch = currentPitch;
			CurrentYaw = currentYaw;
			ShootFrame = shootFrame;
			TrackingTargetPitch = 0f;
			TrackingTargetYaw = 0f;
			RotationSpeed = rotationSpeed;
			TrackingTargetWorldObject = null;
			ShootDistance = shootDistance;
			TrackingLaserEnabled = trackingLaser;
		}

		public SentryState(float trackingTargetPitch, float trackingTargetYaw, uint frame, float currentPitch, float currentYaw, uint shootFrame, float rotationSpeed, float shootDistance, bool trackingLaser)
		{
			TrackingStatus = TrackingTargetType.Direction;
			TrackingTarget = default(NetworkObjectReference);
			TrackingTargetPitch = trackingTargetPitch;
			TrackingTargetYaw = trackingTargetYaw;
			CurrentPitch = currentPitch;
			CurrentYaw = currentYaw;
			ShootFrame = shootFrame;
			NetFrame = frame;
			RotationSpeed = rotationSpeed;
			TrackingTargetWorldObject = null;
			ShootDistance = shootDistance;
			TrackingLaserEnabled = trackingLaser;
		}

		public void NetworkSerialize<SentryState>(BufferSerializer<SentryState> serializer) where SentryState : IReaderWriter
		{
			serializer.SerializeValue(ref TrackingTarget, default(FastBufferWriter.ForNetworkSerializable));
			serializer.SerializeValue(ref TrackingStatus, default(FastBufferWriter.ForEnums));
			if (serializer.IsWriter)
			{
				Compression.SerializeFloatToUShort(serializer, TrackingTargetPitch, -180f, 180f);
				Compression.SerializeFloatToUShort(serializer, TrackingTargetYaw, -180f, 180f);
				Compression.SerializeUInt(serializer, NetFrame);
				Compression.SerializeUIntToByteClamped(serializer, ShootFrame);
				Compression.SerializeFloatToUShort(serializer, CurrentPitch, -180f, 180f);
				Compression.SerializeFloatToUShort(serializer, CurrentYaw, -180f, 180f);
				Compression.SerializeFloatToUShort(serializer, RotationSpeed, 0f, 500f);
				Compression.SerializeFloatToUShort(serializer, ShootDistance, 0f, 200f);
				serializer.SerializeValue(ref TrackingLaserEnabled, default(FastBufferWriter.ForPrimitives));
			}
			else
			{
				TrackingTargetPitch = Compression.DeserializeFloatFromUShort(serializer, -180f, 180f);
				TrackingTargetYaw = Compression.DeserializeFloatFromUShort(serializer, -180f, 180f);
				NetFrame = Compression.DeserializeUInt(serializer);
				ShootFrame = Compression.DeserializeUIntFromByteClamped(serializer);
				CurrentPitch = Compression.DeserializeFloatFromUShort(serializer, -180f, 180f);
				CurrentYaw = Compression.DeserializeFloatFromUShort(serializer, -180f, 180f);
				RotationSpeed = Compression.DeserializeFloatFromUShort(serializer, 0f, 500f);
				ShootDistance = Compression.DeserializeFloatFromUShort(serializer, 0f, 200f);
				serializer.SerializeValue(ref TrackingLaserEnabled, default(FastBufferWriter.ForPrimitives));
			}
		}
	}

	private static RaycastHit[] rayHitCache = new RaycastHit[15];

	[SerializeField]
	private LayerMask hittableLayerMask;

	[SerializeField]
	private LayerMask wallOnlyLayerMask;

	[SerializeField]
	private LayerMask hittableAndWallLayerMask;

	private readonly NetworkVariable<SentryState> networkState = new NetworkVariable<SentryState>();

	internal readonly NetworkVariable<int> damageLevel = new NetworkVariable<int>(0);

	private uint lastLocalFrame;

	internal SentryState localState;

	[SerializeField]
	private SentryAppearanceController sentryAppearanceController;

	private uint lastLocalShot;

	private List<RaycastHit> sortedRayHitList = new List<RaycastHit>();

	public EndlessEvent OnFacingMainTarget;

	public EndlessEvent OnFacingValidTarget;

	public EndlessEvent<Context> OnShotScanResult;

	public EndlessEvent<Context> OnPriorityTargetSet;

	internal float RotationSpeed = 100f;

	internal float ShootDistance = 6f;

	private bool playmode;

	[SerializeField]
	[HideInInspector]
	private SentryReferences references;

	private Context context;

	private Endless.Gameplay.LuaInterfaces.Sentry luaInterface;

	private EndlessScriptComponent scriptComponent;

	public Context Context => context ?? (context = new Context(WorldObject));

	[field: SerializeField]
	[field: HideInInspector]
	public WorldObject WorldObject { get; private set; }

	public Type ComponentReferenceType => typeof(SentryReferences);

	public object LuaObject
	{
		get
		{
			if (luaInterface == null)
			{
				luaInterface = new Endless.Gameplay.LuaInterfaces.Sentry(this);
			}
			return luaInterface;
		}
	}

	public Type LuaObjectType => typeof(Endless.Gameplay.LuaInterfaces.Sentry);

	private void OnEnable()
	{
		NetClock.Register(this);
		NetworkVariable<SentryState> networkVariable = networkState;
		networkVariable.OnValueChanged = (NetworkVariable<SentryState>.OnValueChangedDelegate)Delegate.Combine(networkVariable.OnValueChanged, new NetworkVariable<SentryState>.OnValueChangedDelegate(HandleNetworkStateChanged));
		NetworkVariable<int> networkVariable2 = damageLevel;
		networkVariable2.OnValueChanged = (NetworkVariable<int>.OnValueChangedDelegate)Delegate.Combine(networkVariable2.OnValueChanged, new NetworkVariable<int>.OnValueChangedDelegate(HandleDamageLevelChanged));
	}

	private void OnDisable()
	{
		NetClock.Unregister(this);
		NetworkVariable<SentryState> networkVariable = networkState;
		networkVariable.OnValueChanged = (NetworkVariable<SentryState>.OnValueChangedDelegate)Delegate.Remove(networkVariable.OnValueChanged, new NetworkVariable<SentryState>.OnValueChangedDelegate(HandleNetworkStateChanged));
		NetworkVariable<int> networkVariable2 = damageLevel;
		networkVariable2.OnValueChanged = (NetworkVariable<int>.OnValueChangedDelegate)Delegate.Remove(networkVariable2.OnValueChanged, new NetworkVariable<int>.OnValueChangedDelegate(HandleDamageLevelChanged));
	}

	private void HandleNetworkStateChanged(SentryState oldState, SentryState newState)
	{
		localState = newState;
	}

	private void HandleDamageLevelChanged(int oldState, int newState)
	{
		ToggleParticleSystem(references.SlightlyDamagedParticle.RuntimeParticleSystem, newState == 1);
		ToggleParticleSystem(references.CriticallyDamagedPartical.RuntimeParticleSystem, newState == 2);
		ToggleParticleSystem(references.DestroyedParticle.RuntimeParticleSystem, newState == 3);
	}

	private void FrameBehaviour(uint frame)
	{
		lastLocalFrame = frame;
		localState.RotationSpeed = RotationSpeed;
		float target = localState.TrackingTargetPitch;
		float target2 = localState.TrackingTargetYaw;
		if (localState.TrackingStatus == TrackingTargetType.Target && localState.TrackingTarget.TryGet(out var networkObject))
		{
			UnityEngine.Vector3 eulerAngles = Quaternion.LookRotation(networkObject.GetComponent<WorldObject>().GetUserComponent<HittableComponent>().HittableColliders[0].bounds.center - references.SwivelTransform.position, UnityEngine.Vector3.up).eulerAngles;
			target = eulerAngles.x;
			target2 = eulerAngles.y;
		}
		float maxDelta = localState.RotationSpeed * NetClock.FixedDeltaTime;
		localState.CurrentPitch = Mathf.MoveTowardsAngle(localState.CurrentPitch, target, maxDelta);
		localState.CurrentYaw = Mathf.MoveTowardsAngle(localState.CurrentYaw, target2, maxDelta);
		sentryAppearanceController.SetState(localState);
		UnityEngine.Vector3 vector = Quaternion.Euler(new UnityEngine.Vector3(localState.CurrentPitch, localState.CurrentYaw)) * UnityEngine.Vector3.forward;
		if (base.IsServer && localState.TrackingStatus == TrackingTargetType.Target)
		{
			int num = RaycastNonAlloc(references.SwivelTransform.position, vector, ShootDistance, hittableLayerMask);
			for (int i = 0; i < num; i++)
			{
				HittableComponent hittableFromMap = MonoBehaviourSingleton<HittableMap>.Instance.GetHittableFromMap(rayHitCache[i].collider);
				if ((bool)hittableFromMap && hittableFromMap == localState.TrackingTargetWorldObject.GetUserComponent<HittableComponent>())
				{
					scriptComponent.ExecuteFunction("OnTargetInHitScan", localState.TrackingTargetWorldObject.Context);
					break;
				}
			}
		}
		if (!base.IsServer || localState.ShootFrame != frame)
		{
			return;
		}
		int num2 = RaycastNonAlloc(references.LaserPoint.position, vector, ShootDistance, hittableAndWallLayerMask);
		Sort(rayHitCache, num2);
		for (int j = 0; j < num2; j++)
		{
			HittableComponent hittableFromMap2 = MonoBehaviourSingleton<HittableMap>.Instance.GetHittableFromMap(sortedRayHitList[j].collider);
			if (!hittableFromMap2)
			{
				break;
			}
			if (!(hittableFromMap2.healthComponent != null) || hittableFromMap2.healthComponent.CurrentHealth >= 1)
			{
				scriptComponent.ExecuteFunction("OnHit", hittableFromMap2.WorldObject.Context);
				break;
			}
		}
		ShootClientRpc(vector);
	}

	[ClientRpc]
	private void ShootClientRpc(UnityEngine.Vector3 lookRotation)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams = default(ClientRpcParams);
				FastBufferWriter bufferWriter = __beginSendClientRpc(1692595414u, clientRpcParams, RpcDelivery.Reliable);
				bufferWriter.WriteValueSafe(in lookRotation);
				__endSendClientRpc(ref bufferWriter, 1692595414u, clientRpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				sentryAppearanceController.PlayShootVisuals(lookRotation, localState.ShootDistance, localState.TrackingTarget);
			}
		}
	}

	private int RaycastNonAlloc(UnityEngine.Vector3 position, UnityEngine.Vector3 direction, float distance, LayerMask layerMask)
	{
		return Physics.RaycastNonAlloc(position, direction, rayHitCache, distance, layerMask, QueryTriggerInteraction.Ignore);
	}

	private void Sort(RaycastHit[] array, int count)
	{
		sortedRayHitList.Clear();
		for (int i = 0; i < count; i++)
		{
			sortedRayHitList.Add(array[i]);
		}
		sortedRayHitList = sortedRayHitList.OrderBy((RaycastHit hit) => hit.distance).ToList();
	}

	internal void SyncState()
	{
		if (base.IsServer)
		{
			networkState.Value = localState;
		}
	}

	public void SimulateFrameLate(uint frame)
	{
		if (playmode)
		{
			if (frame == NetClock.CurrentFrame)
			{
				FrameBehaviour(frame);
			}
			return;
		}
		localState.TrackingStatus = TrackingTargetType.Direction;
		UnityEngine.Vector3 vector = (WorldObject ? WorldObject.transform.eulerAngles : UnityEngine.Vector3.zero);
		localState.TrackingTargetPitch = vector.x;
		localState.TrackingTargetYaw = vector.y;
		localState.CurrentPitch = localState.TrackingTargetPitch;
		localState.CurrentYaw = localState.TrackingTargetYaw;
		sentryAppearanceController.SetState(localState);
	}

	internal void SetFollowTarget(Context context)
	{
		SetFollowTarget(context.WorldObject);
	}

	private void SetFollowTarget(WorldObject worldObject)
	{
		if (worldObject.NetworkObject != null)
		{
			localState.TrackingStatus = TrackingTargetType.Target;
			localState.TrackingTarget = worldObject.NetworkObject;
			localState.TrackingTargetWorldObject = worldObject;
			SyncState();
		}
	}

	internal void SetLookDirection(float pitch, float yaw)
	{
		pitch = Mathf.DeltaAngle(0f, pitch);
		pitch = ClampAngle(pitch, -89f, 89f);
		yaw = Mathf.DeltaAngle(0f, yaw + WorldObject.transform.eulerAngles.y);
		localState.TrackingStatus = TrackingTargetType.Direction;
		localState.TrackingTargetPitch = pitch;
		localState.TrackingTargetYaw = yaw;
		localState.TrackingTargetWorldObject = null;
		SyncState();
	}

	public static float ClampAngle(float angle, float min, float max)
	{
		float num = (min + max) * 0.5f - 180f;
		float num2 = Mathf.FloorToInt((angle - num) / 360f) * 360;
		return Mathf.Clamp(angle, min + num2, max + num2);
	}

	private void ToggleParticleSystem(ParticleSystem particleSystem, bool enabled)
	{
		if ((bool)particleSystem && particleSystem.isPlaying != enabled)
		{
			if (enabled)
			{
				particleSystem.Play();
			}
			else
			{
				particleSystem.Stop();
			}
		}
	}

	void IStartSubscriber.EndlessStart()
	{
		lastLocalFrame = NetClock.CurrentFrame - 1;
		playmode = true;
	}

	void IGameEndSubscriber.EndlessGameEnd()
	{
		playmode = false;
	}

	public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
	{
		sentryAppearanceController.ComponentInitialize(referenceBase);
		references = (SentryReferences)referenceBase;
	}

	public void PrefabInitialize(WorldObject worldObject)
	{
		WorldObject = worldObject;
	}

	public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
	{
		scriptComponent = endlessScriptComponent;
	}

	protected override void __initializeVariables()
	{
		if (networkState == null)
		{
			throw new Exception("Sentry.networkState cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		networkState.Initialize(this);
		__nameNetworkVariable(networkState, "networkState");
		NetworkVariableFields.Add(networkState);
		if (damageLevel == null)
		{
			throw new Exception("Sentry.damageLevel cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		damageLevel.Initialize(this);
		__nameNetworkVariable(damageLevel, "damageLevel");
		NetworkVariableFields.Add(damageLevel);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(1692595414u, __rpc_handler_1692595414, "ShootClientRpc");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_1692595414(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out UnityEngine.Vector3 value);
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((Sentry)target).ShootClientRpc(value);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "Sentry";
	}
}
