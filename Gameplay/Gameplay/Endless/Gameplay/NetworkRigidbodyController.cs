using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.LuaInterfaces;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Endless.Gameplay
{
	// Token: 0x0200010E RID: 270
	public class NetworkRigidbodyController : EndlessNetworkBehaviour, IStartSubscriber, IGameEndSubscriber, IPersistantStateSubscriber, IPhysicsTaker, IScriptInjector, NetClock.IRollbackSubscriber, NetClock.ISimulateFrameEnvironmentSubscriber, NetClock.IPostFixedUpdateSubscriber, NetClock.ISimulateFrameEarlySubscriber
	{
		// Token: 0x17000109 RID: 265
		// (get) Token: 0x060005FD RID: 1533 RVA: 0x0001DBB4 File Offset: 0x0001BDB4
		public Vector3 CenterOfMassOffset
		{
			get
			{
				return Vector3.zero;
			}
		}

		// Token: 0x1700010A RID: 266
		// (get) Token: 0x060005FE RID: 1534 RVA: 0x00017586 File Offset: 0x00015786
		public bool ShouldSaveAndLoad
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700010B RID: 267
		// (get) Token: 0x060005FF RID: 1535 RVA: 0x0001DBBB File Offset: 0x0001BDBB
		float IPhysicsTaker.GravityAccelerationRate
		{
			get
			{
				return -Physics.gravity.y;
			}
		}

		// Token: 0x1700010C RID: 268
		// (get) Token: 0x06000600 RID: 1536 RVA: 0x0001DBC8 File Offset: 0x0001BDC8
		float IPhysicsTaker.Mass
		{
			get
			{
				return this.targetRigidbody.mass;
			}
		}

		// Token: 0x1700010D RID: 269
		// (get) Token: 0x06000601 RID: 1537 RVA: 0x0001DBD5 File Offset: 0x0001BDD5
		float IPhysicsTaker.Drag
		{
			get
			{
				return this.targetRigidbody.drag;
			}
		}

		// Token: 0x1700010E RID: 270
		// (get) Token: 0x06000602 RID: 1538 RVA: 0x0001DBD5 File Offset: 0x0001BDD5
		float IPhysicsTaker.AirborneDrag
		{
			get
			{
				return this.targetRigidbody.drag;
			}
		}

		// Token: 0x1700010F RID: 271
		// (get) Token: 0x06000603 RID: 1539 RVA: 0x0001DBE2 File Offset: 0x0001BDE2
		Vector3 IPhysicsTaker.CurrentVelocity
		{
			get
			{
				return this.targetRigidbody.velocity;
			}
		}

		// Token: 0x17000110 RID: 272
		// (get) Token: 0x06000604 RID: 1540 RVA: 0x0001DBEF File Offset: 0x0001BDEF
		public float BlastForceMultiplier
		{
			get
			{
				return this.blastForceMultiplier;
			}
		}

		// Token: 0x17000111 RID: 273
		// (get) Token: 0x06000605 RID: 1541 RVA: 0x0001DBF7 File Offset: 0x0001BDF7
		public NetworkRigidbodyAppearanceController AppearanceController
		{
			get
			{
				return this.appearanceController;
			}
		}

		// Token: 0x17000112 RID: 274
		// (get) Token: 0x06000606 RID: 1542 RVA: 0x0001DC00 File Offset: 0x0001BE00
		private bool clientNeedsUpdateNextSleepFrame
		{
			get
			{
				return this.lastAwakeFrame_server > this.sleepState.Value.NetFrame;
			}
		}

		// Token: 0x17000113 RID: 275
		// (get) Token: 0x06000607 RID: 1543 RVA: 0x0001DC28 File Offset: 0x0001BE28
		private bool IsPlaying
		{
			get
			{
				return MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying;
			}
		}

		// Token: 0x17000114 RID: 276
		// (get) Token: 0x06000608 RID: 1544 RVA: 0x0001DC34 File Offset: 0x0001BE34
		public RigidbodyState CurrentState
		{
			get
			{
				return this.resultingState;
			}
		}

		// Token: 0x17000115 RID: 277
		// (get) Token: 0x06000609 RID: 1545 RVA: 0x0001DC3C File Offset: 0x0001BE3C
		// (set) Token: 0x0600060A RID: 1546 RVA: 0x0001DC44 File Offset: 0x0001BE44
		public bool IgnorePhysics { get; set; }

		// Token: 0x17000116 RID: 278
		// (get) Token: 0x0600060B RID: 1547 RVA: 0x0001DC4D File Offset: 0x0001BE4D
		public object LuaObject
		{
			get
			{
				if (this.luaObject == null)
				{
					this.luaObject = new PhysicsComponent(this);
				}
				return this.luaObject;
			}
		}

		// Token: 0x0600060C RID: 1548 RVA: 0x0001DC6C File Offset: 0x0001BE6C
		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			this.worldCollidable.isSimulatedCheckOverride = () => true;
			if (this.collisionClonePrefab)
			{
				this.collisionClone = global::UnityEngine.Object.Instantiate<GameObject>(this.collisionClonePrefab, this.targetRigidbody.position, this.targetRigidbody.rotation).GetComponent<Rigidbody>();
			}
			MonoBehaviourSingleton<RigidbodyManager>.Instance.AddListener(new UnityAction(this.HandleRigidbodySimulationStart), new UnityAction(this.HandleRigidbodySimulationEnd));
			if (base.IsServer)
			{
				this.sleepIntervalOffset = (uint)global::UnityEngine.Random.Range(0f, 2f);
			}
			NetClock.Register(this);
			this.SimulateFrameEnvironment(0U);
		}

		// Token: 0x0600060D RID: 1549 RVA: 0x0001DD30 File Offset: 0x0001BF30
		public override void OnNetworkDespawn()
		{
			base.OnNetworkDespawn();
			if (this.collisionClone)
			{
				global::UnityEngine.Object.Destroy(this.collisionClone.gameObject);
			}
			MonoBehaviourSingleton<RigidbodyManager>.Instance.RemoveListener(new UnityAction(this.HandleRigidbodySimulationStart), new UnityAction(this.HandleRigidbodySimulationEnd));
			this.ProcessWorldTriggerCheck_NetFrame(NetClock.CurrentFrame, NetworkRigidbodyController.WorldOverlapCheckType.Destroy);
			NetClock.Unregister(this);
		}

		// Token: 0x0600060E RID: 1550 RVA: 0x0001DD94 File Offset: 0x0001BF94
		private void HandleRigidbodySimulationStart()
		{
			if (!this.IsPlaying)
			{
				if (this.collisionClone)
				{
					this.collisionClone.transform.position = base.transform.position;
				}
				return;
			}
			this.targetRigidbody.isKinematic = false;
			this.targetRigidbody.velocity = this.storedRigidbodyVelocity;
			this.targetRigidbody.angularVelocity = this.storedRigidbodyAngularVelocity;
			this.targetRigidbody.constraints = this.storedConstraints;
		}

		// Token: 0x0600060F RID: 1551 RVA: 0x0001DE14 File Offset: 0x0001C014
		private void HandleRigidbodySimulationEnd()
		{
			if (!this.IsPlaying)
			{
				return;
			}
			this.storedRigidbodyVelocity = this.targetRigidbody.velocity;
			this.storedRigidbodyAngularVelocity = this.targetRigidbody.angularVelocity;
			this.storedConstraints = this.targetRigidbody.constraints;
			this.targetRigidbody.isKinematic = true;
		}

		// Token: 0x06000610 RID: 1552 RVA: 0x0001DE6C File Offset: 0x0001C06C
		private void Awake()
		{
			this.appearanceController = global::UnityEngine.Object.Instantiate<NetworkRigidbodyAppearanceController>(this.appearancePrefab, this.targetRigidbody.position, this.targetRigidbody.rotation);
			this.appearanceController.enabled = false;
			this.targetRigidbody.isKinematic = true;
			this.targetRigidbody.interpolation = RigidbodyInterpolation.None;
			this.appearanceController.transform.SetParent(base.transform);
			this.appearanceController.transform.localPosition = Vector3.zero;
			if (this.draggablePhysicsCube)
			{
				this.appearanceController.InitAppearance(this.draggablePhysicsCube.WorldObject, this.draggablePhysicsCube.GetAppearanceObject());
			}
		}

		// Token: 0x06000611 RID: 1553 RVA: 0x0001DF20 File Offset: 0x0001C120
		public override void OnDestroy()
		{
			if (this.appearanceController != null && this.appearanceController.gameObject != null)
			{
				global::UnityEngine.Object.Destroy(this.appearanceController.gameObject);
			}
			if (this.collisionClone)
			{
				global::UnityEngine.Object.Destroy(this.collisionClone.gameObject);
			}
			if (base.IsServer && base.NetworkObject.IsSpawned)
			{
				base.NetworkObject.Despawn(true);
			}
			base.OnDestroy();
		}

		// Token: 0x06000612 RID: 1554 RVA: 0x0001DFA4 File Offset: 0x0001C1A4
		public void EndlessStart()
		{
			this.appearanceController.transform.localPosition = Vector3.zero;
			this.appearanceController.enabled = true;
			this.targetRigidbody.isKinematic = false;
			this.targetRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
			this.appearanceController.transform.SetParent(null);
			this.storedRigidbodyAngularVelocity = Vector3.zero;
			this.storedRigidbodyVelocity = Vector3.zero;
			this.ProcessWorldTriggerCheck_NetFrame(0U, NetworkRigidbodyController.WorldOverlapCheckType.Initialization);
		}

		// Token: 0x06000613 RID: 1555 RVA: 0x0001E01C File Offset: 0x0001C21C
		public void EndlessGameEnd()
		{
			this.appearanceController.enabled = false;
			this.targetRigidbody.isKinematic = true;
			this.appearanceController.transform.SetParent(base.transform);
			this.appearanceController.transform.localPosition = Vector3.zero;
			this.storedRigidbodyAngularVelocity = Vector3.zero;
			this.storedRigidbodyVelocity = Vector3.zero;
		}

		// Token: 0x06000614 RID: 1556 RVA: 0x0001E082 File Offset: 0x0001C282
		public object GetSaveState()
		{
			return this.resultingState;
		}

		// Token: 0x06000615 RID: 1557 RVA: 0x0001E090 File Offset: 0x0001C290
		public void LoadState(object loadedState)
		{
			if (base.IsServer && loadedState != null)
			{
				this.resultingState = (RigidbodyState)loadedState;
				this.targetRigidbody.position = this.resultingState.Position;
				this.targetRigidbody.rotation = Quaternion.Euler(this.resultingState.Angles);
				this.targetRigidbody.velocity = this.resultingState.Velocity;
				this.targetRigidbody.angularVelocity = this.resultingState.AngularVelocity;
			}
		}

		// Token: 0x06000616 RID: 1558 RVA: 0x0001E114 File Offset: 0x0001C314
		private void SetStateFromBuffer(uint frame)
		{
			if (this.stateRingBuffer.GetValue(frame).NetFrame != frame)
			{
				return;
			}
			RigidbodyState value = this.stateRingBuffer.GetValue(frame);
			this.targetRigidbody.position = value.Position;
			this.targetRigidbody.rotation = Quaternion.Euler(value.Angles);
			this.targetRigidbody.velocity = value.Velocity;
			this.targetRigidbody.angularVelocity = value.AngularVelocity;
			this.resultingState = value;
		}

		// Token: 0x06000617 RID: 1559 RVA: 0x0001E198 File Offset: 0x0001C398
		public void TakePhysicsForce(float force, Vector3 directionNormal, uint startFrame, ulong source, bool forceFreeFall = false, bool friendlyForce = false, bool applyRandomTorque = false)
		{
			if (this.IgnorePhysics)
			{
				return;
			}
			this.forceQueue.Add(new PhysicsForceInfo
			{
				Force = force * this.blastForceMultiplier,
				DirectionNormal = directionNormal,
				StartFrame = startFrame,
				SourceID = source,
				ApplyRandomTorque = applyRandomTorque
			});
		}

		// Token: 0x06000618 RID: 1560 RVA: 0x0001E1F4 File Offset: 0x0001C3F4
		private void ProcessWorldTriggerCheck_NetFrame(uint frame, NetworkRigidbodyController.WorldOverlapCheckType overlapCheckType = NetworkRigidbodyController.WorldOverlapCheckType.Frame)
		{
			if (this.worldCollidable != null)
			{
				int num = Physics.OverlapBoxNonAlloc(this.targetRigidbody.position, this.boxExtents, this.hitPool, this.targetRigidbody.rotation, this.worldCollideMask, QueryTriggerInteraction.Collide);
				for (int i = 0; i < num; i++)
				{
					WorldTriggerCollider component = this.hitPool[i].GetComponent<WorldTriggerCollider>();
					if (component != null)
					{
						if (overlapCheckType == NetworkRigidbodyController.WorldOverlapCheckType.Initialization)
						{
							component.WorldTrigger.PreloadOverlap(this.worldCollidable);
						}
						else if (overlapCheckType == NetworkRigidbodyController.WorldOverlapCheckType.Frame)
						{
							component.WorldTrigger.Overlapped(this.worldCollidable, frame, true);
						}
						else
						{
							component.WorldTrigger.DestroyOverlap(this.worldCollidable, frame);
						}
					}
				}
			}
		}

		// Token: 0x06000619 RID: 1561 RVA: 0x0001E2AC File Offset: 0x0001C4AC
		public void UpdatedState(uint frame)
		{
			this.stateRingBuffer.FrameUpdated(frame);
			RigidbodyState value = this.stateRingBuffer.GetValue(frame);
			if (!value.Sleeping)
			{
				this.mostRecentAwakeState = value;
				return;
			}
			if (this.mostRecentAwakeState.NetFrame < this.sleepState.Value.NetFrame)
			{
				RigidbodyState value2 = this.sleepState.Value;
				value2.serverVerifiedState = true;
				value2.Velocity = Vector3.zero;
				value2.AngularVelocity = Vector3.zero;
				value2.NetFrame = frame;
				value2.Sleeping = true;
				this.stateRingBuffer.UpdateValue(ref value2);
				return;
			}
			RigidbodyState rigidbodyState = this.mostRecentAwakeState;
			rigidbodyState.serverVerifiedState = true;
			rigidbodyState.Velocity = Vector3.zero;
			rigidbodyState.AngularVelocity = Vector3.zero;
			rigidbodyState.NetFrame = frame;
			rigidbodyState.Sleeping = true;
			this.stateRingBuffer.UpdateValue(ref rigidbodyState);
		}

		// Token: 0x0600061A RID: 1562 RVA: 0x0001E393 File Offset: 0x0001C593
		public void WriteStateToBuffer(ref RigidbodyState state)
		{
			this.stateRingBuffer.UpdateValue(ref state);
		}

		// Token: 0x0600061B RID: 1563 RVA: 0x0001E3A1 File Offset: 0x0001C5A1
		public void Rollback(uint frame)
		{
			if (!this.IsPlaying)
			{
				return;
			}
			this.SetStateFromBuffer(frame - 1U);
		}

		// Token: 0x0600061C RID: 1564 RVA: 0x0001E3B8 File Offset: 0x0001C5B8
		public unsafe void SimulateFrameEnvironment(uint frame)
		{
			if (!this.IsPlaying)
			{
				return;
			}
			RigidbodyState rigidbodyState = *this.stateRingBuffer.GetAtPosition(frame);
			if (!base.IsServer && rigidbodyState.NetFrame == frame && rigidbodyState.serverVerifiedState)
			{
				this.SetStateFromBuffer(frame);
			}
			else
			{
				this.resultingState.NetFrame = frame;
				this.resultingState.serverVerifiedState = false;
				this.resultingState.Position = this.targetRigidbody.position;
				this.resultingState.Angles = this.targetRigidbody.rotation.eulerAngles;
				this.resultingState.Velocity = this.targetRigidbody.velocity;
				this.resultingState.AngularVelocity = this.targetRigidbody.angularVelocity;
				this.resultingState.Sleeping = this.targetRigidbody.IsSleeping();
				if (this.resultingState.Teleporting)
				{
					this.resultingState.FramesUntilTeleport = this.resultingState.FramesUntilTeleport - 1U;
				}
				*this.stateRingBuffer.GetReferenceFromBuffer(frame) = this.resultingState;
			}
			if (this.collisionClone)
			{
				this.collisionClone.position = this.targetRigidbody.position;
				this.collisionClone.rotation = this.targetRigidbody.rotation;
			}
			this.ProcessWorldTriggerCheck_NetFrame(frame, NetworkRigidbodyController.WorldOverlapCheckType.Frame);
			if (this.rigidbodyWorldFalloffController != null)
			{
				this.rigidbodyWorldFalloffController.CheckFallOffStage();
			}
		}

		// Token: 0x0600061D RID: 1565 RVA: 0x0001E52C File Offset: 0x0001C72C
		public void TriggerTeleport(Vector3 position, TeleportType teleportType)
		{
			if (base.IsServer && !this.resultingState.Teleporting)
			{
				this.resultingState.Teleporting = true;
				this.resultingState.FramesUntilTeleport = RuntimeDatabase.GetTeleportInfo(teleportType).FramesToTeleport;
				this.resultingState.TeleportType = teleportType;
				this.resultingState.TeleportPosition = position;
			}
		}

		// Token: 0x0600061E RID: 1566 RVA: 0x0001E588 File Offset: 0x0001C788
		public void PostFixedUpdate(uint frame)
		{
			if (base.IsServer)
			{
				bool flag = this.targetRigidbody.IsSleeping();
				this.resultingState.FullSync = !flag;
				this.resultingState.Sleeping = flag;
				if (this.IsPlaying && (!flag || (frame + this.sleepIntervalOffset) % 1U == 0U))
				{
					NetworkBehaviourSingleton<GameplayMessagingManager>.Instance.SendRigidBodyState(this, this.resultingState);
				}
				if (flag && this.clientNeedsUpdateNextSleepFrame)
				{
					this.resultingState.FullSync = true;
					this.sleepState.Value = this.resultingState;
				}
				else if (!flag)
				{
					this.lastAwakeFrame_server = frame;
				}
				this.stateRingBuffer.UpdateValue(ref this.resultingState);
			}
			this.appearanceController.AddState(this.resultingState);
		}

		// Token: 0x0600061F RID: 1567 RVA: 0x0001E648 File Offset: 0x0001C848
		public void SimulateFrameEarly(uint frame)
		{
			if (this.resultingState.Teleporting && this.resultingState.FramesUntilTeleport == 0U)
			{
				this.targetRigidbody.position = this.resultingState.TeleportPosition;
				this.resultingState.Teleporting = false;
				this.resultingState.NetFrame = frame;
			}
			bool forceUnground = false;
			this.forceQueue.RemoveAll(delegate(PhysicsForceInfo forceInfo)
			{
				if (forceInfo.StartFrame == frame)
				{
					forceUnground = true;
					this.targetRigidbody.AddForceAtPosition(forceInfo.Force * forceInfo.DirectionNormal, this.targetRigidbody.position - forceInfo.DirectionNormal, this.forceMode);
					if (forceInfo.ApplyRandomTorque)
					{
						this.targetRigidbody.AddTorque(forceInfo.Force * forceInfo.DirectionNormal, this.forceMode);
					}
				}
				return !this.IsServer || forceInfo.StartFrame <= frame;
			});
			if (!this.physicsCubeController)
			{
				return;
			}
			forceUnground = true;
			if (base.IsServer)
			{
				this.physicsCubeController.Frame(frame, forceUnground);
				return;
			}
			if (!this.stateRingBuffer.GetAtPosition(frame).serverVerifiedState)
			{
				this.physicsCubeController.Frame(frame, forceUnground);
			}
		}

		// Token: 0x06000621 RID: 1569 RVA: 0x0001E7B0 File Offset: 0x0001C9B0
		protected override void __initializeVariables()
		{
			bool flag = this.sleepState == null;
			if (flag)
			{
				throw new Exception("NetworkRigidbodyController.sleepState cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.sleepState.Initialize(this);
			base.__nameNetworkVariable(this.sleepState, "sleepState");
			this.NetworkVariableFields.Add(this.sleepState);
			base.__initializeVariables();
		}

		// Token: 0x06000622 RID: 1570 RVA: 0x0001E813 File Offset: 0x0001CA13
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x06000623 RID: 1571 RVA: 0x0001E81D File Offset: 0x0001CA1D
		protected internal override string __getTypeName()
		{
			return "NetworkRigidbodyController";
		}

		// Token: 0x0400047F RID: 1151
		private const uint SLEEP_SEND_FRAME_INTERVAL = 1U;

		// Token: 0x04000480 RID: 1152
		[Header("Required")]
		[SerializeField]
		private Rigidbody targetRigidbody;

		// Token: 0x04000481 RID: 1153
		[SerializeField]
		private NetworkRigidbodyAppearanceController appearancePrefab;

		// Token: 0x04000482 RID: 1154
		[SerializeField]
		private DraggablePhysicsCube draggablePhysicsCube;

		// Token: 0x04000483 RID: 1155
		[Header("Situational")]
		[Tooltip("Kinematic Rigidbody with the same collider as this rigidbody, specifically for interacting with player. Not necessary for smaller objects that dont block character movement.")]
		[SerializeField]
		private GameObject collisionClonePrefab;

		// Token: 0x04000484 RID: 1156
		[SerializeField]
		private WorldCollidable worldCollidable;

		// Token: 0x04000485 RID: 1157
		[SerializeField]
		private LayerMask worldCollideMask;

		// Token: 0x04000486 RID: 1158
		[FormerlySerializedAs("physicsCube")]
		[SerializeField]
		private PhysicsCubeController physicsCubeController;

		// Token: 0x04000487 RID: 1159
		[Header("Physics taker")]
		[SerializeField]
		private ForceMode forceMode;

		// Token: 0x04000488 RID: 1160
		[SerializeField]
		private float blastForceMultiplier = 1f;

		// Token: 0x04000489 RID: 1161
		[Header("Stage Fall off")]
		[SerializeField]
		private RigidbodyWorldFalloffController rigidbodyWorldFalloffController;

		// Token: 0x0400048A RID: 1162
		private RingBuffer<RigidbodyState> stateRingBuffer = new RingBuffer<RigidbodyState>(30);

		// Token: 0x0400048B RID: 1163
		private RigidbodyState resultingState;

		// Token: 0x0400048C RID: 1164
		private NetworkRigidbodyAppearanceController appearanceController;

		// Token: 0x0400048D RID: 1165
		private NetworkVariable<RigidbodyState> sleepState = new NetworkVariable<RigidbodyState>(default(RigidbodyState), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x0400048E RID: 1166
		private RigidbodyState mostRecentAwakeState;

		// Token: 0x0400048F RID: 1167
		private uint lastAwakeFrame_server;

		// Token: 0x04000490 RID: 1168
		private uint sleepIntervalOffset;

		// Token: 0x04000491 RID: 1169
		private Collider[] hitPool = new Collider[10];

		// Token: 0x04000492 RID: 1170
		private Vector3 boxExtents = new Vector3(0.5f, 0.5f, 0.5f);

		// Token: 0x04000493 RID: 1171
		private Vector3 storedRigidbodyVelocity;

		// Token: 0x04000494 RID: 1172
		private Vector3 storedRigidbodyAngularVelocity;

		// Token: 0x04000495 RID: 1173
		private RigidbodyConstraints storedConstraints;

		// Token: 0x04000496 RID: 1174
		private Rigidbody collisionClone;

		// Token: 0x04000497 RID: 1175
		private List<PhysicsForceInfo> forceQueue = new List<PhysicsForceInfo>();

		// Token: 0x04000499 RID: 1177
		private object luaObject;

		// Token: 0x0200010F RID: 271
		private enum WorldOverlapCheckType
		{
			// Token: 0x0400049B RID: 1179
			Frame,
			// Token: 0x0400049C RID: 1180
			Initialization,
			// Token: 0x0400049D RID: 1181
			Destroy
		}

		// Token: 0x02000110 RID: 272
		public interface IPhysicsFrameListener
		{
			// Token: 0x06000624 RID: 1572
			void Frame(uint frame);
		}
	}
}
