using System.Collections.Generic;

namespace Endless.Gameplay;

public class ActionPlan
{
	public Goal Goal { get; }

	public Stack<GoapAction> Actions { get; }

	public float TotalCost { get; set; }

	public ActionPlan(Goal goal, Stack<GoapAction> actions, float totalCost)
	{
		Goal = goal;
		Actions = actions;
		TotalCost = totalCost;
	}
}
