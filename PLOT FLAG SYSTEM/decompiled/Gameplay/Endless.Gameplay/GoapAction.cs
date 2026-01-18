using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;

namespace Endless.Gameplay;

public class GoapAction
{
	public enum ActionKind
	{
		Idle,
		Fidget,
		MoveToCommandDestination,
		MoveToBehaviorDestination,
		ReturnToNavGraph,
		CombatIdle,
		CombatFidget,
		ApproachTarget,
		Strafe,
		Taunt,
		WaitToAttack,
		Backstep,
		MeleeAttack,
		MoveNearTarget,
		FallBack,
		GetInLos,
		RangedAttack,
		ZombieMeleeAttack,
		FindTarget
	}

	public class Builder
	{
		private readonly GoapAction action;

		public Builder(ActionKind actionKind)
		{
			action = new GoapAction(actionKind)
			{
				staticCost = 1f
			};
		}

		public Builder WithCost(float cost)
		{
			action.staticCost = cost;
			return this;
		}

		public Builder WithStrategy(IActionStrategy strategy)
		{
			action.strategy = strategy;
			return this;
		}

		public Builder AddPrerequisite(GenericWorldState prerequisite)
		{
			action.Prerequisites.Add(prerequisite);
			return this;
		}

		public Builder AddPrerequisite(WorldState worldState)
		{
			action.Prerequisites.Add(MonoBehaviourSingleton<WorldStateDictionary>.Instance[worldState]);
			return this;
		}

		public Builder AddEffect(GenericWorldState effect)
		{
			action.Effects.Add(effect);
			return this;
		}

		public Builder AddEffect(WorldState worldState)
		{
			action.Effects.Add(MonoBehaviourSingleton<WorldStateDictionary>.Instance[worldState]);
			return this;
		}

		public GoapAction Build()
		{
			return action;
		}
	}

	public enum Status
	{
		InProgress,
		Complete,
		Failed
	}

	private IActionStrategy strategy;

	private float staticCost;

	public ActionKind Action { get; }

	public string Name => Action.ToString();

	public HashSet<GenericWorldState> Prerequisites { get; } = new HashSet<GenericWorldState>();

	public HashSet<GenericWorldState> Effects { get; } = new HashSet<GenericWorldState>();

	public Status ActionStatus => strategy.Status;

	public float Cost => strategy.GetCost?.Invoke() ?? staticCost;

	private GoapAction(ActionKind action)
	{
		Action = action;
	}

	public bool ArePrerequisitesMet(NpcEntity entity)
	{
		foreach (GenericWorldState prerequisite in Prerequisites)
		{
			if (!prerequisite.Evaluate(entity))
			{
				return false;
			}
		}
		return true;
	}

	public void Start()
	{
		strategy.Start();
	}

	public void Stop()
	{
		strategy.Stop();
	}

	public void Tick(uint frame)
	{
		strategy.Tick(frame);
	}

	public void Update(float deltaTime)
	{
		strategy.Update(deltaTime);
	}

	public bool SatisfiesState(GenericWorldState worldState)
	{
		foreach (GenericWorldState effect in Effects)
		{
			if (effect.Equals(worldState))
			{
				return true;
			}
		}
		return false;
	}
}
