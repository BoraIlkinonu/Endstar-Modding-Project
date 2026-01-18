using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Unity.Netcode;

namespace Endless.Core
{
	// Token: 0x0200001E RID: 30
	public class SetActiveIfIsClient : BaseSetActiveIf
	{
		// Token: 0x0600006A RID: 106 RVA: 0x00004145 File Offset: 0x00002345
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			NetworkManager.Singleton.OnClientConnectedCallback += this.OnClientConnected;
		}

		// Token: 0x0600006B RID: 107 RVA: 0x00004175 File Offset: 0x00002375
		private void OnClientConnected(ulong clientId)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnClientConnected", new object[] { clientId });
			}
			base.SetActive(NetworkManager.Singleton.IsClient);
		}
	}
}
