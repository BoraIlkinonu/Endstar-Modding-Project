using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay;

public class WorldStateDictionary : MonoBehaviourSingleton<WorldStateDictionary>
{
	[SerializeField]
	private LayerMask losMask;

	[SerializeField]
	private LayerMask blockingLayers;

	private readonly Dictionary<WorldState, GenericWorldState> worldStateMap = new Dictionary<WorldState, GenericWorldState>();

	private static readonly RaycastHit[] hits = new RaycastHit[5];

	private static readonly Collider[] overlappedColliders = new Collider[5];

	public GenericWorldState this[WorldState key] => worldStateMap[key];

	private static LayerMask LosMask => MonoBehaviourSingleton<WorldStateDictionary>.Instance.losMask;

	private static LayerMask BlockingLayers => MonoBehaviourSingleton<WorldStateDictionary>.Instance.blockingLayers;

	protected override void Awake()
	{
		base.Awake();
		InitDictionary();
	}

	private void InitDictionary()
	{
		worldStateMap.Add(WorldState.AttackTarget, new GenericWorldState(WorldState.AttackTarget, (NpcEntity _) => false));
		worldStateMap.Add(WorldState.Nothing, new GenericWorldState(WorldState.Nothing, (NpcEntity _) => false));
		worldStateMap.Add(WorldState.CanBackstep, new GenericWorldState(WorldState.CanBackstep, CanBackstep));
		worldStateMap.Add(WorldState.CanReachHealthPickup, new GenericWorldState(WorldState.CanReachHealthPickup, CanReachHealthPickup));
		worldStateMap.Add(WorldState.EngageTarget, new GenericWorldState(WorldState.EngageTarget, (NpcEntity _) => false));
		worldStateMap.Add(WorldState.GotAway, new GenericWorldState(WorldState.GotAway, (NpcEntity _) => false));
		worldStateMap.Add(WorldState.HasAttackPermission, new GenericWorldState(WorldState.HasAttackPermission, HasAttackPermission));
		worldStateMap.Add(WorldState.HasFollowTarget, new GenericWorldState(WorldState.HasFollowTarget, HasFollowTarget));
		worldStateMap.Add(WorldState.HasLos, new GenericWorldState(WorldState.HasLos, HasLos));
		worldStateMap.Add(WorldState.HasAttackTarget, new GenericWorldState(WorldState.HasAttackTarget, HasAttackTarget));
		worldStateMap.Add(WorldState.Idle, new GenericWorldState(WorldState.Idle, (NpcEntity _) => false));
		worldStateMap.Add(WorldState.IsAtDestination, new GenericWorldState(WorldState.IsAtDestination, IsAtDestination));
		worldStateMap.Add(WorldState.IsAttacker, new GenericWorldState(WorldState.IsAttacker, IsAttacker));
		worldStateMap.Add(WorldState.IsEngaged, new GenericWorldState(WorldState.IsEngaged, IsEngaged));
		worldStateMap.Add(WorldState.IsRangedAttacker, new GenericWorldState(WorldState.IsRangedAttacker, IsRangedAttacker));
		worldStateMap.Add(WorldState.IsFarEnoughAwayForMelee, new GenericWorldState(WorldState.IsFarEnoughAwayForMelee, IsFarEnoughAwayForMelee));
		worldStateMap.Add(WorldState.IsFarEnoughAwayForRanged, new GenericWorldState(WorldState.IsFarEnoughAwayForRanged, IsFarEnoughAwayForRanged));
		worldStateMap.Add(WorldState.IsInEngageRange, new GenericWorldState(WorldState.IsInEngageRange, IsInEngageRange));
		worldStateMap.Add(WorldState.IsInMeleeRange, new GenericWorldState(WorldState.IsInMeleeRange, IsInMeleeRange));
		worldStateMap.Add(WorldState.IsInRangedAttackRange, new GenericWorldState(WorldState.IsInRangedAttackRange, IsInRangedAttackRange));
		worldStateMap.Add(WorldState.IsNearFollowTarget, new GenericWorldState(WorldState.IsNearFollowTarget, IsNearFollowTarget));
		worldStateMap.Add(WorldState.IsOutsideCloseRange, new GenericWorldState(WorldState.IsOutsideCloseRange, IsOutsideCloseRange));
		worldStateMap.Add(WorldState.IsOutsideNearRange, new GenericWorldState(WorldState.IsOutsideNearRange, IsOutsideNearRange));
		worldStateMap.Add(WorldState.RecoverHealth, new GenericWorldState(WorldState.RecoverHealth, (NpcEntity _) => false));
		worldStateMap.Add(WorldState.Rove, new GenericWorldState(WorldState.Rove, (NpcEntity _) => false));
		worldStateMap.Add(WorldState.Wander, new GenericWorldState(WorldState.Wander, (NpcEntity _) => false));
		worldStateMap.Add(WorldState.CanFidget, new GenericWorldState(WorldState.CanFidget, CanFidget));
		worldStateMap.Add(WorldState.IsOnNavigableCell, new GenericWorldState(WorldState.IsOnNavigableCell, IsOnNavigableCell));
		worldStateMap.Add(WorldState.LostTarget, new GenericWorldState(WorldState.LostTarget, LostTarget));
		worldStateMap.Add(WorldState.IsNotPassive, new GenericWorldState(WorldState.IsNotPassive, IsNotPassive));
	}

	private static bool IsOnNavigableCell(NpcEntity entity)
	{
		return MonoBehaviourSingleton<Pathfinding>.Instance.IsPositionNavigable(entity.FootPosition);
	}

