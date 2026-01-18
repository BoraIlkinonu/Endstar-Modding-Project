namespace Endless.Gameplay;

public class NpcComponent : EndlessBehaviour
{
	private NpcEntity entity;

	private Components components;

	protected NpcEntity NpcEntity => entity ?? (entity = GetComponentInParent<NpcEntity>());

	protected Components Components => components ?? (components = GetComponentInParent<Components>());
}
