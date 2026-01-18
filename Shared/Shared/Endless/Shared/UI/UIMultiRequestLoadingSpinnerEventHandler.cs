using System;
using System.Collections.Generic;
using Endless.Shared.Debugging;

namespace Endless.Shared.UI
{
	// Token: 0x020001C4 RID: 452
	public class UIMultiRequestLoadingSpinnerEventHandler<T> where T : Enum
	{
		// Token: 0x17000222 RID: 546
		// (get) Token: 0x06000B4B RID: 2891 RVA: 0x00030DC5 File Offset: 0x0002EFC5
		public bool Initialized
		{
			get
			{
				return this.loadingSpinnerViewCompatible != null;
			}
		}

		// Token: 0x06000B4C RID: 2892 RVA: 0x00030DD0 File Offset: 0x0002EFD0
		public void Initialize(IUILoadingSpinnerViewCompatible loadingSpinnerViewCompatible)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", new object[] { loadingSpinnerViewCompatible });
			}
			this.loadingSpinnerViewCompatible = loadingSpinnerViewCompatible;
		}

		// Token: 0x06000B4D RID: 2893 RVA: 0x00030DF8 File Offset: 0x0002EFF8
		public void TrackRequest(T item)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "TrackRequest", new object[] { item });
			}
			if (this.loadingSpinnerViewCompatible == null)
			{
				DebugUtility.LogException(new Exception("Uninitialized UIMultiRequestLoadingSpinnerEventHandler!"), null);
			}
			if (!this.requests.Add(item))
			{
				return;
			}
			if (this.requests.Count == 1)
			{
				IUILoadingSpinnerViewCompatible iuiloadingSpinnerViewCompatible = this.loadingSpinnerViewCompatible;
				if (iuiloadingSpinnerViewCompatible == null)
				{
					return;
				}
				iuiloadingSpinnerViewCompatible.OnLoadingStarted.Invoke();
			}
		}

		// Token: 0x06000B4E RID: 2894 RVA: 0x00030E74 File Offset: 0x0002F074
		public void UntrackRequest(T item)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "UntrackRequest", new object[] { item });
			}
			if (this.loadingSpinnerViewCompatible == null)
			{
				DebugUtility.LogException(new Exception("Uninitialized UIMultiRequestLoadingSpinnerEventHandler!"), null);
			}
			if (!this.requests.Remove(item))
			{
				return;
			}
			if (this.requests.Count == 0)
			{
				IUILoadingSpinnerViewCompatible iuiloadingSpinnerViewCompatible = this.loadingSpinnerViewCompatible;
				if (iuiloadingSpinnerViewCompatible == null)
				{
					return;
				}
				iuiloadingSpinnerViewCompatible.OnLoadingEnded.Invoke();
			}
		}

		// Token: 0x04000737 RID: 1847
		private readonly HashSet<T> requests = new HashSet<T>();

		// Token: 0x04000738 RID: 1848
		private IUILoadingSpinnerViewCompatible loadingSpinnerViewCompatible;

		// Token: 0x04000739 RID: 1849
		private bool verboseLogging;
	}
}
