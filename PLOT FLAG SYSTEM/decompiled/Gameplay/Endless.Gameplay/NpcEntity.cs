using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Data;
using Endless.Gameplay.Fsm;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Matchmaking;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay;

public class NpcEntity : EndlessNetworkBehaviour, IAttributeSourceController, IAwakeSubscriber, IGameEndSubscriber, IPersistantStateSubscriber, IBaseType, IComponentBase, IScriptInjector
{
	private class PersistenceData
	{
		public readonly UnityEngine.Vector3 InitialLocation;

		public readonly Quaternion InitialRotation;

		public readonly UnityEngine.Vector3 MostRecentLocation;

		public readonly float MostRecentRotation;

		public readonly List<Vector3Int> CurrentNodes;

		public PersistenceData(UnityEngine.Vector3 mostRecentLocation, float mostRecentRotation, UnityEngine.Vector3 initialLocation, Quaternion initialRotation, List<Vector3Int> currentNodes)
		{
			MostRecentLocation = mostRecentLocation;
			MostRecentRotation = mostRecentRotation;
			InitialLocation = initialLocation;
			InitialRotation = initialRotation;
			CurrentNodes = currentNodes;
		}
	}

	public const float DISTANCE_TOLERANCE = 0.2f;

	public const uint MAX_DOWN_FRAMES = 100u;

	public const uint EXTRAPOLATION_FRAMES = 8u;

	public const float OPTIMAL_ATTACK_DISTANCE = 1.25f;

	public const float MAX_RANGED_ATTACK_DISTANCE = 60f;

	private const float CLOSE_DISTANCE = 1f;

	private const float MELEE_DISTANCE = 1.5f;

	private const float NEAR_DISTANCE = 2f;

	private const float AROUND_DISTANCE = 4f;

	private const float IMMUNE_TARGET_REDUCTION = 0.1f;

	private const float MAX_IMMUNE_TARGET_REDUCTION = 0.5f;

	public const uint LANDING_FRAMES = 4u;

	[SerializeField]
	public Components Components;

	[SerializeField]
	[SerializeReference]
	private NpcClassCustomizationData npcClass = new GruntNpcCustomizationData();

	[SerializeField]
	private NpcGroup group;

	[SerializeField]
	private PropCombatMode combatMode;

	[SerializeField]
	private PropDamageMode damageMode;

	[SerializeField]
	private PropPhysicsMode physicsMode;

	[SerializeField]
	private CharacterVisualsReference characterVisuals;

	public uint RangedAttackFrames = 12u;

	public bool FinishedSpawnAnimation;

	private NpcEnum.CombatState combatState;

	private NpcEnum.PropFallMode fallMode;

	private PropMovementMode movementMode = PropMovementMode.Run;

	public Action OnNpcDespawned;

	private CombatMode baseCombatMode;

	private DamageMode baseDamageMode;

	private PhysicsMode basePhysicsMode;

	private NpcEnum.FallMode baseFallMode;

	private MovementMode baseMovementMode;

	private bool isDynamicSpawn;

	private NpcConfiguration configuration;

	private readonly Dictionary<HittableComponent, float> scoreReductionDictionary = new Dictionary<HittableComponent, float>();

	private FsmState currentState;

	public EndlessEvent<Context> OnEntityHealthZeroed = new EndlessEvent<Context>();

	public EndlessEvent<Context> OnTargetHealthZeroed = new EndlessEvent<Context>();

	public List<GoapAction.ActionKind> Actions;

	private bool haveSpawnedVisuals;

	public NetworkVariable<DamageMode> NetworkedDamageMode = new NetworkVariable<DamageMode>(DamageMode.TakeDamage);

	public NetworkVariable<PhysicsMode> NetworkedPhysicsMode = new NetworkVariable<PhysicsMode>(PhysicsMode.TakePhysics);

	public string CurrentStateName;

	public UnityEngine.Vector3 LastKnownTargetLocation;

	private uint downedFrame;

	private IInteractionBehavior interactionBehavior;

	private PersistenceData persistenceData;

	private Context context;

	private Npc luaInterface;

	[SerializeField]
	[HideInInspector]
	private EndlessScriptComponent scriptComponent;

	public static NavMeshQueryFilter NavFilter => new NavMeshQueryFilter
	{
		agentTypeID = 0,
		areaMask = -1
	};

	[field: SerializeField]
	public NpcSettings Settings { get; private set; }

	[field: SerializeField]
	public IdleBehavior IdleBehavior { get; private set; }

	[field: SerializeField]
	public PathfindingRange PathfindingRange { get; private set; }

	[field: SerializeField]
	public int StartingHealth { get; private set; }

