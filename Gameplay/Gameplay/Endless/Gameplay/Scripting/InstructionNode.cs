using System;
using System.Collections.Generic;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Props.ReferenceComponents;
using Endless.Shared;
using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay.Scripting
{
	// Token: 0x020004BC RID: 1212
	public abstract class InstructionNode : AbstractBlock, IInstructionNode, IScriptInjector, IAwakeSubscriber, IGameEndSubscriber
	{
		// Token: 0x170005DC RID: 1500
		// (get) Token: 0x06001E21 RID: 7713 RVA: 0x00083BFC File Offset: 0x00081DFC
		// (set) Token: 0x06001E22 RID: 7714 RVA: 0x00083C04 File Offset: 0x00081E04
		protected EndlessProp Prop { get; set; }

		// Token: 0x170005DD RID: 1501
		// (get) Token: 0x06001E23 RID: 7715 RVA: 0x00083C0D File Offset: 0x00081E0D
		public Vector3Int CellPosition
		{
			get
			{
				return Stage.WorldSpacePointToGridCoordinate(this.Prop.transform.position);
			}
		}

		// Token: 0x06001E24 RID: 7716 RVA: 0x00083C24 File Offset: 0x00081E24
		public override void ComponentInitialize(ReferenceBase referenceBase, EndlessProp endlessProp)
		{
			base.ComponentInitialize(referenceBase, endlessProp);
			this.Prop = endlessProp;
		}

		// Token: 0x170005DE RID: 1502
		// (get) Token: 0x06001E25 RID: 7717
		public abstract string InstructionName { get; }

		// Token: 0x06001E26 RID: 7718
		public abstract void GiveInstruction(Context context);

		// Token: 0x06001E27 RID: 7719
		public abstract void RescindInstruction(Context context);

		// Token: 0x06001E28 RID: 7720 RVA: 0x00083C35 File Offset: 0x00081E35
		public Context GetContext()
		{
			return base.Context;
		}

		// Token: 0x170005DF RID: 1503
		// (get) Token: 0x06001E29 RID: 7721
		public abstract object LuaObject { get; }

		// Token: 0x170005E0 RID: 1504
		// (get) Token: 0x06001E2A RID: 7722
		public abstract Type LuaObjectType { get; }

		// Token: 0x06001E2B RID: 7723 RVA: 0x00083C3D File Offset: 0x00081E3D
		public void ScriptInitialize(EndlessScriptComponent endlessScriptComponent)
		{
			this.scriptComponent = endlessScriptComponent;
		}

		// Token: 0x06001E2C RID: 7724 RVA: 0x00083C46 File Offset: 0x00081E46
		public void EndlessAwake()
		{
			MonoBehaviourSingleton<NodeMap>.Instance.InstructionNodesByCellPosition.Add(this.CellPosition, this);
		}

		// Token: 0x06001E2D RID: 7725 RVA: 0x00083C5E File Offset: 0x00081E5E
		public void EndlessGameEnd()
		{
			MonoBehaviourSingleton<NodeMap>.Instance.InstructionNodesByCellPosition.Remove(this.CellPosition);
		}

		// Token: 0x0400175E RID: 5982
		public NpcInstanceReference NpcReference = new NpcInstanceReference(SerializableGuid.Empty, false);

		// Token: 0x0400175F RID: 5983
		protected readonly Dictionary<NpcEntity, List<Goal>> AddedGoalsByNpc = new Dictionary<NpcEntity, List<Goal>>();

		// Token: 0x04001760 RID: 5984
		internal EndlessScriptComponent scriptComponent;

		// Token: 0x04001762 RID: 5986
		protected object luaObject;
	}
}
