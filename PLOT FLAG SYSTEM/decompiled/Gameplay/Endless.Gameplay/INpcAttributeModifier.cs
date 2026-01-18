using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay;

public interface INpcAttributeModifier
{
	CombatMode CombatMode { get; }

	DamageMode DamageMode { get; }

	PhysicsMode PhysicsMode { get; }

	NpcEnum.FallMode FallMode { get; }

	MovementMode MovementMode { get; }

	NpcEnum.AttributeRank AttributeRank { get; }

	Endless.Gameplay.Scripting.InstructionNode GetNode();
}
