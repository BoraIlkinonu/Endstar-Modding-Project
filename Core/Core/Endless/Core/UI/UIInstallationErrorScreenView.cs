using System;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Core.UI
{
	// Token: 0x02000087 RID: 135
	public class UIInstallationErrorScreenView : UIBaseScreenView
	{
		// Token: 0x060002AE RID: 686 RVA: 0x0000ECD4 File Offset: 0x0000CED4
		public static UIInstallationErrorScreenView Display(UIInstallationErrorScreenModel model, UIScreenManager.DisplayStackActions displayStackAction)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object> { { "model", model } };
			return (UIInstallationErrorScreenView)MonoBehaviourSingleton<UIScreenManager>.Instance.Display<UIInstallationErrorScreenView>(displayStackAction, dictionary);
		}

		// Token: 0x060002AF RID: 687 RVA: 0x0000ED0C File Offset: 0x0000CF0C
		public override void Initialize(Dictionary<string, object> supplementalData)
		{
			base.Initialize(supplementalData);
			UIInstallationErrorScreenModel uiinstallationErrorScreenModel = (UIInstallationErrorScreenModel)supplementalData["model"];
			TMP_Text tmp_Text = this.errorInfoText;
			string text = "Error Code: {0}\n{1}";
			object obj = (int)uiinstallationErrorScreenModel.ErrorCode;
			UserFacingTextAttribute attributeOfType = uiinstallationErrorScreenModel.ErrorCode.GetAttributeOfType<UserFacingTextAttribute>();
			tmp_Text.text = string.Format(text, obj, ((attributeOfType != null) ? attributeOfType.UserFacingText : null) ?? "Unknown Error");
		}

		// Token: 0x04000203 RID: 515
		[Header("UIInstallationErrorScreenView")]
		[SerializeField]
		private TextMeshProUGUI errorInfoText;
	}
}
