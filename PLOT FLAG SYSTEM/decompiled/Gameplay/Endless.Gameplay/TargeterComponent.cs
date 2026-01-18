using System;
using System.Collections;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Props.ReferenceComponents;
using Endless.Shared;
using Unity.Mathematics;
using UnityEngine;

namespace Endless.Gameplay;

public class TargeterComponent : EndlessNetworkBehaviour, IAwakeSubscriber, IGameEndSubscriber, IComponentBase, IScriptInjector, IAwarenessComponent, IUpdateComponent
{
	private const float CURRENT_TARGET_SCORE_INCREASE = 25f;

	private const float MINIMUM_AWARENESS_THRESHOLD = 30f;

	private const float MAXIMUM_AWARENESS = 100f;

	private const float MAXIMUM_PARTIAL_SCORE = 100f;

	private const float AWARENESS_PER_HIT = 40f;

	private const float AWARENESS_GAIN_SCALAR = 3f;

	private const float DEFAULT_AWARENESS_LOSS_RATE = 17.5f;

	[SerializeField]
	private TargetSelectionMode initialTargetSelectionMode;

	[SerializeField]
	private TargetPrioritizationMode initialTargetPrioritizationMode;

	[SerializeField]
	private CurrentTargetHandlingMode initialCurrentTargetHandlingMode;

	[SerializeField]
	private TargetHostilityMode initialTargetHostilityMode;

	[SerializeField]
	private ZeroHealthTargetMode initialZeroHealthTargetMode;

	[SerializeField]
	private float secondaryTargetThreshold;

	[SerializeField]
	[HideInInspector]
	private Transform losProbe;

	public int TickOffset;

	public bool isNavigationDependent;

	public PathfindingRange range = PathfindingRange.Global;

	public bool useXRayLos;

	public float awarenessLossRate;

	private HittableComponent forcedTarget;

	private HealthComponent healthComponent;

	private PerceptionDebuggingData perceptionDebuggingData;

	private readonly List<PerceptionResult> perceptionResults = new List<PerceptionResult>();

	private TeamComponent teamComponent;

	private readonly List<(float score, HittableComponent hittableComponent)> scorePairs = new List<(float, HittableComponent)>();

	private HittableComponent target;

	private readonly List<HittableComponent> currentTargets = new List<HittableComponent>();

	private HittableComponent hittable;

	private TargetSelectionMode targetSelectionMode;

	private TargetPrioritizationMode targetPrioritizationMode;

	private CurrentTargetHandlingMode currentTargetHandlingMode;

	private TargetHostilityMode targetHostilityMode;

	private ZeroHealthTargetMode zeroHealthTargetMode;

	private readonly Dictionary<HittableComponent, float> knownEntities = new Dictionary<HittableComponent, float>();

	private Dictionary<HittableComponent, float> mostRecentAwarenessScore;

	private bool awaitingResponse;

	private const float DEFAULT_FORCED_TARGET_TIME = 1.5f;

	private Coroutine removeForcedTargetRoutine;

	[SerializeField]
	[HideInInspector]
	private EndlessScriptComponent scriptComponent;

	private Targeter luaInterface;

	[field: SerializeField]
	public float MaxLookDistance { get; internal set; }

	[field: SerializeField]
	public float VerticalViewAngle { get; internal set; }

	[field: SerializeField]
	public float HorizontalViewAngle { get; internal set; }

	[field: SerializeField]
	public float ProximitySenseDistance { get; internal set; }

	public IReadOnlyCollection<HittableComponent> KnownHittables => knownEntities.Keys;

	private UnityEngine.Vector3 LookVector => losProbe.transform.forward;

	private UnityEngine.Vector3 LosPosition => losProbe.transform.position;

	private Team Team
	{
		get
		{
			if (!teamComponent)
			{
				return Team.Friendly;
			}
			return teamComponent.Team;
		}
	}

	public UnityEngine.Vector3 Position => WorldObject.transform.position;

	public Func<HittableComponent, float, float> TargetScoreModifier { private get; set; }

	public Transform LosProbe => losProbe;

