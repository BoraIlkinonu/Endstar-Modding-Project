using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Unity.Netcode;

namespace Endless.Core
{
	// Token: 0x02000021 RID: 33
	public class SetActiveIfIsServer : BaseSetActiveIf
	{
		// Token: 0x06000074 RID: 116 RVA: 0x00004331 File Offset: 0x00002531
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			NetworkManager.Singleton.OnClientConnectedCallback += this.OnClientConnected;
		}

		// Token: 0x06000075 RID: 117 RVA: 0x00004361 File Offset: 0x00002561
		private void OnClientConnected(ulong clientId)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnClientConnected", new object[] { clientId });
			}
			base.SetActive(NetworkManager.Singleton.IsServer);
		}
	}
}
