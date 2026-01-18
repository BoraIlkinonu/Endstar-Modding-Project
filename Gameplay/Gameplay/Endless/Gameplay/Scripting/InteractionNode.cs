using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.LuaInterfaces;
using UnityEngine;

namespace Endless.Gameplay.Scripting
{
	// Token: 0x020004BD RID: 1213
	public class InteractionNode : AttributeModifierNode, IInteractionBehavior, INpcAttributeModifier
	{
		// Token: 0x170005E1 RID: 1505
		// (get) Token: 0x06001E2F RID: 7727 RVA: 0x00083C9A File Offset: 0x00081E9A
		public override string InstructionName
		{
			get
			{
				return "Generated Interaction";
			}
		}

		// Token: 0x170005E2 RID: 1506
		// (get) Token: 0x06001E30 RID: 7728 RVA: 0x00083CA1 File Offset: 0x00081EA1
		public override Type LuaObjectType
		{
			get
			{
				return typeof(Interaction);
			}
		}

		// Token: 0x170005E3 RID: 1507
		// (get) Token: 0x06001E31 RID: 7729 RVA: 0x0001BD04 File Offset: 0x00019F04
		public override NpcEnum.AttributeRank AttributeRank
		{
			get
			{
				return NpcEnum.AttributeRank.Interaction;
			}
		}

		// Token: 0x170005E4 RID: 1508
		// (get) Token: 0x06001E32 RID: 7730 RVA: 0x00083CB0 File Offset: 0x00081EB0
		public override object LuaObject
		{
			get
			{
				object obj;
				if ((obj = this.luaObject) == null)
				{
					obj = (this.luaObject = new Interaction(this));
				}
				return obj;
			}
		}

		// Token: 0x170005E5 RID: 1509
		// (get) Token: 0x06001E33 RID: 7731 RVA: 0x00083CD6 File Offset: 0x00081ED6
		// (set) Token: 0x06001E34 RID: 7732 RVA: 0x00083CEC File Offset: 0x00081EEC
		public float InteractionDuration
		{
			get
			{
				if (!this.IsHeldInteraction)
				{
					return 0f;
				}
				return this.interactionDuration;
			}
			internal set
			{
				foreach (NpcEntity npcEntity in this.currentEntities)
				{
					npcEntity.Components.Interactable.InteractionDuration = value;
				}
				this.interactionDuration = value;
			}
		}

		// Token: 0x06001E35 RID: 7733 RVA: 0x00083D50 File Offset: 0x00081F50
		public override void RescindInstruction(Context context)
		{
			NpcEntity npcEntity = this.NpcReference.GetNpcEntity(context);
			if (npcEntity)
			{
				this.RescindInstruction(npcEntity);
			}
		}

		// Token: 0x06001E36 RID: 7734 RVA: 0x000831B1 File Offset: 0x000813B1
		public override InstructionNode GetNode()
		{
			return this;
		}

		// Token: 0x06001E37 RID: 7735 RVA: 0x00083D7C File Offset: 0x00081F7C
		public override void GiveInstruction(Context context)
		{
			NpcEntity npcEntity = this.NpcReference.GetNpcEntity(context);
			if (npcEntity)
			{
				this.GiveInstruction(npcEntity);
			}
		}

		// Token: 0x06001E38 RID: 7736 RVA: 0x00083DA8 File Offset: 0x00081FA8
		protected void GiveInstruction(NpcEntity npcEntity)
		{
			npcEntity.SetInteractionBehavior(this);
			npcEntity.Components.Interactable.IsHeldInteraction = this.isHeldInteraction;
			npcEntity.Components.Interactable.InteractionDuration = this.interactionDuration;
			object[] array;
			this.scriptComponent.TryExecuteFunction("ConfigureNpc", out array, new object[] { npcEntity.Context });
			npcEntity.Components.Interactable.SetAllInteractablesEnabled(true);
			this.currentEntities.Add(npcEntity);
		}

		// Token: 0x170005E6 RID: 1510
		// (get) Token: 0x06001E39 RID: 7737 RVA: 0x00083E27 File Offset: 0x00082027
		// (set) Token: 0x06001E3A RID: 7738 RVA: 0x00083E30 File Offset: 0x00082030
		public bool IsHeldInteraction
		{
			get
			{
				return this.isHeldInteraction;
			}
			internal set
			{
				foreach (NpcEntity npcEntity in this.currentEntities)
				{
					npcEntity.Components.Interactable.IsHeldInteraction = value;
				}
				this.isHeldInteraction = value;
			}
		}

