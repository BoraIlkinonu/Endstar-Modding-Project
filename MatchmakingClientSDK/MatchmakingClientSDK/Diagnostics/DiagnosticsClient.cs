using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MatchmakingClientSDK.Diagnostics
{
	// Token: 0x0200006D RID: 109
	public class DiagnosticsClient : IDisposable
	{
		// Token: 0x1400003D RID: 61
		// (add) Token: 0x06000418 RID: 1048 RVA: 0x00012404 File Offset: 0x00010604
		// (remove) Token: 0x06000419 RID: 1049 RVA: 0x0001243C File Offset: 0x0001063C
		public event Action<string, bool, string, int> ReportEvent;

		// Token: 0x0600041B RID: 1051 RVA: 0x000124A8 File Offset: 0x000106A8
		public void Dispose()
		{
			this.disposed = true;
		}

		// Token: 0x0600041C RID: 1052 RVA: 0x000124B4 File Offset: 0x000106B4
		public async Task StartDiagnostics()
		{
			this.CheckNetworkInterfaces();
			await this.CheckDns();
			if (!this.disposed)
			{
				await this.CheckPing();
				if (!this.disposed)
				{
					await this.CheckSockets();
					if (!this.disposed)
					{
						await this.CheckSpeed();
					}
				}
			}
		}

		// Token: 0x0600041D RID: 1053 RVA: 0x000124F8 File Offset: 0x000106F8
		private void CheckNetworkInterfaces()
		{
			int num = 0;
			Action<string, bool, string, int> reportEvent = this.ReportEvent;
			if (reportEvent != null)
			{
				reportEvent("NIC is connected to the network.", false, "One or more of system's network interfaces is connected to the network.", num);
			}
			foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
			{
				switch (networkInterface.OperationalStatus)
				{
				case OperationalStatus.Up:
				{
					Action<string, bool, string, int> reportEvent2 = this.ReportEvent;
					if (reportEvent2 != null)
					{
						reportEvent2("Network Interface: " + networkInterface.Name + ": Connected to network.", false, "The network interface is up; it can transmit data packets.", num);
					}
					break;
				}
				case OperationalStatus.Down:
				{
					Action<string, bool, string, int> reportEvent3 = this.ReportEvent;
					if (reportEvent3 != null)
					{
						reportEvent3("Network Interface: " + networkInterface.Name + ": Disconnected from network.", true, "The network interface is down; it cannot transmit data packets.", num);
					}
					break;
				}
				case OperationalStatus.Testing:
				{
					Action<string, bool, string, int> reportEvent4 = this.ReportEvent;
					if (reportEvent4 != null)
					{
						reportEvent4("Network Interface: " + networkInterface.Name + ": In test mode.", true, "The network interface is running tests.", num);
					}
					break;
				}
				case OperationalStatus.Unknown:
				{
					Action<string, bool, string, int> reportEvent5 = this.ReportEvent;
					if (reportEvent5 != null)
					{
						reportEvent5("Network Interface: " + networkInterface.Name + ": Unknown status.", true, "The network interface status is not known.", num);
					}
					break;
				}
				case OperationalStatus.Dormant:
				{
					Action<string, bool, string, int> reportEvent6 = this.ReportEvent;
					if (reportEvent6 != null)
					{
						reportEvent6("Network Interface: " + networkInterface.Name + ": In dormant state.", true, "The network interface is not in a condition to transmit data packets; it is waiting for an external event.", num);
					}
					break;
				}
				case OperationalStatus.NotPresent:
				{
					Action<string, bool, string, int> reportEvent7 = this.ReportEvent;
					if (reportEvent7 != null)
					{
						reportEvent7("Network Interface: " + networkInterface.Name + ": Not present.", true, "The network interface is unable to transmit data packets because of a missing component, typically a hardware component.", num);
					}
					break;
				}
				case OperationalStatus.LowerLayerDown:
				{
					Action<string, bool, string, int> reportEvent8 = this.ReportEvent;
					if (reportEvent8 != null)
					{
						reportEvent8("Network Interface: " + networkInterface.Name + ": Sub-interface is disconnected.", true, "The network interface is unable to transmit data packets because it runs on top of one or more other interfaces, and at least one of these \"lower layer\" interfaces is down.", num);
					}
					break;
				}
				}
			}
		}

		// Token: 0x0600041E RID: 1054 RVA: 0x000126D4 File Offset: 0x000108D4
		private async Task CheckDns()
		{
			int reportIndex = 1;
			StringBuilder strBuilder = new StringBuilder();
			foreach (string domain in this.domains)
			{
				try
				{
					IPHostEntry iphostEntry = await Dns.GetHostEntryAsync(domain);
					if (this.disposed)
					{
						return;
					}
					strBuilder.AppendLine("DNS Host: " + iphostEntry.HostName);
					IPAddress[] addressList = iphostEntry.AddressList;
					for (int j = 0; j < addressList.Length; j++)
					{
						strBuilder.AppendLine(string.Format("DNS IP: {0}", addressList[j]));
					}
					string[] aliases = iphostEntry.Aliases;
					for (int j = 0; j < aliases.Length; j++)
					{
						strBuilder.AppendLine("DNS Alias: " + aliases[j]);
					}
					Action<string, bool, string, int> reportEvent = this.ReportEvent;
					if (reportEvent != null)
					{
						reportEvent("DNS found " + domain + ".", false, string.Format("DNS was able to resolve the domain name {0}: {1} {2}", domain, Environment.NewLine, strBuilder), reportIndex);
					}
				}
				catch (Exception)
				{
					Action<string, bool, string, int> reportEvent2 = this.ReportEvent;
					if (reportEvent2 != null)
					{
						reportEvent2("DNS was not able to resolve " + domain, true, "DNS server could not find " + domain + ".", reportIndex);
					}
				}
				domain = null;
			}
			string[] array = null;
		}

		// Token: 0x0600041F RID: 1055 RVA: 0x00012718 File Offset: 0x00010918
		private async Task CheckPing()
		{
			int reportIndex = 2;
			using (Ping ping = new Ping())
			{
				foreach (string domain in this.domains)
				{
					try
					{
						PingReply pingReply = await ping.SendPingAsync(domain);
						if (this.disposed)
						{
							return;
						}
						IPStatus status = pingReply.Status;
						if (status <= IPStatus.Success)
						{
							if (status != IPStatus.Unknown)
							{
								if (status == IPStatus.Success)
								{
									Action<string, bool, string, int> reportEvent = this.ReportEvent;
									if (reportEvent == null)
									{
										goto IL_0650;
									}
									reportEvent(string.Format("Ping to {0} successful. Time: {1}ms", domain, pingReply.RoundtripTime), false, "The ICMP echo request succeeded; an ICMP echo reply was received.", reportIndex);
									goto IL_0650;
								}
							}
							else
							{
								Action<string, bool, string, int> reportEvent2 = this.ReportEvent;
								if (reportEvent2 == null)
								{
									goto IL_0650;
								}
								reportEvent2("Ping to " + domain + " failed. Unknown error.", true, "The ICMP echo request failed for an unknown reason.", reportIndex);
								goto IL_0650;
							}
						}
						else
						{
							switch (status)
							{
							case IPStatus.DestinationNetworkUnreachable:
							{
								Action<string, bool, string, int> reportEvent3 = this.ReportEvent;
								if (reportEvent3 == null)
								{
									goto IL_0650;
								}
								reportEvent3("Ping to " + domain + " failed. Destination network unreachable.", true, "The ICMP echo request failed because the network that contains the destination computer is not reachable.", reportIndex);
								goto IL_0650;
							}
							case IPStatus.DestinationHostUnreachable:
							{
								Action<string, bool, string, int> reportEvent4 = this.ReportEvent;
								if (reportEvent4 == null)
								{
									goto IL_0650;
								}
								reportEvent4("Ping to " + domain + " failed. Destination host unreachable.", true, "The ICMP echo request failed because the destination computer is not reachable.", reportIndex);
								goto IL_0650;
							}
							case IPStatus.DestinationProtocolUnreachable:
							case (IPStatus)11011:
							case (IPStatus)11017:
								break;
							case IPStatus.DestinationPortUnreachable:
							{
								Action<string, bool, string, int> reportEvent5 = this.ReportEvent;
								if (reportEvent5 == null)
								{
									goto IL_0650;
								}
								reportEvent5("Ping to " + domain + " failed. Destination port unreachable.", true, "The ICMP echo request failed because the port on the destination computer is not available.", reportIndex);
								goto IL_0650;
							}
							case IPStatus.NoResources:
							{
								Action<string, bool, string, int> reportEvent6 = this.ReportEvent;
								if (reportEvent6 == null)
								{
									goto IL_0650;
								}
								reportEvent6("Ping to " + domain + " failed. No sufficient network resources.", true, "The ICMP echo request failed because of insufficient network resources. Network might be under heavy load.", reportIndex);
								goto IL_0650;
							}
							case IPStatus.BadOption:
							{
								Action<string, bool, string, int> reportEvent7 = this.ReportEvent;
								if (reportEvent7 == null)
								{
									goto IL_0650;
								}
								reportEvent7("Ping to " + domain + " failed. Bad option.", true, "The ICMP echo request failed because it contains an invalid option.", reportIndex);
								goto IL_0650;
							}
							case IPStatus.HardwareError:
							{
								Action<string, bool, string, int> reportEvent8 = this.ReportEvent;
								if (reportEvent8 == null)
								{
									goto IL_0650;
								}
								reportEvent8("Ping to " + domain + " failed. Hardware error.", true, "The ICMP echo request failed because of a hardware error.", reportIndex);
								goto IL_0650;
							}
							case IPStatus.PacketTooBig:
							{
								Action<string, bool, string, int> reportEvent9 = this.ReportEvent;
								if (reportEvent9 == null)
								{
									goto IL_0650;
								}
								reportEvent9("Ping to " + domain + " failed. Packet too big.", true, "The ICMP echo request failed because the packet containing the request is larger than the maximum transmission unit (MTU) of a node (router or gateway) located between the source and destination. The MTU defines the maximum size of a transmittable packet.", reportIndex);
								goto IL_0650;
							}
							case IPStatus.TimedOut:
							{
								Action<string, bool, string, int> reportEvent10 = this.ReportEvent;
								if (reportEvent10 == null)
								{
									goto IL_0650;
								}
								reportEvent10("Ping to " + domain + " timed out.", true, "The ICMP echo reply was not received within 5 seconds.", reportIndex);
								goto IL_0650;
							}
							case IPStatus.BadRoute:
							{
								Action<string, bool, string, int> reportEvent11 = this.ReportEvent;
								if (reportEvent11 == null)
								{
									goto IL_0650;
								}
								reportEvent11("Ping to " + domain + " failed. Bad route.", true, "The ICMP echo request failed because there is no valid route between the source and destination computers.", reportIndex);
								goto IL_0650;
							}
							case IPStatus.TtlExpired:
							{
								Action<string, bool, string, int> reportEvent12 = this.ReportEvent;
								if (reportEvent12 == null)
								{
									goto IL_0650;
								}
								reportEvent12("Ping to " + domain + " failed. TTL expired.", true, "The ICMP echo request failed because the Time-to-Live (TTL) value of the packet reached zero, causing the forwarding node (router or gateway) to discard the packet.", reportIndex);
								goto IL_0650;
							}
							case IPStatus.TtlReassemblyTimeExceeded:
							{
								Action<string, bool, string, int> reportEvent13 = this.ReportEvent;
								if (reportEvent13 == null)
								{
									goto IL_0650;
								}
								reportEvent13("Ping to " + domain + " failed. TTL reassembly time exceeded.", true, "The ICMP echo request failed because the packet was divided into fragments for transmission and all of the fragments were not received within the time allotted for reassembly. RFC 2460 specifies 60 seconds as the time limit within which all packet fragments must be received.", reportIndex);
								goto IL_0650;
							}
							case IPStatus.ParameterProblem:
							{
								Action<string, bool, string, int> reportEvent14 = this.ReportEvent;
								if (reportEvent14 == null)
								{
									goto IL_0650;
								}
								reportEvent14("Ping to " + domain + " failed. Parameter problem.", true, "The ICMP echo request failed because a node (router or gateway) encountered problems while processing the packet header. This is the status if, for example, the header contains invalid field data or an unrecognized option.", reportIndex);
								goto IL_0650;
							}
							case IPStatus.SourceQuench:
							{
								Action<string, bool, string, int> reportEvent15 = this.ReportEvent;
								if (reportEvent15 == null)
								{
									goto IL_0650;
								}
								reportEvent15("Ping to " + domain + " failed. Source quench.", true, "The ICMP echo request failed because the packet was discarded. This occurs when the this computer's output queue has insufficient storage space, or when packets arrive at the destination too quickly to be processed.", reportIndex);
								goto IL_0650;
							}
							case IPStatus.BadDestination:
							{
								Action<string, bool, string, int> reportEvent16 = this.ReportEvent;
								if (reportEvent16 == null)
								{
									goto IL_0650;
								}
								reportEvent16("Ping to " + domain + " failed. Bad destination.", true, "The ICMP echo request failed because the destination IP address cannot receive ICMP echo requests or should never appear in the destination address field of any IP datagram. For example, specifying IP address \"000.0.0.0\" returns this status.", reportIndex);
								goto IL_0650;
							}
							default:
								switch (status)
								{
								case IPStatus.DestinationUnreachable:
								{
									Action<string, bool, string, int> reportEvent17 = this.ReportEvent;
									if (reportEvent17 == null)
									{
										goto IL_0650;
									}
									reportEvent17("Ping to " + domain + " failed. Destination unreachable.", true, "The ICMP echo request failed because the destination computer that is specified in an ICMP echo message is not reachable; the exact cause of problem is unknown.", reportIndex);
									goto IL_0650;
								}
								case IPStatus.TimeExceeded:
								{
									Action<string, bool, string, int> reportEvent18 = this.ReportEvent;
									if (reportEvent18 == null)
									{
										goto IL_0650;
									}
									reportEvent18("Ping to " + domain + " failed. Time exceeded.", true, "The ICMP echo request failed because the Time-to-Live (TTL) value of the packet reached zero, causing the forwarding node (router or gateway) to discard the packet.", reportIndex);
									goto IL_0650;
								}
								case IPStatus.BadHeader:
								{
									Action<string, bool, string, int> reportEvent19 = this.ReportEvent;
									if (reportEvent19 == null)
									{
										goto IL_0650;
									}
									reportEvent19("Ping to " + domain + " failed. Bad header.", true, "The ICMP echo request failed because the header is invalid.", reportIndex);
									goto IL_0650;
								}
								case IPStatus.UnrecognizedNextHeader:
								{
									Action<string, bool, string, int> reportEvent20 = this.ReportEvent;
									if (reportEvent20 == null)
									{
										goto IL_0650;
									}
									reportEvent20("Ping to " + domain + " failed. Unrecognized next header.", true, "The ICMP echo request failed because the Next Header field does not contain a recognized value. The Next Header field indicates the extension header type (if present) or the protocol above the IP layer, for example, TCP or UDP.", reportIndex);
									goto IL_0650;
								}
								case IPStatus.IcmpError:
								{
									Action<string, bool, string, int> reportEvent21 = this.ReportEvent;
									if (reportEvent21 == null)
									{
										goto IL_0650;
									}
									reportEvent21("Ping to " + domain + " failed. ICMP error.", true, "The ICMP echo request failed because of an ICMP protocol error.", reportIndex);
									goto IL_0650;
								}
								}
								break;
							}
						}
						Action<string, bool, string, int> reportEvent22 = this.ReportEvent;
						if (reportEvent22 != null)
						{
							reportEvent22("Ping to " + domain + " failed. Unknown error.", true, "The ICMP echo request failed for an unknown reason.", reportIndex);
						}
						IL_0650:;
					}
					catch (Exception ex)
					{
						Action<string, bool, string, int> reportEvent23 = this.ReportEvent;
						if (reportEvent23 != null)
						{
							reportEvent23("Ping to " + domain + " failed. " + ex.Message, true, "The ICMP echo request failed for an unknown reason.", reportIndex);
						}
					}
					domain = null;
				}
				string[] array = null;
			}
		}

		// Token: 0x06000420 RID: 1056 RVA: 0x0001275C File Offset: 0x0001095C
		private async Task CheckSockets()
		{
			int reportIndex = 3;
			foreach (string domain in this.domains)
			{
				try
				{
					using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
					{
						await socket.ConnectAsync(domain, 443);
						if (this.disposed)
						{
							return;
						}
						if (socket.Connected)
						{
							Action<string, bool, string, int> reportEvent = this.ReportEvent;
							if (reportEvent != null)
							{
								reportEvent("Socket [TCP] connected to " + domain + ".", false, "The socket was able to connect to the remote host.", reportIndex);
							}
						}
						else
						{
							Action<string, bool, string, int> reportEvent2 = this.ReportEvent;
							if (reportEvent2 != null)
							{
								reportEvent2("Socket [TCP] failed to connect to " + domain + ".", true, "The socket was unable to connect to the remote host.", reportIndex);
							}
						}
					}
					Socket socket = null;
				}
				catch (SocketException ex)
				{
					Action<string, bool, string, int> reportEvent3 = this.ReportEvent;
					if (reportEvent3 != null)
					{
						reportEvent3("Socket [TCP] failed to connect to " + domain + ". ", true, string.Format("Error {0}: {1}", ex.ErrorCode, ex.Message), reportIndex);
					}
				}
				catch (Exception ex2)
				{
					Action<string, bool, string, int> reportEvent4 = this.ReportEvent;
					if (reportEvent4 != null)
					{
						reportEvent4("Socket [TCP] failed to connect to " + domain + ".", true, "Error -1: " + ex2.Message + ".", reportIndex);
					}
				}
				domain = null;
			}
			string[] array = null;
		}

		// Token: 0x06000421 RID: 1057 RVA: 0x000127A0 File Offset: 0x000109A0
		private async Task CheckSpeed()
		{
			int reportIndex = 4;
			using (HttpClient httpClient = new HttpClient())
			{
				Stopwatch stopwatch = new Stopwatch();
				foreach (string domain in this.domains)
				{
					httpClient.BaseAddress = new Uri(this.protocol + domain);
					httpClient.Timeout = TimeSpan.FromSeconds(30.0);
					double totalReceivedSeconds = 0.0;
					long totalBytesReceived = 0L;
					double totalSentSeconds = 0.0;
					long totalBytesSent = 0L;
					for (int i = 0; i < 10; i++)
					{
						double stepReceivedSeconds = 0.0;
						double stepSentSeconds = 0.0;
						stopwatch.Restart();
						try
						{
							HttpResponseMessage httpResponseMessage = await httpClient.GetAsync("/speedTest");
							if (this.disposed)
							{
								stopwatch.Stop();
								return;
							}
							if (httpResponseMessage.IsSuccessStatusCode)
							{
								stepReceivedSeconds = stopwatch.Elapsed.TotalSeconds;
								totalReceivedSeconds += stopwatch.Elapsed.TotalSeconds;
								totalBytesReceived += (long)this.payload.Length;
							}
						}
						catch (Exception)
						{
						}
						stopwatch.Stop();
						stopwatch.Restart();
						try
						{
							HttpResponseMessage httpResponseMessage2 = await httpClient.PostAsync("/speedTest", new ByteArrayContent(this.payload));
							if (this.disposed)
							{
								stopwatch.Stop();
								return;
							}
							if (httpResponseMessage2.IsSuccessStatusCode)
							{
								stepSentSeconds = stopwatch.Elapsed.TotalSeconds;
								totalSentSeconds += stopwatch.Elapsed.TotalSeconds;
								totalBytesSent += (long)this.payload.Length;
							}
						}
						catch (Exception)
						{
						}
						stopwatch.Stop();
						double num = (double)this.payload.Length / 1048576.0 / stepReceivedSeconds;
						double num2 = (double)this.payload.Length / 1048576.0 / stepSentSeconds;
						Action<string, bool, string, int> reportEvent = this.ReportEvent;
						if (reportEvent != null)
						{
							reportEvent(string.Format("Speed test for {0} update [{1}].", domain, i), false, string.Format("Download: {0:#.##} MB/s, Upload: {1:#.##} MB/s.", num, num2), reportIndex);
						}
					}
					double num3 = (double)totalBytesReceived / 1048576.0 / totalReceivedSeconds;
					double num4 = (double)totalBytesSent / 1048576.0 / totalSentSeconds;
					Action<string, bool, string, int> reportEvent2 = this.ReportEvent;
					if (reportEvent2 != null)
					{
						reportEvent2("Speed test for " + domain + " completed.", false, string.Format("Download: {0:#.##} MB/s, Upload: {1:#.##} MB/s.", num3, num4), reportIndex);
					}
					domain = null;
				}
				string[] array = null;
			}
		}

		// Token: 0x040002A6 RID: 678
		private bool disposed;

		// Token: 0x040002A7 RID: 679
		private string protocol = "https://";

		// Token: 0x040002A8 RID: 680
		private string[] domains = new string[] { "diagnostics.endlessstudios.com" };

		// Token: 0x040002A9 RID: 681
		private byte[] payload = new byte[5242880];
	}
}
