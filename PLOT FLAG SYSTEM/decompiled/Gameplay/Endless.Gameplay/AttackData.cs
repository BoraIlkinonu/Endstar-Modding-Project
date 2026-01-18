using System.Collections.Generic;

namespace Endless.Gameplay;

public readonly struct AttackData
{
	public readonly uint StartFrame;

	public readonly uint EndFrame;

	public readonly MeleeAttackData MeleeAttackData;

	public readonly HashSet<HittableComponent> meleeHits;

	public AttackData(uint startFrame, uint endFrame, MeleeAttackData meleeAttackData)
	{
		StartFrame = startFrame;
		EndFrame = endFrame;
		MeleeAttackData = meleeAttackData;
		meleeHits = new HashSet<HittableComponent>();
	}
}
