using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Props.ReferenceComponents;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x020002FC RID: 764
	public class BouncePad : EndlessNetworkBehaviour, NetClock.ISimulateFrameEnvironmentSubscriber, IStartSubscriber, IGameEndSubscriber, IBaseType, IComponentBase, IScriptInjector
	{
		// Token: 0x06001156 RID: 4438 RVA: 0x00056A14 File Offset: 0x00054C14
		private void Awake()
		{
			this.worldTrigger.OnTriggerEnter.AddListener(new UnityAction<WorldCollidable, bool>(this.HandleEntered));
			this.worldTrigger.OnTriggerExit.AddListener(new UnityAction<WorldCollidable, bool>(this.HandleExited));
			this.worldTrigger.OnTriggerEnter_Unsimulated.AddListener(new UnityAction<WorldCollidable>(this.HandleEntered_Unsimulated));
		}

		// Token: 0x06001157 RID: 4439 RVA: 0x0001CAAE File Offset: 0x0001ACAE
		private void OnEnable()
		{
			NetClock.Register(this);
		}

		// Token: 0x06001158 RID: 4440 RVA: 0x0001CAB6 File Offset: 0x0001ACB6
		private void OnDisable()
		{
			NetClock.Unregister(this);
		}

		// Token: 0x06001159 RID: 4441 RVA: 0x00056A75 File Offset: 0x00054C75
		private void HandleEntered_Unsimulated(WorldCollidable collidedObject)
		{
			if (this.playing && this.runtimeActive && NetClock.CurrentSimulationFrame == NetClock.CurrentFrame)
			{
				this.LocalBounceEffect();
			}
		}

		// Token: 0x0600115A RID: 4442 RVA: 0x00056A9C File Offset: 0x00054C9C
		public void Activate()
		{
			if (!base.IsServer)
			{
				return;
			}
			this.toggleInfo.Value = new BouncePad.ToggleInfo
			{
				Frame = NetClock.CurrentFrame + 5U,
				Active = true
			};
		}

		// Token: 0x0600115B RID: 4443 RVA: 0x00056ADC File Offset: 0x00054CDC
		public void Deactivate()
		{
			if (!base.IsServer)
			{
				return;
			}
			this.toggleInfo.Value = new BouncePad.ToggleInfo
			{
				Frame = NetClock.CurrentFrame + 5U,
				Active = false
			};
		}

		// Token: 0x0600115C RID: 4444 RVA: 0x00056B1C File Offset: 0x00054D1C
		private void ActivateLocal()
		{
			bool flag = !this.runtimeActive;
			this.runtimeActive = true;
			if (flag)
			{
				foreach (WorldCollidable worldCollidable in this.runtimeOverlaps)
				{
					this.TriggerForce(worldCollidable, true);
				}
			}
		}

		// Token: 0x0600115D RID: 4445 RVA: 0x00056B84 File Offset: 0x00054D84
		private void DeactivateLocal()
		{
			this.runtimeActive = false;
		}

		// Token: 0x0600115E RID: 4446 RVA: 0x00056B8D File Offset: 0x00054D8D
		private void ToggleActivatedLocal()
		{
			if (this.runtimeActive)
			{
				this.DeactivateLocal();
				return;
			}
			this.ActivateLocal();
		}

		// Token: 0x0600115F RID: 4447 RVA: 0x00056BA4 File Offset: 0x00054DA4
		public void ToggleActivated()
		{
			if (!base.IsServer)
			{
				return;
			}
			if (this.runtimeActive)
			{
				this.Deactivate();
				return;
			}
			this.Activate();
		}

		// Token: 0x06001160 RID: 4448 RVA: 0x00056BC4 File Offset: 0x00054DC4
		private void HandleEntered(WorldCollidable collidedObject, bool isRollbackFrame)
		{
			if (base.IsServer && collidedObject.WorldObject)
			{
				this.scriptComponent.ExecuteFunction("OnContextOverlapped", new object[] { collidedObject.WorldObject.Context });
			}
			this.runtimeOverlaps.Add(collidedObject);
			this.TriggerForce(collidedObject, !isRollbackFrame);
		}

		// Token: 0x06001161 RID: 4449 RVA: 0x00056C24 File Offset: 0x00054E24
		private void TriggerForce(WorldCollidable collidedObject, bool triggerVisuals)
		{
			if (!this.runtimeActive)
			{
				return;
			}
			if (base.IsServer)
			{
				this.LocalBounceEffect();
			}
			else if (collidedObject.IsOwner && triggerVisuals)
			{
				base.Invoke("LocalBounceEffect", NetClock.FixedDeltaTime * 2f);
			}
			if (collidedObject.PhysicsTaker != null)
			{
				float num = this.CalculateForceToReachCellHeight((float)this.runtimeHeight, collidedObject.PhysicsTaker) / collidedObject.PhysicsTaker.BlastForceMultiplier;
				global::UnityEngine.Vector3 vector = (this.references.BounceNormal ? this.references.BounceNormal.up : global::UnityEngine.Vector3.up);
				collidedObject.PhysicsTaker.TakePhysicsForce(num, vector, NetClock.CurrentSimulationFrame + 1U, base.NetworkObject.NetworkObjectId, false, true, true);
				PlayerController component = collidedObject.GetComponent<PlayerController>();
				if (component)
				{
					component.CurrentState.BouncedThisFrame();
				}
				if (base.IsServer && collidedObject.WorldObject)
				{
					this.scriptComponent.ExecuteFunction("OnContextBounced", new object[] { collidedObject.WorldObject.Context });
				}
			}
		}

		// Token: 0x06001162 RID: 4450 RVA: 0x00056D38 File Offset: 0x00054F38
		private void LocalBounceEffect()
		{
			this.references.BounceParticleEffect.Play();
		}

		// Token: 0x06001163 RID: 4451 RVA: 0x00056D4A File Offset: 0x00054F4A
		private void HandleExited(WorldCollidable collidedObject, bool isRollbackFrame)
		{
			this.runtimeOverlaps.Remove(collidedObject);
		}

		// Token: 0x06001164 RID: 4452 RVA: 0x00056D5C File Offset: 0x00054F5C
		private float CalculateForceToReachCellHeight(float cellHeight, IPhysicsTaker physicsTaker)
		{
			float num = cellHeight + 0.5f;
			return (Mathf.Sqrt(2f * physicsTaker.GravityAccelerationRate * num) - physicsTaker.CurrentVelocity.y) * Mathf.Pow(physicsTaker.Mass, 2f);
		}

		// Token: 0x06001165 RID: 4453 RVA: 0x00056DA4 File Offset: 0x00054FA4
		internal void SetBounceHeight(int value)
		{
			if (!base.IsServer)
			{
				return;
			}
			this.heightInfo.Value = new BouncePad.HeightInfo
			{
				Frame = NetClock.CurrentFrame + 5U,
				Height = Mathf.Clamp(value, 1, 35)
			};
		}

		// Token: 0x06001166 RID: 4454 RVA: 0x00056DEC File Offset: 0x00054FEC
		public override void OnNetworkSpawn()
		{
			base.OnNetworkSpawn();
			if (base.IsServer)
			{
				this.Activate();
				this.heightInfo.Value = new BouncePad.HeightInfo
				{
					Frame = NetClock.CurrentFrame,
					Height = this.heightInfo.Value.Height
				};
			}
		}

		// Token: 0x06001167 RID: 4455 RVA: 0x00056E44 File Offset: 0x00055044
		public void SimulateFrameEnvironment(uint frame)
		{
			if (!this.playing)
			{
				return;
			}
			if (!base.IsServer && frame != NetClock.CurrentFrame)
			{
				return;
			}
			if (frame >= this.toggleInfo.Value.Frame)
			{
				if (this.runtimeActive != this.toggleInfo.Value.Active)
				{
					this.ToggleActivatedLocal();
				}
			}
			else if (this.runtimeActive == this.toggleInfo.Value.Active)
			{
				this.ToggleActivatedLocal();
			}
			if (frame >= this.heightInfo.Value.Frame)
			{
				this.runtimeHeight = this.heightInfo.Value.Height;
			}
		}

		// Token: 0x06001168 RID: 4456 RVA: 0x00056EE5 File Offset: 0x000550E5
		void IStartSubscriber.EndlessStart()
		{
			this.playing = true;
		}

		// Token: 0x06001169 RID: 4457 RVA: 0x00056EEE File Offset: 0x000550EE
		void IGameEndSubscriber.EndlessGameEnd()
		{
			this.playing = false;
		}

		// Token: 0x1700036B RID: 875
		// (get) Token: 0x0600116A RID: 4458 RVA: 0x00056EF7 File Offset: 0x000550F7
		// (set) Token: 0x0600116B RID: 4459 RVA: 0x00056EFF File Offset: 0x000550FF
		public WorldObject WorldObject { get; private set; }

		// Token: 0x1700036C RID: 876
		// (get) Token: 0x0600116C RID: 4460 RVA: 0x00056F08 File Offset: 0x00055108
		public Type ComponentReferenceType
		{
			get
			{
				return typeof(BouncePadReferences);
			}
		}

		// Token: 0x1700036D RID: 877
		// (get) Token: 0x0600116D RID: 4461 RVA: 0x00056F14 File Offset: 0x00055114
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

		// Token: 0x0600116E RID: 4462 RVA: 0x00056F40 File Offset: 0x00055140
		public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
		{
			this.references = (BouncePadReferences)referenceBase;
			if (this.references.WorldTriggerArea)
			{
				foreach (Collider collider in this.references.WorldTriggerArea.CachedColliders)
				{
					collider.isTrigger = true;
					collider.gameObject.AddComponent<WorldTriggerCollider>().Initialize(this.worldTrigger);
				}
			}
		}

		// Token: 0x0600116F RID: 4463 RVA: 0x00056FA9 File Offset: 0x000551A9
		public void PrefabInitialize(WorldObject worldObject)
		{
			this.WorldObject = worldObject;
		}

		// Token: 0x1700036E RID: 878
		// (get) Token: 0x06001170 RID: 4464 RVA: 0x00056FB2 File Offset: 0x000551B2
		public object LuaObject
		{
			get
			{
				if (this.luaInterface == null)
				{
					this.luaInterface = new BouncePad(this);
				}
				return this.luaInterface;
			}
		}

		// Token: 0x1700036F RID: 879
		// (get) Token: 0x06001171 RID: 4465 RVA: 0x00056FCE File Offset: 0x000551CE
		public Type LuaObjectType
		{
			get
			{
				return typeof(BouncePad);
			}
		}

		// Token: 0x06001172 RID: 4466 RVA: 0x00056FDA File Offset: 0x000551DA
		public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
		{
			this.scriptComponent = endlessScriptComponent;
		}

		// Token: 0x06001174 RID: 4468 RVA: 0x00057038 File Offset: 0x00055238
		protected override void __initializeVariables()
		{
			bool flag = this.toggleInfo == null;
			if (flag)
			{
				throw new Exception("BouncePad.toggleInfo cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.toggleInfo.Initialize(this);
			base.__nameNetworkVariable(this.toggleInfo, "toggleInfo");
			this.NetworkVariableFields.Add(this.toggleInfo);
			flag = this.heightInfo == null;
			if (flag)
			{
				throw new Exception("BouncePad.heightInfo cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.heightInfo.Initialize(this);
			base.__nameNetworkVariable(this.heightInfo, "heightInfo");
			this.NetworkVariableFields.Add(this.heightInfo);
			base.__initializeVariables();
		}

		// Token: 0x06001175 RID: 4469 RVA: 0x0001E813 File Offset: 0x0001CA13
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x06001176 RID: 4470 RVA: 0x000570E8 File Offset: 0x000552E8
		protected internal override string __getTypeName()
		{
			return "BouncePad";
		}

		// Token: 0x04000EE6 RID: 3814
		private const float BASE_HEIGHT_ADD = 0.5f;

		// Token: 0x04000EE7 RID: 3815
		private const uint TRIGGER_FRAME_DELAY = 5U;

		// Token: 0x04000EE8 RID: 3816
		private const int MIN_BOUNCE_HEIGHT = 1;

		// Token: 0x04000EE9 RID: 3817
		private const int MAX_BOUNCE_HEIGHT = 35;

		// Token: 0x04000EEA RID: 3818
		[SerializeField]
		private WorldTrigger worldTrigger;

		// Token: 0x04000EEB RID: 3819
		internal NetworkVariable<BouncePad.ToggleInfo> toggleInfo = new NetworkVariable<BouncePad.ToggleInfo>(default(BouncePad.ToggleInfo), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04000EEC RID: 3820
		internal NetworkVariable<BouncePad.HeightInfo> heightInfo = new NetworkVariable<BouncePad.HeightInfo>(default(BouncePad.HeightInfo), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04000EED RID: 3821
		private List<WorldCollidable> runtimeOverlaps = new List<WorldCollidable>();

		// Token: 0x04000EEE RID: 3822
		private bool runtimeActive = true;

		// Token: 0x04000EEF RID: 3823
		private int runtimeHeight;

		// Token: 0x04000EF0 RID: 3824
		private bool playing;

		// Token: 0x04000EF1 RID: 3825
		private Context context;

		// Token: 0x04000EF2 RID: 3826
		[SerializeField]
		[HideInInspector]
		private BouncePadReferences references;

		// Token: 0x04000EF4 RID: 3828
		private BouncePad luaInterface;

		// Token: 0x04000EF5 RID: 3829
		private EndlessScriptComponent scriptComponent;

		// Token: 0x020002FD RID: 765
		internal struct ToggleInfo : INetworkSerializable
		{
			// Token: 0x06001177 RID: 4471 RVA: 0x000570F0 File Offset: 0x000552F0
			public void NetworkSerialize<ToggleInfo>(BufferSerializer<ToggleInfo> serializer) where ToggleInfo : IReaderWriter
			{
				serializer.SerializeValue<uint>(ref this.Frame, default(FastBufferWriter.ForPrimitives));
				serializer.SerializeValue<bool>(ref this.Active, default(FastBufferWriter.ForPrimitives));
			}

			// Token: 0x04000EF6 RID: 3830
			public uint Frame;

			// Token: 0x04000EF7 RID: 3831
			public bool Active;
		}

		// Token: 0x020002FE RID: 766
		internal struct HeightInfo : INetworkSerializable
		{
			// Token: 0x06001178 RID: 4472 RVA: 0x0005712C File Offset: 0x0005532C
			public void NetworkSerialize<HeightInfo>(BufferSerializer<HeightInfo> serializer) where HeightInfo : IReaderWriter
			{
				serializer.SerializeValue<uint>(ref this.Frame, default(FastBufferWriter.ForPrimitives));
				serializer.SerializeValue<int>(ref this.Height, default(FastBufferWriter.ForPrimitives));
			}

			// Token: 0x04000EF8 RID: 3832
			public uint Frame;

			// Token: 0x04000EF9 RID: 3833
			public int Height;
		}
	}
}
