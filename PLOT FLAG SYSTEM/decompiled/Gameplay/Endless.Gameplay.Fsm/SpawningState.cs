namespace Endless.Gameplay.Fsm;

public class SpawningState : FsmState
{
	public override NpcEnum.FsmState State => NpcEnum.FsmState.Spawning;

	public SpawningState(NpcEntity entity)
		: base(entity)
	{
	}

	public override void Enter()
	{
		base.Enter();
		base.Entity.Components.NpcAnimator.PlaySpawnAnimation();
	}
}
