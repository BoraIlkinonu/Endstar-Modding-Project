namespace Endless.Gameplay;

public abstract class WorldUsableController : EndlessNetworkBehaviour
{
	public abstract WorldUsableDefinition WorldUsableDefinition { get; }

	public abstract bool AttemptInteract(InteractorBase interactorBase, int colliderIndex);

	public abstract void CancelInteraction(InteractorBase interactor);

	public abstract bool IsInteractingWith(InteractorBase interactor);

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected internal override string __getTypeName()
	{
		return "WorldUsableController";
	}
}
