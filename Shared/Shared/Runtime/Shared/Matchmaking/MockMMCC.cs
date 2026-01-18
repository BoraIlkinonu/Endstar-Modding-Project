using System;
using Endless;
using Endless.GraphQl;
using MatchmakingClientSDK;
using UnityEngine;

namespace Runtime.Shared.Matchmaking
{
	// Token: 0x02000019 RID: 25
	public class MockMMCC : MonoBehaviour
	{
		// Token: 0x060000BE RID: 190 RVA: 0x0000588B File Offset: 0x00003A8B
		private void Awake()
		{
			UnityServicesManager unityServicesManager = new UnityServicesManager();
			unityServicesManager.OnServicesInitialized += this.Startup;
			unityServicesManager.Initialize(this.networkEnv == NetworkEnvironment.PROD);
		}

		// Token: 0x060000BF RID: 191 RVA: 0x000058B4 File Offset: 0x00003AB4
		private void Startup()
		{
			GraphQlRequest.Initialize((this.networkEnv == NetworkEnvironment.DEV) ? "https://endstar-api-dev.endlessstudios.com/graphql" : ((this.networkEnv == NetworkEnvironment.STAGING) ? "https://endstar-api-stage.endlessstudios.com/graphql" : "https://endstar-api.endlessstudios.com/graphql"));
			this.clients = new TestClientController[this.clientCount];
			for (int i = 0; i < this.clientCount; i++)
			{
				this.clients[i] = new TestClientController(this.networkEnv);
				if (i < this.userCreds.Length)
				{
					this.clients[i].UserName = this.userCreds[i].userName;
					this.clients[i].Password = this.userCreds[i].password;
					this.clients[i].Connect();
				}
			}
		}

		// Token: 0x060000C0 RID: 192 RVA: 0x00005974 File Offset: 0x00003B74
		private void OnGUI()
		{
			if (this.clients == null || this.clients.Length == 0)
			{
				return;
			}
			int num = Mathf.CeilToInt(Mathf.Sqrt((float)this.clientCount));
			int num2 = Mathf.CeilToInt((float)num);
			int num3 = 0;
			for (int i = 0; i < num2; i++)
			{
				int num4 = 0;
				while (num4 < num && num3 < this.clientCount)
				{
					GUILayout.BeginArea(new Rect((float)(num4 * Screen.width / num), (float)(i * Screen.height / num2), (float)(Screen.width / num), (float)(Screen.height / num2)));
					TestClientController testClientController = this.clients[num3];
					if (testClientController != null)
					{
						testClientController.OnGUI();
					}
					GUILayout.EndArea();
					num3++;
					num4++;
				}
			}
		}

		// Token: 0x060000C1 RID: 193 RVA: 0x00005A20 File Offset: 0x00003C20
		private void Update()
		{
			if (this.clients == null || this.clients.Length == 0)
			{
				return;
			}
			foreach (TestClientController testClientController in this.clients)
			{
				if (testClientController != null)
				{
					testClientController.Update();
				}
			}
		}

		// Token: 0x060000C2 RID: 194 RVA: 0x00005A64 File Offset: 0x00003C64
		private void OnDestroy()
		{
			foreach (TestClientController testClientController in this.clients)
			{
				if (testClientController != null)
				{
					testClientController.Disconnect();
				}
			}
		}

		// Token: 0x0400004D RID: 77
		[SerializeField]
		private int clientCount;

		// Token: 0x0400004E RID: 78
		[SerializeField]
		private NetworkEnvironment networkEnv;

		// Token: 0x0400004F RID: 79
		[SerializeField]
		private MockMMCC.UserCreds[] userCreds;

		// Token: 0x04000050 RID: 80
		private TestClientController[] clients;

		// Token: 0x0200001A RID: 26
		[Serializable]
		public struct UserCreds
		{
			// Token: 0x04000051 RID: 81
			public string userName;

			// Token: 0x04000052 RID: 82
			public string password;
		}
	}
}
