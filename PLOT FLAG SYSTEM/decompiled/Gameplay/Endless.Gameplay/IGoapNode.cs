namespace Endless.Gameplay;

public interface IGoapNode : IInstructionNode, INpcAttributeModifier
{
	void RemoveNodeGoals(NpcEntity entity);

	void AddNodeGoals(NpcEntity entity);
}
