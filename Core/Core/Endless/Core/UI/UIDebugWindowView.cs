using System;
using System.Collections.Generic;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using UnityEngine;

namespace Endless.Core.UI
{
	// Token: 0x020000A3 RID: 163
	public class UIDebugWindowView : UIBaseWindowView
	{
		// Token: 0x0600037B RID: 891 RVA: 0x0001256E File Offset: 0x0001076E
		private void OnEnable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			UILogsListCellController.SelectAction = (Action<UILog>)Delegate.Combine(UILogsListCellController.SelectAction, new Action<UILog>(this.ViewLogInspector));
		}

		// Token: 0x0600037C RID: 892 RVA: 0x000125A8 File Offset: 0x000107A8
		protected override void Start()
		{
			base.Start();
			this.saveLevelToDiskButton.gameObject.SetActive(Application.isEditor);
		}

		// Token: 0x0600037D RID: 893 RVA: 0x000125C5 File Offset: 0x000107C5
		private void OnDisable()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisable", Array.Empty<object>());
			}
			UILogsListCellController.SelectAction = (Action<UILog>)Delegate.Remove(UILogsListCellController.SelectAction, new Action<UILog>(this.ViewLogInspector));
		}

		// Token: 0x0600037E RID: 894 RVA: 0x000125FF File Offset: 0x000107FF
		public static UIDebugWindowView Display(Transform parent = null)
		{
			return (UIDebugWindowView)MonoBehaviourSingleton<UIWindowManager>.Instance.Display<UIDebugWindowView>(parent, null);
		}

		// Token: 0x0600037F RID: 895 RVA: 0x00012614 File Offset: 0x00010814
		public override void Initialize(Dictionary<string, object> supplementalData)
		{
			base.Initialize(supplementalData);
			this.usernameInputField.text = "Loading...";
			this.usernameInputField.text = EndlessServices.Instance.CloudService.ActiveUserName;
			this.developerConsoleToggle.SetIsOn(Debug.developerConsoleEnabled, false, true);
		}

		// Token: 0x06000380 RID: 896 RVA: 0x00012664 File Offset: 0x00010864
		private void ViewLogInspector(UILog log)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ViewLogInspector", new object[] { log });
			}
		}

		// Token: 0x0400028A RID: 650
		[Header("UIDebugWindowView")]
		[SerializeField]
		private UIInputField usernameInputField;

		// Token: 0x0400028B RID: 651
		[SerializeField]
		private UIButton saveLevelToDiskButton;

		// Token: 0x0400028C RID: 652
		[SerializeField]
		private UIToggle developerConsoleToggle;
	}
}
