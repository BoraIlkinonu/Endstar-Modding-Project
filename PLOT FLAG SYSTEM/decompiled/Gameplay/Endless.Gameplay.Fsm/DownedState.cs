namespace Endless.Gameplay.Fsm;

public class DownedState : FsmState
{
	public override NpcEnum.FsmState State => NpcEnum.FsmState.Downed;

	public DownedState(NpcEntity entity)
		: base(entity)
	{
	}

	public override void Enter()
	{
		base.Enter();
		base.Entity.ExplosionsOnlyClientRpc();
		base.Entity.DownedClientRpc();
		base.Entity.Components.Animator.SetTrigger(NpcAnimator.Downed);
		base.Entity.Components.Animator.SetBool(NpcAnimator.Dbno, value: true);
	}
}
