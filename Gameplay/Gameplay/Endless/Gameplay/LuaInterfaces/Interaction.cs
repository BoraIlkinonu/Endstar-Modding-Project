using System;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;

namespace Endless.Gameplay.LuaInterfaces
{
	// Token: 0x02000457 RID: 1111
	public class Interaction : InstructionNode
	{
		// Token: 0x06001BCA RID: 7114 RVA: 0x0007C29B File Offset: 0x0007A49B
		internal Interaction(InstructionNode node)
			: base(node)
		{
		}

		// Token: 0x17000588 RID: 1416
		// (get) Token: 0x06001BCB RID: 7115 RVA: 0x0007CAC7 File Offset: 0x0007ACC7
		private InteractionNode ManagedAsInteraction
		{
			get
			{
				return (InteractionNode)base.ManagedNode;
			}
		}

		// Token: 0x06001BCC RID: 7116 RVA: 0x0007CAD4 File Offset: 0x0007ACD4
		public void SetIsHeldInteraction(Context instigator, bool isHeldInteraction)
		{
			this.ManagedAsInteraction.IsHeldInteraction = isHeldInteraction;
		}

		// Token: 0x06001BCD RID: 7117 RVA: 0x0007CAE2 File Offset: 0x0007ACE2
		public void SetInteractionAnimation(Context instigator, InteractionAnimation interactionAnimation)
		{
			this.ManagedAsInteraction.InteractionAnimation = interactionAnimation;
		}

		// Token: 0x06001BCE RID: 7118 RVA: 0x0007CAF0 File Offset: 0x0007ACF0
		public void SetInteractionDuration(Context instigator, float duration)
		{
			this.ManagedAsInteraction.InteractionDuration = duration;
		}

		// Token: 0x06001BCF RID: 7119 RVA: 0x0007CB00 File Offset: 0x0007AD00
		public void StopInteraction(Context interactor, Context npcContext)
		{
			object[] array;
			this.ManagedAsInteraction.scriptComponent.TryExecuteFunction("OnInteractionStopped", out array, new object[] { interactor, npcContext });
			npcContext.WorldObject.GetUserComponent<NpcEntity>().Components.Parameters.InteractionFinishedTrigger = true;
			interactor.WorldObject.GetComponent<InteractorBase>().InteractableStoppedInteraction();
		}

		// Token: 0x06001BD0 RID: 7120 RVA: 0x0007CB5E File Offset: 0x0007AD5E
		public override void GiveInstruction(Context instigator, Context npc)
		{
			this.ManagedAsInteraction.GiveInstruction(npc);
		}

		// Token: 0x06001BD1 RID: 7121 RVA: 0x0007CB6C File Offset: 0x0007AD6C
		public override void RescindInstruction(Context interactor, Context npcContext)
		{
			this.ManagedAsInteraction.RescindInstruction(npcContext);
		}

		// Token: 0x06001BD2 RID: 7122 RVA: 0x0007CB7A File Offset: 0x0007AD7A
		public void InteractionCompleted(Context interactor)
		{
			this.ManagedAsInteraction.InteractionComplete(interactor);
		}

		// Token: 0x06001BD3 RID: 7123 RVA: 0x0007CB88 File Offset: 0x0007AD88
		public void InteractionCanceled(Context interactor)
		{
			this.ManagedAsInteraction.InteractionCanceled(interactor);
		}
	}
}
