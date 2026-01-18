using System;
using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Props.ReferenceComponents;
using Endless.Shared;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay.Scripting;

public abstract class InstructionNode : AbstractBlock, IInstructionNode, IScriptInjector, IAwakeSubscriber, IGameEndSubscriber
{
	public NpcInstanceReference NpcReference = new NpcInstanceReference(SerializableGuid.Empty, useContext: false);

	protected readonly Dictionary<NpcEntity, List<Goal>> AddedGoalsByNpc = new Dictionary<NpcEntity, List<Goal>>();

	internal EndlessScriptComponent scriptComponent;

	protected object luaObject;

	[field: SerializeField]
	protected EndlessProp Prop { get; set; }

	public Vector3Int CellPosition => Stage.WorldSpacePointToGridCoordinate(Prop.transform.position);

	public abstract string InstructionName { get; }

	public abstract object LuaObject { get; }

	public abstract Type LuaObjectType { get; }

	public override void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
	{
		base.ComponentInitialize(referenceBase, endlessProp);
		Prop = endlessProp;
	}

	public abstract void GiveInstruction(Context context);

	public abstract void RescindInstruction(Context context);

	public Context GetContext()
	{
		return base.Context;
	}

	public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
	{
		scriptComponent = endlessScriptComponent;
	}

	public void EndlessAwake()
	{
		MonoBehaviourSingleton<NodeMap>.Instance.InstructionNodesByCellPosition.Add(CellPosition, this);
	}

	public void EndlessGameEnd()
	{
		MonoBehaviourSingleton<NodeMap>.Instance.InstructionNodesByCellPosition.Remove(CellPosition);
	}
}
