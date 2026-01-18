using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x02000083 RID: 131
	public abstract class UIBaseGameScreenController : UIGameObject
	{
		// Token: 0x17000043 RID: 67
		// (get) Token: 0x0600029E RID: 670 RVA: 0x0000E9D7 File Offset: 0x0000CBD7
		// (set) Token: 0x0600029F RID: 671 RVA: 0x0000E9DF File Offset: 0x0000CBDF
		protected bool VerboseLogging { get; set; }

		// Token: 0x060002A0 RID: 672 RVA: 0x0000E9E8 File Offset: 0x0000CBE8
		protected virtual void Start()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Start", this);
			}
			this.editGameNameButton.onClick.AddListener(new UnityAction(this.ToggleGameNameInputField));
			base.TryGetComponent<UIBaseGameScreenView>(out this.baseView);
		}

		// Token: 0x060002A1 RID: 673 RVA: 0x0000EA26 File Offset: 0x0000CC26
		private void ToggleGameNameInputField()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("ToggleGameNameInputField", this);
			}
			this.baseView.SetGameNameInputFieldActive(this.editGameNameButton.gameObject.activeSelf);
		}

		// Token: 0x040001F0 RID: 496
		[Header("UIBaseGameScreenController")]
		[SerializeField]
		protected TextMeshProUGUI GameNameText;

		// Token: 0x040001F1 RID: 497
		[SerializeField]
		protected UIInputField DescriptionInputField;

		// Token: 0x040001F2 RID: 498
		[SerializeField]
		private UIButton editGameNameButton;

		// Token: 0x040001F4 RID: 500
		private UIBaseGameScreenView baseView;
	}
}
