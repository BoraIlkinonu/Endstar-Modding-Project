using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x0200003F RID: 63
	[RequireComponent(typeof(UIButton))]
	public class UIOpenMatchButton : UIGameObject
	{
		// Token: 0x0600012E RID: 302 RVA: 0x00008250 File Offset: 0x00006450
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			UIButton uibutton;
			base.TryGetComponent<UIButton>(out uibutton);
			uibutton.onClick.AddListener(new UnityAction(this.OpenMatch));
		}

		// Token: 0x0600012F RID: 303 RVA: 0x00008295 File Offset: 0x00006495
		private void OpenMatch()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OpenMatch", Array.Empty<object>());
			}
		}

		// Token: 0x040000C7 RID: 199
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
