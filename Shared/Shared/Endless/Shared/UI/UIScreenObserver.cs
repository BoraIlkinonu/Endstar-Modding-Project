using System;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000250 RID: 592
	public class UIScreenObserver : MonoBehaviour
	{
		// Token: 0x06000F01 RID: 3841 RVA: 0x000409A0 File Offset: 0x0003EBA0
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.lastFullScreenMode = Screen.fullScreenMode;
			this.lastWidth = Screen.width;
			this.lastHeight = Screen.height;
			base.enabled = !MobileUtility.IsMobile;
		}

		// Token: 0x06000F02 RID: 3842 RVA: 0x000409F4 File Offset: 0x0003EBF4
		private void Update()
		{
			if (this.lastWidth != Screen.width || this.lastHeight != Screen.height)
			{
				if (this.verboseLogging)
				{
					Debug.Log("Screen size changed!", this);
				}
				this.lastWidth = Screen.width;
				this.lastHeight = Screen.height;
				Action onSizeChange = UIScreenObserver.OnSizeChange;
				if (onSizeChange != null)
				{
					onSizeChange();
				}
			}
			if (this.lastFullScreenMode != Screen.fullScreenMode)
			{
				if (this.verboseLogging)
				{
					Debug.Log("Screen mode changed!", this);
				}
				this.lastFullScreenMode = Screen.fullScreenMode;
				Action onFullScreenModeChange = UIScreenObserver.OnFullScreenModeChange;
				if (onFullScreenModeChange == null)
				{
					return;
				}
				onFullScreenModeChange();
			}
		}

		// Token: 0x04000978 RID: 2424
		public static Action OnSizeChange;

		// Token: 0x04000979 RID: 2425
		public static Action OnFullScreenModeChange;

		// Token: 0x0400097A RID: 2426
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x0400097B RID: 2427
		private FullScreenMode lastFullScreenMode;

		// Token: 0x0400097C RID: 2428
		private int lastWidth;

		// Token: 0x0400097D RID: 2429
		private int lastHeight;
	}
}