	public HittableComponent Target
	{
		get
		{
			return target;
		}
		private set
		{
			if (!(value == target))
			{
				if ((bool)target)
				{
					target.Targeters.Remove(this);
				}
				this.OnTargetChanging?.Invoke(target);
				scriptComponent.TryExecuteFunction("OnTargetChanging", out var _, target ? target.WorldObject.Context : null, value ? value.WorldObject.Context : null);
				target = value;
				this.OnTargetChanged?.Invoke(target);
				if ((bool)target)
				{
					target.Targeters.Add(this);
				}
			}
		}
	}

	public HittableComponent LastTarget { get; private set; }

	public float CombatWeight { get; set; } = 1f;

	public IReadOnlyList<HittableComponent> CurrentTargets => currentTargets;

	internal TargetSelectionMode TargetSelectionMode
	{
		get
		{
			return targetSelectionMode;
		}
		set
		{
			Target = null;
			targetSelectionMode = value;
		}
	}

	internal TargetPrioritizationMode TargetPrioritizationMode
	{
		get
		{
			return targetPrioritizationMode;
		}
		set
		{
			Target = null;
			targetPrioritizationMode = value;
		}
	}

	internal CurrentTargetHandlingMode CurrentTargetHandlingMode
	{
		get
		{
			return currentTargetHandlingMode;
		}
		set
		{
			Target = null;
			currentTargetHandlingMode = value;
		}
	}

	internal TargetHostilityMode TargetHostilityMode
	{
		get
		{
			return targetHostilityMode;
		}
		set
		{
			Target = null;
			targetHostilityMode = value;
		}
	}

	internal ZeroHealthTargetMode ZeroHealthTargetMode
	{
		get
		{
			return zeroHealthTargetMode;
		}
		set
		{
			Target = null;
			zeroHealthTargetMode = value;
		}
	}

	public Type ComponentReferenceType => typeof(TargeterReferences);

	[field: SerializeField]
	[field: HideInInspector]
	public WorldObject WorldObject { get; private set; }

	public object LuaObject => luaInterface ?? (luaInterface = new Targeter(this));

	public Type LuaObjectType => typeof(Targeter);

	public event Action<PerceptionDebuggingData> OnScoresUpdated;

	public event Action<HittableComponent> OnTargetChanging;

	public event Action<HittableComponent> OnTargetChanged;

	[ContextMenu("Debug Targeting")]
	public void Debug()
	{
		MonoBehaviourSingleton<TargetingDebugger>.Instance.DebugTargeting(this);
	}

	private void OnPerceptionUpdated()
	{
		if ((object)healthComponent != null && healthComponent.CurrentHealth <= 0)
		{
			return;
		}
		awaitingResponse = false;
		HashSet<HittableComponent> hashSet = new HashSet<HittableComponent>(knownEntities.Keys);
		foreach (PerceptionResult perceptionResult in perceptionResults)
		{
			if (!(perceptionResult.HittableComponent == null) && !(WorldObject == perceptionResult.HittableComponent.WorldObject))
			{
				float num = perceptionResult.Awareness * NetClock.FixedDeltaTime * 3f * (float)MonoBehaviourSingleton<TargeterManager>.Instance.TickOffsetRange;
				if (num > 0f)
				{
					hashSet.Remove(perceptionResult.HittableComponent);
				}
				if (!knownEntities.TryAdd(perceptionResult.HittableComponent, num))
				{
					float num2 = knownEntities[perceptionResult.HittableComponent];
					knownEntities[perceptionResult.HittableComponent] = Mathf.Min(num2 + num, 100f);
				}
			}
		}
		foreach (HittableComponent item in hashSet)
		{
			float num3 = knownEntities[item];
			knownEntities[item] = Mathf.Max(num3 - NetClock.FixedDeltaTime * awarenessLossRate * (float)MonoBehaviourSingleton<TargeterManager>.Instance.TickOffsetRange, 0f);
		}
		mostRecentAwarenessScore = new Dictionary<HittableComponent, float>(knownEntities);
		UpdateTarget();
	}

