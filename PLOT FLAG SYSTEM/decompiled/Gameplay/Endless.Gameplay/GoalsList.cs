using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Endless.Gameplay;

[CreateAssetMenu(menuName = "ScriptableObject/GoalsList", fileName = "GoalsList")]
public class GoalsList : ScriptableObject
{
	[SerializeField]
	private List<GoalBuilderSo> goalBuilders;

	public IEnumerable<Goal> GetGoals(NpcEntity entity)
	{
		return goalBuilders.Select((GoalBuilderSo goalBuilder) => goalBuilder.GetGoal(entity));
	}
}