	[field: SerializeField]
	public AnimationClipSet Fidgets { get; private set; }

	[field: SerializeField]
	public AnimationClipSet CombatFidgets { get; private set; }

	[field: SerializeField]
	public AnimationClipSet TauntClipSet { get; private set; }

	public NpcSpawnAnimation SpawnAnimation { get; private set; }

	public CharacterVisualsReference CharacterVisuals
	{
		get
		{
			return characterVisuals;
		}
		set
		{
			if (!(characterVisuals.Id == value.Id))
			{
				characterVisuals = value;
				CharacterCosmeticsDefinition characterCosmeticsDefinition = characterVisuals?.GetDefinition();
				if ((object)characterVisuals != null && (bool)characterCosmeticsDefinition && !characterCosmeticsDefinition.IsMissingAsset)
				{
					haveSpawnedVisuals = true;
					Components.NpcVisuals.UpdateVisuals(characterCosmeticsDefinition);
				}
			}
		}
	}

	public bool IsInteractable
	{
		get
		{
			NpcEnum.FsmState state = CurrentState.State;
			return state == NpcEnum.FsmState.Neutral || state == NpcEnum.FsmState.Interaction;
		}
	}

	public FsmState CurrentState
	{
		get
		{
			return currentState;
		}
		set
		{
			if (currentState != value)
			{
				currentState = value;
				CurrentStateName = value.StateName;
			}
		}
	}

	public NpcClassCustomizationData NpcClass
	{
		get
		{
			return npcClass;
		}
		set
		{
			if (!MatchSession.Instance.MatchData.IsEditSession || (MatchSession.Instance.MatchData.IsEditSession && !NetworkManager.Singleton.IsHost))
			{
				if (!EndlessCloudService.CanHaveRiflemen() && value.NpcClass == Endless.Gameplay.LuaEnums.NpcClass.Rifleman)
				{
					ErrorHandler.HandleError(ErrorCodes.NpcEntity_NpcClasSet_ContentRestricted_Rifleman, new Exception("Content Restricted: Account is not allowed to have riflemen."), displayModal: true, leaveMatch: true);
				}
				else if (!EndlessCloudService.CanHaveGrunt() && value.NpcClass == Endless.Gameplay.LuaEnums.NpcClass.Grunt)
				{
					ErrorHandler.HandleError(ErrorCodes.NpcEntity_NpcClasSet_ContentRestricted_Grunt, new Exception("Content Restricted: Account is not allowed to have grunts."), displayModal: true, leaveMatch: true);
				}
				else if (!EndlessCloudService.CanHaveZombies() && value.NpcClass == Endless.Gameplay.LuaEnums.NpcClass.Zombie)
				{
					ErrorHandler.HandleError(ErrorCodes.NpcEntity_NpcClasSet_ContentRestricted_Zombie, new Exception("Content Restricted: Account is not allowed to have zombies."), displayModal: true, leaveMatch: true);
				}
			}
			if (npcClass != value)
			{
				npcClass = value;
				Components.NpcVisuals.RespawnCosmetics();
			}
		}
	}

	public uint IdleCompleteFrame { get; set; }

	public NpcGroup Group
	{
		get
		{
			return group;
		}
		set
		{
			if (value != group)
			{
				NpcGroup oldGroup = group;
				group = value;
				MonoBehaviourSingleton<NpcManager>.Instance.UpdateNpcGroup(this, oldGroup);
			}
		}
	}

	public NpcBlackboard NpcBlackboard { get; } = new NpcBlackboard();

	public bool CanDoubleJump => false;

	public bool IsRangedAttacker { get; set; }

	public Team Team => WorldObject.GetUserComponent<TeamComponent>().Team;

	public bool IsDowned => CurrentState.State == NpcEnum.FsmState.Downed;

	public float AnimationSpeed => NpcBlackboard.GetValueOrDefault(NpcBlackboard.Key.OverrideSpeed, Components.Agent.speed);

	public int Health => Components.Health.CurrentHealth;

	public NpcEnum.CombatState CombatState
	{
		get
		{
			return combatState;
		}
		set
		{
			combatState = value;
			this.OnCombatStateChanged?.Invoke();
		}
	}

	public HittableComponent Target => Components.TargeterComponent.Target;

	public HittableComponent FollowTarget { get; set; }

	public bool HasAttackToken { get; set; }

	public AttackRequest CurrentRequest { get; set; }

	public UnityEngine.Vector3 Position
	{
		get
		{
			return base.transform.position;
		}
		set
		{
			base.transform.position = value;
		}
	}

	public float Rotation
	{
		get
		{
			return base.transform.rotation.y;
		}
		set
		{
			base.transform.rotation = Quaternion.Euler(0f, value, 0f);
		}
	}

