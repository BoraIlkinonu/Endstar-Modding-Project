using System;
using System.Collections.Generic;
using Endless.Creator.UI;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x02000082 RID: 130
	public class UINewGameScreenView : UIBaseGameScreenView
	{
		// Token: 0x06000295 RID: 661 RVA: 0x0000E7EE File Offset: 0x0000C9EE
		protected override void Start()
		{
			base.Start();
			this.GameNameInputField.onSubmit.AddListener(new UnityAction<string>(this.SetGameNameText));
			this.GameNameInputField.onEndEdit.AddListener(new UnityAction<string>(this.SetGameNameText));
		}

		// Token: 0x06000296 RID: 662 RVA: 0x0000E82E File Offset: 0x0000CA2E
		public static UINewGameScreenView Display(UIScreenManager.DisplayStackActions displayStackAction)
		{
			return (UINewGameScreenView)MonoBehaviourSingleton<UIScreenManager>.Instance.Display<UINewGameScreenView>(displayStackAction, null);
		}

		// Token: 0x06000297 RID: 663 RVA: 0x0000E844 File Offset: 0x0000CA44
		public override void Initialize(Dictionary<string, object> supplementalData)
		{
			base.Initialize(supplementalData);
			this.ownerText.text = EndlessServices.Instance.CloudService.ActiveUser.UserName;
			this.GameNameText.text = this.GameNameGuideText.Value;
			base.UserRolesModel.Initialize(SerializableGuid.Empty, string.Empty, SerializableGuid.Empty, AssetContexts.NewGame);
			this.SetGameNameInputFieldActive(true);
			this.GameNameInputField.Select();
			this.createButton.interactable = false;
		}

		// Token: 0x06000298 RID: 664 RVA: 0x0000D92B File Offset: 0x0000BB2B
		public override void OnBack()
		{
			base.OnBack();
			UIMainMenuScreenView.Display(UIScreenManager.DisplayStackActions.ClearAndPush);
		}

		// Token: 0x06000299 RID: 665 RVA: 0x0000E8C6 File Offset: 0x0000CAC6
		public override void OnDespawn()
		{
			base.OnDespawn();
			this.createButton.interactable = false;
		}

		// Token: 0x0600029A RID: 666 RVA: 0x0000E8DA File Offset: 0x0000CADA
		public override void SetGameNameInputFieldActive(bool state)
		{
			base.SetGameNameInputFieldActive(state);
			this.HandleCreateButtonInteractableState();
		}

		// Token: 0x0600029B RID: 667 RVA: 0x0000E8EC File Offset: 0x0000CAEC
		private void HandleCreateButtonInteractableState()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "HandleCreateButtonInteractableState", Array.Empty<object>());
			}
			this.createButton.interactable = false;
			if (this.GameNameText.text == this.GameNameGuideText.Value)
			{
				return;
			}
			this.createButton.interactable = true;
		}

		// Token: 0x0600029C RID: 668 RVA: 0x0000E948 File Offset: 0x0000CB48
		private void SetGameNameText(string text)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetGameNameText", new object[] { text });
			}
			if (text.Replace(" ", string.Empty).IsNullOrEmptyOrWhiteSpace())
			{
				text = this.GameNameGuideText.Value;
			}
			this.GameNameText.text = text;
			this.SetGameNameInputFieldActive(false);
			base.UserRolesModel.SetAssetName((text == this.GameNameGuideText.Value) ? string.Empty : text);
		}

		// Token: 0x040001EF RID: 495
		[Header("UINewGameScreenView")]
		[SerializeField]
		private UIButton createButton;
	}
}
