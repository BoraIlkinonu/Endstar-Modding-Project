using Endless.Gameplay.LuaEnums;
using UnityEngine;

namespace Endless.Gameplay.Scripting;

public abstract class AttributeModifierNode : InstructionNode, INpcAttributeModifier
{
	[SerializeField]
	protected CombatMode combatMode;

	[SerializeField]
	protected DamageMode damageMode;

	[SerializeField]
	protected PhysicsMode physicsMode;

	[SerializeField]
	protected NpcEnum.FallMode fallMode;

	[SerializeField]
	protected MovementMode movementMode;

	public CombatMode CombatMode => combatMode;

	public DamageMode DamageMode => damageMode;

	public PhysicsMode PhysicsMode => physicsMode;

	public NpcEnum.FallMode FallMode => fallMode;

	public MovementMode MovementMode => movementMode;

	public abstract NpcEnum.AttributeRank AttributeRank { get; }

	public abstract InstructionNode GetNode();
}
