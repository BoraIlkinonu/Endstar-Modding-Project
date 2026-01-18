using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay;

public class IdleNode : BehaviorNode
{
	[SerializeField]
	private bool canFidget;

	public override void GiveInstruction(Context context)
	{
		base.GiveInstruction(context);
		base.Entity.NpcBlackboard.Set(NpcBlackboard.Key.CanFidget, canFidget);
	}
}
