namespace Endless.Gameplay.Fsm;

public class FsmParameters
{
	public bool InteractionStartedTrigger { get; set; }

	public bool PhysicsTrigger { get; set; }

	public bool FlinchTrigger { get; set; }

	public bool FlinchFinishedTrigger { get; set; }

	public bool InteractionFinishedTrigger { get; set; }

	public bool WillRoll { get; set; }

	public bool WillSplat { get; set; }

	public bool WarpCompleteTrigger { get; set; }

	public bool JumpCompleteTrigger { get; set; }

	public bool ReviveTrigger { get; set; }

	public bool WarpTrigger { get; set; }

	public bool JumpTrigger { get; set; }

	public bool LandingCompleteTrigger { get; set; }

	public bool StandUpCompleteTrigger { get; set; }

	public bool LocalPhysicsTrigger { get; set; }

	public bool NetworkSimulationTrigger { get; set; }

	public bool TeleportCompleteTrigger { get; set; }

	public bool TeleportTrigger { get; set; }

	public bool IsSpawnAnimationComplete { get; set; }

	public FsmParameters(IndividualStateUpdater stateUpdater)
	{
		stateUpdater.OnCleanupTriggers += HandleCleanupTriggers;
	}

	private void HandleCleanupTriggers()
	{
		InteractionStartedTrigger = false;
		PhysicsTrigger = false;
		FlinchTrigger = false;
		FlinchFinishedTrigger = false;
		InteractionFinishedTrigger = false;
		WarpCompleteTrigger = false;
		JumpCompleteTrigger = false;
		ReviveTrigger = false;
		WarpTrigger = false;
		JumpTrigger = false;
		LandingCompleteTrigger = false;
		StandUpCompleteTrigger = false;
		LocalPhysicsTrigger = false;
		NetworkSimulationTrigger = false;
		TeleportCompleteTrigger = false;
		TeleportTrigger = false;
	}
}
