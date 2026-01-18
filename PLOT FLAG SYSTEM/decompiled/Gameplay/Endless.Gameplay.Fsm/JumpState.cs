namespace Endless.Gameplay.Fsm;

public class JumpState : FsmState
{
	public override NpcEnum.FsmState State => NpcEnum.FsmState.Jump;

	public JumpState(NpcEntity entity)
		: base(entity)
	{
	}
}
