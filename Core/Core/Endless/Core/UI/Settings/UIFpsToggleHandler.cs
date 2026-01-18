using System;
using Endless.Data;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI.Settings
{
	// Token: 0x020000BC RID: 188
	public class UIFpsToggleHandler : MonoBehaviour
	{
		// Token: 0x0600044E RID: 1102 RVA: 0x00015774 File Offset: 0x00013974
		private void Start()
		{
			this.toggle.OnChange.AddListener(new UnityAction<bool>(DiagnosticSettings.SetFpsVisible));
			bool fpsVisible = DiagnosticSettings.GetFpsVisible();
			this.toggle.SetIsOn(fpsVisible, true, true);
		}

		// Token: 0x0600044F RID: 1103 RVA: 0x000157B1 File Offset: 0x000139B1
		private void OnDestroy()
		{
			this.toggle.OnChange.RemoveListener(new UnityAction<bool>(DiagnosticSettings.SetFpsVisible));
		}

		// Token: 0x040002E3 RID: 739
		[SerializeField]
		private UIToggle toggle;
	}
}