	private static bool CanBackstep(NpcEntity entity)
	{
		Vector3 position = entity.Components.TargeterComponent.Target.Position;
		NavMeshHit hit;
		return NavMesh.SamplePosition(position + (position - position).normalized * entity.OptimalAttackDistance, out hit, 0.5f, -1);
	}

	private static bool CanReachHealthPickup(NpcEntity entity)
	{
		return false;
	}

	private static bool HasAttackPermission(NpcEntity entity)
	{
		return entity.HasAttackToken;
	}

	private static bool HasFollowTarget(NpcEntity entity)
	{
		return entity.FollowTarget;
	}

	private static bool HasLos(NpcEntity entity)
	{
		if (!entity.IsRangedAttacker || !entity.Components.TargeterComponent.LosProbe)
		{
			return false;
		}
		if (!TryGetAttackTarget(entity, out var attackTarget))
		{
			return false;
		}
		Vector3 position = entity.Components.TargeterComponent.LosProbe.position;
		if (Physics.OverlapSphereNonAlloc(position, 0.1f, overlappedColliders, BlockingLayers) > 0)
		{
			return false;
		}
		foreach (TargetDatum targetableColliderDatum in attackTarget.GetTargetableColliderData())
		{
			Vector3 direction = targetableColliderDatum.Position - position;
			int num = Physics.RaycastNonAlloc(new Ray(position, direction), hits, direction.magnitude, LosMask);
			for (int i = 0; i < num; i++)
			{
				if (((1 << hits[i].collider.gameObject.layer) & BlockingLayers.value) != 0)
				{
					return false;
				}
				if (hits[i].collider.GetInstanceID() == targetableColliderDatum.ColliderId)
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool HasAttackTarget(NpcEntity entity)
	{
		HittableComponent attackTarget;
		return TryGetAttackTarget(entity, out attackTarget);
	}

	private static bool IsAtDestination(NpcEntity entity)
	{
		if (!entity.NpcBlackboard.TryGet<Vector3>(NpcBlackboard.Key.CommandDestination, out var value))
		{
			return false;
		}
		return Vector3.Distance(entity.FootPosition, value) < entity.DestinationTolerance;
	}

	private static bool IsAttacker(NpcEntity entity)
	{
		return entity.CombatState == NpcEnum.CombatState.Attacking;
	}

	private static bool IsEngaged(NpcEntity entity)
	{
		return entity.CombatState == NpcEnum.CombatState.Engaged;
	}

	private static bool IsRangedAttacker(NpcEntity entity)
	{
		return entity.IsRangedAttacker;
	}

	private static bool IsNotPassive(NpcEntity entity)
	{
		return entity.Components.DynamicAttributes.CombatMode != CombatMode.Passive;
	}

	private static bool IsFarEnoughAwayForMelee(NpcEntity entity)
	{
		if (!TryGetAttackTarget(entity, out var attackTarget))
		{
			return false;
		}
		return Vector3.Distance(entity.transform.position, attackTarget.Position) > entity.CloseDistance - 0.2f;
	}

	private static bool IsFarEnoughAwayForRanged(NpcEntity entity)
	{
		if (!TryGetAttackTarget(entity, out var attackTarget))
		{
			return false;
		}
		return Vector3.Distance(entity.transform.position, attackTarget.Position) > entity.CloseDistance - 0.2f;
	}

	private static bool IsInEngageRange(NpcEntity entity)
	{
		if (!TryGetAttackTarget(entity, out var attackTarget))
		{
			return false;
		}
		return Vector3.Distance(entity.transform.position, attackTarget.Position) < entity.AroundDistance + 0.2f;
	}

	private static bool TryGetAttackTarget(NpcEntity entity, out HittableComponent attackTarget)
	{
		attackTarget = entity.Components.TargeterComponent.Target;
		if ((bool)attackTarget)
		{
			return attackTarget.transform;
		}
		return false;
	}

	private static bool IsInMeleeRange(NpcEntity entity)
	{
		if (!TryGetAttackTarget(entity, out var attackTarget))
		{
			return false;
		}
		Vector3 position = attackTarget.Position;
		return Vector3.Distance(entity.transform.position, position) < entity.NearDistance + 0.2f;
	}

	private static bool IsInRangedAttackRange(NpcEntity entity)
	{
		if (!TryGetAttackTarget(entity, out var attackTarget))
		{
			return false;
		}
		Vector3 position = attackTarget.Position;
		return Vector3.Distance(entity.transform.position, position) < entity.MaxRangedAttackDistance;
	}

	private static bool IsNearFollowTarget(NpcEntity entity)
	{
		HittableComponent followTarget = entity.FollowTarget;
		if (!followTarget)
		{
			return false;
		}
		return Vector3.Distance(entity.transform.position, followTarget.Position) <= 1f;
	}

	private static bool IsOutsideCloseRange(NpcEntity entity)
	{
		if (!TryGetAttackTarget(entity, out var attackTarget))
		{
			return false;
		}
		return Vector3.Distance(entity.transform.position, attackTarget.Position) > entity.CloseDistance - 0.2f;
	}

	private static bool LostTarget(NpcEntity entity)
	{
		if (!entity.Target && entity.LostTarget)
		{
			return true;
		}
		return false;
	}

	private static bool IsOutsideNearRange(NpcEntity entity)
	{
		if (!TryGetAttackTarget(entity, out var attackTarget))
		{
			return false;
		}
		return Vector3.Distance(entity.transform.position, attackTarget.Position) > entity.NearDistance - 0.2f;
	}

	private static bool CanFidget(NpcEntity entity)
	{
		bool value;
		return entity.NpcBlackboard.TryGet<bool>(NpcBlackboard.Key.CanFidget, out value) && value;
	}
}
