using Endless.Gameplay.LuaEnums;

namespace Endless.Gameplay.Scripting;

public interface IGroupNode
{
	void GiveGroupInstruction(Context _, NpcGroup group)
	{
	}

	void RescindGroupInstruction(Context _, NpcGroup group)
	{
	}
}
