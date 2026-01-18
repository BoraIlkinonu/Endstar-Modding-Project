using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Endless.Gameplay;

public abstract class GoapNode : AttributeModifierNode, IGoapNode, IInstructionNode, INpcAttributeModifier
{
	[SerializeField]
	protected List<Object> InstructionGoals = new List<Object>();

	protected readonly Dictionary<NpcEntity, List<Goal>> AddedGoalsByNpcEntity = new Dictionary<NpcEntity, List<Goal>>();

	protected void OnValidate()
	{
		foreach (Object item in InstructionGoals.Where((Object obj) => (bool)obj && !(obj is IGoalBuilder)).ToList())
		{
			InstructionGoals.Remove(item);
			Debug.LogWarning("objects in instructionGoals must reference an IGoalBuilder");
		}
	}

	public abstract void AddNodeGoals(NpcEntity entity);

	public abstract void RemoveNodeGoals(NpcEntity entity);
}