	public UnityEngine.Vector3 FootPosition
	{
		get
		{
			if (NavMesh.SamplePosition(Position + UnityEngine.Vector3.down * 0.5f, out var hit, 0.25f, -1))
			{
				return hit.position;
			}
			return Position + UnityEngine.Vector3.down * 0.5f;
		}
	}

	internal CombatMode BaseCombatMode
	{
		get
		{
			return baseCombatMode;
		}
		set
		{
			if (baseCombatMode != value)
			{
				baseCombatMode = value;
				this.OnBaseAttributeChanged?.Invoke();
			}
		}
	}

	internal DamageMode BaseDamageMode
	{
		get
		{
			return baseDamageMode;
		}
		set
		{
			DamageMode damageMode = value;
			if (damageMode == DamageMode.UseDefault)
			{
				damageMode = DamageMode.TakeDamage;
			}
			if (baseDamageMode != damageMode)
			{
				baseDamageMode = damageMode;
				this.OnBaseAttributeChanged?.Invoke();
			}
		}
	}

	internal PhysicsMode BasePhysicsMode
	{
		get
		{
			return basePhysicsMode;
		}
		set
		{
			if (basePhysicsMode != value)
			{
				basePhysicsMode = value;
				this.OnBaseAttributeChanged?.Invoke();
			}
		}
	}

	internal NpcEnum.FallMode BaseFallMode
	{
		get
		{
			return baseFallMode;
		}
		set
		{
			if (baseFallMode != value)
			{
				baseFallMode = value;
				this.OnBaseAttributeChanged?.Invoke();
			}
		}
	}

	internal MovementMode BaseMovementMode
	{
		get
		{
			return baseMovementMode;
		}
		set
		{
			if (baseMovementMode != value)
			{
				baseMovementMode = value;
				this.OnBaseAttributeChanged?.Invoke();
			}
		}
	}

	public CombatMode CombatMode => Components.DynamicAttributes.CombatMode;

	public DamageMode DamageMode => Components.DynamicAttributes.DamageMode;

	public PhysicsMode PhysicsMode => Components.DynamicAttributes.PhysicsMode;

	public NpcEnum.FallMode FallMode => Components.DynamicAttributes.FallMode;

	public MovementMode MovementMode => Components.DynamicAttributes.MovementMode;

	public float CloseDistance => NpcBlackboard.GetValueOrDefault(NpcBlackboard.Key.CloseDistance, 1f);

	public float OptimalAttackDistance => NpcBlackboard.GetValueOrDefault(NpcBlackboard.Key.OptimalAttackDistance, 1.25f);

	public float MeleeDistance => NpcBlackboard.GetValueOrDefault(NpcBlackboard.Key.MeleeDistance, 1.5f);

	public float NearDistance => NpcBlackboard.GetValueOrDefault(NpcBlackboard.Key.NearDistance, 2f);

	public float AroundDistance => NpcBlackboard.GetValueOrDefault(NpcBlackboard.Key.AroundDistance, 4f);

	public float MaxRangedAttackDistance => NpcBlackboard.GetValueOrDefault(NpcBlackboard.Key.MaxRangedAttackDistance, 60f);

	public bool CanFidget
	{
		get
		{
			bool value;
			return !NpcBlackboard.TryGet<bool>(NpcBlackboard.Key.CanFidget, out value) || value;
		}
	}

	public float DestinationTolerance
	{
		get
		{
			if (NpcBlackboard.TryGet<float>(NpcBlackboard.Key.DestinationTolerance, out var value))
			{
				return value;
			}
			return 1f;
		}
	}

	public bool IsConfigured => configuration != null;

	public bool LostTarget { get; set; }

	public IInteractionBehavior InteractionBehavior
	{
		get
		{
			return interactionBehavior;
		}
		private set
		{
			if (interactionBehavior != value)
			{
				interactionBehavior = value;
				this.OnAttributeSourceChanged?.Invoke();
			}
		}
	}

	public bool ShouldSaveAndLoad => !isDynamicSpawn;

	[field: SerializeField]
	[field: HideInInspector]
	public WorldObject WorldObject { get; private set; }

	public Type ComponentReferenceType => typeof(NpcEntity);

	public ReferenceFilter Filter => ReferenceFilter.NonStatic | ReferenceFilter.Npc;

	public Context Context => context ?? (context = new Context(WorldObject));

	public NavType NavValue => NavType.Intangible;

	public object LuaObject => luaInterface ?? (luaInterface = new Npc(this));

	public Type LuaObjectType => typeof(Npc);

	public event Action OnBaseAttributeChanged;

	public event Action OnCombatStateChanged;

