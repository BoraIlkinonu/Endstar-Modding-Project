using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay.LuaInterfaces;

public class InstructionNode
{
	protected Endless.Gameplay.Scripting.InstructionNode ManagedNode { get; }

	internal InstructionNode(Endless.Gameplay.Scripting.InstructionNode node)
	{
		ManagedNode = node;
	}

	public Context GetContext()
	{
		return ManagedNode.Context;
	}

	public void SetContextBehavior(bool useContext)
	{
		ManagedNode.NpcReference.useContext = useContext;
	}

	public Context GetNpcReference()
	{
		return ManagedNode.NpcReference.GetNpc();
	}

	public float GetTime()
	{
		return Time.time;
	}

	public virtual void GiveInstruction(Context _, Context context)
	{
		ManagedNode.GiveInstruction(context);
	}

	public virtual void RescindInstruction(Context _, Context context)
	{
		ManagedNode.RescindInstruction(context);
	}
}
