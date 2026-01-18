using Endless.Gameplay.Scripting;

namespace Endless.Gameplay;

public interface IInstructionNode
{
	string InstructionName { get; }

	void GiveInstruction(Context context);

	void RescindInstruction(Context context);

	Context GetContext();
}
