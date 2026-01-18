using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace MatchmakingClientSDK.Diagnostics
{
	// Token: 0x02000070 RID: 112
	public class NetworkUsageMonitor : IDisposable
	{
		// Token: 0x1400003E RID: 62
		// (add) Token: 0x06000428 RID: 1064 RVA: 0x00012824 File Offset: 0x00010A24
		// (remove) Token: 0x06000429 RID: 1065 RVA: 0x0001285C File Offset: 0x00010A5C
		public event Action<TimeSpan, long, long> OnNetworkUsageUpdate;

		// Token: 0x17000074 RID: 116
		// (get) Token: 0x0600042A RID: 1066 RVA: 0x00012891 File Offset: 0x00010A91
		// (set) Token: 0x0600042B RID: 1067 RVA: 0x00012899 File Offset: 0x00010A99
		public TimeSpan TotalTime { get; private set; }

		// Token: 0x17000075 RID: 117
		// (get) Token: 0x0600042C RID: 1068 RVA: 0x000128A2 File Offset: 0x00010AA2
		// (set) Token: 0x0600042D RID: 1069 RVA: 0x000128AA File Offset: 0x00010AAA
		public long ReceivedBytes { get; private set; }

		// Token: 0x17000076 RID: 118
		// (get) Token: 0x0600042E RID: 1070 RVA: 0x000128B3 File Offset: 0x00010AB3
		// (set) Token: 0x0600042F RID: 1071 RVA: 0x000128BB File Offset: 0x00010ABB
		public long SentBytes { get; private set; }

		// Token: 0x06000430 RID: 1072 RVA: 0x000128C4 File Offset: 0x00010AC4
		public NetworkUsageMonitor(int monitorIntervalSeconds)
		{
			this.MonitorIntervalSeconds = monitorIntervalSeconds;
			this.networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
			for (int i = 0; i < this.networkInterfaces.Length; i++)
			{
				this.initialBytesSent += this.networkInterfaces[i].GetIPStatistics().BytesSent;
				this.initialBytesReceived += this.networkInterfaces[i].GetIPStatistics().BytesReceived;
			}
			this.NetworkMonitor();
		}

		// Token: 0x06000431 RID: 1073 RVA: 0x00012944 File Offset: 0x00010B44
		private async Task NetworkMonitor()
		{
			for (;;)
			{
				await Task.Delay(this.MonitorIntervalSeconds * 1000);
				if (this.disposed)
				{
					break;
				}
				long num = 0L;
				for (int i = 0; i < this.networkInterfaces.Length; i++)
				{
					num += this.networkInterfaces[i].GetIPStatistics().BytesSent;
				}
				long num2 = 0L;
				for (int j = 0; j < this.networkInterfaces.Length; j++)
				{
					num2 += this.networkInterfaces[j].GetIPStatistics().BytesReceived;
				}
				this.TotalTime += TimeSpan.FromSeconds((double)this.MonitorIntervalSeconds);
				this.ReceivedBytes = num2 - this.initialBytesReceived;
				this.SentBytes = num - this.initialBytesSent;
				Action<TimeSpan, long, long> onNetworkUsageUpdate = this.OnNetworkUsageUpdate;
				if (onNetworkUsageUpdate != null)
				{
					onNetworkUsageUpdate(this.TotalTime, this.SentBytes, this.ReceivedBytes);
				}
			}
		}

		// Token: 0x06000432 RID: 1074 RVA: 0x00012987 File Offset: 0x00010B87
		public void Dispose()
		{
			this.disposed = true;
		}

		// Token: 0x040002AB RID: 683
		public readonly int MonitorIntervalSeconds;

		// Token: 0x040002B0 RID: 688
		private bool disposed;

		// Token: 0x040002B1 RID: 689
		private readonly long initialBytesSent;

		// Token: 0x040002B2 RID: 690
		private readonly long initialBytesReceived;

		// Token: 0x040002B3 RID: 691
		private NetworkInterface[] networkInterfaces;
	}
}
