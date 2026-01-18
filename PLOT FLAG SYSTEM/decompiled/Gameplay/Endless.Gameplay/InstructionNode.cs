using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay;

public abstract class InstructionNode : EndlessBehaviour, IInstructionNode
{
	private Component entityObjectSource;

	private INpcSource entitySource;

	private NpcEntity providedEntity;

	private Component EntitySourceObject => entityObjectSource ?? (entityObjectSource = GetComponentInParent<INpcSource>() as Component);

	protected NpcEntity Entity
	{
		get
		{
			if (!providedEntity)
			{
				return entitySource.Npc;
			}
			return providedEntity;
		}
	}

	public abstract string InstructionName { get; }

	private void Awake()
	{
		entitySource = EntitySourceObject.GetComponent<INpcSource>();
	}

	public Context GetContext()
	{
		return entitySource.Npc.Context;
	}

	public virtual void GiveInstruction(Context context)
	{
		if (context.IsNpc())
		{
			providedEntity = context.WorldObject.GetUserComponent<NpcEntity>();
		}
	}

	public virtual void RescindInstruction(Context context)
	{
		providedEntity = null;
	}
}
