using System;
using System.Runtime.CompilerServices;
using MatchmakingClientSDK;

namespace DedicatedServerSDK
{
	// Token: 0x02000008 RID: 8
	[NullableContext(1)]
	[Nullable(0)]
	public class MatchmakingInterface
	{
		// Token: 0x0600001B RID: 27 RVA: 0x00002767 File Offset: 0x00000967
		public MatchmakingInterface(Action<string, bool> log, string stage = "STAGING", Func<float> time = null)
		{
			this.stage = stage;
			this.log = log;
			this.matchmakingWebsocketClient = new MatchmakingWebsocketClient(stage, log, time);
		}

		// Token: 0x0600001C RID: 28 RVA: 0x0000278D File Offset: 0x0000098D
		public void Update()
		{
			this.matchmakingWebsocketClient.Update();
		}

		// Token: 0x0400007F RID: 127
		private readonly MatchmakingWebsocketClient matchmakingWebsocketClient;

		// Token: 0x04000080 RID: 128
		private readonly Action<string, bool> log;

		// Token: 0x04000081 RID: 129
		private readonly string stage;
	}
}
