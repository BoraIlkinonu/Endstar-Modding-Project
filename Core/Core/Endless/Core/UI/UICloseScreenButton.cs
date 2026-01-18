using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x02000090 RID: 144
	[RequireComponent(typeof(UIButton))]
	public class UICloseScreenButton : UIGameObject
	{
		// Token: 0x060002EC RID: 748 RVA: 0x0000FD74 File Offset: 0x0000DF74
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			UIButton uibutton;
			base.TryGetComponent<UIButton>(out uibutton);
			uibutton.onClick.AddListener(new UnityAction(this.CloseScreen));
		}

		// Token: 0x060002ED RID: 749 RVA: 0x0000FDB9 File Offset: 0x0000DFB9
		private void CloseScreen()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CloseScreen", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIScreenManager>.Instance.Close(this.closeStackAction, null, true, false);
		}

		// Token: 0x04000229 RID: 553
		[SerializeField]
		private UIScreenManager.CloseStackActions closeStackAction;

		// Token: 0x0400022A RID: 554
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
