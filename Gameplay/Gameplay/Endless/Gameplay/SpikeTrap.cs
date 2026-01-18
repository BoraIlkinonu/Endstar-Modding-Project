using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Props.ReferenceComponents;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay
{
	// Token: 0x0200033E RID: 830
	public class SpikeTrap : EndlessNetworkBehaviour, IGameEndSubscriber, IAwakeSubscriber, NetClock.ISimulateFrameEnvironmentSubscriber, IPersistantStateSubscriber, IBaseType, IComponentBase, IScriptInjector
	{
		// Token: 0x17000410 RID: 1040
		// (get) Token: 0x060013ED RID: 5101 RVA: 0x000606CB File Offset: 0x0005E8CB
		private uint painFrames
		{
			get
			{
				return (uint)Mathf.Max(1, Mathf.RoundToInt(this.stabDuration / NetClock.FixedDeltaTime));
			}
		}

		// Token: 0x060013EE RID: 5102 RVA: 0x000606E4 File Offset: 0x0005E8E4
		private void Awake()
		{
			NetworkVariable<uint> networkVariable = this.triggerFrame;
			networkVariable.OnValueChanged = (NetworkVariable<uint>.OnValueChangedDelegate)Delegate.Combine(networkVariable.OnValueChanged, new NetworkVariable<uint>.OnValueChangedDelegate(this.TriggerFrameUpdated));
		}

		// Token: 0x060013EF RID: 5103 RVA: 0x0006070D File Offset: 0x0005E90D
		protected override void Start()
		{
			base.Start();
			NetClock.Register(this);
		}

		// Token: 0x060013F0 RID: 5104 RVA: 0x0006071B File Offset: 0x0005E91B
		public override void OnDestroy()
		{
			base.OnDestroy();
			NetClock.Unregister(this);
		}

		// Token: 0x060013F1 RID: 5105 RVA: 0x00060729 File Offset: 0x0005E929
		public virtual void EndlessAwake()
		{
			this.damageTrigger.OnTriggerEnter.AddListener(new UnityAction<WorldCollidable, bool>(this.HandleTriggerEntered));
			this.damageTrigger.OnTriggerStay.AddListener(new UnityAction<WorldCollidable, bool>(this.HandleTriggerStay));
		}

		// Token: 0x060013F2 RID: 5106 RVA: 0x00060763 File Offset: 0x0005E963
		private void TriggerFrameUpdated(uint oldValue, uint newValue)
		{
			this.triggeredLocally = false;
		}

		// Token: 0x060013F3 RID: 5107 RVA: 0x0006076C File Offset: 0x0005E96C
		public void EndlessGameEnd()
		{
			if (this.stabCoroutine != null)
			{
				base.StopCoroutine(this.stabCoroutine);
				this.stabCoroutine = null;
				this.MoveSpikes(0f);
			}
			this.hitEntities.Clear();
			if (base.IsServer)
			{
				this.triggerOnSense.Value = true;
			}
		}

		// Token: 0x060013F4 RID: 5108 RVA: 0x000607BE File Offset: 0x0005E9BE
		private IEnumerator StabCoroutine()
		{
			double frameTime = NetClock.GetFrameTime(this.triggerFrame.Value);
			this.OnTriggered.Invoke(this.mostRecentOverlap ? this.mostRecentOverlap.WorldObject.Context : this.WorldObject.Context);
			double num;
			if (base.IsServer)
			{
				num = NetClock.LocalNetworkTime;
			}
			else
			{
				num = NetClock.ClientExtrapolatedAppearanceTime;
			}
			float elapsedTime = (float)(num - frameTime);
			double totalAnimationTime = (double)(this.stabDuration + this.retractDuration);
			while ((double)elapsedTime < totalAnimationTime)
			{
				this.isStabbing = elapsedTime < this.stabDuration;
				if (this.isStabbing)
				{
					this.MoveSpikes(this.stabSpeedCurve.Evaluate(elapsedTime / this.stabDuration));
				}
				else
				{
					this.MoveSpikes(this.retractSpeedCurve.Evaluate((elapsedTime - this.stabDuration) / this.retractDuration));
				}
				yield return null;
				elapsedTime += Time.deltaTime;
			}
			this.MoveSpikes(0f);
			this.isStabbing = false;
			this.stabCoroutine = null;
			this.hitEntities.Clear();
			yield break;
		}

		// Token: 0x060013F5 RID: 5109 RVA: 0x000607CD File Offset: 0x0005E9CD
		private void MoveSpikes(float percentage)
		{
			this.references.SpikeTransform.position = global::UnityEngine.Vector3.Lerp(this.references.RetractedPosition.position, this.references.StabPosition.position, percentage);
		}

		// Token: 0x060013F6 RID: 5110 RVA: 0x00060805 File Offset: 0x0005EA05
		private void HandleTriggerEntered(WorldCollidable worldCollidable, bool isRollbackFrame)
		{
			this.HandleOverlap(worldCollidable, isRollbackFrame);
		}

		// Token: 0x060013F7 RID: 5111 RVA: 0x00060805 File Offset: 0x0005EA05
		private void HandleTriggerStay(WorldCollidable worldCollidable, bool isRollbackFrame)
		{
			this.HandleOverlap(worldCollidable, isRollbackFrame);
		}

		// Token: 0x060013F8 RID: 5112 RVA: 0x00060810 File Offset: 0x0005EA10
		private void HandleOverlap(WorldCollidable worldCollidable, bool isRollbackFrame)
		{
			if (base.IsServer && this.triggerOnSense.Value)
			{
				this.TriggerSpikes();
			}
			if (NetClock.CurrentSimulationFrame >= this.triggerFrame.Value && NetClock.CurrentSimulationFrame < this.painFrames + this.triggerFrame.Value)
			{
				this.TryApplyDamage(worldCollidable);
			}
		}

		// Token: 0x060013F9 RID: 5113 RVA: 0x0006086C File Offset: 0x0005EA6C
		private void TryApplyDamage(WorldCollidable hitReference)
		{
			if (hitReference.WorldObject == null || this.hitEntities.Contains(hitReference))
			{
				return;
			}
			this.hitEntities.Add(hitReference);
			HittableComponent hittableComponent;
			if (hitReference.WorldObject.TryGetUserComponent<HittableComponent>(out hittableComponent) && (base.IsServer || hittableComponent.WorldObject.NetworkObject.IsOwner))
			{
				this.OnEntityHit.Invoke(hitReference.WorldObject.Context);
				hittableComponent.ModifyHealth(new HealthModificationArgs(-this.spikeDamage, this.WorldObject, DamageType.Normal, HealthChangeType.Damage));
			}
		}

		// Token: 0x060013FA RID: 5114 RVA: 0x000608FC File Offset: 0x0005EAFC
		internal void TriggerSpikes()
		{
			if (base.IsServer && NetClock.CurrentFrame > this.triggerFrame.Value && this.stabCoroutine == null)
			{
				this.triggerFrame.Value = NetClock.CurrentFrame + this.triggerDelayFrames;
			}
		}

		// Token: 0x060013FB RID: 5115 RVA: 0x00060938 File Offset: 0x0005EB38
		public void SimulateFrameEnvironment(uint frame)
		{
			if (MonoBehaviourSingleton<GameplayManager>.Instance.IsPlaying && !this.triggeredLocally && frame >= this.triggerFrame.Value)
			{
				if (this.stabCoroutine != null)
				{
					base.StopCoroutine(this.stabCoroutine);
					this.stabCoroutine = null;
				}
				this.stabCoroutine = base.StartCoroutine(this.StabCoroutine());
				this.triggeredLocally = true;
			}
		}

		// Token: 0x17000411 RID: 1041
		// (get) Token: 0x060013FC RID: 5116 RVA: 0x00017586 File Offset: 0x00015786
		bool IPersistantStateSubscriber.ShouldSaveAndLoad
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060013FD RID: 5117 RVA: 0x0006099B File Offset: 0x0005EB9B
		object IPersistantStateSubscriber.GetSaveState()
		{
			return new ValueTuple<int, bool>(this.spikeDamage, this.triggerOnSense.Value);
		}

		// Token: 0x060013FE RID: 5118 RVA: 0x000609B8 File Offset: 0x0005EBB8
		void IPersistantStateSubscriber.LoadState(object loadedState)
		{
			if (loadedState != null)
			{
				ValueTuple<int, bool> valueTuple = (ValueTuple<int, bool>)loadedState;
				this.spikeDamage = valueTuple.Item1;
				if (base.IsServer)
				{
					this.triggerOnSense.Value = valueTuple.Item2;
				}
			}
		}

		// Token: 0x17000412 RID: 1042
		// (get) Token: 0x060013FF RID: 5119 RVA: 0x000609F4 File Offset: 0x0005EBF4
		// (set) Token: 0x06001400 RID: 5120 RVA: 0x000609FC File Offset: 0x0005EBFC
		public WorldObject WorldObject { get; private set; }

		// Token: 0x17000413 RID: 1043
		// (get) Token: 0x06001401 RID: 5121 RVA: 0x00060A05 File Offset: 0x0005EC05
		public Type ComponentReferenceType
		{
			get
			{
				return typeof(SpikeTrapReferences);
			}
		}

		// Token: 0x17000414 RID: 1044
		// (get) Token: 0x06001402 RID: 5122 RVA: 0x00060A14 File Offset: 0x0005EC14
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

		// Token: 0x06001403 RID: 5123 RVA: 0x00060A40 File Offset: 0x0005EC40
		public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
		{
			this.references = (SpikeTrapReferences)referenceBase;
			if (this.references.WorldTriggerArea)
			{
				foreach (Collider collider in this.references.WorldTriggerArea.CachedColliders)
				{
					collider.isTrigger = true;
					collider.gameObject.AddComponent<WorldTriggerCollider>().Initialize(this.damageTrigger);
				}
			}
		}

		// Token: 0x06001404 RID: 5124 RVA: 0x00060AA9 File Offset: 0x0005ECA9
		public void PrefabInitialize(WorldObject worldObject)
		{
			this.WorldObject = worldObject;
		}

		// Token: 0x17000415 RID: 1045
		// (get) Token: 0x06001405 RID: 5125 RVA: 0x00060AB2 File Offset: 0x0005ECB2
		public object LuaObject
		{
			get
			{
				if (this.luaInterface == null)
				{
					this.luaInterface = new SpikeTrap(this);
				}
				return this.luaInterface;
			}
		}

		// Token: 0x17000416 RID: 1046
		// (get) Token: 0x06001406 RID: 5126 RVA: 0x00060ACE File Offset: 0x0005ECCE
		public Type LuaObjectType
		{
			get
			{
				return typeof(SpikeTrap);
			}
		}

		// Token: 0x06001407 RID: 5127 RVA: 0x00060ADA File Offset: 0x0005ECDA
		public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
		{
			this.scriptComponent = endlessScriptComponent;
		}

		// Token: 0x06001409 RID: 5129 RVA: 0x00060B60 File Offset: 0x0005ED60
		protected override void __initializeVariables()
		{
			bool flag = this.triggerOnSense == null;
			if (flag)
			{
				throw new Exception("SpikeTrap.triggerOnSense cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.triggerOnSense.Initialize(this);
			base.__nameNetworkVariable(this.triggerOnSense, "triggerOnSense");
			this.NetworkVariableFields.Add(this.triggerOnSense);
			flag = this.triggerFrame == null;
			if (flag)
			{
				throw new Exception("SpikeTrap.triggerFrame cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.triggerFrame.Initialize(this);
			base.__nameNetworkVariable(this.triggerFrame, "triggerFrame");
			this.NetworkVariableFields.Add(this.triggerFrame);
			base.__initializeVariables();
		}

		// Token: 0x0600140A RID: 5130 RVA: 0x0001E813 File Offset: 0x0001CA13
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x0600140B RID: 5131 RVA: 0x00060C10 File Offset: 0x0005EE10
		protected internal override string __getTypeName()
		{
			return "SpikeTrap";
		}

		// Token: 0x040010B7 RID: 4279
		[Tooltip("Should start at 0, end at 1, and take 1 second")]
		[SerializeField]
		private AnimationCurve stabSpeedCurve;

		// Token: 0x040010B8 RID: 4280
		[SerializeField]
		private float stabDuration = 1f;

		// Token: 0x040010B9 RID: 4281
		[Tooltip("Should start at 1, end at 0, and take 1 second")]
		[SerializeField]
		private AnimationCurve retractSpeedCurve;

		// Token: 0x040010BA RID: 4282
		[SerializeField]
		private float retractDuration = 1f;

		// Token: 0x040010BB RID: 4283
		[SerializeField]
		private WorldTrigger damageTrigger;

		// Token: 0x040010BC RID: 4284
		[Header("Frames")]
		[SerializeField]
		private uint triggerDelayFrames = 12U;

		// Token: 0x040010BD RID: 4285
		internal NetworkVariable<bool> triggerOnSense = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x040010BE RID: 4286
		private Coroutine stabCoroutine;

		// Token: 0x040010BF RID: 4287
		private NetworkVariable<uint> triggerFrame = new NetworkVariable<uint>(0U, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x040010C0 RID: 4288
		private bool triggeredLocally = true;

		// Token: 0x040010C1 RID: 4289
		private Context scriptingContext;

		// Token: 0x040010C2 RID: 4290
		private bool isStabbing;

		// Token: 0x040010C3 RID: 4291
		internal int spikeDamage = 1;

		// Token: 0x040010C4 RID: 4292
		public EndlessEvent OnEntityHit = new EndlessEvent();

		// Token: 0x040010C5 RID: 4293
		public EndlessEvent OnTriggered = new EndlessEvent();

		// Token: 0x040010C6 RID: 4294
		private WorldCollidable mostRecentOverlap;

		// Token: 0x040010C7 RID: 4295
		private List<WorldCollidable> hitEntities = new List<WorldCollidable>();

		// Token: 0x040010C8 RID: 4296
		private Context context;

		// Token: 0x040010CA RID: 4298
		[SerializeField]
		[HideInInspector]
		private SpikeTrapReferences references;

		// Token: 0x040010CB RID: 4299
		private SpikeTrap luaInterface;

		// Token: 0x040010CC RID: 4300
		private EndlessScriptComponent scriptComponent;
	}
}
