using System;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces
{
	// Token: 0x02000450 RID: 1104
	public class GroupInteraction : GroupInstruction
	{
		// Token: 0x06001B92 RID: 7058 RVA: 0x0007C629 File Offset: 0x0007A829
		internal GroupInteraction(IGroupInstructionNode node)
			: base(node)
		{
		}

		// Token: 0x17000586 RID: 1414
		// (get) Token: 0x06001B93 RID: 7059 RVA: 0x0007C68E File Offset: 0x0007A88E
		private InteractionNode GroupNodeAsInteraction
		{
			get
			{
				return (InteractionNode)base.GroupNode;
			}
		}

		// Token: 0x06001B94 RID: 7060 RVA: 0x0007C69B File Offset: 0x0007A89B
		public void SetIsHeldInteraction(Context instigator, bool isHeldInteraction)
		{
			this.GroupNodeAsInteraction.IsHeldInteraction = isHeldInteraction;
		}

		// Token: 0x06001B95 RID: 7061 RVA: 0x0007C6A9 File Offset: 0x0007A8A9
		public void SetInteractionAnimation(Context instigator, InteractionAnimation interactionAnimation)
		{
			this.GroupNodeAsInteraction.InteractionAnimation = interactionAnimation;
		}

		// Token: 0x06001B96 RID: 7062 RVA: 0x0007C6B7 File Offset: 0x0007A8B7
		public void SetInteractionDuration(Context instigator, float duration)
		{
			this.GroupNodeAsInteraction.InteractionDuration = duration;
		}

		// Token: 0x06001B97 RID: 7063 RVA: 0x0007C6C8 File Offset: 0x0007A8C8
		public void StopInteraction(Context interactor, Context npcContext)
		{
			object[] array;
			this.GroupNodeAsInteraction.scriptComponent.TryExecuteFunction("OnInteractionStopped", out array, new object[] { interactor, npcContext });
			npcContext.WorldObject.GetUserComponent<NpcEntity>().Components.Parameters.InteractionFinishedTrigger = true;
			interactor.WorldObject.GetComponent<InteractorBase>().InteractableStoppedInteraction();
		}

		// Token: 0x06001B98 RID: 7064 RVA: 0x0007C726 File Offset: 0x0007A926
		public void InteractionCompleted(Context interactor)
		{
			this.GroupNodeAsInteraction.InteractionComplete(interactor);
		}

		// Token: 0x06001B99 RID: 7065 RVA: 0x0007C734 File Offset: 0x0007A934
		public void InteractionCanceled(Context interactor)
		{
			this.GroupNodeAsInteraction.InteractionCanceled(interactor);
		}
	}
}
