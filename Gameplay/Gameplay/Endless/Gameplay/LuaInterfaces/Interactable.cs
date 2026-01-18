using System;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay.LuaInterfaces
{
	// Token: 0x02000456 RID: 1110
	public class Interactable
	{
		// Token: 0x06001BBD RID: 7101 RVA: 0x0007C996 File Offset: 0x0007AB96
		internal Interactable(Interactable interactable)
		{
			this.interactable = interactable;
		}

		// Token: 0x06001BBE RID: 7102 RVA: 0x0007C9A5 File Offset: 0x0007ABA5
		public int GetNumberOfInteractables()
		{
			return this.interactable.GetNumberOfInteractables();
		}

		// Token: 0x06001BBF RID: 7103 RVA: 0x0007C9B2 File Offset: 0x0007ABB2
		public void SetInteractionAnimation(Context instigator, int interactionAnimation)
		{
			this.interactable.InteractionAnimation = (InteractionAnimation)interactionAnimation;
		}

		// Token: 0x06001BC0 RID: 7104 RVA: 0x0007C9C0 File Offset: 0x0007ABC0
		public void SetIsHeldInteraction(Context instigator, bool isHeldInteraction)
		{
			this.interactable.IsHeldInteraction = isHeldInteraction;
		}

		// Token: 0x06001BC1 RID: 7105 RVA: 0x0007C9D0 File Offset: 0x0007ABD0
		public void StopInteraction(Context instigator, Context targetInteractor)
		{
			object[] array;
			this.interactable.scriptComponent.TryExecuteFunction("OnInteractionStopped", out array, new object[] { targetInteractor });
			InteractorBase component = targetInteractor.WorldObject.GetComponent<InteractorBase>();
			if (component != null)
			{
				component.InteractableStoppedInteraction();
			}
		}

		// Token: 0x06001BC2 RID: 7106 RVA: 0x0007CA1A File Offset: 0x0007AC1A
		internal void SetInteractionDuration(Context instigator, float duration)
		{
			this.interactable.InteractionDuration = duration;
		}

		// Token: 0x06001BC3 RID: 7107 RVA: 0x0007CA28 File Offset: 0x0007AC28
		public void SetInteractableEnabled(Context instigator, int index, bool isEnabled)
		{
			this.interactable.SetInteractableEnabled(instigator, index, isEnabled);
		}

		// Token: 0x06001BC4 RID: 7108 RVA: 0x0007CA38 File Offset: 0x0007AC38
		public void SetAllInteractablesEnabled(Context instigator, bool isEnabled)
		{
			for (int i = 0; i < this.GetNumberOfInteractables(); i++)
			{
				this.interactable.SetInteractableEnabled(instigator, i, isEnabled);
			}
		}

		// Token: 0x06001BC5 RID: 7109 RVA: 0x0007CA64 File Offset: 0x0007AC64
		public void SetIconFromPropReference(Context instigator, PropLibraryReference propLibraryReference)
		{
			this.interactable.SetIconFromPropReference(propLibraryReference);
		}

		// Token: 0x06001BC6 RID: 7110 RVA: 0x0007CA72 File Offset: 0x0007AC72
		public void SetAnchorOverride(Context instigator, int index, global::UnityEngine.Vector3 overridePosition)
		{
			this.interactable.SetAnchorPosition(new global::UnityEngine.Vector3?(overridePosition), index);
		}

		// Token: 0x06001BC7 RID: 7111 RVA: 0x0007CA88 File Offset: 0x0007AC88
		public void ClearAnchorOverride(Context instigator, int index)
		{
			this.interactable.SetAnchorPosition(null, index);
		}

		// Token: 0x06001BC8 RID: 7112 RVA: 0x0007CAAA File Offset: 0x0007ACAA
		public void SetUsePlayerAnchor(Context instigator, int index, bool usePlayerAnchor)
		{
			this.interactable.SetUsePlayerAnchor(usePlayerAnchor, index);
		}

		// Token: 0x06001BC9 RID: 7113 RVA: 0x0007CAB9 File Offset: 0x0007ACB9
		public void SetHidePromptDuringInteraction(Context instigator, bool value)
		{
			this.interactable.HidePromptDuringInteraction = value;
		}

		// Token: 0x040015B9 RID: 5561
		private readonly Interactable interactable;
	}
}
