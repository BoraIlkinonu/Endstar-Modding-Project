using System;
using System.Threading;

namespace Endless.Shared
{
	// Token: 0x0200009D RID: 157
	public static class CancellationTokenSourceUtility
	{
		// Token: 0x06000472 RID: 1138 RVA: 0x00013316 File Offset: 0x00011516
		public static void RecreateTokenSource(ref CancellationTokenSource tokenSource)
		{
			CancellationTokenSource cancellationTokenSource = tokenSource;
			if (cancellationTokenSource != null)
			{
				cancellationTokenSource.Cancel();
			}
			CancellationTokenSource cancellationTokenSource2 = tokenSource;
			if (cancellationTokenSource2 != null)
			{
				cancellationTokenSource2.Dispose();
			}
			tokenSource = new CancellationTokenSource();
		}

		// Token: 0x06000473 RID: 1139 RVA: 0x0001333C File Offset: 0x0001153C
		public static void CancelAndCleanup(ref CancellationTokenSource tokenSource)
		{
			if (tokenSource == null)
			{
				return;
			}
			try
			{
				tokenSource.Cancel();
			}
			catch (ObjectDisposedException)
			{
			}
			tokenSource.Dispose();
			tokenSource = null;
		}
	}
}
