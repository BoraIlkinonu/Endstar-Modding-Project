using System;
using Endless.Shared.Debugging;
using Runtime.Shared;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x020001E9 RID: 489
	public class UIEnumDropdownView : UIBaseEnumView, IUIInteractable
	{
		// Token: 0x17000237 RID: 567
		// (get) Token: 0x06000C15 RID: 3093 RVA: 0x00034498 File Offset: 0x00032698
		// (set) Token: 0x06000C16 RID: 3094 RVA: 0x000344A0 File Offset: 0x000326A0
		public override UIBaseEnumView.Styles Style { get; protected set; }

		// Token: 0x06000C17 RID: 3095 RVA: 0x000344A9 File Offset: 0x000326A9
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.enumDropdownHandler.OnEnumValueChanged.AddListener(new UnityAction<Enum>(base.InvokeOnEnumChanged));
		}

		// Token: 0x06000C18 RID: 3096 RVA: 0x000344DF File Offset: 0x000326DF
		public override void View(Enum model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { model });
			}
			this.enumDropdownHandler.InitializeDropdownWithEnum(model.GetType());
			this.enumDropdownHandler.SetEnumValue(model, false);
		}

		// Token: 0x06000C19 RID: 3097 RVA: 0x0003451C File Offset: 0x0003271C
		public void SetInteractable(bool interactable)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetInteractable", new object[] { interactable });
			}
			this.enumDropdownHandler.SetIsInteractable(interactable);
		}

		// Token: 0x06000C1A RID: 3098 RVA: 0x0003454C File Offset: 0x0003274C
		public void SetLabel(string label)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetLabel", new object[] { label });
			}
			this.enumDropdownHandler.SetLabel(label);
		}

		// Token: 0x040007CD RID: 1997
		[Header("UIEnumDropdownView")]
		[SerializeField]
		private UIDropdownEnum enumDropdownHandler;
	}
}
