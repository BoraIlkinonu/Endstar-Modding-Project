using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.LuaInterfaces;
using Endless.Gameplay.Scripting;
using Endless.Props;
using Endless.Props.ReferenceComponents;
using Endless.Shared;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay;

public class HittableComponent : EndlessBehaviour, IAwakeSubscriber, IGameEndSubscriber, IComponentBase, IScriptInjector
{
	public ThreatLevel ThreatLevel;

	[SerializeField]
	private PropDamageReaction propDamageReaction;

	[SerializeField]
	internal HealthComponent healthComponent;

	[SerializeField]
	private ShieldComponent shieldComponent;

	[SerializeField]
	private TeamComponent teamComponent;

	private bool isTargetable = true;

	private readonly List<TargetDatum> targetData = new List<TargetDatum>();

	public float CurrentAttackersWeight;

	public readonly Dictionary<Collider, PositionPrediction> PositionPredictions = new Dictionary<Collider, PositionPrediction>();

	[SerializeField]
	private EndlessScriptComponent scriptComponent;

	private Hittable luaHittableInterface;

	[field: SerializeField]
	public float CombatCapacity { get; private set; }

	[field: SerializeField]
	public PositionPrediction PositionPrediction { get; private set; }

	[field: SerializeField]
	public CombatPositionGenerator CombatPositionGenerator { get; private set; }

	[field: SerializeField]
	public HostilityComponent HostilityComponent { get; private set; }

	[field: SerializeField]
	public List<Collider> HittableColliders { get; private set; }

	public uint LastAttackedFrame { get; set; }

	public HashSet<TargeterComponent> Targeters { get; } = new HashSet<TargeterComponent>();

	public UnityEngine.Vector3 Position => base.transform.position;

	public bool HasHealth
	{
		get
		{
			if ((bool)healthComponent)
			{
				return healthComponent.CurrentHealth > 0;
			}
			return true;
		}
	}

	public bool IsFullHealth
	{
		get
		{
			if ((bool)healthComponent)
			{
				if ((bool)healthComponent)
				{
					return healthComponent.CurrentHealth == healthComponent.MaxHealth;
				}
				return false;
			}
			return true;
		}
	}

	public Team Team
	{
		get
		{
			if (!teamComponent)
			{
				return Team.Team1;
			}
			return teamComponent.Team;
		}
	}

	public bool IsTargetable
	{
		get
		{
			return isTargetable;
		}
		private set
		{
			if (value)
			{
				MonoBehaviourSingleton<PerceptionManager>.Instance.Perceivables.Add(this);
				MonoBehaviourSingleton<TargetingManager>.Instance.AddTargetable(this);
			}
			else
			{
				MonoBehaviourSingleton<PerceptionManager>.Instance.Perceivables.Remove(this);
				MonoBehaviourSingleton<TargetingManager>.Instance.RemoveTargetable(this);
			}
			isTargetable = value;
		}
	}

	public bool IsDamageable { get; set; } = true;

	public UnityEngine.Vector3 NavPosition
	{
		get
		{
			if (NavMesh.SamplePosition(base.transform.position + UnityEngine.Vector3.down * 0.5f, out var hit, 0.25f, -1))
			{
				return hit.position;
			}
			return WorldObject.transform.position;
		}
	}

	[field: SerializeField]
	public WorldObject WorldObject { get; private set; }

	public Type ComponentReferenceType => typeof(HittableReferences);

	public object LuaObject => luaHittableInterface ?? (luaHittableInterface = new Hittable(this));

	public Type LuaObjectType => typeof(Hittable);

	public event Action<HittableComponent, HealthModificationArgs> OnDamaged;

	internal HealthChangeResult ModifyHealth(HealthModificationArgs modificationArgs)
	{
		if (modificationArgs.Delta < 0)
		{
			if (!IsDamageable && modificationArgs.HealthChangeType != HealthChangeType.Unavoidable)
			{
				return HealthChangeResult.NoChange;
			}
			int num = modificationArgs.Delta;
			if ((bool)shieldComponent)
			{
				num = shieldComponent.DamageShields(modificationArgs);
			}
			modificationArgs.Delta = num;
			if (num < 0)
			{
				this.OnDamaged?.Invoke(this, modificationArgs);
			}
		}
		RaiseHealthModifiedEvent(modificationArgs.Source, modificationArgs.Delta);
		if (!healthComponent)
		{
			return HealthChangeResult.NoChange;
		}
		return healthComponent.ModifyHealth(modificationArgs);
	}

