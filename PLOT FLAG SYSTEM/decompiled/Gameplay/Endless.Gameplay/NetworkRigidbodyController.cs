using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.LuaInterfaces;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Endless.Gameplay;

public class NetworkRigidbodyController : EndlessNetworkBehaviour, IStartSubscriber, IGameEndSubscriber, IPersistantStateSubscriber, IPhysicsTaker, IScriptInjector, NetClock.IRollbackSubscriber, NetClock.ISimulateFrameEnvironmentSubscriber, NetClock.IPostFixedUpdateSubscriber, NetClock.ISimulateFrameEarlySubscriber
{
	private enum WorldOverlapCheckType
	{
		Frame,
		Initialization,
		Destroy
	}

	public interface IPhysicsFrameListener
	{
		void Frame(uint frame);
	}

	private const uint SLEEP_SEND_FRAME_INTERVAL = 1u;

	[Header("Required")]
	[SerializeField]
	private Rigidbody targetRigidbody;

	[SerializeField]
	private NetworkRigidbodyAppearanceController appearancePrefab;

	[SerializeField]
	private DraggablePhysicsCube draggablePhysicsCube;

	[Header("Situational")]
	[Tooltip("Kinematic Rigidbody with the same collider as this rigidbody, specifically for interacting with player. Not necessary for smaller objects that dont block character movement.")]
	[SerializeField]
	private GameObject collisionClonePrefab;

	[SerializeField]
	private WorldCollidable worldCollidable;

	[SerializeField]
	private LayerMask worldCollideMask;

	[FormerlySerializedAs("physicsCube")]
	[SerializeField]
	private PhysicsCubeController physicsCubeController;

	[Header("Physics taker")]
	[SerializeField]
	private ForceMode forceMode;

	[SerializeField]
	private float blastForceMultiplier = 1f;

	[Header("Stage Fall off")]
	[SerializeField]
	private RigidbodyWorldFalloffController rigidbodyWorldFalloffController;

	private RingBuffer<RigidbodyState> stateRingBuffer = new RingBuffer<RigidbodyState>(30);

	private RigidbodyState resultingState;

	private NetworkRigidbodyAppearanceController appearanceController;

	private NetworkVariable<RigidbodyState> sleepState = new NetworkVariable<RigidbodyState>();

	private RigidbodyState mostRecentAwakeState;

	private uint lastAwakeFrame_server;

	private uint sleepIntervalOffset;

	private Collider[] hitPool = new Collider[10];

	private Vector3 boxExtents = new Vector3(0.5f, 0.5f, 0.5f);

	private Vector3 storedRigidbodyVelocity;

	private Vector3 storedRigidbodyAngularVelocity;

	private RigidbodyConstraints storedConstraints;

	private Rigidbody collisionClone;

	private List<PhysicsForceInfo> forceQueue = new List<PhysicsForceInfo>();

	private object luaObject;

	public Vector3 CenterOfMassOffset => Vector3.zero;

	public bool ShouldSaveAndLoad => true;

	float IPhysicsTaker.GravityAccelerationRate => 0f - Physics.gravity.y;

	float IPhysicsTaker.Mass => targetRigidbody.mass;

	float IPhysicsTaker.Drag => targetRigidbody.drag;

	float IPhysicsTaker.AirborneDrag => targetRigidbody.drag;

	Vector3 IPhysicsTaker.CurrentVelocity => targetRigidbody.velocity;

	public float BlastForceMultiplier => blastForceMultiplier;

	public NetworkRigidbodyAppearanceController AppearanceController => appearanceController;

	private bool clientNeedsUpdateNextSleepFrame => lastAwakeFrame_server > sleepState.Value.NetFrame;

	private bool IsPlaying => MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying;

	public RigidbodyState CurrentState => resultingState;

	public bool IgnorePhysics { get; set; }

