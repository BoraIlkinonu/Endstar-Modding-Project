using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using Endless.Matchmaking;
using Endless.Networking;
using UnityEngine;

namespace Runtime.Shared.Matchmaking
{
	// Token: 0x0200001D RID: 29
	public class SubDomainTester : MonoBehaviour
	{
		// Token: 0x060000D9 RID: 217 RVA: 0x00005E4B File Offset: 0x0000404B
		private void Awake()
		{
			MatchmakingClientController.OnAuthenticationProcessSuccessful = (Action<ClientData>)Delegate.Combine(MatchmakingClientController.OnAuthenticationProcessSuccessful, new Action<ClientData>(this.OnAuthenticated));
		}

		// Token: 0x060000DA RID: 218 RVA: 0x00005E6D File Offset: 0x0000406D
		private void OnAuthenticated(ClientData clientData)
		{
			this.TrySubDomain(clientData);
		}

		// Token: 0x060000DB RID: 219 RVA: 0x00005E78 File Offset: 0x00004078
		private async Task TrySubDomain(ClientData clientData)
		{
			using (HttpClient httpClient = new HttpClient())
			{
				try
				{
					HttpResponseMessage httpResponseMessage = await httpClient.GetAsync("http://relay-test.endstar.endlessstudios.com/status");
					if (httpResponseMessage.IsSuccessStatusCode)
					{
						this.ReportSubDomainTestResult(clientData, null);
					}
					else
					{
						this.ReportSubDomainTestResult(clientData, string.Format("Failed to connect to {0}: {1}", "relay-test.endstar.endlessstudios.com", httpResponseMessage.StatusCode));
					}
				}
				catch (Exception ex)
				{
					this.ReportSubDomainTestResult(clientData, ex.ToString());
				}
			}
		}

		// Token: 0x060000DC RID: 220 RVA: 0x00005EC4 File Offset: 0x000040C4
		private async Task ReportSubDomainTestResult(ClientData clientData, string errorMessage)
		{
			using (HttpClient httpClient = new HttpClient())
			{
				try
				{
					Document document = new Document();
					document["userName"] = clientData.DisplayName;
					document["userId"] = clientData.CoreData.PlatformId;
					Document document2 = document;
					if (!string.IsNullOrWhiteSpace(errorMessage))
					{
						document2["errorMessage"] = errorMessage;
					}
					StringContent stringContent = new StringContent(document2.ToJson(), Encoding.UTF8, "application/json");
					await httpClient.PostAsync("https://diagnostics.endlessstudios.com/subDomainResult", stringContent);
				}
				catch
				{
					Debug.LogError("Failed to report subdomain test result. Result: " + errorMessage);
				}
			}
		}

		// Token: 0x060000DD RID: 221 RVA: 0x00005F0F File Offset: 0x0000410F
		private void OnDestroy()
		{
			MatchmakingClientController.OnAuthenticationProcessSuccessful = (Action<ClientData>)Delegate.Remove(MatchmakingClientController.OnAuthenticationProcessSuccessful, new Action<ClientData>(this.OnAuthenticated));
		}

		// Token: 0x0400005C RID: 92
		private const string TEST_SUB_DOMAIN = "relay-test.endstar.endlessstudios.com";

		// Token: 0x0400005D RID: 93
		private const string ENDLESS_SUB_DOMAIN = "diagnostics.endlessstudios.com";
	}
}