	public void SetIsTargetable(bool value)
	{
		if (value != IsTargetable)
		{
			IsTargetable = value;
		}
	}

	public List<TargetDatum> GetTargetableColliderData()
	{
		targetData.Clear();
		foreach (Collider hittableCollider in HittableColliders)
		{
			targetData.Add(new TargetDatum
			{
				Position = hittableCollider.bounds.center,
				ColliderId = hittableCollider.GetInstanceID()
			});
		}
		return targetData;
	}

	public float ModifyOwnTargetScore(Context context, float currentScore)
	{
		if ((bool)scriptComponent && scriptComponent.TryExecuteFunction("ModifyOwnTargetScore", out var returnValues, currentScore, context) && returnValues[0] is double num)
		{
			return (float)num;
		}
		return currentScore;
	}

	public List<Collider> GetTargetableColliders()
	{
		return HittableColliders;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)MonoBehaviourSingleton<PerceptionManager>.Instance)
		{
			MonoBehaviourSingleton<PerceptionManager>.Instance.Perceivables.Remove(this);
		}
		if ((bool)MonoBehaviourSingleton<TargetingManager>.Instance)
		{
			MonoBehaviourSingleton<TargetingManager>.Instance.RemoveTargetable(this);
		}
		if ((bool)MonoBehaviourSingleton<HittableMap>.Instance)
		{
			MonoBehaviourSingleton<HittableMap>.Instance.RemoveCollidersFromMaps(this);
		}
	}

	public void EndlessAwake()
	{
		MonoBehaviourSingleton<CombatManager>.Instance.OnUpdatingCombat += HandleOnUpdatingCombat;
		if (IsTargetable)
		{
			MonoBehaviourSingleton<PerceptionManager>.Instance.Perceivables.Add(this);
			MonoBehaviourSingleton<TargetingManager>.Instance.AddTargetable(this);
		}
		WorldObject.TryGetUserComponent<HealthComponent>(out healthComponent);
		WorldObject.TryGetUserComponent<ShieldComponent>(out shieldComponent);
		WorldObject.TryGetUserComponent<TeamComponent>(out teamComponent);
		MonoBehaviourSingleton<HittableMap>.Instance.AddCollidersToMaps(this);
		foreach (Collider hittableCollider in HittableColliders)
		{
			PositionPrediction value = hittableCollider.gameObject.AddComponent<PositionPrediction>();
			PositionPredictions.Add(hittableCollider, value);
		}
	}

	private void HandleOnUpdatingCombat()
	{
		CurrentAttackersWeight = 0f;
	}

	public void EndlessGameEnd()
	{
		if (IsTargetable)
		{
			MonoBehaviourSingleton<PerceptionManager>.Instance.Perceivables.Remove(this);
			MonoBehaviourSingleton<TargetingManager>.Instance.RemoveTargetable(this);
			Targeters.Clear();
		}
		foreach (KeyValuePair<Collider, PositionPrediction> positionPrediction in PositionPredictions)
		{
			UnityEngine.Object.Destroy(positionPrediction.Value);
		}
		PositionPredictions.Clear();
		MonoBehaviourSingleton<HittableMap>.Instance.RemoveCollidersFromMaps(this);
		MonoBehaviourSingleton<CombatManager>.Instance.OnUpdatingCombat -= HandleOnUpdatingCombat;
	}

	public void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
	{
		HittableReferences hittableReferences = (HittableReferences)referenceBase;
		List<Collider> list = new List<Collider>();
		foreach (ColliderInfo hittableCollider in hittableReferences.HittableColliders)
		{
			list.AddRange(hittableCollider.CachedColliders);
		}
		HittableColliders = list;
		propDamageReaction.Initialize(this, hittableReferences);
	}

	public void PrefabInitialize(WorldObject worldObject)
	{
		WorldObject = worldObject;
	}

	public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
	{
		scriptComponent = endlessScriptComponent;
	}

	private void RaiseHealthModifiedEvent(Context other, int delta)
	{
		if ((bool)scriptComponent)
		{
			scriptComponent.TryExecuteFunction("OnHealthModified", out var _, other, delta);
		}
	}
}