	public object LuaObject
	{
		get
		{
			if (luaObject == null)
			{
				luaObject = new PhysicsComponent(this);
			}
			return luaObject;
		}
	}

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
		worldCollidable.isSimulatedCheckOverride = () => true;
		if ((bool)collisionClonePrefab)
		{
			collisionClone = UnityEngine.Object.Instantiate(collisionClonePrefab, targetRigidbody.position, targetRigidbody.rotation).GetComponent<Rigidbody>();
		}
		MonoBehaviourSingleton<RigidbodyManager>.Instance.AddListener(HandleRigidbodySimulationStart, HandleRigidbodySimulationEnd);
		if (base.IsServer)
		{
			sleepIntervalOffset = (uint)UnityEngine.Random.Range(0f, 2f);
		}
		NetClock.Register(this);
		SimulateFrameEnvironment(0u);
	}

	public override void OnNetworkDespawn()
	{
		base.OnNetworkDespawn();
		if ((bool)collisionClone)
		{
			UnityEngine.Object.Destroy(collisionClone.gameObject);
		}
		MonoBehaviourSingleton<RigidbodyManager>.Instance.RemoveListener(HandleRigidbodySimulationStart, HandleRigidbodySimulationEnd);
		ProcessWorldTriggerCheck_NetFrame(NetClock.CurrentFrame, WorldOverlapCheckType.Destroy);
		NetClock.Unregister(this);
	}

	private void HandleRigidbodySimulationStart()
	{
		if (!IsPlaying)
		{
			if ((bool)collisionClone)
			{
				collisionClone.transform.position = base.transform.position;
			}
		}
		else
		{
			targetRigidbody.isKinematic = false;
			targetRigidbody.velocity = storedRigidbodyVelocity;
			targetRigidbody.angularVelocity = storedRigidbodyAngularVelocity;
			targetRigidbody.constraints = storedConstraints;
		}
	}

	private void HandleRigidbodySimulationEnd()
	{
		if (IsPlaying)
		{
			storedRigidbodyVelocity = targetRigidbody.velocity;
			storedRigidbodyAngularVelocity = targetRigidbody.angularVelocity;
			storedConstraints = targetRigidbody.constraints;
			targetRigidbody.isKinematic = true;
		}
	}

	private void Awake()
	{
		appearanceController = UnityEngine.Object.Instantiate(appearancePrefab, targetRigidbody.position, targetRigidbody.rotation);
		appearanceController.enabled = false;
		targetRigidbody.isKinematic = true;
		targetRigidbody.interpolation = RigidbodyInterpolation.None;
		appearanceController.transform.SetParent(base.transform);
		appearanceController.transform.localPosition = Vector3.zero;
		if ((bool)draggablePhysicsCube)
		{
			appearanceController.InitAppearance(draggablePhysicsCube.WorldObject, draggablePhysicsCube.GetAppearanceObject());
		}
	}

	public override void OnDestroy()
	{
		if (appearanceController != null && appearanceController.gameObject != null)
		{
			UnityEngine.Object.Destroy(appearanceController.gameObject);
		}
		if ((bool)collisionClone)
		{
			UnityEngine.Object.Destroy(collisionClone.gameObject);
		}
		if (base.IsServer && base.NetworkObject.IsSpawned)
		{
			base.NetworkObject.Despawn();
		}
		base.OnDestroy();
	}

	public void EndlessStart()
	{
		appearanceController.transform.localPosition = Vector3.zero;
		appearanceController.enabled = true;
		targetRigidbody.isKinematic = false;
		targetRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
		appearanceController.transform.SetParent(null);
		storedRigidbodyAngularVelocity = Vector3.zero;
		storedRigidbodyVelocity = Vector3.zero;
		ProcessWorldTriggerCheck_NetFrame(0u, WorldOverlapCheckType.Initialization);
	}

	public void EndlessGameEnd()
	{
		appearanceController.enabled = false;
		targetRigidbody.isKinematic = true;
		appearanceController.transform.SetParent(base.transform);
		appearanceController.transform.localPosition = Vector3.zero;
		storedRigidbodyAngularVelocity = Vector3.zero;
		storedRigidbodyVelocity = Vector3.zero;
	}

	public object GetSaveState()
	{
		return resultingState;
	}

	public void LoadState(object loadedState)
	{
		if (base.IsServer && loadedState != null)
		{
			resultingState = (RigidbodyState)loadedState;
			targetRigidbody.position = resultingState.Position;
			targetRigidbody.rotation = Quaternion.Euler(resultingState.Angles);
			targetRigidbody.velocity = resultingState.Velocity;
			targetRigidbody.angularVelocity = resultingState.AngularVelocity;
		}
	}

	private void SetStateFromBuffer(uint frame)
	{
		if (stateRingBuffer.GetValue(frame).NetFrame == frame)
		{
			RigidbodyState value = stateRingBuffer.GetValue(frame);
			targetRigidbody.position = value.Position;
			targetRigidbody.rotation = Quaternion.Euler(value.Angles);
			targetRigidbody.velocity = value.Velocity;
			targetRigidbody.angularVelocity = value.AngularVelocity;
			resultingState = value;
		}
	}

	public void TakePhysicsForce(float force, Vector3 directionNormal, uint startFrame, ulong source, bool forceFreeFall = false, bool friendlyForce = false, bool applyRandomTorque = false)
	{
		if (!IgnorePhysics)
		{
			forceQueue.Add(new PhysicsForceInfo
			{
				Force = force * blastForceMultiplier,
				DirectionNormal = directionNormal,
				StartFrame = startFrame,
				SourceID = source,
				ApplyRandomTorque = applyRandomTorque
			});
		}
	}

	private void ProcessWorldTriggerCheck_NetFrame(uint frame, WorldOverlapCheckType overlapCheckType = WorldOverlapCheckType.Frame)
	{
		if (!(worldCollidable != null))
		{
			return;
		}
		int num = Physics.OverlapBoxNonAlloc(targetRigidbody.position, boxExtents, hitPool, targetRigidbody.rotation, worldCollideMask, QueryTriggerInteraction.Collide);
		for (int i = 0; i < num; i++)
		{
			WorldTriggerCollider component = hitPool[i].GetComponent<WorldTriggerCollider>();
			if (component != null)
			{
				switch (overlapCheckType)
				{
				case WorldOverlapCheckType.Initialization:
					component.WorldTrigger.PreloadOverlap(worldCollidable);
					break;
				case WorldOverlapCheckType.Frame:
					component.WorldTrigger.Overlapped(worldCollidable, frame);
					break;
				default:
					component.WorldTrigger.DestroyOverlap(worldCollidable, frame);
					break;
				}
			}
		}
	}

	public void UpdatedState(uint frame)
	{
		stateRingBuffer.FrameUpdated(frame);
		RigidbodyState value = stateRingBuffer.GetValue(frame);
		if (value.Sleeping)
		{
			if (mostRecentAwakeState.NetFrame < sleepState.Value.NetFrame)
			{
				RigidbodyState value2 = sleepState.Value;
				value2.serverVerifiedState = true;
				value2.Velocity = Vector3.zero;
				value2.AngularVelocity = Vector3.zero;
				value2.NetFrame = frame;
				value2.Sleeping = true;
				stateRingBuffer.UpdateValue(ref value2);
			}
			else
			{
				RigidbodyState value3 = mostRecentAwakeState;
				value3.serverVerifiedState = true;
				value3.Velocity = Vector3.zero;
				value3.AngularVelocity = Vector3.zero;
				value3.NetFrame = frame;
				value3.Sleeping = true;
				stateRingBuffer.UpdateValue(ref value3);
			}
		}
		else
		{
			mostRecentAwakeState = value;
		}
	}

	public void WriteStateToBuffer(ref RigidbodyState state)
	{
		stateRingBuffer.UpdateValue(ref state);
	}

	public void Rollback(uint frame)
	{
		if (IsPlaying)
		{
			SetStateFromBuffer(frame - 1);
		}
	}

	public void SimulateFrameEnvironment(uint frame)
	{
		if (!IsPlaying)
		{
			return;
		}
		RigidbodyState atPosition = stateRingBuffer.GetAtPosition(frame);
		if (!base.IsServer && atPosition.NetFrame == frame && atPosition.serverVerifiedState)
		{
			SetStateFromBuffer(frame);
		}
		else
		{
			resultingState.NetFrame = frame;
			resultingState.serverVerifiedState = false;
			resultingState.Position = targetRigidbody.position;
			resultingState.Angles = targetRigidbody.rotation.eulerAngles;
			resultingState.Velocity = targetRigidbody.velocity;
			resultingState.AngularVelocity = targetRigidbody.angularVelocity;
			resultingState.Sleeping = targetRigidbody.IsSleeping();
			if (resultingState.Teleporting)
			{
				resultingState.FramesUntilTeleport--;
			}
			stateRingBuffer.GetReferenceFromBuffer(frame) = resultingState;
		}
		if ((bool)collisionClone)
		{
			collisionClone.position = targetRigidbody.position;
			collisionClone.rotation = targetRigidbody.rotation;
		}
		ProcessWorldTriggerCheck_NetFrame(frame);
		if (rigidbodyWorldFalloffController != null)
		{
			rigidbodyWorldFalloffController.CheckFallOffStage();
		}
	}

	public void TriggerTeleport(Vector3 position, TeleportType teleportType)
	{
		if (base.IsServer && !resultingState.Teleporting)
		{
			resultingState.Teleporting = true;
			resultingState.FramesUntilTeleport = RuntimeDatabase.GetTeleportInfo(teleportType).FramesToTeleport;
			resultingState.TeleportType = teleportType;
			resultingState.TeleportPosition = position;
		}
	}

	public void PostFixedUpdate(uint frame)
	{
		if (base.IsServer)
		{
			bool flag = targetRigidbody.IsSleeping();
			resultingState.FullSync = !flag;
			resultingState.Sleeping = flag;
			if (IsPlaying && (!flag || (frame + sleepIntervalOffset) % 1 == 0))
			{
				NetworkBehaviourSingleton<GameplayMessagingManager>.Instance.SendRigidBodyState(this, resultingState);
			}
			if (flag && clientNeedsUpdateNextSleepFrame)
			{
				resultingState.FullSync = true;
				sleepState.Value = resultingState;
			}
			else if (!flag)
			{
				lastAwakeFrame_server = frame;
			}
			stateRingBuffer.UpdateValue(ref resultingState);
		}
		appearanceController.AddState(resultingState);
	}

	public void SimulateFrameEarly(uint frame)
	{
		if (resultingState.Teleporting && resultingState.FramesUntilTeleport == 0)
		{
			targetRigidbody.position = resultingState.TeleportPosition;
			resultingState.Teleporting = false;
			resultingState.NetFrame = frame;
		}
		bool forceUnground = false;
		forceQueue.RemoveAll(delegate(PhysicsForceInfo forceInfo)
		{
			if (forceInfo.StartFrame == frame)
			{
				forceUnground = true;
				targetRigidbody.AddForceAtPosition(forceInfo.Force * forceInfo.DirectionNormal, targetRigidbody.position - forceInfo.DirectionNormal, forceMode);
				if (forceInfo.ApplyRandomTorque)
				{
					targetRigidbody.AddTorque(forceInfo.Force * forceInfo.DirectionNormal, forceMode);
				}
			}
			return !base.IsServer || forceInfo.StartFrame <= frame;
		});
		if ((bool)physicsCubeController)
		{
			forceUnground = true;
			if (base.IsServer)
			{
				physicsCubeController.Frame(frame, forceUnground);
			}
			else if (!stateRingBuffer.GetAtPosition(frame).serverVerifiedState)
			{
				physicsCubeController.Frame(frame, forceUnground);
			}
		}
	}

	protected override void __initializeVariables()
	{
		if (sleepState == null)
		{
			throw new Exception("NetworkRigidbodyController.sleepState cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		sleepState.Initialize(this);
		__nameNetworkVariable(sleepState, "sleepState");
		NetworkVariableFields.Add(sleepState);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected internal override string __getTypeName()
	{
		return "NetworkRigidbodyController";
	}
}
