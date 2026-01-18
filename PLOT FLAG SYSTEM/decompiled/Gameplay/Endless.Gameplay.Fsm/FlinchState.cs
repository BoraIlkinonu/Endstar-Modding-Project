namespace Endless.Gameplay.Fsm;

public class FlinchState : FsmState
{
	private uint exitFrame;

	public override NpcEnum.FsmState State => NpcEnum.FsmState.Flinch;

	public FlinchState(NpcEntity entity)
		: base(entity)
	{
	}

	public override void Enter()
	{
		base.Enter();
		exitFrame = NetClock.CurrentFrame + 5;
		base.Entity.Components.Animator.SetTrigger(NpcAnimator.Flinch);
		base.Entity.Components.IndividualStateUpdater.OnTickAi += HandleOnTickAi;
	}

	protected override void Exit()
	{
		base.Exit();
		base.Entity.Components.IndividualStateUpdater.OnTickAi -= HandleOnTickAi;
	}

	private void HandleOnTickAi()
	{
		if (NetClock.CurrentFrame >= exitFrame)
		{
			base.Components.Parameters.FlinchFinishedTrigger = true;
		}
	}
}
