using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x020001CC RID: 460
	[RequireComponent(typeof(UIButton))]
	public class UICloseModalButton : UIGameObject
	{
		// Token: 0x06000B77 RID: 2935 RVA: 0x000315D4 File Offset: 0x0002F7D4
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			UIButton uibutton;
			base.TryGetComponent<UIButton>(out uibutton);
			uibutton.onClick.AddListener(new UnityAction(this.Close));
		}

		// Token: 0x06000B78 RID: 2936 RVA: 0x0003161C File Offset: 0x0002F81C
		private void Close()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Close", Array.Empty<object>());
			}
			UIBaseModalView spawnedModal = MonoBehaviourSingleton<UIModalManager>.Instance.SpawnedModal;
			if (this.clearStack)
			{
				MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
			}
			else
			{
				MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack();
			}
			Action<UIBaseModalView> onModalClosedByUser = UIModalManager.OnModalClosedByUser;
			if (onModalClosedByUser == null)
			{
				return;
			}
			onModalClosedByUser(spawnedModal);
		}

		// Token: 0x04000756 RID: 1878
		[SerializeField]
		private bool clearStack = true;

		// Token: 0x04000757 RID: 1879
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
