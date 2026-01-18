using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020001C6 RID: 454
	public abstract class UIBasePropCreationModalController : UIGameObject, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x170000BD RID: 189
		// (get) Token: 0x060006C1 RID: 1729 RVA: 0x000228DE File Offset: 0x00020ADE
		// (set) Token: 0x060006C2 RID: 1730 RVA: 0x000228E6 File Offset: 0x00020AE6
		protected bool VerboseLogging { get; set; }

		// Token: 0x170000BE RID: 190
		// (get) Token: 0x060006C3 RID: 1731 RVA: 0x000228EF File Offset: 0x00020AEF
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x170000BF RID: 191
		// (get) Token: 0x060006C4 RID: 1732 RVA: 0x000228F7 File Offset: 0x00020AF7
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x170000C0 RID: 192
		// (get) Token: 0x060006C5 RID: 1733 RVA: 0x000228FF File Offset: 0x00020AFF
		protected string Name
		{
			get
			{
				return this.nameInputField.text;
			}
		}

		// Token: 0x170000C1 RID: 193
		// (get) Token: 0x060006C6 RID: 1734 RVA: 0x0002290C File Offset: 0x00020B0C
		protected string Description
		{
			get
			{
				return this.descriptionInputField.text;
			}
		}

		// Token: 0x170000C2 RID: 194
		// (get) Token: 0x060006C7 RID: 1735 RVA: 0x00022919 File Offset: 0x00020B19
		protected bool GrantEditRightsToCollaborators
		{
			get
			{
				return this.grantEditRightsToCollaboratorsToggle.IsOn;
			}
		}

		// Token: 0x060006C8 RID: 1736 RVA: 0x00022928 File Offset: 0x00020B28
		protected virtual void Start()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Start", this);
			}
			this.createButton.onClick.AddListener(new UnityAction(this.Create));
			this.propTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<PropTool>();
		}

		// Token: 0x060006C9 RID: 1737 RVA: 0x00022975 File Offset: 0x00020B75
		protected bool ValidatePropCreation()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("ValidatePropCreation", this);
			}
			return !this.nameInputField.IsNullOrEmptyOrWhiteSpace(true);
		}

		// Token: 0x060006CA RID: 1738
		protected abstract void Create();

		// Token: 0x04000610 RID: 1552
		[Header("UIBasePropCreationModalController")]
		[SerializeField]
		protected UIBasePropCreationModalView view;

		// Token: 0x04000611 RID: 1553
		[SerializeField]
		private UIInputField nameInputField;

		// Token: 0x04000612 RID: 1554
		[SerializeField]
		private UIInputField descriptionInputField;

		// Token: 0x04000613 RID: 1555
		[SerializeField]
		private UIToggle grantEditRightsToCollaboratorsToggle;

		// Token: 0x04000614 RID: 1556
		[SerializeField]
		private UIButton createButton;

		// Token: 0x04000616 RID: 1558
		protected PropTool propTool;
	}
}
