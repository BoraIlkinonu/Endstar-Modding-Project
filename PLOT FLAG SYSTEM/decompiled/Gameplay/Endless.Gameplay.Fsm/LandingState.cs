using UnityEngine;

namespace Endless.Gameplay.Fsm;

public class LandingState : FsmState
{
	private uint exitFrame;

	public override NpcEnum.FsmState State => NpcEnum.FsmState.Landing;

	public LandingState(NpcEntity entity)
		: base(entity)
	{
	}

	public override void Enter()
	{
		base.Enter();
		base.Entity.Components.Animator.SetTrigger(NpcAnimator.Landed);
		base.Entity.Components.IndividualStateUpdater.GetCurrentState().landed = true;
		exitFrame = NetClock.CurrentFrame + 4;
		base.Entity.Components.IndividualStateUpdater.OnTickAi += HandleOnTickAi;
	}

	protected override void Update()
	{
		base.Update();
		base.Components.CharacterController.SimpleMove(Vector3.zero);
	}

	private void HandleOnTickAi()
	{
		if (NetClock.CurrentFrame >= exitFrame)
		{
			base.Components.Parameters.LandingCompleteTrigger = true;
		}
	}

	protected override void Exit()
	{
		base.Exit();
		base.Entity.Components.IndividualStateUpdater.OnTickAi -= HandleOnTickAi;
		base.Components.Animator.ResetTrigger(NpcAnimator.Landed);
	}
}
