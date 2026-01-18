namespace Endless.Gameplay.Fsm;

public class NeutralState : FsmState
{
	public override NpcEnum.FsmState State => NpcEnum.FsmState.Neutral;

	public NeutralState(NpcEntity entity)
		: base(entity)
	{
	}

	public override void Enter()
	{
		base.Enter();
		base.Components.IndividualStateUpdater.OnTickAi += HandleOnTickAi;
		base.Components.Agent.enabled = true;
		base.Components.Animator.SetBool(NpcAnimator.Grounded, value: true);
		base.Components.Animator.SetFloat(NpcAnimator.FallTime, 0f);
	}

	private void HandleOnTickAi()
	{
		base.Components.PlanFollower.Tick(NetClock.CurrentFrame);
		base.Components.GoapController.TickGoap(NetClock.CurrentFrame);
	}

	protected override void Update()
	{
		base.Update();
		base.Components.Animator.SetFloat(NpcAnimator.SlopeAngle, base.Components.SlopeComponent.SlopeAngle);
		base.Components.Animator.SetBool(NpcAnimator.Moving, base.Components.VelocityTracker.SmoothedSpeed > 0.2f);
		base.Components.PlanFollower.Update();
	}

	protected override void WriteState(ref NpcState npcState)
	{
		npcState.slopeAngle = base.Components.SlopeComponent.SlopeAngle;
		npcState.isMoving = base.Components.VelocityTracker.SmoothedSpeed > 0.2f;
		npcState.isGrounded = !base.Components.PathFollower.IsJumping;
	}

	protected override void Exit()
	{
		base.Exit();
		base.Components.IndividualStateUpdater.OnTickAi -= HandleOnTickAi;
		base.Components.Attack?.ClearAttackQueue();
		base.Components.GoapController.Uncontrolled();
		base.Components.PathFollower.StopPath();
		base.Components.Agent.enabled = false;
		base.Components.Animator.SetBool(NpcAnimator.Moving, value: false);
	}
}
