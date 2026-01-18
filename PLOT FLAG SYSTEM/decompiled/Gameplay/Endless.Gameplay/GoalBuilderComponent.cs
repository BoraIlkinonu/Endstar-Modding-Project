using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay;

public abstract class GoalBuilderComponent : MonoBehaviour, IGoalBuilder
{
	[SerializeField]
	protected string GoalName;

	[SerializeField]
	protected WorldState DesiredWorldStateObject;

	[SerializeField]
	protected float BasePriority;

	public GenericWorldState DesiredWorldState => MonoBehaviourSingleton<WorldStateDictionary>.Instance[DesiredWorldStateObject];

	protected virtual float Priority(NpcEntity entity)
	{
		return BasePriority;
	}

	protected virtual void Activate(NpcEntity entity)
	{
	}

	protected virtual void Deactivate(NpcEntity entity)
	{
	}

	public Goal GetGoal(NpcEntity entity)
	{
		return new CoreGoal(GoalName, DesiredWorldState, entity, Priority, Activate, Deactivate);
	}
}
