using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using Endless.Matchmaking;
using MatchmakingClientSDK;
using UnityEngine;

namespace Runtime.Shared.Matchmaking
{
	// Token: 0x0200000B RID: 11
	public class EndlessAllocator : IMatchAllocator
	{
		// Token: 0x14000001 RID: 1
		// (add) Token: 0x06000055 RID: 85 RVA: 0x000045D0 File Offset: 0x000027D0
		// (remove) Token: 0x06000056 RID: 86 RVA: 0x00004608 File Offset: 0x00002808
		public event Action OnMatchAllocated;

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000057 RID: 87 RVA: 0x0000463D File Offset: 0x0000283D
		public object LastAllocation
		{
			get
			{
				return this.LastAllocationData;
			}
		}

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000058 RID: 88 RVA: 0x0000464A File Offset: 0x0000284A
		// (set) Token: 0x06000059 RID: 89 RVA: 0x00004652 File Offset: 0x00002852
		public AllocationData? LastAllocationData { get; private set; }

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x0600005A RID: 90 RVA: 0x0000465C File Offset: 0x0000285C
		public string PublicIp
		{
			get
			{
				if (this.LastAllocationData == null)
				{
					return null;
				}
				AllocationData? allocationData;
				return allocationData.GetValueOrDefault().publicIp;
			}
		}

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x0600005B RID: 91 RVA: 0x00004688 File Offset: 0x00002888
		public string LocalIp
		{
			get
			{
				if (this.LastAllocationData == null)
				{
					return null;
				}
				AllocationData? allocationData;
				return allocationData.GetValueOrDefault().localIp;
			}
		}

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x0600005C RID: 92 RVA: 0x000046B4 File Offset: 0x000028B4
		public string Name
		{
			get
			{
				if (this.LastAllocationData == null)
				{
					return null;
				}
				AllocationData? allocationData;
				return allocationData.GetValueOrDefault().name;
			}
		}

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x0600005D RID: 93 RVA: 0x000046E0 File Offset: 0x000028E0
		public int Port
		{
			get
			{
				if (this.LastAllocationData == null)
				{
					return 0;
				}
				AllocationData? allocationData;
				return allocationData.GetValueOrDefault().port;
			}
		}

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x0600005E RID: 94 RVA: 0x0000470C File Offset: 0x0000290C
		public string Key
		{
			get
			{
				if (this.LastAllocationData == null)
				{
					return null;
				}
				AllocationData? allocationData;
				return allocationData.GetValueOrDefault().key;
			}
		}

		// Token: 0x0600005F RID: 95 RVA: 0x00004737 File Offset: 0x00002937
		public EndlessAllocator(string stage)
		{
			this.Stage = stage;
		}

		// Token: 0x06000060 RID: 96 RVA: 0x00004748 File Offset: 0x00002948
		public async void Allocate()
		{
			this.lastAllocationGuid = Guid.NewGuid().ToString();
			this.allocationTask(this.lastAllocationGuid);
		}

		// Token: 0x06000061 RID: 97 RVA: 0x00004780 File Offset: 0x00002980
		public static string GetLocalIPAddress()
		{
			try
			{
				foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
				{
					if (networkInterface.OperationalStatus == OperationalStatus.Up && networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback && networkInterface.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
					{
						foreach (UnicastIPAddressInformation unicastIPAddressInformation in networkInterface.GetIPProperties().UnicastAddresses)
						{
							if (unicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(unicastIPAddressInformation.Address))
							{
								return unicastIPAddressInformation.Address.ToString();
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("Error retrieving local IP address: " + ex.Message);
			}
			Debug.LogWarning("Could not find a valid local network IPv4 address. Defaulting to 127.0.0.1.");
			return "127.0.0.1";
		}

		// Token: 0x06000062 RID: 98 RVA: 0x00004874 File Offset: 0x00002A74
		private async Task allocationTask(string allocationGuid)
		{
			try
			{
				using (HttpClient httpClient = new HttpClient())
				{
					HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(string.Concat(new string[]
					{
						"https://",
						EndlessAllocator.relayEndpoints[this.Stage],
						"/allocate?stage=",
						this.MMCC.NetworkEnv.ToString(),
						"&hostToken=",
						this.MMCC.UserToken
					}));
					if (allocationGuid != this.lastAllocationGuid)
					{
						return;
					}
					string text = await httpResponseMessage.Content.ReadAsStringAsync();
					Debug.Log("Allocation Response: " + text);
					Document document = Document.FromJson(text);
					this.LastAllocationData = new AllocationData?(new AllocationData
					{
						publicIp = (MatchmakingClientController.Instance.UseHostName ? document["hostName"].AsString() : document["publicIp"].AsString()),
						localIp = EndlessAllocator.GetLocalIPAddress(),
						name = "Endless Relay Server",
						port = document["port"].AsInt(),
						key = document["key"].AsString()
					});
					Action onMatchAllocated = this.OnMatchAllocated;
					if (onMatchAllocated != null)
					{
						onMatchAllocated();
					}
				}
				HttpClient httpClient = null;
			}
			catch (Exception ex)
			{
				if (!(allocationGuid != this.lastAllocationGuid))
				{
					Debug.LogError(ex.ToString());
					Action onMatchAllocated2 = this.OnMatchAllocated;
					if (onMatchAllocated2 != null)
					{
						onMatchAllocated2();
					}
				}
			}
		}

		// Token: 0x06000063 RID: 99 RVA: 0x000048C0 File Offset: 0x00002AC0
		public void Reset()
		{
			this.LastAllocationData = null;
		}

		// Token: 0x06000064 RID: 100 RVA: 0x000048DC File Offset: 0x00002ADC
		// Note: this type is marked as 'beforefieldinit'.
		static EndlessAllocator()
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary["DEV"] = "relay-dev.endstar.endlessstudios.com";
			dictionary["STAGING"] = "relay.endstar.endlessstudios.com";
			dictionary["PROD"] = "relay.endstar.endlessstudios.com";
			EndlessAllocator.relayEndpoints = dictionary;
		}

		// Token: 0x0400000A RID: 10
		public static readonly IReadOnlyDictionary<string, string> relayEndpoints;

		// Token: 0x0400000B RID: 11
		public MatchmakingClientController MMCC;

		// Token: 0x0400000C RID: 12
		public readonly string Stage;

		// Token: 0x0400000F RID: 15
		private string lastAllocationGuid;
	}
}
