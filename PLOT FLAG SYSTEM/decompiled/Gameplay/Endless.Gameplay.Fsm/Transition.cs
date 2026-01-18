using System;
using System.Collections.Generic;

namespace Endless.Gameplay.Fsm;

public class Transition
{
	public FsmState TargetState { get; }

	private List<Func<NpcEntity, bool>> Conditions { get; }

	public Action<NpcEntity> TransitionAction { get; }

	public Transition(FsmState targetState, List<Func<NpcEntity, bool>> conditions, Action<NpcEntity> transitionAction = null)
	{
		TargetState = targetState;
		Conditions = conditions;
		TransitionAction = transitionAction;
	}

	public Transition(FsmState targetState, Func<NpcEntity, bool> condition, Action<NpcEntity> transitionAction = null)
	{
		TargetState = targetState;
		Conditions = new List<Func<NpcEntity, bool>> { condition };
		TransitionAction = transitionAction;
	}

	public bool AreConditionsMet(NpcEntity entity)
	{
		for (int i = 0; i < Conditions.Count; i++)
		{
			Func<NpcEntity, bool> func = Conditions[i];
			if (func != null && !func(entity))
			{
				return false;
			}
		}
		return true;
	}
}
