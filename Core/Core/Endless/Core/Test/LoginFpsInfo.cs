using System;
using Endless.Matchmaking;
using Endless.Networking;

namespace Endless.Core.Test
{
	// Token: 0x020000E0 RID: 224
	public class LoginFpsInfo : BaseFpsInfo
	{
		// Token: 0x170000A0 RID: 160
		// (get) Token: 0x0600050F RID: 1295 RVA: 0x000185A5 File Offset: 0x000167A5
		public override bool IsDone
		{
			get
			{
				return this.isDone;
			}
		}

		// Token: 0x06000510 RID: 1296 RVA: 0x000185AD File Offset: 0x000167AD
		public override void StartTest()
		{
			this.isDone = false;
			MatchmakingClientController.OnAuthenticationProcessSuccessful = (Action<ClientData>)Delegate.Combine(MatchmakingClientController.OnAuthenticationProcessSuccessful, new Action<ClientData>(this.HandleConnected));
		}

		// Token: 0x06000511 RID: 1297 RVA: 0x000185D6 File Offset: 0x000167D6
		private void HandleConnected(ClientData obj)
		{
			this.isDone = true;
		}

		// Token: 0x06000512 RID: 1298 RVA: 0x000185DF File Offset: 0x000167DF
		public override void StopTest()
		{
			MatchmakingClientController.OnAuthenticationProcessSuccessful = (Action<ClientData>)Delegate.Remove(MatchmakingClientController.OnAuthenticationProcessSuccessful, new Action<ClientData>(this.HandleConnected));
		}

		// Token: 0x06000513 RID: 1299 RVA: 0x0000229D File Offset: 0x0000049D
		protected override void ProcessFrame_Internal()
		{
		}

		// Token: 0x0400036E RID: 878
		private bool isDone;
	}
}
