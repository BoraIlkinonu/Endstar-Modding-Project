using System;
using Endless.Shared.Debugging;
using Endless.Shared.SoVariables;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x02000085 RID: 133
	public class UIInstallationErrorScreenController : UIGameObject
	{
		// Token: 0x060002AA RID: 682 RVA: 0x0000EC44 File Offset: 0x0000CE44
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.viewTroubleshootingInstallationButton.onClick.AddListener(new UnityAction(this.ViewTroubleshootingInstallation));
			this.quitButton.onClick.AddListener(new UnityAction(Application.Quit));
		}

		// Token: 0x060002AB RID: 683 RVA: 0x0000ECA1 File Offset: 0x0000CEA1
		private void ViewTroubleshootingInstallation()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ViewTroubleshootingInstallation", Array.Empty<object>());
			}
			Application.OpenURL(this.troubleshootingInstallationURL.Value);
		}

		// Token: 0x040001FE RID: 510
		[SerializeField]
		private StringVariable troubleshootingInstallationURL;

		// Token: 0x040001FF RID: 511
		[SerializeField]
		private UIButton viewTroubleshootingInstallationButton;

		// Token: 0x04000200 RID: 512
		[SerializeField]
		private UIButton quitButton;

		// Token: 0x04000201 RID: 513
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