	private bool IsValidTeamTarget(Team targetTeam)
	{
		return targetSelectionMode switch
		{
			TargetSelectionMode.Allies => Team == targetTeam, 
			TargetSelectionMode.Neutral => targetTeam == Team.Neutral, 
			TargetSelectionMode.Enemies => Team.IsHostileTo(targetTeam), 
			TargetSelectionMode.NonAllies => Team != targetTeam, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private void UpdateTarget()
	{
		scorePairs.Clear();
		currentTargets.Clear();
		LastTarget = Target;
		if (forcedTarget != null && knownEntities.TryGetValue(forcedTarget, out var value) && value > 30f && IsValidTeamTarget(forcedTarget.Team) && (zeroHealthTargetMode == ZeroHealthTargetMode.Attend || forcedTarget.HasHealth))
		{
			Target = forcedTarget;
			currentTargets.Add(Target);
		}
		else
		{
			if ((bool)target && CurrentTargetHandlingMode == CurrentTargetHandlingMode.Preserve && knownEntities.TryGetValue(target, out var value2) && value2 > 30f && (ZeroHealthTargetMode == ZeroHealthTargetMode.Attend || target.HasHealth))
			{
				return;
			}
			foreach (KeyValuePair<HittableComponent, float> knownEntity in knownEntities)
			{
				if (knownEntity.Value < 30f || !knownEntity.Key || !knownEntity.Key.transform || (isNavigationDependent && !MonoBehaviourSingleton<Pathfinding>.Instance.IsValidDestination(hittable.NavPosition, knownEntity.Key.NavPosition, range, canDoubleJump: false)))
				{
					continue;
				}
				WorldObject worldObject = knownEntity.Key.WorldObject;
				TeamComponent component;
				switch (TargetSelectionMode)
				{
				case TargetSelectionMode.Allies:
					if (worldObject.TryGetUserComponent<TeamComponent>(out component) && Team == component.Team)
					{
						scorePairs.Add((0f, knownEntity.Key));
					}
					break;
				case TargetSelectionMode.Neutral:
					if (worldObject.TryGetUserComponent<TeamComponent>(out component) && component.Team == Team.Neutral)
					{
						scorePairs.Add((0f, knownEntity.Key));
					}
					break;
				case TargetSelectionMode.Enemies:
					if (Team.IsHostileTo(knownEntity.Key.Team))
					{
						scorePairs.Add((0f, knownEntity.Key));
					}
					break;
				case TargetSelectionMode.NonAllies:
					if (worldObject.TryGetUserComponent<TeamComponent>(out component) && Team != component.Team)
					{
						scorePairs.Add((0f, knownEntity.Key));
					}
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
			if (TargetHostilityMode == TargetHostilityMode.Attend && (bool)hittable)
			{
				Dictionary<HittableComponent, float> recentAttackers = hittable.HostilityComponent.RecentAttackers;
				for (int i = 0; i < scorePairs.Count; i++)
				{
					(float, HittableComponent) tuple = scorePairs[i];
					if (recentAttackers.TryGetValue(tuple.Item2, out var value3))
					{
						scorePairs[i] = (Mathf.Max(value3, 0f), tuple.Item2);
					}
				}
			}
			switch (TargetPrioritizationMode)
			{
			case TargetPrioritizationMode.Default:
				AddClosenessScores();
				AddAngleScores();
				break;
			case TargetPrioritizationMode.Closest:
				AddClosenessScores();
				break;
			case TargetPrioritizationMode.SmallestAngle:
				AddAngleScores();
				break;
			case TargetPrioritizationMode.HighHealth:
				AddHighHealthScores();
				break;
			case TargetPrioritizationMode.LowHealth:
				AddLowHealthScores();
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case TargetPrioritizationMode.None:
				break;
			}
			if (CurrentTargetHandlingMode == CurrentTargetHandlingMode.Prefer)
			{
				for (int j = 0; j < scorePairs.Count; j++)
				{
					(float, HittableComponent) value4 = scorePairs[j];
					if (!(value4.Item2 != target))
					{
						value4.Item1 += 25f;
						scorePairs[j] = value4;
					}
				}
			}
			if (ZeroHealthTargetMode == ZeroHealthTargetMode.Ignore)
			{
				for (int k = 0; k < scorePairs.Count; k++)
				{
					(float, HittableComponent) value5 = scorePairs[k];
					if (!value5.Item2.HasHealth)
					{
						value5.Item1 = 0f;
						scorePairs[k] = value5;
					}
				}
			}
			for (int l = 0; l < scorePairs.Count; l++)
			{
				(float, HittableComponent) value6 = scorePairs[l];
				float item = value6.Item1;
				item = value6.Item2.ThreatLevel switch
				{
					ThreatLevel.Passive => item * 0.5f, 
					ThreatLevel.Low => item * 0.75f, 
					ThreatLevel.Medium => item, 
					ThreatLevel.High => item * 1.5f, 
					_ => throw new ArgumentOutOfRangeException(), 
				};
				float arg = value6.Item2.ModifyOwnTargetScore(WorldObject.Context, item);
				if (TargetScoreModifier != null)
				{
					value6.Item1 = TargetScoreModifier(value6.Item2, arg);
				}
				scorePairs[l] = value6;
			}
			for (int m = 0; m < scorePairs.Count; m++)
			{
				(float, HittableComponent) value7 = scorePairs[m];
				if (scriptComponent.TryExecuteFunction("ModifyTargetScore", out var returnValues, value7.Item1, value7.Item2.WorldObject.Context) && returnValues[0] is double num)
				{
					value7.Item1 = (float)num;
					scorePairs[m] = value7;
				}
			}
			scorePairs.Sort(((float score, HittableComponent hittableComponent) a, (float score, HittableComponent hittableComponent) b) => -a.score.CompareTo(b.score));
			this.OnScoresUpdated?.Invoke(new PerceptionDebuggingData(perceptionResults, mostRecentAwarenessScore, scorePairs));
			if (scorePairs.Count > 0 && scorePairs[0].score > 0f)
			{
				Target = scorePairs[0].hittableComponent;
				float item2 = scorePairs[0].score;
				foreach (var scorePair in scorePairs)
				{
					if (scorePair.score > item2 - secondaryTargetThreshold && currentTargets.Count <= 3)
					{
						currentTargets.Add(scorePair.hittableComponent);
					}
				}
			}
			else
			{
				Target = null;
			}
			mostRecentAwarenessScore = null;
		}
	}

	private void AddClosenessScores()
	{
		for (int i = 0; i < scorePairs.Count; i++)
		{
			(float, HittableComponent) value = scorePairs[i];
			float num = UnityEngine.Vector3.Distance(WorldObject.transform.position, value.Item2.transform.position);
			float num2 = Mathf.Lerp(100f, 0f, math.square(num / MaxLookDistance));
			value.Item1 += num2;
			scorePairs[i] = value;
		}
	}

	private void AddAngleScores()
	{
		for (int i = 0; i < scorePairs.Count; i++)
		{
			(float, HittableComponent) value = scorePairs[i];
			UnityEngine.Vector3 to = value.Item2.transform.position - WorldObject.transform.position;
			float num = UnityEngine.Vector3.Angle(LookVector, to);
			float num2 = Mathf.Lerp(100f, 0f, num / 180f);
			value.Item1 += num2;
			scorePairs[i] = value;
		}
	}

	private void AddHighHealthScores()
	{
		for (int i = 0; i < scorePairs.Count; i++)
		{
			(float, HittableComponent) value = scorePairs[i];
			if (value.Item2.WorldObject.TryGetUserComponent<HealthComponent>(out var component))
			{
				float num = Mathf.Lerp(0f, 100f, (float)component.CurrentHealth / (float)component.MaxHealth);
				value.Item1 += num;
				scorePairs[i] = value;
			}
		}
	}

	private void AddLowHealthScores()
	{
		for (int i = 0; i < scorePairs.Count; i++)
		{
			(float, HittableComponent) value = scorePairs[i];
			if (value.Item2.WorldObject.TryGetUserComponent<HealthComponent>(out var component))
			{
				float num = Mathf.Lerp(100f, 0f, (float)component.CurrentHealth / (float)component.MaxHealth);
				value.Item1 += num;
				scorePairs[i] = value;
			}
		}
	}

	private void HandleOnDamaged(HittableComponent _, HealthModificationArgs healthModificationArgs)
	{
		Context source = healthModificationArgs.Source;
		if ((object)source?.WorldObject != null && source.WorldObject.TryGetUserComponent<HittableComponent>(out var component) && !knownEntities.TryAdd(component, 40f))
		{
			knownEntities[component] = Mathf.Min(knownEntities[component] + 40f, 100f);
		}
	}

	public void AttemptSwitchTarget(Context targetContext)
	{
		if (targetContext != null && targetContext.WorldObject.TryGetUserComponent<HittableComponent>(out var component))
		{
			forcedTarget = component;
			if (removeForcedTargetRoutine != null)
			{
				StopCoroutine(removeForcedTargetRoutine);
			}
			removeForcedTargetRoutine = StartCoroutine(RemoveForcedTargetRoutine());
		}
	}

	private IEnumerator RemoveForcedTargetRoutine(float targetOverrideTime = 1.5f)
	{
		yield return new WaitForSeconds(targetOverrideTime);
		forcedTarget = null;
		removeForcedTargetRoutine = null;
	}

	internal void ForceOverrideTarget(HittableComponent newTarget)
	{
		if (currentTargets.Contains(newTarget))
		{
			Target = newTarget;
			forcedTarget = newTarget;
			if (removeForcedTargetRoutine != null)
			{
				StopCoroutine(removeForcedTargetRoutine);
			}
			removeForcedTargetRoutine = StartCoroutine(RemoveForcedTargetRoutine());
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (base.IsServer)
		{
			UnifiedStateUpdater.UnregisterUpdateComponent(this);
		}
	}

	public void EndlessAwake()
	{
		if (base.IsServer)
		{
			awarenessLossRate = 17.5f;
			WorldObject.TryGetUserComponent<TeamComponent>(out var component);
			teamComponent = component;
			if (WorldObject.TryGetUserComponent<HittableComponent>(out var component2))
			{
				component2.OnDamaged += HandleOnDamaged;
				hittable = component2;
			}
			if (WorldObject.TryGetUserComponent<HealthComponent>(out var component3))
			{
				healthComponent = component3;
				healthComponent.OnHealthZeroed_Internal.AddListener(HandleOnHealthZeroed);
			}
			UnifiedStateUpdater.RegisterUpdateComponent(this);
			TargetSelectionMode = initialTargetSelectionMode;
			TargetPrioritizationMode = initialTargetPrioritizationMode;
			CurrentTargetHandlingMode = initialCurrentTargetHandlingMode;
			TargetHostilityMode = initialTargetHostilityMode;
			ZeroHealthTargetMode = initialZeroHealthTargetMode;
			MonoBehaviourSingleton<TargeterManager>.Instance.RegisterTargeter(this);
		}
	}

	private void HandleOnHealthZeroed()
	{
		UnifiedStateUpdater.UnregisterUpdateComponent(this);
		MonoBehaviourSingleton<TargeterManager>.Instance.UnregisterTargeter(this);
		Target = null;
	}

	public void EndlessGameEnd()
	{
		if (base.IsServer)
		{
			UnifiedStateUpdater.UnregisterUpdateComponent(this);
			MonoBehaviourSingleton<TargeterManager>.Instance.UnregisterTargeter(this);
		}
	}

	public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
	{
		TargeterReferences targeterReferences = (TargeterReferences)referenceBase;
		losProbe = targeterReferences.LosProbe;
	}

	public void PrefabInitialize(WorldObject worldObject)
	{
		WorldObject = worldObject;
	}

	public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
	{
		scriptComponent = endlessScriptComponent;
	}

	public void UpdateAwareness()
	{
		if (!awaitingResponse && (bool)losProbe && NetClock.CurrentFrame % MonoBehaviourSingleton<TargeterManager>.Instance.TickOffsetRange == TickOffset)
		{
			PerceptionRequest request = new PerceptionRequest
			{
				Position = LosPosition,
				LookVector = LookVector,
				MaxDistance = MaxLookDistance,
				VerticalValue = VerticalViewAngle,
				HorizontalValue = HorizontalViewAngle,
				ProximityDistance = ProximitySenseDistance,
				UseXray = useXRayLos,
				PerceptionResults = perceptionResults,
				PerceptionUpdatedCallback = OnPerceptionUpdated
			};
			awaitingResponse = true;
			MonoBehaviourSingleton<PerceptionManager>.Instance.RequestPerception(request);
		}
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected internal override string __getTypeName()
	{
		return "TargeterComponent";
	}
}
