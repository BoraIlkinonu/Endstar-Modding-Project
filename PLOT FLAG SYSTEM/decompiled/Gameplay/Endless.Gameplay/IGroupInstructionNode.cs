using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay;

public interface IGroupInstructionNode : IInstructionNode
{
	void GiveGroupInstruction(Context instigator, NpcGroup group);

	void RescindGroupInstruction(Context instigator, NpcGroup group);
}
