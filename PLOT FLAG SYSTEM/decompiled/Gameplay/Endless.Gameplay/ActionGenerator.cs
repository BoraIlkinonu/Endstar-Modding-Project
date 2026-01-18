using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;

namespace Endless.Gameplay;

public static class ActionGenerator
{
	public static HashSet<GoapAction> InitializeActions(NpcEntity entity)
	{
		HashSet<GoapAction> hashSet = new HashSet<GoapAction>
		{
			new GoapAction.Builder(GoapAction.ActionKind.Idle).WithCost(5f).WithStrategy(new IdleStrategy(entity.NpcBlackboard, 2f)).AddEffect(WorldState.Idle)
				.Build(),
			new GoapAction.Builder(GoapAction.ActionKind.Fidget).WithStrategy(new FidgetStrategy(entity)).AddPrerequisite(WorldState.CanFidget).AddEffect(WorldState.Idle)
				.Build(),
			new GoapAction.Builder(GoapAction.ActionKind.MoveToCommandDestination).WithStrategy(new MoveToCommandDestinationStrategy(entity)).AddPrerequisite(WorldState.IsOnNavigableCell).AddEffect(WorldState.IsAtDestination)
				.Build(),
			new GoapAction.Builder(GoapAction.ActionKind.ReturnToNavGraph).WithCost(1f).WithStrategy(new ReturnToNavGraphStrategy(entity)).AddEffect(MonoBehaviourSingleton<WorldStateDictionary>.Instance[WorldState.IsOnNavigableCell])
				.Build(),
			new GoapAction.Builder(GoapAction.ActionKind.MoveToBehaviorDestination).WithStrategy(new MoveToBehaviorDestinationStrategy(entity)).AddPrerequisite(WorldState.IsOnNavigableCell).AddEffect(WorldState.Rove)
				.AddEffect(WorldState.Wander)
				.Build()
		};
		switch (entity.NpcClass.NpcClass)
		{
		case NpcClass.Blank:
			return hashSet;
		case NpcClass.Grunt:
			hashSet.Add(new GoapAction.Builder(GoapAction.ActionKind.FindTarget).WithCost(5f).WithStrategy(new FindTargetStrategy(entity)).AddPrerequisite(WorldState.LostTarget)
				.AddPrerequisite(WorldState.IsOnNavigableCell)
				.AddPrerequisite(WorldState.IsNotPassive)
				.AddEffect(WorldState.EngageTarget)
				.Build());
			hashSet.Add(new GoapAction.Builder(GoapAction.ActionKind.FallBack).WithCost(5f).WithStrategy(new FallBackStrategy(entity)).AddPrerequisite(WorldState.HasAttackTarget)
				.AddPrerequisite(WorldState.IsEngaged)
				.AddPrerequisite(WorldState.IsOnNavigableCell)
				.AddEffect(WorldState.IsOutsideNearRange)
				.Build());
			hashSet.Add(new GoapAction.Builder(GoapAction.ActionKind.ApproachTarget).WithCost(5f).WithStrategy(new ApproachTargetStrategy(entity)).AddPrerequisite(WorldState.HasAttackTarget)
				.AddPrerequisite(WorldState.IsEngaged)
				.AddPrerequisite(WorldState.IsOnNavigableCell)
				.AddEffect(WorldState.IsInEngageRange)
				.Build());
			hashSet.Add(new GoapAction.Builder(GoapAction.ActionKind.CombatIdle).WithCost(5f).WithStrategy(new CombatIdleStrategy(2f, entity)).AddPrerequisite(WorldState.HasAttackTarget)
				.AddPrerequisite(WorldState.IsEngaged)
				.AddPrerequisite(WorldState.IsInEngageRange)
				.AddPrerequisite(WorldState.IsOutsideNearRange)
				.AddEffect(WorldState.EngageTarget)
				.Build());
			hashSet.Add(new GoapAction.Builder(GoapAction.ActionKind.CombatFidget).WithStrategy(new CombatFidgetStrategy(entity)).AddPrerequisite(WorldState.HasAttackTarget).AddPrerequisite(WorldState.CanFidget)
				.AddPrerequisite(WorldState.IsEngaged)
				.AddPrerequisite(WorldState.IsInEngageRange)
				.AddPrerequisite(WorldState.IsOutsideNearRange)
				.AddEffect(WorldState.EngageTarget)
				.Build());
			hashSet.Add(new GoapAction.Builder(GoapAction.ActionKind.Strafe).WithStrategy(new StrafeStrategy(entity)).AddPrerequisite(WorldState.HasAttackTarget).AddPrerequisite(WorldState.IsEngaged)
				.AddPrerequisite(WorldState.IsInEngageRange)
				.AddPrerequisite(WorldState.IsOutsideNearRange)
				.AddPrerequisite(WorldState.IsOnNavigableCell)
				.AddEffect(WorldState.EngageTarget)
				.Build());
			hashSet.Add(new GoapAction.Builder(GoapAction.ActionKind.Taunt).WithStrategy(new TauntStrategy(entity)).AddPrerequisite(WorldState.HasAttackTarget).AddPrerequisite(WorldState.CanFidget)
				.AddPrerequisite(WorldState.IsEngaged)
				.AddPrerequisite(WorldState.IsInEngageRange)
				.AddPrerequisite(WorldState.IsOutsideNearRange)
				.AddEffect(WorldState.EngageTarget)
				.Build());
			hashSet.Add(new GoapAction.Builder(GoapAction.ActionKind.MoveNearTarget).WithCost(5f).WithStrategy(new MoveNearTargetStrategy(entity)).AddPrerequisite(WorldState.HasAttackTarget)
				.AddPrerequisite(WorldState.IsOnNavigableCell)
				.AddEffect(MonoBehaviourSingleton<WorldStateDictionary>.Instance[WorldState.IsInMeleeRange])
				.Build());
			hashSet.Add(new GoapAction.Builder(GoapAction.ActionKind.WaitToAttack).WithCost(5f).WithStrategy(new WaitToAttackStrategy(entity)).AddPrerequisite(WorldState.HasAttackTarget)
				.AddPrerequisite(WorldState.IsInMeleeRange)
				.AddPrerequisite(WorldState.IsAttacker)
				.AddEffect(WorldState.HasAttackPermission)
				.Build());
			hashSet.Add(new GoapAction.Builder(GoapAction.ActionKind.Backstep).WithCost(5f).WithStrategy(new BackstepStrategy(entity)).AddPrerequisite(WorldState.HasAttackTarget)
				.AddPrerequisite(WorldState.CanBackstep)
				.AddPrerequisite(WorldState.IsOnNavigableCell)
				.AddEffect(WorldState.IsFarEnoughAwayForMelee)
				.Build());
			hashSet.Add(new GoapAction.Builder(GoapAction.ActionKind.MeleeAttack).WithCost(5f).WithStrategy(new MeleeAttackStrategy(entity)).AddPrerequisite(WorldState.HasAttackTarget)
				.AddPrerequisite(WorldState.IsAttacker)
				.AddPrerequisite(WorldState.HasAttackPermission)
				.AddEffect(WorldState.AttackTarget)
				.Build());
			return hashSet;
		case NpcClass.Rifleman:
			hashSet.Add(new GoapAction.Builder(GoapAction.ActionKind.FindTarget).WithCost(5f).WithStrategy(new FindTargetStrategy(entity)).AddPrerequisite(WorldState.LostTarget)
				.AddPrerequisite(WorldState.IsOnNavigableCell)
				.AddPrerequisite(WorldState.IsNotPassive)
				.AddEffect(WorldState.EngageTarget)
				.Build());
			hashSet.Add(new GoapAction.Builder(GoapAction.ActionKind.CombatIdle).WithCost(5f).WithStrategy(new CombatIdleStrategy(2f, entity)).AddPrerequisite(WorldState.HasAttackTarget)
				.AddPrerequisite(WorldState.HasLos)
				.AddPrerequisite(WorldState.IsEngaged)
				.AddPrerequisite(WorldState.IsInEngageRange)
				.AddPrerequisite(WorldState.IsOutsideNearRange)
				.AddEffect(WorldState.EngageTarget)
				.Build());
			hashSet.Add(new GoapAction.Builder(GoapAction.ActionKind.CombatFidget).WithStrategy(new CombatFidgetStrategy(entity)).AddPrerequisite(WorldState.HasAttackTarget).AddPrerequisite(WorldState.HasLos)
				.AddPrerequisite(WorldState.CanFidget)
				.AddPrerequisite(WorldState.IsEngaged)
				.AddPrerequisite(WorldState.IsInEngageRange)
				.AddPrerequisite(WorldState.IsOutsideNearRange)
				.AddEffect(WorldState.EngageTarget)
				.Build());
			hashSet.Add(new GoapAction.Builder(GoapAction.ActionKind.Taunt).WithStrategy(new TauntStrategy(entity)).AddPrerequisite(WorldState.HasAttackTarget).AddPrerequisite(WorldState.HasLos)
				.AddPrerequisite(WorldState.CanFidget)
				.AddPrerequisite(WorldState.IsEngaged)
				.AddPrerequisite(WorldState.IsInEngageRange)
				.AddPrerequisite(WorldState.IsOutsideNearRange)
				.AddEffect(WorldState.EngageTarget)
				.Build());
			hashSet.Add(new GoapAction.Builder(GoapAction.ActionKind.Strafe).WithStrategy(new StrafeStrategy(entity)).AddPrerequisite(WorldState.HasAttackTarget).AddPrerequisite(WorldState.HasLos)
				.AddPrerequisite(WorldState.IsEngaged)
				.AddPrerequisite(WorldState.IsInEngageRange)
				.AddPrerequisite(WorldState.IsOutsideNearRange)
				.AddPrerequisite(WorldState.IsOnNavigableCell)
				.AddEffect(WorldState.EngageTarget)
				.Build());
			hashSet.Add(new GoapAction.Builder(GoapAction.ActionKind.ApproachTarget).WithCost(5f).WithStrategy(new ApproachTargetStrategy(entity)).AddPrerequisite(WorldState.HasAttackTarget)
				.AddPrerequisite(WorldState.IsEngaged)
				.AddPrerequisite(WorldState.IsOnNavigableCell)
				.AddEffect(WorldState.IsInEngageRange)
				.Build());
			hashSet.Add(new GoapAction.Builder(GoapAction.ActionKind.FallBack).WithCost(5f).WithStrategy(new FallBackStrategy(entity)).AddPrerequisite(WorldState.HasAttackTarget)
				.AddPrerequisite(WorldState.IsEngaged)
				.AddPrerequisite(WorldState.IsOnNavigableCell)
				.AddEffect(WorldState.IsOutsideNearRange)
				.Build());
			hashSet.Add(new GoapAction.Builder(GoapAction.ActionKind.GetInLos).WithCost(5f).WithStrategy(new GetInLosStrategy(entity)).AddPrerequisite(WorldState.HasAttackTarget)
				.AddPrerequisite(WorldState.IsOnNavigableCell)
				.AddEffect(WorldState.HasLos)
				.Build());
			hashSet.Add(new GoapAction.Builder(GoapAction.ActionKind.WaitToAttack).WithCost(5f).WithStrategy(new RangedWaitToAttackStrategy(entity)).AddPrerequisite(WorldState.HasLos)
				.AddPrerequisite(WorldState.HasAttackTarget)
				.AddPrerequisite(WorldState.IsAttacker)
				.AddEffect(WorldState.HasAttackPermission)
				.Build());
			hashSet.Add(new GoapAction.Builder(GoapAction.ActionKind.RangedAttack).WithCost(5f).WithStrategy(new RangedAttackStrategy(entity)).AddPrerequisite(WorldState.HasAttackTarget)
				.AddPrerequisite(WorldState.HasLos)
				.AddPrerequisite(WorldState.IsInRangedAttackRange)
				.AddPrerequisite(WorldState.IsAttacker)
				.AddPrerequisite(WorldState.HasAttackPermission)
				.AddEffect(WorldState.AttackTarget)
				.Build());
			return hashSet;
		case NpcClass.Zombie:
			hashSet.Add(new GoapAction.Builder(GoapAction.ActionKind.FindTarget).WithCost(5f).WithStrategy(new FindTargetStrategy(entity)).AddPrerequisite(WorldState.LostTarget)
				.AddPrerequisite(WorldState.IsOnNavigableCell)
				.AddPrerequisite(WorldState.IsNotPassive)
				.AddEffect(WorldState.EngageTarget)
				.Build());
			hashSet.Add(new GoapAction.Builder(GoapAction.ActionKind.ApproachTarget).WithCost(5f).WithStrategy(new ApproachTargetStrategy(entity)).AddPrerequisite(WorldState.HasAttackTarget)
				.AddPrerequisite(WorldState.IsEngaged)
				.AddPrerequisite(WorldState.IsOnNavigableCell)
				.AddEffect(WorldState.IsInEngageRange)
				.Build());
			hashSet.Add(new GoapAction.Builder(GoapAction.ActionKind.MoveNearTarget).WithCost(5f).WithStrategy(new MoveNearTargetStrategy(entity)).AddPrerequisite(WorldState.HasAttackTarget)
				.AddPrerequisite(WorldState.IsAttacker)
				.AddPrerequisite(WorldState.IsOnNavigableCell)
				.AddEffect(WorldState.IsInMeleeRange)
				.Build());
			hashSet.Add(new GoapAction.Builder(GoapAction.ActionKind.FallBack).WithCost(5f).WithStrategy(new FallBackStrategy(entity)).AddPrerequisite(WorldState.HasAttackTarget)
				.AddPrerequisite(WorldState.IsEngaged)
				.AddPrerequisite(WorldState.IsOnNavigableCell)
				.AddEffect(WorldState.IsOutsideNearRange)
				.Build());
			hashSet.Add(new GoapAction.Builder(GoapAction.ActionKind.Backstep).WithCost(5f).WithStrategy(new BackstepStrategy(entity)).AddPrerequisite(WorldState.HasAttackTarget)
				.AddPrerequisite(WorldState.CanBackstep)
				.AddPrerequisite(WorldState.IsOnNavigableCell)
				.AddEffect(WorldState.IsFarEnoughAwayForMelee)
				.Build());
			hashSet.Add(new GoapAction.Builder(GoapAction.ActionKind.ZombieMeleeAttack).WithCost(5f).WithStrategy(new ZombieMeleeAttackStrategy(entity)).AddPrerequisite(WorldState.HasAttackTarget)
				.AddPrerequisite(WorldState.IsAttacker)
				.AddPrerequisite(WorldState.IsInMeleeRange)
				.AddEffect(WorldState.AttackTarget)
				.Build());
			return hashSet;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public static Dictionary<GenericWorldState, HashSet<GoapAction>> BuildActionMap(HashSet<GoapAction> actions)
	{
		Dictionary<GenericWorldState, HashSet<GoapAction>> dictionary = new Dictionary<GenericWorldState, HashSet<GoapAction>>();
		HashSet<GenericWorldState> hashSet = new HashSet<GenericWorldState>();
		foreach (GoapAction action in actions)
		{
			foreach (GenericWorldState prerequisite in action.Prerequisites)
			{
				hashSet.Add(prerequisite);
			}
			foreach (GenericWorldState effect in action.Effects)
			{
				hashSet.Add(effect);
			}
		}
		foreach (GenericWorldState item in hashSet)
		{
			HashSet<GoapAction> hashSet2 = new HashSet<GoapAction>();
			foreach (GoapAction action2 in actions)
			{
				if (action2.Effects.Contains(item))
				{
					hashSet2.Add(action2);
				}
			}
			dictionary.Add(item, hashSet2);
		}
		return dictionary;
	}
}
