using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Unity.Netcode;

namespace Endless.Core
{
	// Token: 0x02000020 RID: 32
	public class SetActiveIfIsHost : BaseSetActiveIf
	{
		// Token: 0x06000071 RID: 113 RVA: 0x00004287 File Offset: 0x00002487
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			NetworkManager.Singleton.OnClientConnectedCallback += this.OnClientConnected;
		}

		// Token: 0x06000072 RID: 114 RVA: 0x000042B8 File Offset: 0x000024B8
		private void OnClientConnected(ulong clientId)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethodWithAppension(this, "OnClientConnected", string.Format("{0}.{1}.{2}: {3}", new object[]
				{
					"NetworkManager",
					"Singleton",
					"IsHost",
					NetworkManager.Singleton.IsHost
				}), new object[] { clientId });
			}
			base.SetActive(NetworkManager.Singleton.IsHost);
		}
	}
}
