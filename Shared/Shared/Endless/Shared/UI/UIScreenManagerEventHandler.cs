using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000256 RID: 598
	public class UIScreenManagerEventHandler : MonoBehaviour
	{
		// Token: 0x06000F20 RID: 3872 RVA: 0x00041398 File Offset: 0x0003F598
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			UIScreenManager.OnScreenSystemOpen = (Action)Delegate.Combine(UIScreenManager.OnScreenSystemOpen, new Action(this.OnDisplayingScreen));
			UIScreenManager.OnScreenSystemClose = (Action)Delegate.Combine(UIScreenManager.OnScreenSystemClose, new Action(this.OnClosingScreen));
		}

		// Token: 0x06000F21 RID: 3873 RVA: 0x000413FD File Offset: 0x0003F5FD
		private void OnDisplayingScreen()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisplayingScreen", Array.Empty<object>());
			}
			this.OnDisplay.Invoke();
		}

		// Token: 0x06000F22 RID: 3874 RVA: 0x00041422 File Offset: 0x0003F622
		private void OnClosingScreen()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnClosingScreen", Array.Empty<object>());
			}
			this.OnClose.Invoke();
		}

		// Token: 0x0400099A RID: 2458
		public UnityEvent OnDisplay = new UnityEvent();

		// Token: 0x0400099B RID: 2459
		public UnityEvent OnClose = new UnityEvent();

		// Token: 0x0400099C RID: 2460
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
