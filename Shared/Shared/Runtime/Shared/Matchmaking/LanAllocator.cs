using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using UnityEngine;

namespace Runtime.Shared.Matchmaking
{
	// Token: 0x02000012 RID: 18
	public class LanAllocator : IMatchAllocator
	{
		// Token: 0x14000003 RID: 3
		// (add) Token: 0x060000A1 RID: 161 RVA: 0x00005148 File Offset: 0x00003348
		// (remove) Token: 0x060000A2 RID: 162 RVA: 0x00005180 File Offset: 0x00003380
		public event Action OnMatchAllocated;

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x060000A3 RID: 163 RVA: 0x000051B5 File Offset: 0x000033B5
		// (set) Token: 0x060000A4 RID: 164 RVA: 0x000051BD File Offset: 0x000033BD
		public object LastAllocation { get; private set; }

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x060000A5 RID: 165 RVA: 0x000051C6 File Offset: 0x000033C6
		// (set) Token: 0x060000A6 RID: 166 RVA: 0x000051CE File Offset: 0x000033CE
		public string PublicIp { get; private set; }

		// Token: 0x17000020 RID: 32
		// (get) Token: 0x060000A7 RID: 167 RVA: 0x000051D7 File Offset: 0x000033D7
		// (set) Token: 0x060000A8 RID: 168 RVA: 0x000051DF File Offset: 0x000033DF
		public string LocalIp { get; private set; }

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x060000A9 RID: 169 RVA: 0x000051E8 File Offset: 0x000033E8
		// (set) Token: 0x060000AA RID: 170 RVA: 0x000051F0 File Offset: 0x000033F0
		public string Name { get; private set; }

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x060000AB RID: 171 RVA: 0x000051F9 File Offset: 0x000033F9
		// (set) Token: 0x060000AC RID: 172 RVA: 0x00005201 File Offset: 0x00003401
		public int Port { get; private set; }

		// Token: 0x17000023 RID: 35
		// (get) Token: 0x060000AD RID: 173 RVA: 0x0000520A File Offset: 0x0000340A
		// (set) Token: 0x060000AE RID: 174 RVA: 0x00005212 File Offset: 0x00003412
		public string Key { get; private set; }

		// Token: 0x060000AF RID: 175 RVA: 0x0000521C File Offset: 0x0000341C
		public void Allocate()
		{
			try
			{
				foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
				{
					if (networkInterface.OperationalStatus == OperationalStatus.Up && networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback && !networkInterface.Description.ToLower().Contains("virtual") && !networkInterface.Description.ToLower().Contains("vpn"))
					{
						foreach (UnicastIPAddressInformation unicastIPAddressInformation in networkInterface.GetIPProperties().UnicastAddresses)
						{
							if (unicastIPAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork)
							{
								this.LocalIp = unicastIPAddressInformation.Address.ToString();
								break;
							}
						}
						if (this.LocalIp != null)
						{
							break;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.ToString());
				this.LastAllocation = null;
				Action onMatchAllocated = this.OnMatchAllocated;
				if (onMatchAllocated != null)
				{
					onMatchAllocated();
				}
				return;
			}
			this.PublicIp = this.LocalIp;
			this.Name = "LAN";
			this.Port = 37000;
			this.Key = Guid.NewGuid().ToString();
			this.LastAllocation = new object();
			Action onMatchAllocated2 = this.OnMatchAllocated;
			if (onMatchAllocated2 == null)
			{
				return;
			}
			onMatchAllocated2();
		}

		// Token: 0x060000B0 RID: 176 RVA: 0x00005384 File Offset: 0x00003584
		public void Reset()
		{
			this.LastAllocation = null;
			this.PublicIp = null;
			this.LocalIp = null;
			this.Name = null;
			this.Port = 0;
			this.Key = null;
		}
	}
}