		// Token: 0x06001E3B RID: 7739 RVA: 0x00083E94 File Offset: 0x00082094
		protected void RescindInstruction(NpcEntity npcEntity)
		{
			if (npcEntity.InteractionBehavior == this)
			{
				npcEntity.ClearInteractionBehavior();
				npcEntity.Components.Interactable.SetAllInteractablesEnabled(false);
				this.currentEntities.Remove(npcEntity);
			}
		}

		// Token: 0x06001E3C RID: 7740 RVA: 0x00083EC3 File Offset: 0x000820C3
		public void InteractionComplete(Context interactor)
		{
			EndlessEvent onInteractionCompleted = this.OnInteractionCompleted;
			if (onInteractionCompleted != null)
			{
				onInteractionCompleted.Invoke(interactor);
			}
			EndlessEvent onInteractionFinished = this.OnInteractionFinished;
			if (onInteractionFinished == null)
			{
				return;
			}
			onInteractionFinished.Invoke(interactor);
		}

		// Token: 0x06001E3D RID: 7741 RVA: 0x00083EE8 File Offset: 0x000820E8
		public void InteractionCanceled(Context interactor)
		{
			EndlessEvent onInteractionCanceled = this.OnInteractionCanceled;
			if (onInteractionCanceled != null)
			{
				onInteractionCanceled.Invoke(interactor);
			}
			EndlessEvent onInteractionFinished = this.OnInteractionFinished;
			if (onInteractionFinished == null)
			{
				return;
			}
			onInteractionFinished.Invoke(interactor);
		}

		// Token: 0x170005E7 RID: 1511
		// (get) Token: 0x06001E3E RID: 7742 RVA: 0x00083F0D File Offset: 0x0008210D
		// (set) Token: 0x06001E3F RID: 7743 RVA: 0x00083F15 File Offset: 0x00082115
		public InteractionAnimation InteractionAnimation { get; set; }

		// Token: 0x06001E40 RID: 7744 RVA: 0x00083F20 File Offset: 0x00082120
		public bool AttemptInteractServerLogic(Context interactor, Context npc, int colliderIndex)
		{
			bool flag = true;
			object[] array;
			if (this.scriptComponent.TryExecuteFunction("AttemptInteraction", out array, new object[] { interactor, npc, colliderIndex }))
			{
				bool flag2;
				if (array.Length != 0)
				{
					object obj = array[0];
					flag2 = obj is bool && (bool)obj;
				}
				else
				{
					flag2 = false;
				}
				flag = flag2;
			}
			return flag;
		}

		// Token: 0x06001E41 RID: 7745 RVA: 0x00083F78 File Offset: 0x00082178
		public void OnInteracted(Context interactor, Context npc, int colliderIndex)
		{
			object[] array;
			this.scriptComponent.TryExecuteFunction("OnInteracted", out array, new object[] { interactor, npc, colliderIndex });
			npc.WorldObject.GetUserComponent<NpcEntity>().Components.Parameters.InteractionStartedTrigger = true;
		}

		// Token: 0x06001E42 RID: 7746 RVA: 0x00083FCC File Offset: 0x000821CC
		public void InteractionStopped(Context interactor, Context npc)
		{
			object[] array;
			this.scriptComponent.TryExecuteFunction("OnInteractionStopped", out array, new object[] { interactor, npc });
			NpcEntity userComponent = npc.WorldObject.GetUserComponent<NpcEntity>();
			if (userComponent.Components.Interactable.ActiveInteractors.Count == 0)
			{
				userComponent.Components.Parameters.InteractionFinishedTrigger = true;
			}
		}

		// Token: 0x04001763 RID: 5987
		private readonly List<NpcEntity> currentEntities = new List<NpcEntity>();

		// Token: 0x04001764 RID: 5988
		private float interactionDuration;

		// Token: 0x04001765 RID: 5989
		private bool isHeldInteraction;

		// Token: 0x04001766 RID: 5990
		[HideInInspector]
		public EndlessEvent OnInteractionCompleted = new EndlessEvent();

		// Token: 0x04001767 RID: 5991
		[HideInInspector]
		public EndlessEvent OnInteractionCanceled = new EndlessEvent();

		// Token: 0x04001768 RID: 5992
		[HideInInspector]
		public EndlessEvent OnInteractionFinished = new EndlessEvent();
	}
}
