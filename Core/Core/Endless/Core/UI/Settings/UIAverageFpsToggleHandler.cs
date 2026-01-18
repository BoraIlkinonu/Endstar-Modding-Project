using System;
using Endless.Data;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI.Settings
{
	// Token: 0x020000BB RID: 187
	public class UIAverageFpsToggleHandler : MonoBehaviour
	{
		// Token: 0x0600044B RID: 1099 RVA: 0x00015718 File Offset: 0x00013918
		private void Start()
		{
			this.toggle.OnChange.AddListener(new UnityAction<bool>(DiagnosticSettings.SetAverageFpsVisible));
			bool averageFpsVisible = DiagnosticSettings.GetAverageFpsVisible();
			this.toggle.SetIsOn(averageFpsVisible, true, true);
		}

		// Token: 0x0600044C RID: 1100 RVA: 0x00015755 File Offset: 0x00013955
		private void OnDestroy()
		{
			this.toggle.OnChange.RemoveListener(new UnityAction<bool>(DiagnosticSettings.SetAverageFpsVisible));
		}

		// Token: 0x040002E2 RID: 738
		[SerializeField]
		private UIToggle toggle;
	}
}
