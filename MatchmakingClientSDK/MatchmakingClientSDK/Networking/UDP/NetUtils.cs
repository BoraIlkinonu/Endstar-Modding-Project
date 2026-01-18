using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace Networking.UDP
{
	// Token: 0x0200002F RID: 47
	public static class NetUtils
	{
		// Token: 0x06000157 RID: 343 RVA: 0x00008439 File Offset: 0x00006639
		public static IPEndPoint MakeEndPoint(string hostStr, int port)
		{
			return new IPEndPoint(NetUtils.ResolveAddress(hostStr), port);
		}

		// Token: 0x06000158 RID: 344 RVA: 0x00008448 File Offset: 0x00006648
		public static IPAddress ResolveAddress(string hostStr)
		{
			if (hostStr == "localhost")
			{
				return IPAddress.Loopback;
			}
			IPAddress ipaddress;
			if (!IPAddress.TryParse(hostStr, out ipaddress))
			{
				if (NetManager.IPv6Support)
				{
					ipaddress = NetUtils.ResolveAddress(hostStr, AddressFamily.InterNetworkV6);
				}
				if (ipaddress == null)
				{
					ipaddress = NetUtils.ResolveAddress(hostStr, AddressFamily.InterNetwork);
				}
			}
			if (ipaddress == null)
			{
				throw new ArgumentException("Invalid address: " + hostStr);
			}
			return ipaddress;
		}

		// Token: 0x06000159 RID: 345 RVA: 0x000084A4 File Offset: 0x000066A4
		public static IPAddress ResolveAddress(string hostStr, AddressFamily addressFamily)
		{
			foreach (IPAddress ipaddress in Dns.GetHostEntry(hostStr).AddressList)
			{
				if (ipaddress.AddressFamily == addressFamily)
				{
					return ipaddress;
				}
			}
			return null;
		}

		// Token: 0x0600015A RID: 346 RVA: 0x000084DB File Offset: 0x000066DB
		public static List<string> GetLocalIpList(LocalAddrType addrType)
		{
			List<string> list = new List<string>();
			NetUtils.GetLocalIpList(list, addrType);
			return list;
		}

		// Token: 0x0600015B RID: 347 RVA: 0x000084EC File Offset: 0x000066EC
		public static void GetLocalIpList(IList<string> targetList, LocalAddrType addrType)
		{
			bool flag = (addrType & LocalAddrType.IPv4) == LocalAddrType.IPv4;
			bool flag2 = (addrType & LocalAddrType.IPv6) == LocalAddrType.IPv6;
			try
			{
				NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
				Array.Sort<NetworkInterface>(allNetworkInterfaces, NetUtils.NetworkSorter);
				foreach (NetworkInterface networkInterface in allNetworkInterfaces)
				{
					if (networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback && networkInterface.OperationalStatus == OperationalStatus.Up)
					{
						IPInterfaceProperties ipproperties = networkInterface.GetIPProperties();
						if (ipproperties.GatewayAddresses.Count != 0)
						{
							foreach (UnicastIPAddressInformation unicastIPAddressInformation in ipproperties.UnicastAddresses)
							{
								IPAddress address = unicastIPAddressInformation.Address;
								if ((flag && address.AddressFamily == AddressFamily.InterNetwork) || (flag2 && address.AddressFamily == AddressFamily.InterNetworkV6))
								{
									targetList.Add(address.ToString());
								}
							}
						}
					}
				}
				if (targetList.Count == 0)
				{
					foreach (IPAddress ipaddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
					{
						if ((flag && ipaddress.AddressFamily == AddressFamily.InterNetwork) || (flag2 && ipaddress.AddressFamily == AddressFamily.InterNetworkV6))
						{
							targetList.Add(ipaddress.ToString());
						}
					}
				}
			}
			catch
			{
			}
			if (targetList.Count == 0)
			{
				if (flag)
				{
					targetList.Add("127.0.0.1");
				}
				if (flag2)
				{
					targetList.Add("::1");
				}
			}
		}

		// Token: 0x0600015C RID: 348 RVA: 0x00008670 File Offset: 0x00006870
		public static string GetLocalIp(LocalAddrType addrType)
		{
			List<string> ipList = NetUtils.IpList;
			string text;
			lock (ipList)
			{
				NetUtils.IpList.Clear();
				NetUtils.GetLocalIpList(NetUtils.IpList, addrType);
				text = ((NetUtils.IpList.Count == 0) ? string.Empty : NetUtils.IpList[0]);
			}
			return text;
		}

		// Token: 0x0600015D RID: 349 RVA: 0x000086E0 File Offset: 0x000068E0
		internal static void PrintInterfaceInfos()
		{
			try
			{
				NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
				for (int i = 0; i < allNetworkInterfaces.Length; i++)
				{
					foreach (UnicastIPAddressInformation unicastIPAddressInformation in allNetworkInterfaces[i].GetIPProperties().UnicastAddresses)
					{
						if (unicastIPAddressInformation.Address.AddressFamily != AddressFamily.InterNetwork)
						{
							AddressFamily addressFamily = unicastIPAddressInformation.Address.AddressFamily;
						}
					}
				}
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x0600015E RID: 350 RVA: 0x00008770 File Offset: 0x00006970
		internal static int RelativeSequenceNumber(int number, int expected)
		{
			return (number - expected + 32768 + 16384) % 32768 - 16384;
		}

		// Token: 0x0600015F RID: 351 RVA: 0x0000878D File Offset: 0x0000698D
		internal static T[] AllocatePinnedUninitializedArray<[IsUnmanaged] T>(int count) where T : struct, ValueType
		{
			return new T[count];
		}

		// Token: 0x0400013A RID: 314
		private static readonly NetworkSorter NetworkSorter = new NetworkSorter();

		// Token: 0x0400013B RID: 315
		private static readonly List<string> IpList = new List<string>();
	}
}
