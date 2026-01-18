using System.Collections.Generic;

namespace Endless.Gameplay.Fsm;

public abstract class FsmState
{
	protected NpcEntity Entity { get; }

	protected Components Components => Entity.Components;

	private List<Transition> Transitions { get; set; } = new List<Transition>();

	public string StateName => State.ToString();

	public abstract NpcEnum.FsmState State { get; }

	protected FsmState(NpcEntity entity)
	{
		Entity = entity;
	}

	public void Initialize(List<Transition> transitions)
	{
		Transitions = transitions;
	}

	public void Initialize(Transition transition)
	{
		Initialize(new List<Transition> { transition });
	}

	public virtual void Enter()
	{
		Entity.CurrentState = this;
		Entity.Components.IndividualStateUpdater.OnProcessTransitions += HandleOnProcessTransitions;
		Entity.Components.Proxy.OnUpdate += Update;
		Entity.Components.IndividualStateUpdater.OnWriteState += WriteState;
	}

	protected virtual void Exit()
	{
		Entity.Components.IndividualStateUpdater.OnProcessTransitions -= HandleOnProcessTransitions;
		Entity.Components.Proxy.OnUpdate -= Update;
		Entity.Components.IndividualStateUpdater.OnWriteState -= WriteState;
	}

	private void HandleOnProcessTransitions()
	{
		for (int i = 0; i < Transitions.Count; i++)
		{
			Transition transition = Transitions[i];
			if (transition.AreConditionsMet(Entity))
			{
				Exit();
				transition.TargetState.Enter();
				transition.TransitionAction?.Invoke(Entity);
				break;
			}
		}
	}

	protected virtual void Update()
	{
	}

	protected virtual void WriteState(ref NpcState npcState)
	{
	}
}
