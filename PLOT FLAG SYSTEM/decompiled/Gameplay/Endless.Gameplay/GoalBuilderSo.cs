using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay;

[CreateAssetMenu(menuName = "ScriptableObject/GoalBuilders/GenericGoalBuilder", fileName = "GoalBuilder")]
public class GoalBuilderSo : ScriptableObject, IGoalBuilder
{
	[SerializeField]
	protected string GoalName;

	[SerializeField]
	protected WorldState DesiredWorldStateObject;

	[SerializeField]
	protected float basePriority;

	public GenericWorldState DesiredWorldState => MonoBehaviourSingleton<WorldStateDictionary>.Instance[DesiredWorldStateObject];

	public Goal GetGoal(NpcEntity entity)
	{
		return new CoreGoal(GoalName, DesiredWorldState, entity, Priority, Activate, Deactivate);
	}

	protected virtual float Priority(NpcEntity entity)
	{
		return basePriority;
	}

	protected virtual void Activate(NpcEntity entity)
	{
	}

	protected virtual void Deactivate(NpcEntity entity)
	{
	}
}
