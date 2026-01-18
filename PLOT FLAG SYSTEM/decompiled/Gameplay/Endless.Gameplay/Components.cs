using System;
using System.Collections.Generic;
using Endless.Gameplay.Fsm;
using Endless.Gameplay.LuaEnums;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay;

public class Components : NpcComponent, NetClock.ISimulateFrameActorsSubscriber
{
	private HealthComponent healthComponent;

	private HittableComponent hittableComponent;

	private TextBubble textBubble;

	private TargeterComponent targeterComponent;

	[HideInInspector]
	public Transform EmitterPoint;

	[HideInInspector]
	public AttackComponent Attack;

	[field: SerializeField]
	public NavMeshAgent Agent { get; private set; }

	[field: SerializeField]
	public CharacterController CharacterController { get; private set; }

	[field: SerializeField]
	public Animator Animator { get; private set; }

	public PathingComponent Pathing { get; private set; }

	public PathFollower PathFollower { get; private set; }

	public GroundingComponent Grounding { get; private set; }

	public VelocityTracker VelocityTracker { get; private set; }

	public PlanFollower PlanFollower { get; private set; }

	public GoapController GoapController { get; private set; }

	public SlopeComponent SlopeComponent { get; private set; }

	public MonoBehaviorProxy Proxy { get; private set; }

	public FsmParameters Parameters { get; private set; }

	public NpcAnimator NpcAnimator { get; private set; }

	public DynamicAttributes DynamicAttributes { get; private set; }

	public NpcVisuals NpcVisuals { get; private set; }

	public HealthComponent Health => healthComponent ?? (healthComponent = base.NpcEntity.WorldObject.GetUserComponent<HealthComponent>());

	public HittableComponent HittableComponent => hittableComponent ?? (hittableComponent = base.NpcEntity.WorldObject.GetUserComponent<HittableComponent>());

	public TextBubble TextBubble => textBubble ?? (textBubble = base.NpcEntity.WorldObject.GetUserComponent<TextBubble>());

	public TargeterComponent TargeterComponent => targeterComponent ?? (targeterComponent = base.NpcEntity.WorldObject.GetUserComponent<TargeterComponent>());

	[field: SerializeField]
	public ClassDataList ClassDataList { get; private set; }

	[field: SerializeField]
	public DefaultBehaviorList DefaultBehaviorList { get; private set; }

	[field: SerializeField]
	public IndividualStateUpdater IndividualStateUpdater { get; private set; }

	[field: SerializeField]
	public WorldCollidable WorldCollidable { get; private set; }

	[field: SerializeField]
	public PhysicsTaker PhysicsTaker { get; private set; }

	[field: SerializeField]
	public NpcInteractable Interactable { get; private set; }

	[field: SerializeField]
	public ProjectileShooter ProjectileShooter { get; private set; }

	[field: SerializeField]
	public AttackAlert AttackAlert { get; private set; }

	[field: SerializeField]
	public AnimationEvents AnimationEvents { get; private set; }

	public MaterialModifier MaterialModifier => NpcVisuals.MaterialModifier;

	[field: SerializeField]
	public Transform VisualRoot { get; private set; }

	protected override void Start()
	{
		base.Start();
		NetClock.Register(this);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		NetClock.Unregister(this);
	}

	public void InitializeVisuals()
	{
		NpcVisuals = new NpcVisuals(base.NpcEntity.WorldObject.EndlessVisuals, this, base.NpcEntity);
	}

	public void InitializeComponents()
	{
		Agent.updateRotation = false;
		CharacterController.enableOverlapRecovery = false;
		CharacterController.enabled = true;
		InstantiateAttackComponent();
		if (base.IsServer)
		{
			Proxy = base.NpcEntity.gameObject.AddComponent<MonoBehaviorProxy>();
			PlanFollower = new PlanFollower(base.NpcEntity);
			GoapController = new GoapController(base.NpcEntity, ClassDataList, DefaultBehaviorList, base.NpcEntity.Settings.ReplanFrames);
			GoapController.SetDefaultBehavior();
			DynamicAttributes = new DynamicAttributes(base.NpcEntity, new List<IAttributeSourceController> { base.NpcEntity, GoapController });
			PathFollower = new PathFollower(base.NpcEntity, IndividualStateUpdater, Agent, DynamicAttributes, Proxy);
			Pathing = new PathingComponent(base.NpcEntity, PathFollower, Proxy);
			Grounding = new GroundingComponent(Agent, IndividualStateUpdater);
			VelocityTracker = new VelocityTracker(base.NpcEntity.transform, Animator, Agent, base.NpcEntity, Proxy, IndividualStateUpdater);
			SlopeComponent = new SlopeComponent(base.NpcEntity.transform, base.NpcEntity.Settings.GroundCollisionMask, IndividualStateUpdater);
			Parameters = new FsmParameters(IndividualStateUpdater);
			NpcAnimator = new NpcAnimator(base.NpcEntity, Animator, AnimationEvents);
			new RotationWriter(base.NpcEntity.transform, IndividualStateUpdater);
			new NpcAnimationWriter(Animator, IndividualStateUpdater);
			new FalloffChecker(base.NpcEntity.transform, hittableComponent, base.NpcEntity, IndividualStateUpdater);
			HittableComponent.IsDamageable = base.NpcEntity.DamageMode == DamageMode.TakeDamage;
			TargeterComponent targeterComponent = this.targeterComponent;
			targeterComponent.CombatWeight = base.NpcEntity.NpcClass.NpcClass switch
			{
				NpcClass.Blank => 1f, 
				NpcClass.Grunt => 1f, 
				NpcClass.Rifleman => 0.5f, 
				NpcClass.Zombie => 0.5f, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
		else
		{
			new PositionReader(base.NpcEntity.transform, IndividualStateUpdater);
			new RotationReader(base.NpcEntity.transform, base.NpcEntity.Settings, IndividualStateUpdater);
			new NpcAnimationReader(Animator, IndividualStateUpdater);
		}
	}

	private void InstantiateAttackComponent()
	{
		if (!ClassDataList.TryGetClassData(base.NpcEntity.NpcClass.NpcClass, out var classData))
		{
			ClassDataList.TryGetClassData(NpcClass.Blank, out classData);
			Debug.LogException(new Exception($"No class data associated with the AiClass {classData.Class}, Initializing Ai as a blank Class"));
		}
		if ((bool)classData.AttackComponent)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(classData.AttackComponent, base.transform);
			Attack = gameObject.GetComponentInChildren<AttackComponent>();
			if ((bool)Attack)
			{
				gameObject.name = Attack.GetType().Name;
			}
		}
		if (classData.Class == NpcClass.Rifleman)
		{
			TargeterComponent.isNavigationDependent = false;
			base.NpcEntity.IsRangedAttacker = true;
		}
		else
		{
			TargeterComponent.isNavigationDependent = true;
			TargeterComponent.range = base.NpcEntity.PathfindingRange;
		}
	}

	public void SimulateFrameActors(uint frame)
	{
		PathFollower?.Tick();
	}
}
