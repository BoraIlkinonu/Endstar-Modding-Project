using System;
using Endless.Matchmaking;
using Endless.Networking;
using UnityEngine;

namespace Runtime.Shared.Matchmaking
{
	// Token: 0x0200000F RID: 15
	public class EndlessServices : MonoBehaviour
	{
		// Token: 0x17000010 RID: 16
		// (get) Token: 0x06000075 RID: 117 RVA: 0x00004F5F File Offset: 0x0000315F
		// (set) Token: 0x06000076 RID: 118 RVA: 0x00004F66 File Offset: 0x00003166
		public static EndlessServices Instance { get; private set; }

		// Token: 0x06000077 RID: 119 RVA: 0x00004F6E File Offset: 0x0000316E
		public static void New(Transform parent)
		{
			if (EndlessServices.Instance != null)
			{
				global::UnityEngine.Object.Destroy(EndlessServices.Instance.gameObject);
			}
			new GameObject("Endless Services").AddComponent<EndlessServices>().transform.SetParent(parent);
		}

		// Token: 0x06000078 RID: 120 RVA: 0x00004FA6 File Offset: 0x000031A6
		public static void Remove()
		{
			if (EndlessServices.Instance == null)
			{
				return;
			}
			global::UnityEngine.Object.Destroy(EndlessServices.Instance.gameObject);
		}

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x06000079 RID: 121 RVA: 0x00004FC5 File Offset: 0x000031C5
		// (set) Token: 0x0600007A RID: 122 RVA: 0x00004FCD File Offset: 0x000031CD
		public EndlessCloudService CloudService { get; private set; }

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x0600007B RID: 123 RVA: 0x00004FD6 File Offset: 0x000031D6
		// (set) Token: 0x0600007C RID: 124 RVA: 0x00004FDE File Offset: 0x000031DE
		public bool Initialized { get; private set; }

		// Token: 0x0600007D RID: 125 RVA: 0x00004FE7 File Offset: 0x000031E7
		private void Awake()
		{
			if (EndlessServices.Instance != null)
			{
				throw new Exception("Duplicate EndlessServices object detected.");
			}
			EndlessServices.Instance = this;
		}

		// Token: 0x0600007E RID: 126 RVA: 0x00005007 File Offset: 0x00003207
		public void Initialize(string authToken, TargetPlatforms authPlatform)
		{
			if (this.Initialized)
			{
				Debug.LogException(new Exception("Endless Services already initialized."));
				return;
			}
			this.CloudService = new EndlessCloudService(authToken, true);
			this.Initialized = true;
		}

		// Token: 0x0600007F RID: 127 RVA: 0x00005035 File Offset: 0x00003235
		private void OnDestroy()
		{
			this.CloudService.Dispose();
			EndlessServices.Instance = null;
		}
	}
}
