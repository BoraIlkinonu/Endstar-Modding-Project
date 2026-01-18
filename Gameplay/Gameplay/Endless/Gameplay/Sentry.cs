using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Props.ReferenceComponents;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000335 RID: 821
	public class Sentry : EndlessNetworkBehaviour, NetClock.ISimulateFrameLateSubscriber, IStartSubscriber, IGameEndSubscriber, IBaseType, IComponentBase, IScriptInjector
	{
		// Token: 0x060013AA RID: 5034 RVA: 0x0005F0FC File Offset: 0x0005D2FC
		private void OnEnable()
		{
			NetClock.Register(this);
			NetworkVariable<Sentry.SentryState> networkVariable = this.networkState;
			networkVariable.OnValueChanged = (NetworkVariable<Sentry.SentryState>.OnValueChangedDelegate)Delegate.Combine(networkVariable.OnValueChanged, new NetworkVariable<Sentry.SentryState>.OnValueChangedDelegate(this.HandleNetworkStateChanged));
			NetworkVariable<int> networkVariable2 = this.damageLevel;
			networkVariable2.OnValueChanged = (NetworkVariable<int>.OnValueChangedDelegate)Delegate.Combine(networkVariable2.OnValueChanged, new NetworkVariable<int>.OnValueChangedDelegate(this.HandleDamageLevelChanged));
		}

		// Token: 0x060013AB RID: 5035 RVA: 0x0005F160 File Offset: 0x0005D360
		private void OnDisable()
		{
			NetClock.Unregister(this);
			NetworkVariable<Sentry.SentryState> networkVariable = this.networkState;
			networkVariable.OnValueChanged = (NetworkVariable<Sentry.SentryState>.OnValueChangedDelegate)Delegate.Remove(networkVariable.OnValueChanged, new NetworkVariable<Sentry.SentryState>.OnValueChangedDelegate(this.HandleNetworkStateChanged));
			NetworkVariable<int> networkVariable2 = this.damageLevel;
			networkVariable2.OnValueChanged = (NetworkVariable<int>.OnValueChangedDelegate)Delegate.Remove(networkVariable2.OnValueChanged, new NetworkVariable<int>.OnValueChangedDelegate(this.HandleDamageLevelChanged));
		}

		// Token: 0x060013AC RID: 5036 RVA: 0x0005F1C1 File Offset: 0x0005D3C1
		private void HandleNetworkStateChanged(Sentry.SentryState oldState, Sentry.SentryState newState)
		{
			this.localState = newState;
		}

		// Token: 0x060013AD RID: 5037 RVA: 0x0005F1CC File Offset: 0x0005D3CC
		private void HandleDamageLevelChanged(int oldState, int newState)
		{
			this.ToggleParticleSystem(this.references.SlightlyDamagedParticle.RuntimeParticleSystem, newState == 1);
			this.ToggleParticleSystem(this.references.CriticallyDamagedPartical.RuntimeParticleSystem, newState == 2);
			this.ToggleParticleSystem(this.references.DestroyedParticle.RuntimeParticleSystem, newState == 3);
		}

		// Token: 0x060013AE RID: 5038 RVA: 0x0005F228 File Offset: 0x0005D428
		private void FrameBehaviour(uint frame)
		{
			this.lastLocalFrame = frame;
			this.localState.RotationSpeed = this.RotationSpeed;
			float num = this.localState.TrackingTargetPitch;
			float num2 = this.localState.TrackingTargetYaw;
			NetworkObject networkObject;
			if (this.localState.TrackingStatus == Sentry.TrackingTargetType.Target && this.localState.TrackingTarget.TryGet(out networkObject, null))
			{
				global::UnityEngine.Vector3 eulerAngles = Quaternion.LookRotation(networkObject.GetComponent<WorldObject>().GetUserComponent<HittableComponent>().HittableColliders[0].bounds.center - this.references.SwivelTransform.position, global::UnityEngine.Vector3.up).eulerAngles;
				num = eulerAngles.x;
				num2 = eulerAngles.y;
			}
			float num3 = this.localState.RotationSpeed * NetClock.FixedDeltaTime;
			this.localState.CurrentPitch = Mathf.MoveTowardsAngle(this.localState.CurrentPitch, num, num3);
			this.localState.CurrentYaw = Mathf.MoveTowardsAngle(this.localState.CurrentYaw, num2, num3);
			this.sentryAppearanceController.SetState(this.localState);
			global::UnityEngine.Vector3 vector = Quaternion.Euler(new global::UnityEngine.Vector3(this.localState.CurrentPitch, this.localState.CurrentYaw)) * global::UnityEngine.Vector3.forward;
			if (base.IsServer && this.localState.TrackingStatus == Sentry.TrackingTargetType.Target)
			{
				int num4 = this.RaycastNonAlloc(this.references.SwivelTransform.position, vector, this.ShootDistance, this.hittableLayerMask);
				for (int i = 0; i < num4; i++)
				{
					HittableComponent hittableFromMap = MonoBehaviourSingleton<HittableMap>.Instance.GetHittableFromMap(Sentry.rayHitCache[i].collider);
					if (hittableFromMap && hittableFromMap == this.localState.TrackingTargetWorldObject.GetUserComponent<HittableComponent>())
					{
						this.scriptComponent.ExecuteFunction("OnTargetInHitScan", new object[] { this.localState.TrackingTargetWorldObject.Context });
						break;
					}
				}
			}
			if (base.IsServer && this.localState.ShootFrame == frame)
			{
				int num5 = this.RaycastNonAlloc(this.references.LaserPoint.position, vector, this.ShootDistance, this.hittableAndWallLayerMask);
				this.Sort(Sentry.rayHitCache, num5);
				for (int j = 0; j < num5; j++)
				{
					HittableComponent hittableFromMap2 = MonoBehaviourSingleton<HittableMap>.Instance.GetHittableFromMap(this.sortedRayHitList[j].collider);
					if (!hittableFromMap2)
					{
						break;
					}
					if (!(hittableFromMap2.healthComponent != null) || hittableFromMap2.healthComponent.CurrentHealth >= 1)
					{
						this.scriptComponent.ExecuteFunction("OnHit", new object[] { hittableFromMap2.WorldObject.Context });
						break;
					}
				}
				this.ShootClientRpc(vector);
			}
		}

		// Token: 0x060013AF RID: 5039 RVA: 0x0005F504 File Offset: 0x0005D704
		[ClientRpc]
		private void ShootClientRpc(global::UnityEngine.Vector3 lookRotation)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(1692595414U, clientRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe(in lookRotation);
				base.__endSendClientRpc(ref fastBufferWriter, 1692595414U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			this.sentryAppearanceController.PlayShootVisuals(lookRotation, this.localState.ShootDistance, this.localState.TrackingTarget);
		}

		// Token: 0x060013B0 RID: 5040 RVA: 0x0005F606 File Offset: 0x0005D806
		private int RaycastNonAlloc(global::UnityEngine.Vector3 position, global::UnityEngine.Vector3 direction, float distance, LayerMask layerMask)
		{
			return Physics.RaycastNonAlloc(position, direction, Sentry.rayHitCache, distance, layerMask, QueryTriggerInteraction.Ignore);
		}

		// Token: 0x060013B1 RID: 5041 RVA: 0x0005F620 File Offset: 0x0005D820
		private void Sort(RaycastHit[] array, int count)
		{
			this.sortedRayHitList.Clear();
			for (int i = 0; i < count; i++)
			{
				this.sortedRayHitList.Add(array[i]);
			}
			this.sortedRayHitList = this.sortedRayHitList.OrderBy((RaycastHit hit) => hit.distance).ToList<RaycastHit>();
		}

		// Token: 0x060013B2 RID: 5042 RVA: 0x0005F68B File Offset: 0x0005D88B
		internal void SyncState()
		{
			if (base.IsServer)
			{
				this.networkState.Value = this.localState;
			}
		}

		// Token: 0x060013B3 RID: 5043 RVA: 0x0005F6A8 File Offset: 0x0005D8A8
		public void SimulateFrameLate(uint frame)
		{
			if (!this.playmode)
			{
				this.localState.TrackingStatus = Sentry.TrackingTargetType.Direction;
				global::UnityEngine.Vector3 vector = (this.WorldObject ? this.WorldObject.transform.eulerAngles : global::UnityEngine.Vector3.zero);
				this.localState.TrackingTargetPitch = vector.x;
				this.localState.TrackingTargetYaw = vector.y;
				this.localState.CurrentPitch = this.localState.TrackingTargetPitch;
				this.localState.CurrentYaw = this.localState.TrackingTargetYaw;
				this.sentryAppearanceController.SetState(this.localState);
				return;
			}
			if (frame != NetClock.CurrentFrame)
			{
				return;
			}
			this.FrameBehaviour(frame);
		}

		// Token: 0x060013B4 RID: 5044 RVA: 0x0005F75E File Offset: 0x0005D95E
		internal void SetFollowTarget(Context context)
		{
			this.SetFollowTarget(context.WorldObject);
		}

		// Token: 0x060013B5 RID: 5045 RVA: 0x0005F76C File Offset: 0x0005D96C
		private void SetFollowTarget(WorldObject worldObject)
		{
			if (worldObject.NetworkObject != null)
			{
				this.localState.TrackingStatus = Sentry.TrackingTargetType.Target;
				this.localState.TrackingTarget = worldObject.NetworkObject;
				this.localState.TrackingTargetWorldObject = worldObject;
				this.SyncState();
			}
		}

		// Token: 0x060013B6 RID: 5046 RVA: 0x0005F7BC File Offset: 0x0005D9BC
		internal void SetLookDirection(float pitch, float yaw)
		{
			pitch = Mathf.DeltaAngle(0f, pitch);
			pitch = Sentry.ClampAngle(pitch, -89f, 89f);
			yaw = Mathf.DeltaAngle(0f, yaw + this.WorldObject.transform.eulerAngles.y);
			this.localState.TrackingStatus = Sentry.TrackingTargetType.Direction;
			this.localState.TrackingTargetPitch = pitch;
			this.localState.TrackingTargetYaw = yaw;
			this.localState.TrackingTargetWorldObject = null;
			this.SyncState();
		}

		// Token: 0x060013B7 RID: 5047 RVA: 0x0005F844 File Offset: 0x0005DA44
		public static float ClampAngle(float angle, float min, float max)
		{
			float num = (min + max) * 0.5f - 180f;
			float num2 = (float)(Mathf.FloorToInt((angle - num) / 360f) * 360);
			return Mathf.Clamp(angle, min + num2, max + num2);
		}

		// Token: 0x060013B8 RID: 5048 RVA: 0x0005F883 File Offset: 0x0005DA83
		private void ToggleParticleSystem(ParticleSystem particleSystem, bool enabled)
		{
			if (particleSystem && particleSystem.isPlaying != enabled)
			{
				if (enabled)
				{
					particleSystem.Play();
					return;
				}
				particleSystem.Stop();
			}
		}

		// Token: 0x060013B9 RID: 5049 RVA: 0x0005F8A6 File Offset: 0x0005DAA6
		void IStartSubscriber.EndlessStart()
		{
			this.lastLocalFrame = NetClock.CurrentFrame - 1U;
			this.playmode = true;
		}

		// Token: 0x060013BA RID: 5050 RVA: 0x0005F8BC File Offset: 0x0005DABC
		void IGameEndSubscriber.EndlessGameEnd()
		{
			this.playmode = false;
		}

		// Token: 0x17000408 RID: 1032
		// (get) Token: 0x060013BB RID: 5051 RVA: 0x0005F8C8 File Offset: 0x0005DAC8
		public Context Context
		{
			get
			{
				Context context;
				if ((context = this.context) == null)
				{
					context = (this.context = new Context(this.WorldObject));
				}
				return context;
			}
		}

		// Token: 0x17000409 RID: 1033
		// (get) Token: 0x060013BC RID: 5052 RVA: 0x0005F8F3 File Offset: 0x0005DAF3
		// (set) Token: 0x060013BD RID: 5053 RVA: 0x0005F8FB File Offset: 0x0005DAFB
		public WorldObject WorldObject { get; private set; }

		// Token: 0x1700040A RID: 1034
		// (get) Token: 0x060013BE RID: 5054 RVA: 0x0005F904 File Offset: 0x0005DB04
		public Type ComponentReferenceType
		{
			get
			{
				return typeof(SentryReferences);
			}
		}

		// Token: 0x060013BF RID: 5055 RVA: 0x0005F910 File Offset: 0x0005DB10
		public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
		{
			this.sentryAppearanceController.ComponentInitialize(referenceBase);
			this.references = (SentryReferences)referenceBase;
		}

		// Token: 0x060013C0 RID: 5056 RVA: 0x0005F92A File Offset: 0x0005DB2A
		public void PrefabInitialize(WorldObject worldObject)
		{
			this.WorldObject = worldObject;
		}

		// Token: 0x1700040B RID: 1035
		// (get) Token: 0x060013C1 RID: 5057 RVA: 0x0005F933 File Offset: 0x0005DB33
		public object LuaObject
		{
			get
			{
				if (this.luaInterface == null)
				{
					this.luaInterface = new Sentry(this);
				}
				return this.luaInterface;
			}
		}

		// Token: 0x1700040C RID: 1036
		// (get) Token: 0x060013C2 RID: 5058 RVA: 0x0005F94F File Offset: 0x0005DB4F
		public Type LuaObjectType
		{
			get
			{
				return typeof(Sentry);
			}
		}

		// Token: 0x060013C3 RID: 5059 RVA: 0x0005F95B File Offset: 0x0005DB5B
		public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
		{
			this.scriptComponent = endlessScriptComponent;
		}

		// Token: 0x060013C6 RID: 5062 RVA: 0x0005F9CC File Offset: 0x0005DBCC
		protected override void __initializeVariables()
		{
			bool flag = this.networkState == null;
			if (flag)
			{
				throw new Exception("Sentry.networkState cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.networkState.Initialize(this);
			base.__nameNetworkVariable(this.networkState, "networkState");
			this.NetworkVariableFields.Add(this.networkState);
			flag = this.damageLevel == null;
			if (flag)
			{
				throw new Exception("Sentry.damageLevel cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.damageLevel.Initialize(this);
			base.__nameNetworkVariable(this.damageLevel, "damageLevel");
			this.NetworkVariableFields.Add(this.damageLevel);
			base.__initializeVariables();
		}

		// Token: 0x060013C7 RID: 5063 RVA: 0x0005FA7C File Offset: 0x0005DC7C
		protected override void __initializeRpcs()
		{
			base.__registerRpc(1692595414U, new NetworkBehaviour.RpcReceiveHandler(Sentry.__rpc_handler_1692595414), "ShootClientRpc");
			base.__initializeRpcs();
		}

		// Token: 0x060013C8 RID: 5064 RVA: 0x0005FAA4 File Offset: 0x0005DCA4
		private static void __rpc_handler_1692595414(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			global::UnityEngine.Vector3 vector;
			reader.ReadValueSafe(out vector);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((Sentry)target).ShootClientRpc(vector);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060013C9 RID: 5065 RVA: 0x0005FB06 File Offset: 0x0005DD06
		protected internal override string __getTypeName()
		{
			return "Sentry";
		}

		// Token: 0x04001071 RID: 4209
		private static RaycastHit[] rayHitCache = new RaycastHit[15];

		// Token: 0x04001072 RID: 4210
		[SerializeField]
		private LayerMask hittableLayerMask;

		// Token: 0x04001073 RID: 4211
		[SerializeField]
		private LayerMask wallOnlyLayerMask;

		// Token: 0x04001074 RID: 4212
		[SerializeField]
		private LayerMask hittableAndWallLayerMask;

		// Token: 0x04001075 RID: 4213
		private readonly NetworkVariable<Sentry.SentryState> networkState = new NetworkVariable<Sentry.SentryState>(default(Sentry.SentryState), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04001076 RID: 4214
		internal readonly NetworkVariable<int> damageLevel = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04001077 RID: 4215
		private uint lastLocalFrame;

		// Token: 0x04001078 RID: 4216
		internal Sentry.SentryState localState;

		// Token: 0x04001079 RID: 4217
		[SerializeField]
		private SentryAppearanceController sentryAppearanceController;

		// Token: 0x0400107A RID: 4218
		private uint lastLocalShot;

		// Token: 0x0400107B RID: 4219
		private List<RaycastHit> sortedRayHitList = new List<RaycastHit>();

		// Token: 0x0400107C RID: 4220
		public EndlessEvent OnFacingMainTarget;

		// Token: 0x0400107D RID: 4221
		public EndlessEvent OnFacingValidTarget;

		// Token: 0x0400107E RID: 4222
		public EndlessEvent<Context> OnShotScanResult;

		// Token: 0x0400107F RID: 4223
		public EndlessEvent<Context> OnPriorityTargetSet;

		// Token: 0x04001080 RID: 4224
		internal float RotationSpeed = 100f;

		// Token: 0x04001081 RID: 4225
		internal float ShootDistance = 6f;

		// Token: 0x04001082 RID: 4226
		private bool playmode;

		// Token: 0x04001083 RID: 4227
		[SerializeField]
		[HideInInspector]
		private SentryReferences references;

		// Token: 0x04001084 RID: 4228
		private Context context;

		// Token: 0x04001086 RID: 4230
		private Sentry luaInterface;

		// Token: 0x04001087 RID: 4231
		private EndlessScriptComponent scriptComponent;

		// Token: 0x02000336 RID: 822
		public enum TrackingTargetType : byte
		{
			// Token: 0x04001089 RID: 4233
			Direction,
			// Token: 0x0400108A RID: 4234
			Target
		}

		// Token: 0x02000337 RID: 823
		public struct SentryState : INetworkSerializable, IFrameInfo
		{
			// Token: 0x1700040D RID: 1037
			// (get) Token: 0x060013CA RID: 5066 RVA: 0x0005FB0D File Offset: 0x0005DD0D
			// (set) Token: 0x060013CB RID: 5067 RVA: 0x0005FB15 File Offset: 0x0005DD15
			public uint NetFrame { readonly get; set; }

			// Token: 0x060013CC RID: 5068 RVA: 0x00002DB0 File Offset: 0x00000FB0
			void IFrameInfo.Clear()
			{
			}

			// Token: 0x060013CD RID: 5069 RVA: 0x00002DB0 File Offset: 0x00000FB0
			void IFrameInfo.Initialize()
			{
			}

			// Token: 0x060013CE RID: 5070 RVA: 0x0005FB20 File Offset: 0x0005DD20
			public SentryState(NetworkObject target, uint frame, float currentPitch, float currentYaw, uint shootFrame, float rotationSpeed, float shootDistance, bool trackingLaser)
			{
				this.TrackingStatus = Sentry.TrackingTargetType.Target;
				this.TrackingTarget = new NetworkObjectReference(target);
				this.NetFrame = frame;
				this.CurrentPitch = currentPitch;
				this.CurrentYaw = currentYaw;
				this.ShootFrame = shootFrame;
				this.TrackingTargetPitch = 0f;
				this.TrackingTargetYaw = 0f;
				this.RotationSpeed = rotationSpeed;
				this.TrackingTargetWorldObject = null;
				this.ShootDistance = shootDistance;
				this.TrackingLaserEnabled = trackingLaser;
			}

			// Token: 0x060013CF RID: 5071 RVA: 0x0005FB94 File Offset: 0x0005DD94
			public SentryState(float trackingTargetPitch, float trackingTargetYaw, uint frame, float currentPitch, float currentYaw, uint shootFrame, float rotationSpeed, float shootDistance, bool trackingLaser)
			{
				this.TrackingStatus = Sentry.TrackingTargetType.Direction;
				this.TrackingTarget = default(NetworkObjectReference);
				this.TrackingTargetPitch = trackingTargetPitch;
				this.TrackingTargetYaw = trackingTargetYaw;
				this.CurrentPitch = currentPitch;
				this.CurrentYaw = currentYaw;
				this.ShootFrame = shootFrame;
				this.NetFrame = frame;
				this.RotationSpeed = rotationSpeed;
				this.TrackingTargetWorldObject = null;
				this.ShootDistance = shootDistance;
				this.TrackingLaserEnabled = trackingLaser;
			}

			// Token: 0x060013D0 RID: 5072 RVA: 0x0005FC00 File Offset: 0x0005DE00
			public void NetworkSerialize<SentryState>(BufferSerializer<SentryState> serializer) where SentryState : IReaderWriter
			{
				serializer.SerializeValue<NetworkObjectReference>(ref this.TrackingTarget, default(FastBufferWriter.ForNetworkSerializable));
				serializer.SerializeValue<Sentry.TrackingTargetType>(ref this.TrackingStatus, default(FastBufferWriter.ForEnums));
				if (serializer.IsWriter)
				{
					Compression.SerializeFloatToUShort<SentryState>(serializer, this.TrackingTargetPitch, -180f, 180f);
					Compression.SerializeFloatToUShort<SentryState>(serializer, this.TrackingTargetYaw, -180f, 180f);
					Compression.SerializeUInt<SentryState>(serializer, this.NetFrame);
					Compression.SerializeUIntToByteClamped<SentryState>(serializer, this.ShootFrame);
					Compression.SerializeFloatToUShort<SentryState>(serializer, this.CurrentPitch, -180f, 180f);
					Compression.SerializeFloatToUShort<SentryState>(serializer, this.CurrentYaw, -180f, 180f);
					Compression.SerializeFloatToUShort<SentryState>(serializer, this.RotationSpeed, 0f, 500f);
					Compression.SerializeFloatToUShort<SentryState>(serializer, this.ShootDistance, 0f, 200f);
					serializer.SerializeValue<bool>(ref this.TrackingLaserEnabled, default(FastBufferWriter.ForPrimitives));
					return;
				}
				this.TrackingTargetPitch = Compression.DeserializeFloatFromUShort<SentryState>(serializer, -180f, 180f);
				this.TrackingTargetYaw = Compression.DeserializeFloatFromUShort<SentryState>(serializer, -180f, 180f);
				this.NetFrame = Compression.DeserializeUInt<SentryState>(serializer);
				this.ShootFrame = Compression.DeserializeUIntFromByteClamped<SentryState>(serializer);
				this.CurrentPitch = Compression.DeserializeFloatFromUShort<SentryState>(serializer, -180f, 180f);
				this.CurrentYaw = Compression.DeserializeFloatFromUShort<SentryState>(serializer, -180f, 180f);
				this.RotationSpeed = Compression.DeserializeFloatFromUShort<SentryState>(serializer, 0f, 500f);
				this.ShootDistance = Compression.DeserializeFloatFromUShort<SentryState>(serializer, 0f, 200f);
				serializer.SerializeValue<bool>(ref this.TrackingLaserEnabled, default(FastBufferWriter.ForPrimitives));
			}

			// Token: 0x0400108B RID: 4235
			public Sentry.TrackingTargetType TrackingStatus;

			// Token: 0x0400108C RID: 4236
			public NetworkObjectReference TrackingTarget;

			// Token: 0x0400108D RID: 4237
			public float TrackingTargetPitch;

			// Token: 0x0400108E RID: 4238
			public float TrackingTargetYaw;

			// Token: 0x0400108F RID: 4239
			public float RotationSpeed;

			// Token: 0x04001090 RID: 4240
			public float ShootDistance;

			// Token: 0x04001091 RID: 4241
			public bool TrackingLaserEnabled;

			// Token: 0x04001093 RID: 4243
			public float CurrentPitch;

			// Token: 0x04001094 RID: 4244
			public float CurrentYaw;

			// Token: 0x04001095 RID: 4245
			public uint ShootFrame;

			// Token: 0x04001096 RID: 4246
			public WorldObject TrackingTargetWorldObject;
		}
	}
}
