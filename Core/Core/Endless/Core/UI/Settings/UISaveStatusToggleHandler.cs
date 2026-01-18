using System;
using Endless.Creator.UI;
using Endless.Shared;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI.Settings
{
	// Token: 0x020000BE RID: 190
	public class UISaveStatusToggleHandler : MonoBehaviour
	{
		// Token: 0x06000454 RID: 1108 RVA: 0x0001582C File Offset: 0x00013A2C
		private void Start()
		{
			this.toggle.OnChange.AddListener(new UnityAction<bool>(this.OnToggleChanged));
			bool flag = PlayerPrefs.GetInt("Save Status Visible", 0) == 1;
			this.toggle.SetIsOn(flag, true, true);
		}

		// Token: 0x06000455 RID: 1109 RVA: 0x00015872 File Offset: 0x00013A72
		private void OnDestroy()
		{
			this.toggle.OnChange.RemoveListener(new UnityAction<bool>(this.OnToggleChanged));
		}

		// Token: 0x06000456 RID: 1110 RVA: 0x00015890 File Offset: 0x00013A90
		private void OnToggleChanged(bool isVisible)
		{
			PlayerPrefs.SetInt("Save Status Visible", isVisible ? 1 : 0);
			if (NetworkBehaviourSingleton<UISaveStatusManager>.Instance != null)
			{
				NetworkBehaviourSingleton<UISaveStatusManager>.Instance.SetCanvasVisibility(isVisible);
			}
		}

		// Token: 0x040002E5 RID: 741
		[SerializeField]
		private UIToggle toggle;
	}
}
