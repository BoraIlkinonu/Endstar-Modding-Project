using System;
using UnityEngine;

namespace Endless.Gameplay;

public class PlanFollower
{
	private GoapAction currentAction;

	private ActionPlan actionPlan;

	public ActionPlan ActionPlan
	{
		get
		{
			return actionPlan;
		}
		set
		{
			if (actionPlan != null)
			{
				currentAction?.Stop();
				currentAction = null;
				actionPlan.Goal.GoalTerminated(Goal.TerminationCode.Interrupted);
			}
			actionPlan = value;
			actionPlan?.Goal.Activate();
			if (actionPlan != null && actionPlan.Actions.Count > 0)
			{
				currentAction = actionPlan.Actions.Pop();
				currentAction.Start();
			}
		}
	}

	public PlanFollower(NpcEntity entity)
	{
	}

	public void Tick(uint frame)
	{
		if (currentAction != null)
		{
			currentAction.Tick(frame);
			switch (currentAction.ActionStatus)
			{
			case GoapAction.Status.Complete:
				currentAction.Stop();
				currentAction = null;
				if (ActionPlan.Actions.Count == 0)
				{
					ActionPlan.Goal.GoalTerminated(Goal.TerminationCode.Completed);
					ActionPlan = null;
				}
				break;
			case GoapAction.Status.Failed:
				currentAction.Stop();
				currentAction = null;
				ActionPlan.Goal.GoalTerminated(Goal.TerminationCode.Failed);
				ActionPlan = null;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case GoapAction.Status.InProgress:
				break;
			}
		}
		if (currentAction == null && ActionPlan != null && ActionPlan.Actions.Count > 0)
		{
			currentAction = ActionPlan.Actions.Pop();
			currentAction.Start();
		}
	}

	public void Update()
	{
		currentAction?.Update(Time.deltaTime);
	}
}
