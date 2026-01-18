using System;
using Amazon.DynamoDBv2.DocumentModel;

namespace MatchmakingClientSDK.Diagnostics
{
	// Token: 0x0200006E RID: 110
	public class DiagnosticsWebsocketClient : IDiagnosticsClientModule, IDisposable
	{
		// Token: 0x06000422 RID: 1058 RVA: 0x000127E3 File Offset: 0x000109E3
		public DiagnosticsWebsocketClient(string stage, Action<string, bool> log, Func<float> time)
		{
		}

		// Token: 0x06000423 RID: 1059 RVA: 0x000127EB File Offset: 0x000109EB
		private void OnConnected()
		{
		}

		// Token: 0x06000424 RID: 1060 RVA: 0x000127ED File Offset: 0x000109ED
		private void OnConnectionFailed()
		{
		}

		// Token: 0x06000425 RID: 1061 RVA: 0x000127EF File Offset: 0x000109EF
		private void OnDisconnected()
		{
		}

		// Token: 0x06000426 RID: 1062 RVA: 0x000127F1 File Offset: 0x000109F1
		private void OnAuthentication(Document document)
		{
		}

		// Token: 0x06000427 RID: 1063 RVA: 0x000127F4 File Offset: 0x000109F4
		public void Dispose()
		{
			try
			{
				this.websocketClient.Disconnect();
			}
			catch
			{
			}
		}

		// Token: 0x040002AA RID: 682
		private readonly MatchmakingWebsocketClient websocketClient;
	}
}