	public event Action OnAttributeSourceChanged;

	private void Awake()
	{
		WorldObject.EndlessVisuals.FadeOnStart = false;
		Components.InitializeVisuals();
	}

	protected override void Start()
	{
		base.Start();
		if (!haveSpawnedVisuals)
		{
			haveSpawnedVisuals = true;
			Components.NpcVisuals.UpdateVisuals(MonoBehaviourSingleton<DefaultContentManager>.Instance.DefaultCharacterCosmetics.Cosmetics[7]);
		}
	}

	internal void Teleport(TeleportType teleportType, UnityEngine.Vector3 position, float rotation)
	{
		NpcBlackboard.Set(NpcBlackboard.Key.TeleportType, (int)teleportType);
		NpcBlackboard.Set(NpcBlackboard.Key.TeleportPosition, position);
		NpcBlackboard.Set(NpcBlackboard.Key.TeleportRotation, rotation);
		Components.Parameters.TeleportTrigger = true;
	}

	protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer)
	{
		if (serializer.IsWriter && configuration != null)
		{
			serializer.GetFastBufferWriter().WriteValueSafe<NetworkableNpcConfig>(new NetworkableNpcConfig(configuration), default(FastBufferWriter.ForNetworkSerializable));
		}
		if (serializer.IsReader)
		{
			serializer.GetFastBufferReader().ReadValueSafe(out NetworkableNpcConfig value, default(FastBufferWriter.ForNetworkSerializable));
			if (value != null)
			{
				configuration = new NpcConfiguration(value);
				UpdateNpcSettings(configuration);
			}
		}
		base.OnSynchronize(ref serializer);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)MonoBehaviourSingleton<NpcManager>.Instance)
		{
			MonoBehaviourSingleton<NpcManager>.Instance.RemoveNpc(this);
		}
		if ((bool)MonoBehaviourSingleton<CombatManager>.Instance)
		{
			MonoBehaviourSingleton<CombatManager>.Instance.UnregisterCombatNpc(this);
		}
	}

	public void StopInteraction(Context context)
	{
		InteractionBehavior?.InteractionStopped(Context, context);
	}

	public void Despawn()
	{
		OnNpcDespawned?.Invoke();
		MonoBehaviourSingleton<NpcManager>.Instance.RemoveNpc(this);
		MonoBehaviourSingleton<TargetingManager>.Instance.RemoveTargetable(Components.HittableComponent);
		if (base.IsServer)
		{
			RaiseOnDiedEvent();
			UnityEngine.Object.Destroy(WorldObject.gameObject);
		}
	}

	public void Hit(Knockback knockback)
	{
		if (base.IsServer && PhysicsMode == PhysicsMode.TakePhysics)
		{
			Components.PhysicsTaker.TakePhysicsForce(knockback.Force, PlayerController.AngleToForwardMotion(knockback.Angle).normalized, knockback.StartFrame, knockback.Source);
		}
	}

	public void RaiseOnModifiedObjectHealthEvent(HealthChangeResult result, Context hitContext)
	{
		if (!base.NetworkManager.IsServer)
		{
			return;
		}
		NpcEntity entity;
		if (result == HealthChangeResult.HealthZeroed)
		{
			OnEntityHealthZeroed?.Invoke(Context, hitContext);
			if (Target != null && hitContext.WorldObject == Target.WorldObject)
			{
				OnTargetHealthZeroed?.Invoke(Context, Target.WorldObject.Context);
			}
			if (npcClass is ZombieNpcCustomizationData { ZombifyTarget: not false } && Team != Team.None && hitContext.IsNpc())
			{
				entity = hitContext.WorldObject.GetUserComponent<NpcEntity>();
				if (entity.npcClass.NpcClass != Endless.Gameplay.LuaEnums.NpcClass.Zombie)
				{
					NpcEntity npcEntity = entity;
					npcEntity.OnNpcDespawned = (Action)Delegate.Combine(npcEntity.OnNpcDespawned, new Action(HandleOnNpcDespawned));
				}
			}
		}
		if (result == HealthChangeResult.NoChange && hitContext.WorldObject.TryGetUserComponent<HittableComponent>(out var component))
		{
			if (scoreReductionDictionary.TryGetValue(component, out var value))
			{
				if (value < 0.5f)
				{
					scoreReductionDictionary[component] += 0.1f;
				}
			}
			else
			{
				scoreReductionDictionary[component] = 0.1f;
				StartCoroutine(DiminishScoreReductionRoutine(component, 3f, 0.1f));
			}
		}
		scriptComponent.TryExecuteFunction("OnModifiedObjectHealth", out var _, hitContext, (int)result);
		void HandleOnNpcDespawned()
		{
			if (!scriptComponent.TryExecuteFunction("GetZombieConfiguration", out NpcConfiguration returnValue))
			{
				returnValue = new NpcConfiguration(this);
			}
			returnValue.SpawnAnimation = 3;
			returnValue.NpcVisuals = entity.characterVisuals;
			returnValue.CombatMode = 2;
			MonoBehaviourSingleton<NpcManager>.Instance.SpawnNpcAtPosition(entity.Position, entity.transform.rotation, returnValue);
		}
	}

	private IEnumerator DiminishScoreReductionRoutine(HittableComponent hittableComponent, float reductionDelay, float reductionAmount)
	{
		if (!scoreReductionDictionary.ContainsKey(hittableComponent))
		{
			yield break;
		}
		do
		{
			yield return new WaitForSeconds(reductionDelay);
			if (!hittableComponent)
			{
				yield break;
			}
			scoreReductionDictionary[hittableComponent] -= reductionAmount;
		}
		while (!(scoreReductionDictionary[hittableComponent] <= 0f));
		scoreReductionDictionary.Remove(hittableComponent);
	}

	private void RaiseOnDiedEvent()
	{
		if (base.NetworkManager.IsServer)
		{
			scriptComponent.TryExecuteFunction("OnNpcDied", out var _);
		}
	}

	public void BindInstantiatedWorldObject()
	{
		WorldObject = GetComponent<WorldObject>();
	}

	public void SetInteractionBehavior(IInteractionBehavior newInteractionBehavior)
	{
		InteractionBehavior?.RescindInstruction(Context);
		InteractionBehavior = newInteractionBehavior;
	}

	public void ClearInteractionBehavior()
	{
		InteractionBehavior = null;
	}

	public List<INpcAttributeModifier> GetAttributeModifiers()
	{
		return new List<INpcAttributeModifier> { InteractionBehavior };
	}

	public void EndlessAwake()
	{
		if (!base.IsServer)
		{
			SetupClientNpc();
			Components.InitializeComponents();
		}
		else
		{
			Components.InitializeComponents();
			Components.DynamicAttributes.OnDamageModeChanged += HandleOnDamageModeChanged;
			Components.TargeterComponent.TargetScoreModifier = TargetScoreModifier;
			Components.Health.OnHealthZeroed_Internal.AddListener(delegate
			{
				MonoBehaviourSingleton<StageManager>.Instance.PropDestroyed(MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetInstanceIdFromGameObject(WorldObject.gameObject));
			});
			Components.Health.OnHealthZeroed_Internal.AddListener(delegate
			{
				downedFrame = NetClock.CurrentFrame;
			});
			if (persistenceData == null)
			{
				persistenceData = new PersistenceData(UnityEngine.Vector3.zero, 0f, Position, WorldObject.transform.rotation, new List<Vector3Int>());
			}
			else
			{
				if (Health <= 0)
				{
					UnityEngine.Object.Destroy(this);
					return;
				}
				Position = persistenceData.MostRecentLocation;
				foreach (Vector3Int currentNode in persistenceData.CurrentNodes)
				{
					MonoBehaviourSingleton<NodeMap>.Instance.InstructionNodesByCellPosition[currentNode].GiveInstruction(Context);
				}
				base.transform.rotation = Quaternion.Euler(new UnityEngine.Vector3(0f, persistenceData.MostRecentRotation, 0f));
			}
			if (!isDynamicSpawn)
			{
				Components.Health.SetMaxHealth(Context, StartingHealth);
				configuration = new NpcConfiguration(this);
				BaseCombatMode = (CombatMode)combatMode;
				BaseDamageMode = (DamageMode)damageMode;
				BasePhysicsMode = (PhysicsMode)physicsMode;
				BaseFallMode = (NpcEnum.FallMode)fallMode;
				BaseMovementMode = (MovementMode)movementMode;
			}
			else
			{
				Components.TextBubble.ShouldSaveAndLoad = false;
				scriptComponent.ShouldSaveAndLoad = false;
			}
			if (SpawnAnimation == NpcSpawnAnimation.None)
			{
				FinishedSpawnAnimation = true;
			}
			SetClassBlackboardValues();
			MonoBehaviourSingleton<NpcManager>.Instance.RegisterNewNpc(this);
			FsmBuilder.BuildFsm(this);
			Components.Animator.SetBool(NpcAnimator.Walking, MovementMode == MovementMode.Walk);
			Components.DynamicAttributes.OnMovementModeChanged += delegate
			{
				Components.Animator.SetBool(NpcAnimator.Walking, MovementMode == MovementMode.Walk);
			};
			Components.DynamicAttributes.OnCombatModeChanged += HandleOnCombatModeChanged;
			Components.TargeterComponent.OnTargetChanged += HandleOnTargetChanged;
			Components.IndividualStateUpdater.OnTickAi += HandleOnTickAi;
			if (npcClass.NpcClass != Endless.Gameplay.LuaEnums.NpcClass.Blank)
			{
				if (combatMode == PropCombatMode.Passive)
				{
					MonoBehaviourSingleton<CombatManager>.Instance.UnregisterCombatNpc(this);
				}
				else
				{
					MonoBehaviourSingleton<CombatManager>.Instance.RegisterCombatNpc(this);
				}
			}
		}
		if (!MatchSession.Instance.MatchData.IsEditSession || !NetworkManager.Singleton.IsHost)
		{
			if (!EndlessCloudService.CanHaveRiflemen() && NpcClass.NpcClass == Endless.Gameplay.LuaEnums.NpcClass.Rifleman)
			{
				ErrorHandler.HandleError(ErrorCodes.NpcEntity_EndlessAwake_ContentRestricted_Rifleman, new Exception("Content Restricted: Account is not allowed to have riflemen."), displayModal: true, leaveMatch: true);
			}
			if (!EndlessCloudService.CanHaveGrunt() && NpcClass.NpcClass == Endless.Gameplay.LuaEnums.NpcClass.Grunt)
			{
				ErrorHandler.HandleError(ErrorCodes.NpcEntity_EndlessAwake_ContentRestricted_Grunt, new Exception("Content Restricted: Account is not allowed to have grunts."), displayModal: true, leaveMatch: true);
			}
			if (!EndlessCloudService.CanHaveZombies() && NpcClass.NpcClass == Endless.Gameplay.LuaEnums.NpcClass.Zombie)
			{
				ErrorHandler.HandleError(ErrorCodes.NpcEntity_EndlessAwake_ContentRestricted_Zombie, new Exception("Content Restricted: Account is not allowed to have zombies."), displayModal: true, leaveMatch: true);
			}
		}
	}

	private void HandleOnCombatModeChanged()
	{
		if (npcClass.NpcClass != Endless.Gameplay.LuaEnums.NpcClass.Blank)
		{
			if (combatMode == PropCombatMode.Passive)
			{
				MonoBehaviourSingleton<CombatManager>.Instance.UnregisterCombatNpc(this);
			}
			else
			{
				MonoBehaviourSingleton<CombatManager>.Instance.RegisterCombatNpc(this);
			}
		}
	}

	private void HandleOnTickAi()
	{
		if ((bool)Target)
		{
			LastKnownTargetLocation = Target.NavPosition;
		}
		if (Health <= 0 && NetClock.CurrentFrame > downedFrame + 100)
		{
			Despawn();
		}
	}

	private void HandleOnTargetChanged(HittableComponent newTarget)
	{
		if (!newTarget && Components.TargeterComponent.LastTarget.WorldObject.TryGetUserComponent<HealthComponent>(out var component) && component.CurrentHealth > 0)
		{
			LostTarget = true;
		}
	}

	private float TargetScoreModifier(HittableComponent target, float currentScore)
	{
		if (!scoreReductionDictionary.TryGetValue(target, out var value))
		{
			return currentScore;
		}
		return currentScore * (1f - value);
	}

	private void SetupClientNpc()
	{
		if (!IsConfigured)
		{
			configuration = new NpcConfiguration(this);
			BaseCombatMode = (CombatMode)combatMode;
			BaseDamageMode = (DamageMode)damageMode;
			BasePhysicsMode = (PhysicsMode)physicsMode;
			BaseFallMode = (NpcEnum.FallMode)fallMode;
			BaseMovementMode = (MovementMode)movementMode;
		}
	}

	private void SetClassBlackboardValues()
	{
		switch (NpcClass.NpcClass)
		{
		case Endless.Gameplay.LuaEnums.NpcClass.Rifleman:
			NpcBlackboard.Set(NpcBlackboard.Key.AroundDistance, 80f);
			NpcBlackboard.Set(NpcBlackboard.Key.MeleeDistance, 80f);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case Endless.Gameplay.LuaEnums.NpcClass.Blank:
		case Endless.Gameplay.LuaEnums.NpcClass.Grunt:
		case Endless.Gameplay.LuaEnums.NpcClass.Zombie:
			break;
		}
	}

	[ClientRpc]
	public void ConfigureSpawnedNpc_ClientRpc(NetworkableNpcConfig config, ClientRpcParams clientRpcParams = default(ClientRpcParams))
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			FastBufferWriter bufferWriter = __beginSendClientRpc(1532021543u, clientRpcParams, RpcDelivery.Reliable);
			bool value = config != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(in config, default(FastBufferWriter.ForNetworkSerializable));
			}
			__endSendClientRpc(ref bufferWriter, 1532021543u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			configuration = new NpcConfiguration(config);
			UpdateNpcSettings(configuration);
			isDynamicSpawn = true;
			if (base.IsServer)
			{
				Components.Health.SetMaxHealth(null, config.Health);
			}
			WorldObject.GetUserComponent<TeamComponent>().Team = (Team)config.Team;
		}
	}

	private void UpdateNpcSettings(NpcConfiguration config)
	{
		CharacterVisuals = config.NpcVisuals;
		if (base.IsServer)
		{
			Components.Health.SetMaxHealth(Context, StartingHealth);
		}
		npcClass = configuration.NpcClass;
		BaseCombatMode = (CombatMode)config.CombatMode;
		BaseDamageMode = (DamageMode)config.DamageMode;
		BaseMovementMode = (MovementMode)config.MovementMode;
		BasePhysicsMode = (PhysicsMode)config.PhysicsMode;
		IdleBehavior = (IdleBehavior)config.IdleBehavior;
		PathfindingRange = (PathfindingRange)config.PathfindingRange;
		SpawnAnimation = (NpcSpawnAnimation)config.SpawnAnimation;
		if (SpawnAnimation == NpcSpawnAnimation.None)
		{
			FinishedSpawnAnimation = true;
		}
		Group = (NpcGroup)config.Group;
	}

	private void HandleOnDamageModeChanged()
	{
		NetworkedDamageMode.Value = DamageMode;
		Components.HittableComponent.IsDamageable = DamageMode == DamageMode.TakeDamage;
	}

	public void EndlessGameEnd()
	{
		MonoBehaviourSingleton<CombatManager>.Instance.UnregisterCombatNpc(this);
	}

	public object GetSaveState()
	{
		Transform transform = base.transform;
		List<Vector3Int> list = new List<Vector3Int>();
		Endless.Gameplay.Scripting.InstructionNode instructionNode = InteractionBehavior?.GetNode();
		if ((object)instructionNode != null)
		{
			list.Add(instructionNode.CellPosition);
		}
		Endless.Gameplay.Scripting.InstructionNode instructionNode2 = Components.GoapController.CurrentBehaviorNode?.GetNode();
		if ((object)instructionNode2 != null)
		{
			list.Add(instructionNode2.CellPosition);
		}
		Endless.Gameplay.Scripting.InstructionNode instructionNode3 = Components.GoapController.CurrentCommandNode?.GetNode();
		if ((object)instructionNode3 != null)
		{
			list.Add(instructionNode3.CellPosition);
		}
		return new PersistenceData(transform.position, transform.rotation.eulerAngles.y, persistenceData.InitialLocation, persistenceData.InitialRotation, list);
	}

	public void LoadState(object loadedState)
	{
		if (base.IsServer && loadedState != null)
		{
			persistenceData = loadedState as PersistenceData;
		}
	}

	public void PrefabInitialize(WorldObject worldObject)
	{
		WorldObject = worldObject;
		worldObject.GetUserComponent<HealthComponent>().SetHealthZeroedBehaviour(HealthZeroedBehavior.Custom);
	}

	public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
	{
		scriptComponent = endlessScriptComponent;
	}

	[ClientRpc]
	public void DownedClientRpc()
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(3594010860u, clientRpcParams, RpcDelivery.Reliable);
			__endSendClientRpc(ref bufferWriter, 3594010860u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (!base.NetworkManager.IsHost)
			{
				Components.Animator.SetTrigger(NpcAnimator.Downed);
				Components.Animator.SetBool(NpcAnimator.Dbno, value: true);
			}
		}
	}

	[ClientRpc]
	public void ExplosionsOnlyClientRpc()
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
		{
			ClientRpcParams clientRpcParams = default(ClientRpcParams);
			FastBufferWriter bufferWriter = __beginSendClientRpc(2749909393u, clientRpcParams, RpcDelivery.Reliable);
			__endSendClientRpc(ref bufferWriter, 2749909393u, clientRpcParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		foreach (Collider hittableCollider in WorldObject.GetUserComponent<HittableComponent>().HittableColliders)
		{
			hittableCollider.gameObject.layer = LayerMask.NameToLayer("ExplosionsOnly");
		}
	}

	[ClientRpc]
	public void EnqueueMeleeAttackClientRpc(uint frame, int duration, int meleeAttackIndex)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams = default(ClientRpcParams);
				FastBufferWriter bufferWriter = __beginSendClientRpc(633007624u, clientRpcParams, RpcDelivery.Reliable);
				BytePacker.WriteValueBitPacked(bufferWriter, frame);
				BytePacker.WriteValueBitPacked(bufferWriter, duration);
				BytePacker.WriteValueBitPacked(bufferWriter, meleeAttackIndex);
				__endSendClientRpc(ref bufferWriter, 633007624u, clientRpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				((MeleeAttackComponent)Components.Attack).EnqueueMeleeAttack(frame, duration, meleeAttackIndex);
			}
		}
	}

	[ClientRpc]
	public void StartTeleportClientRpc(TeleportType teleportType)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams = default(ClientRpcParams);
				FastBufferWriter bufferWriter = __beginSendClientRpc(1056222869u, clientRpcParams, RpcDelivery.Reliable);
				bufferWriter.WriteValueSafe(in teleportType, default(FastBufferWriter.ForEnums));
				__endSendClientRpc(ref bufferWriter, 1056222869u, clientRpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				RuntimeDatabase.GetTeleportInfo(teleportType).TeleportStart(WorldObject.EndlessVisuals, Components.Animator, Position);
			}
		}
	}

	[ClientRpc]
	public void EndTeleportClientRpc(TeleportType teleportType)
	{
		NetworkManager networkManager = base.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			if (__rpc_exec_stage != __RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams = default(ClientRpcParams);
				FastBufferWriter bufferWriter = __beginSendClientRpc(1460646776u, clientRpcParams, RpcDelivery.Reliable);
				bufferWriter.WriteValueSafe(in teleportType, default(FastBufferWriter.ForEnums));
				__endSendClientRpc(ref bufferWriter, 1460646776u, clientRpcParams, RpcDelivery.Reliable);
			}
			if (__rpc_exec_stage == __RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				__rpc_exec_stage = __RpcExecStage.Send;
				RuntimeDatabase.GetTeleportInfo(teleportType).TeleportEnd(WorldObject.EndlessVisuals, Components.Animator, Position);
			}
		}
	}

	protected override void __initializeVariables()
	{
		if (NetworkedDamageMode == null)
		{
			throw new Exception("NpcEntity.NetworkedDamageMode cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		NetworkedDamageMode.Initialize(this);
		__nameNetworkVariable(NetworkedDamageMode, "NetworkedDamageMode");
		NetworkVariableFields.Add(NetworkedDamageMode);
		if (NetworkedPhysicsMode == null)
		{
			throw new Exception("NpcEntity.NetworkedPhysicsMode cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		NetworkedPhysicsMode.Initialize(this);
		__nameNetworkVariable(NetworkedPhysicsMode, "NetworkedPhysicsMode");
		NetworkVariableFields.Add(NetworkedPhysicsMode);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(1532021543u, __rpc_handler_1532021543, "ConfigureSpawnedNpc_ClientRpc");
		__registerRpc(3594010860u, __rpc_handler_3594010860, "DownedClientRpc");
		__registerRpc(2749909393u, __rpc_handler_2749909393, "ExplosionsOnlyClientRpc");
		__registerRpc(633007624u, __rpc_handler_633007624, "EnqueueMeleeAttackClientRpc");
		__registerRpc(1056222869u, __rpc_handler_1056222869, "StartTeleportClientRpc");
		__registerRpc(1460646776u, __rpc_handler_1460646776, "EndTeleportClientRpc");
		base.__initializeRpcs();
	}

	private static void __rpc_handler_1532021543(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			NetworkableNpcConfig value2 = null;
			if (value)
			{
				reader.ReadValueSafe(out value2, default(FastBufferWriter.ForNetworkSerializable));
			}
			ClientRpcParams client = rpcParams.Client;
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((NpcEntity)target).ConfigureSpawnedNpc_ClientRpc(value2, client);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_3594010860(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((NpcEntity)target).DownedClientRpc();
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_2749909393(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((NpcEntity)target).ExplosionsOnlyClientRpc();
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_633007624(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ByteUnpacker.ReadValueBitPacked(reader, out uint value);
			ByteUnpacker.ReadValueBitPacked(reader, out int value2);
			ByteUnpacker.ReadValueBitPacked(reader, out int value3);
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((NpcEntity)target).EnqueueMeleeAttackClientRpc(value, value2, value3);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_1056222869(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out TeleportType value, default(FastBufferWriter.ForEnums));
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((NpcEntity)target).StartTeleportClientRpc(value);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_1460646776(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out TeleportType value, default(FastBufferWriter.ForEnums));
			target.__rpc_exec_stage = __RpcExecStage.Execute;
			((NpcEntity)target).EndTeleportClientRpc(value);
			target.__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected internal override string __getTypeName()
	{
		return "NpcEntity";
	}
}
