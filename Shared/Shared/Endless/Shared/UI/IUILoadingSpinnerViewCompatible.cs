using System;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x020001C2 RID: 450
	public interface IUILoadingSpinnerViewCompatible
	{
		// Token: 0x17000220 RID: 544
		// (get) Token: 0x06000B43 RID: 2883
		UnityEvent OnLoadingStarted { get; }

		// Token: 0x17000221 RID: 545
		// (get) Token: 0x06000B44 RID: 2884
		UnityEvent OnLoadingEnded { get; }
	}
}
