using System;
using Endless.Creator.UI;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.SoVariables;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Core.UI
{
	// Token: 0x02000084 RID: 132
	public abstract class UIBaseGameScreenView : UIBaseScreenView
	{
		// Token: 0x17000044 RID: 68
		// (get) Token: 0x060002A3 RID: 675 RVA: 0x0000EA56 File Offset: 0x0000CC56
		// (set) Token: 0x060002A4 RID: 676 RVA: 0x0000EA5E File Offset: 0x0000CC5E
		public UIUserRolesModel UserRolesModel { get; private set; }

		// Token: 0x060002A5 RID: 677 RVA: 0x0000EA68 File Offset: 0x0000CC68
		protected override void Start()
		{
			base.Start();
			if (!this.canEditGameName)
			{
				return;
			}
			this.GameNameInputField.onDeselect.AddListener(new UnityAction<string>(this.OnGameNameInputFieldDeselected));
			this.GameNameInputField.onSubmit.AddListener(new UnityAction<string>(this.OnGameNameInputFieldDeselected));
		}

		// Token: 0x060002A6 RID: 678 RVA: 0x0000EABC File Offset: 0x0000CCBC
		public override void OnDespawn()
		{
			base.OnDespawn();
			this.SetGameNameInputFieldActive(false);
			this.GameNameInputField.Clear(true);
			this.DescriptionInputField.Clear(true);
		}

		// Token: 0x060002A7 RID: 679 RVA: 0x0000EAE4 File Offset: 0x0000CCE4
		public virtual void SetGameNameInputFieldActive(bool state)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetGameNameInputFieldActive", "state", state), this);
			}
			this.GameNameText.gameObject.SetActive(!state);
			this.editGameNameButton.gameObject.SetActive(this.canEditGameName && !state);
			this.gameNameEditImage.gameObject.SetActive(this.canEditGameName && !state);
			this.GameNameInputField.gameObject.SetActive(state);
			if (!state)
			{
				return;
			}
			if (this.GameNameText.text != this.GameNameGuideText.Value)
			{
				this.GameNameInputField.text = this.GameNameText.text;
			}
			this.GameNameInputField.Select();
		}

		// Token: 0x060002A8 RID: 680 RVA: 0x0000EBC0 File Offset: 0x0000CDC0
		private void OnGameNameInputFieldDeselected(string text)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("OnGameNameInputFieldDeselected ( text: " + text + " )", this);
			}
			if (!this.GameNameInputField.text.Replace(" ", string.Empty).IsNullOrEmptyOrWhiteSpace())
			{
				this.GameNameText.text = this.GameNameInputField.text;
			}
			this.GameNameInputField.Clear(true);
			this.SetGameNameInputFieldActive(false);
		}

		// Token: 0x040001F6 RID: 502
		[SerializeField]
		protected StringVariable GameNameGuideText;

		// Token: 0x040001F7 RID: 503
		[SerializeField]
		protected TextMeshProUGUI GameNameText;

		// Token: 0x040001F8 RID: 504
		[SerializeField]
		protected UIInputField GameNameInputField;

		// Token: 0x040001F9 RID: 505
		[SerializeField]
		protected UIInputField DescriptionInputField;

		// Token: 0x040001FA RID: 506
		[SerializeField]
		protected TextMeshProUGUI ownerText;

		// Token: 0x040001FB RID: 507
		[SerializeField]
		private bool canEditGameName = true;

		// Token: 0x040001FC RID: 508
		[SerializeField]
		private UIButton editGameNameButton;

		// Token: 0x040001FD RID: 509
		[SerializeField]
		private Image gameNameEditImage;
	}
}
