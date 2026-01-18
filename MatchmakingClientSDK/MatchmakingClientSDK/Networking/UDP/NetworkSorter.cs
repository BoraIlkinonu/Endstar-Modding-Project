using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace Networking.UDP
{
	// Token: 0x02000030 RID: 48
	internal class NetworkSorter : IComparer<NetworkInterface>
	{
		// Token: 0x06000161 RID: 353 RVA: 0x000087AC File Offset: 0x000069AC
		public int Compare(NetworkInterface a, NetworkInterface b)
		{
			bool flag = a.NetworkInterfaceType == NetworkInterfaceType.Wman || a.NetworkInterfaceType == NetworkInterfaceType.Wwanpp || a.NetworkInterfaceType == NetworkInterfaceType.Wwanpp2;
			bool flag2 = b.NetworkInterfaceType == NetworkInterfaceType.Wman || b.NetworkInterfaceType == NetworkInterfaceType.Wwanpp || b.NetworkInterfaceType == NetworkInterfaceType.Wwanpp2;
			bool flag3 = a.NetworkInterfaceType == NetworkInterfaceType.Wireless80211;
			bool flag4 = b.NetworkInterfaceType == NetworkInterfaceType.Wireless80211;
			bool flag5 = a.NetworkInterfaceType == NetworkInterfaceType.Ethernet || a.NetworkInterfaceType == NetworkInterfaceType.Ethernet3Megabit || a.NetworkInterfaceType == NetworkInterfaceType.GigabitEthernet || a.NetworkInterfaceType == NetworkInterfaceType.FastEthernetFx || a.NetworkInterfaceType == NetworkInterfaceType.FastEthernetT;
			bool flag6 = b.NetworkInterfaceType == NetworkInterfaceType.Ethernet || b.NetworkInterfaceType == NetworkInterfaceType.Ethernet3Megabit || b.NetworkInterfaceType == NetworkInterfaceType.GigabitEthernet || b.NetworkInterfaceType == NetworkInterfaceType.FastEthernetFx || b.NetworkInterfaceType == NetworkInterfaceType.FastEthernetT;
			bool flag7 = !flag && !flag3 && !flag5;
			bool flag8 = !flag2 && !flag4 && !flag6;
			int num = (flag5 ? 3 : (flag3 ? 2 : ((flag7 > false) ? 1 : 0)));
			int num2 = (flag6 ? 3 : (flag4 ? 2 : ((flag8 > false) ? 1 : 0)));
			if (num <= num2)
			{
				return (num < num2) ? 1 : 0;
			}
			return -1;
		}
	}
}
