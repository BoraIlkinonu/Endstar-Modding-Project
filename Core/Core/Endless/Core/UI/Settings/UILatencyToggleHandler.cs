using System;
using Endless.Data;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI.Settings
{
	// Token: 0x020000BD RID: 189
	public class UILatencyToggleHandler : MonoBehaviour
	{
		// Token: 0x06000451 RID: 1105 RVA: 0x000157D0 File Offset: 0x000139D0
		private void Start()
		{
			this.toggle.OnChange.AddListener(new UnityAction<bool>(DiagnosticSettings.SetLatencyVisible));
			bool latencyVisible = DiagnosticSettings.GetLatencyVisible();
			this.toggle.SetIsOn(latencyVisible, true, true);
		}

		// Token: 0x06000452 RID: 1106 RVA: 0x0001580D File Offset: 0x00013A0D
		private void OnDestroy()
		{
			this.toggle.OnChange.RemoveListener(new UnityAction<bool>(DiagnosticSettings.SetLatencyVisible));
		}

		// Token: 0x040002E4 RID: 740
		[SerializeField]
		private UIToggle toggle;
	}
}
