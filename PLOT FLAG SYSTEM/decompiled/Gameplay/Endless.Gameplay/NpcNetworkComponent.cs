namespace Endless.Gameplay;

public abstract class NpcNetworkComponent : EndlessNetworkBehaviour
{
	private NpcEntity entity;

	private Components components;

	protected NpcEntity NpcEntity => entity ?? (entity = GetComponentInParent<NpcEntity>());

	protected Components Components => components ?? (components = GetComponentInParent<Components>());

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
		return "NpcNetworkComponent";
	}
}
