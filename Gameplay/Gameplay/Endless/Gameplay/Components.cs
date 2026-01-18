using System;
using System.Collections.Generic;
using Endless.Gameplay.Fsm;
using Endless.Gameplay.LuaEnums;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay
{
	// Token: 0x02000131 RID: 305
	public class Components : NpcComponent, NetClock.ISimulateFrameActorsSubscriber
	{
		// Token: 0x17000138 RID: 312
		// (get) Token: 0x060006E3 RID: 1763 RVA: 0x00021857 File Offset: 0x0001FA57
		// (set) Token: 0x060006E4 RID: 1764 RVA: 0x0002185F File Offset: 0x0001FA5F
		public NavMeshAgent Agent { get; private set; }

		// Token: 0x17000139 RID: 313
		// (get) Token: 0x060006E5 RID: 1765 RVA: 0x00021868 File Offset: 0x0001FA68
		// (set) Token: 0x060006E6 RID: 1766 RVA: 0x00021870 File Offset: 0x0001FA70
		public CharacterController CharacterController { get; private set; }

		// Token: 0x1700013A RID: 314
		// (get) Token: 0x060006E7 RID: 1767 RVA: 0x00021879 File Offset: 0x0001FA79
		// (set) Token: 0x060006E8 RID: 1768 RVA: 0x00021881 File Offset: 0x0001FA81
		public Animator Animator { get; private set; }

		// Token: 0x1700013B RID: 315
		// (get) Token: 0x060006E9 RID: 1769 RVA: 0x0002188A File Offset: 0x0001FA8A
		// (set) Token: 0x060006EA RID: 1770 RVA: 0x00021892 File Offset: 0x0001FA92
		public PathingComponent Pathing { get; private set; }

		// Token: 0x1700013C RID: 316
		// (get) Token: 0x060006EB RID: 1771 RVA: 0x0002189B File Offset: 0x0001FA9B
		// (set) Token: 0x060006EC RID: 1772 RVA: 0x000218A3 File Offset: 0x0001FAA3
		public PathFollower PathFollower { get; private set; }

		// Token: 0x1700013D RID: 317
		// (get) Token: 0x060006ED RID: 1773 RVA: 0x000218AC File Offset: 0x0001FAAC
		// (set) Token: 0x060006EE RID: 1774 RVA: 0x000218B4 File Offset: 0x0001FAB4
		public GroundingComponent Grounding { get; private set; }

		// Token: 0x1700013E RID: 318
		// (get) Token: 0x060006EF RID: 1775 RVA: 0x000218BD File Offset: 0x0001FABD
		// (set) Token: 0x060006F0 RID: 1776 RVA: 0x000218C5 File Offset: 0x0001FAC5
		public VelocityTracker VelocityTracker { get; private set; }

		// Token: 0x1700013F RID: 319
		// (get) Token: 0x060006F1 RID: 1777 RVA: 0x000218CE File Offset: 0x0001FACE
		// (set) Token: 0x060006F2 RID: 1778 RVA: 0x000218D6 File Offset: 0x0001FAD6
		public PlanFollower PlanFollower { get; private set; }

		// Token: 0x17000140 RID: 320
		// (get) Token: 0x060006F3 RID: 1779 RVA: 0x000218DF File Offset: 0x0001FADF
		// (set) Token: 0x060006F4 RID: 1780 RVA: 0x000218E7 File Offset: 0x0001FAE7
		public GoapController GoapController { get; private set; }

		// Token: 0x17000141 RID: 321
		// (get) Token: 0x060006F5 RID: 1781 RVA: 0x000218F0 File Offset: 0x0001FAF0
		// (set) Token: 0x060006F6 RID: 1782 RVA: 0x000218F8 File Offset: 0x0001FAF8
		public SlopeComponent SlopeComponent { get; private set; }

		// Token: 0x17000142 RID: 322
		// (get) Token: 0x060006F7 RID: 1783 RVA: 0x00021901 File Offset: 0x0001FB01
		// (set) Token: 0x060006F8 RID: 1784 RVA: 0x00021909 File Offset: 0x0001FB09
		public MonoBehaviorProxy Proxy { get; private set; }

		// Token: 0x17000143 RID: 323
		// (get) Token: 0x060006F9 RID: 1785 RVA: 0x00021912 File Offset: 0x0001FB12
		// (set) Token: 0x060006FA RID: 1786 RVA: 0x0002191A File Offset: 0x0001FB1A
		public FsmParameters Parameters { get; private set; }

		// Token: 0x17000144 RID: 324
		// (get) Token: 0x060006FB RID: 1787 RVA: 0x00021923 File Offset: 0x0001FB23
		// (set) Token: 0x060006FC RID: 1788 RVA: 0x0002192B File Offset: 0x0001FB2B
		public NpcAnimator NpcAnimator { get; private set; }

		// Token: 0x17000145 RID: 325
		// (get) Token: 0x060006FD RID: 1789 RVA: 0x00021934 File Offset: 0x0001FB34
		// (set) Token: 0x060006FE RID: 1790 RVA: 0x0002193C File Offset: 0x0001FB3C
		public DynamicAttributes DynamicAttributes { get; private set; }

		// Token: 0x17000146 RID: 326
		// (get) Token: 0x060006FF RID: 1791 RVA: 0x00021945 File Offset: 0x0001FB45
		// (set) Token: 0x06000700 RID: 1792 RVA: 0x0002194D File Offset: 0x0001FB4D
		public NpcVisuals NpcVisuals { get; private set; }

		// Token: 0x17000147 RID: 327
		// (get) Token: 0x06000701 RID: 1793 RVA: 0x00021958 File Offset: 0x0001FB58
		public HealthComponent Health
		{
			get
			{
				HealthComponent healthComponent;
				if ((healthComponent = this.healthComponent) == null)
				{
					healthComponent = (this.healthComponent = base.NpcEntity.WorldObject.GetUserComponent<HealthComponent>());
				}
				return healthComponent;
			}
		}

		// Token: 0x17000148 RID: 328
		// (get) Token: 0x06000702 RID: 1794 RVA: 0x00021988 File Offset: 0x0001FB88
		public HittableComponent HittableComponent
		{
			get
			{
				HittableComponent hittableComponent;
				if ((hittableComponent = this.hittableComponent) == null)
				{
					hittableComponent = (this.hittableComponent = base.NpcEntity.WorldObject.GetUserComponent<HittableComponent>());
				}
				return hittableComponent;
			}
		}

		// Token: 0x17000149 RID: 329
		// (get) Token: 0x06000703 RID: 1795 RVA: 0x000219B8 File Offset: 0x0001FBB8
		public TextBubble TextBubble
		{
			get
			{
				TextBubble textBubble;
				if ((textBubble = this.textBubble) == null)
				{
					textBubble = (this.textBubble = base.NpcEntity.WorldObject.GetUserComponent<TextBubble>());
				}
				return textBubble;
			}
		}

		// Token: 0x1700014A RID: 330
		// (get) Token: 0x06000704 RID: 1796 RVA: 0x000219E8 File Offset: 0x0001FBE8
		public TargeterComponent TargeterComponent
		{
			get
			{
				TargeterComponent targeterComponent;
				if ((targeterComponent = this.targeterComponent) == null)
				{
					targeterComponent = (this.targeterComponent = base.NpcEntity.WorldObject.GetUserComponent<TargeterComponent>());
				}
				return targeterComponent;
			}
		}

		// Token: 0x1700014B RID: 331
		// (get) Token: 0x06000705 RID: 1797 RVA: 0x00021A18 File Offset: 0x0001FC18
		// (set) Token: 0x06000706 RID: 1798 RVA: 0x00021A20 File Offset: 0x0001FC20
		public ClassDataList ClassDataList { get; private set; }

		// Token: 0x1700014C RID: 332
		// (get) Token: 0x06000707 RID: 1799 RVA: 0x00021A29 File Offset: 0x0001FC29
		// (set) Token: 0x06000708 RID: 1800 RVA: 0x00021A31 File Offset: 0x0001FC31
		public DefaultBehaviorList DefaultBehaviorList { get; private set; }

		// Token: 0x1700014D RID: 333
		// (get) Token: 0x06000709 RID: 1801 RVA: 0x00021A3A File Offset: 0x0001FC3A
		// (set) Token: 0x0600070A RID: 1802 RVA: 0x00021A42 File Offset: 0x0001FC42
		public IndividualStateUpdater IndividualStateUpdater { get; private set; }

		// Token: 0x1700014E RID: 334
		// (get) Token: 0x0600070B RID: 1803 RVA: 0x00021A4B File Offset: 0x0001FC4B
		// (set) Token: 0x0600070C RID: 1804 RVA: 0x00021A53 File Offset: 0x0001FC53
		public WorldCollidable WorldCollidable { get; private set; }

		// Token: 0x1700014F RID: 335
		// (get) Token: 0x0600070D RID: 1805 RVA: 0x00021A5C File Offset: 0x0001FC5C
		// (set) Token: 0x0600070E RID: 1806 RVA: 0x00021A64 File Offset: 0x0001FC64
		public PhysicsTaker PhysicsTaker { get; private set; }

		// Token: 0x17000150 RID: 336
		// (get) Token: 0x0600070F RID: 1807 RVA: 0x00021A6D File Offset: 0x0001FC6D
		// (set) Token: 0x06000710 RID: 1808 RVA: 0x00021A75 File Offset: 0x0001FC75
		public NpcInteractable Interactable { get; private set; }

		// Token: 0x17000151 RID: 337
		// (get) Token: 0x06000711 RID: 1809 RVA: 0x00021A7E File Offset: 0x0001FC7E
		// (set) Token: 0x06000712 RID: 1810 RVA: 0x00021A86 File Offset: 0x0001FC86
		public ProjectileShooter ProjectileShooter { get; private set; }

		// Token: 0x17000152 RID: 338
		// (get) Token: 0x06000713 RID: 1811 RVA: 0x00021A8F File Offset: 0x0001FC8F
		// (set) Token: 0x06000714 RID: 1812 RVA: 0x00021A97 File Offset: 0x0001FC97
		public AttackAlert AttackAlert { get; private set; }

		// Token: 0x17000153 RID: 339
		// (get) Token: 0x06000715 RID: 1813 RVA: 0x00021AA0 File Offset: 0x0001FCA0
		// (set) Token: 0x06000716 RID: 1814 RVA: 0x00021AA8 File Offset: 0x0001FCA8
		public AnimationEvents AnimationEvents { get; private set; }

		// Token: 0x17000154 RID: 340
		// (get) Token: 0x06000717 RID: 1815 RVA: 0x00021AB1 File Offset: 0x0001FCB1
		public MaterialModifier MaterialModifier
		{
			get
			{
				return this.NpcVisuals.MaterialModifier;
			}
		}

		// Token: 0x17000155 RID: 341
		// (get) Token: 0x06000718 RID: 1816 RVA: 0x00021ABE File Offset: 0x0001FCBE
		// (set) Token: 0x06000719 RID: 1817 RVA: 0x00021AC6 File Offset: 0x0001FCC6
		public Transform VisualRoot { get; private set; }

		// Token: 0x0600071A RID: 1818 RVA: 0x00021ACF File Offset: 0x0001FCCF
		protected override void Start()
		{
			base.Start();
			NetClock.Register(this);
		}

		// Token: 0x0600071B RID: 1819 RVA: 0x00021ADD File Offset: 0x0001FCDD
		protected override void OnDestroy()
		{
			base.OnDestroy();
			NetClock.Unregister(this);
		}

		// Token: 0x0600071C RID: 1820 RVA: 0x00021AEB File Offset: 0x0001FCEB
		public void InitializeVisuals()
		{
			this.NpcVisuals = new NpcVisuals(base.NpcEntity.WorldObject.EndlessVisuals, this, base.NpcEntity);
		}

		// Token: 0x0600071D RID: 1821 RVA: 0x00021B10 File Offset: 0x0001FD10
		public void InitializeComponents()
		{
			this.Agent.updateRotation = false;
			this.CharacterController.enableOverlapRecovery = false;
			this.CharacterController.enabled = true;
			this.InstantiateAttackComponent();
			if (base.IsServer)
			{
				this.Proxy = base.NpcEntity.gameObject.AddComponent<MonoBehaviorProxy>();
				this.PlanFollower = new PlanFollower(base.NpcEntity);
				this.GoapController = new GoapController(base.NpcEntity, this.ClassDataList, this.DefaultBehaviorList, base.NpcEntity.Settings.ReplanFrames);
				this.GoapController.SetDefaultBehavior();
				this.DynamicAttributes = new DynamicAttributes(base.NpcEntity, new List<IAttributeSourceController> { base.NpcEntity, this.GoapController });
				this.PathFollower = new PathFollower(base.NpcEntity, this.IndividualStateUpdater, this.Agent, this.DynamicAttributes, this.Proxy);
				this.Pathing = new PathingComponent(base.NpcEntity, this.PathFollower, this.Proxy);
				this.Grounding = new GroundingComponent(this.Agent, this.IndividualStateUpdater);
				this.VelocityTracker = new VelocityTracker(base.NpcEntity.transform, this.Animator, this.Agent, base.NpcEntity, this.Proxy, this.IndividualStateUpdater);
				this.SlopeComponent = new SlopeComponent(base.NpcEntity.transform, base.NpcEntity.Settings.GroundCollisionMask, this.IndividualStateUpdater);
				this.Parameters = new FsmParameters(this.IndividualStateUpdater);
				this.NpcAnimator = new NpcAnimator(base.NpcEntity, this.Animator, this.AnimationEvents);
				new RotationWriter(base.NpcEntity.transform, this.IndividualStateUpdater);
				new NpcAnimationWriter(this.Animator, this.IndividualStateUpdater);
				new FalloffChecker(base.NpcEntity.transform, this.hittableComponent, base.NpcEntity, this.IndividualStateUpdater);
				this.HittableComponent.IsDamageable = base.NpcEntity.DamageMode == DamageMode.TakeDamage;
				TargeterComponent targeterComponent = this.targeterComponent;
				float num;
				switch (base.NpcEntity.NpcClass.NpcClass)
				{
				case NpcClass.Blank:
					num = 1f;
					break;
				case NpcClass.Grunt:
					num = 1f;
					break;
				case NpcClass.Rifleman:
					num = 0.5f;
					break;
				case NpcClass.Zombie:
					num = 0.5f;
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
				targeterComponent.CombatWeight = num;
				return;
			}
			new PositionReader(base.NpcEntity.transform, this.IndividualStateUpdater);
			new RotationReader(base.NpcEntity.transform, base.NpcEntity.Settings, this.IndividualStateUpdater);
			new NpcAnimationReader(this.Animator, this.IndividualStateUpdater);
		}

		// Token: 0x0600071E RID: 1822 RVA: 0x00021DD8 File Offset: 0x0001FFD8
		private void InstantiateAttackComponent()
		{
			ClassData classData;
			if (!this.ClassDataList.TryGetClassData(base.NpcEntity.NpcClass.NpcClass, out classData))
			{
				this.ClassDataList.TryGetClassData(NpcClass.Blank, out classData);
				Debug.LogException(new Exception(string.Format("No class data associated with the AiClass {0}, Initializing Ai as a blank Class", classData.Class)));
			}
			if (classData.AttackComponent)
			{
				GameObject gameObject = global::UnityEngine.Object.Instantiate<GameObject>(classData.AttackComponent, base.transform);
				this.Attack = gameObject.GetComponentInChildren<AttackComponent>();
				if (this.Attack)
				{
					gameObject.name = this.Attack.GetType().Name;
				}
			}
			if (classData.Class == NpcClass.Rifleman)
			{
				this.TargeterComponent.isNavigationDependent = false;
				base.NpcEntity.IsRangedAttacker = true;
				return;
			}
			this.TargeterComponent.isNavigationDependent = true;
			this.TargeterComponent.range = base.NpcEntity.PathfindingRange;
		}

		// Token: 0x0600071F RID: 1823 RVA: 0x00021EC4 File Offset: 0x000200C4
		public void SimulateFrameActors(uint frame)
		{
			PathFollower pathFollower = this.PathFollower;
			if (pathFollower == null)
			{
				return;
			}
			pathFollower.Tick();
		}

		// Token: 0x04000598 RID: 1432
		private HealthComponent healthComponent;

		// Token: 0x04000599 RID: 1433
		private HittableComponent hittableComponent;

		// Token: 0x0400059A RID: 1434
		private TextBubble textBubble;

		// Token: 0x0400059B RID: 1435
		private TargeterComponent targeterComponent;

		// Token: 0x040005B5 RID: 1461
		[HideInInspector]
		public Transform EmitterPoint;

		// Token: 0x040005B6 RID: 1462
		[HideInInspector]
		public AttackComponent Attack;
	}
}
