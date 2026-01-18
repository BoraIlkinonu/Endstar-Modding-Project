using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Endless.Networking.Http;

namespace Endless.Networking
{
	// Token: 0x0200000A RID: 10
	public class InstanceSelector
	{
		// Token: 0x14000002 RID: 2
		// (add) Token: 0x06000034 RID: 52 RVA: 0x00002B10 File Offset: 0x00000D10
		// (remove) Token: 0x06000035 RID: 53 RVA: 0x00002B48 File Offset: 0x00000D48
		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<ServerInstance> OnInstanceSelected;

		// Token: 0x14000003 RID: 3
		// (add) Token: 0x06000036 RID: 54 RVA: 0x00002B80 File Offset: 0x00000D80
		// (remove) Token: 0x06000037 RID: 55 RVA: 0x00002BB8 File Offset: 0x00000DB8
		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<ServerInstances> OnInstancesAvailable;

		// Token: 0x06000038 RID: 56 RVA: 0x00002BF0 File Offset: 0x00000DF0
		public InstanceSelector(string getPrefix, int getPort, string uri, string logFile)
		{
			this.requestUrl = getPrefix;
			bool flag = getPort >= 0 && getPort <= 65535;
			if (flag)
			{
				this.requestUrl += string.Format(":{0}", getPort);
			}
			bool flag2 = !string.IsNullOrWhiteSpace(uri);
			if (flag2)
			{
				this.requestUrl = this.requestUrl + "/" + uri;
			}
			this.logFile = logFile;
		}

		// Token: 0x06000039 RID: 57 RVA: 0x00002C80 File Offset: 0x00000E80
		public ServerInstances GetInstances()
		{
			bool flag = this.gettingInstances;
			ServerInstances serverInstances;
			if (flag)
			{
				serverInstances = null;
			}
			else
			{
				try
				{
					Task<byte[]> byteArrayAsync = HTTPClient.Client.GetByteArrayAsync(this.requestUrl);
					byteArrayAsync.Wait();
					byte[] result = byteArrayAsync.Result;
					DataBuffer dataBuffer = DataBuffer.FromBytes(result);
					ServerInstances serverInstances2 = ServerInstances.Deserialize(dataBuffer);
					serverInstances = serverInstances2;
				}
				catch (Exception ex)
				{
					Logger.Log(this.logFile, ex.ToString(), true);
					serverInstances = null;
				}
			}
			return serverInstances;
		}

		// Token: 0x0600003A RID: 58 RVA: 0x00002D00 File Offset: 0x00000F00
		public void GetInstancesAsync()
		{
			bool flag = this.gettingInstances;
			if (!flag)
			{
				this.gettingInstances = true;
				this.getInstancesAsync();
			}
		}

		// Token: 0x0600003B RID: 59 RVA: 0x00002D28 File Offset: 0x00000F28
		private async void getInstancesAsync()
		{
			try
			{
				byte[] array = await HTTPClient.Client.GetByteArrayAsync(this.requestUrl);
				byte[] result = array;
				array = null;
				DataBuffer resultBuffer = DataBuffer.FromBytes(result);
				ServerInstances instances = ServerInstances.Deserialize(resultBuffer);
				this.gettingInstances = false;
				Action<ServerInstances> onInstancesAvailable = this.OnInstancesAvailable;
				if (onInstancesAvailable != null)
				{
					onInstancesAvailable(instances);
				}
				result = null;
				resultBuffer = null;
				instances = null;
			}
			catch (Exception e)
			{
				Logger.Log(this.logFile, e.ToString(), true);
				this.gettingInstances = false;
				Action<ServerInstances> onInstancesAvailable2 = this.OnInstancesAvailable;
				if (onInstancesAvailable2 != null)
				{
					onInstancesAvailable2(null);
				}
			}
		}

		// Token: 0x0600003C RID: 60 RVA: 0x00002D64 File Offset: 0x00000F64
		public void AutoPickInstanceAsync(ServerInstances instances, InstanceSelector.InstanceSelectionType selectionType)
		{
			bool flag = this.pickingInstance;
			if (!flag)
			{
				this.pickingInstance = true;
				if (selectionType != InstanceSelector.InstanceSelectionType.Proximity)
				{
					this.pickRegionByProximityAsync(instances);
				}
				else
				{
					this.pickRegionByProximityAsync(instances);
				}
			}
		}

		// Token: 0x0600003D RID: 61 RVA: 0x00002DA4 File Offset: 0x00000FA4
		public ServerInstance AutoPickInstance(ServerInstances instances, InstanceSelector.InstanceSelectionType selectionType)
		{
			bool flag = this.pickingInstance;
			ServerInstance serverInstance;
			if (flag)
			{
				serverInstance = null;
			}
			else
			{
				long num = long.MaxValue;
				ServerInstance serverInstance2 = null;
				int num2;
				if (instances != null)
				{
					ServerInstance[] instances2 = instances.Instances;
					if (instances2 != null)
					{
						num2 = ((instances2.Length > 0) ? 1 : 0);
						goto IL_0058;
					}
				}
				num2 = 0;
				IL_0058:
				bool flag2 = num2 == 0;
				if (flag2)
				{
					serverInstance = null;
				}
				else
				{
					Task task = Task.Factory.StartNew(delegate
					{
						this.pingInstances(instances);
					});
					task.Wait();
					foreach (ServerInstance serverInstance3 in instances.Instances)
					{
						bool flag3 = serverInstance3.Latency > num;
						if (!flag3)
						{
							serverInstance2 = serverInstance3;
							num = serverInstance3.Latency;
						}
					}
					serverInstance = serverInstance2;
				}
			}
			return serverInstance;
		}

		// Token: 0x0600003E RID: 62 RVA: 0x00002E84 File Offset: 0x00001084
		private async void pickRegionByProximityAsync(ServerInstances instances)
		{
			long smallestLatency = long.MaxValue;
			ServerInstance closestInstance = null;
			bool flag;
			if (instances != null)
			{
				ServerInstance[] instances2 = instances.Instances;
				if (instances2 != null)
				{
					flag = instances2.Length > 0;
					goto IL_007F;
				}
			}
			flag = false;
			IL_007F:
			bool flag2 = flag;
			if (flag2)
			{
				Task task = Task.Factory.StartNew(delegate
				{
					this.pingInstances(instances);
				});
				await task;
				foreach (ServerInstance instance in instances.Instances)
				{
					if (instance.Latency <= smallestLatency)
					{
						closestInstance = instance;
						smallestLatency = instance.Latency;
						instance = null;
					}
				}
				ServerInstance[] array = null;
				task = null;
			}
			this.pickingInstance = false;
			Action<ServerInstance> onInstanceSelected = this.OnInstanceSelected;
			if (onInstanceSelected != null)
			{
				onInstanceSelected(closestInstance);
			}
		}

		// Token: 0x0600003F RID: 63 RVA: 0x00002EC4 File Offset: 0x000010C4
		private void pingInstances(ServerInstances instances)
		{
			foreach (ServerInstance serverInstance in instances.Instances)
			{
				try
				{
					Ping ping = new Ping();
					PingReply pingReply = ping.Send(serverInstance.InstanceIp);
					bool flag = pingReply != null && pingReply.Status == IPStatus.Success;
					if (flag)
					{
						serverInstance.Latency = pingReply.RoundtripTime;
						Logger.Log(this.logFile, string.Format("Ping succeeded for {0}/{1} with [{2}ms]", serverInstance.InstanceIp, serverInstance.InstancePort, pingReply.RoundtripTime), true);
					}
					else
					{
						serverInstance.Latency = long.MaxValue;
						Logger.Log(this.logFile, string.Format("Ping failed for {0}/{1}", serverInstance.InstanceIp, serverInstance.InstancePort), true);
					}
				}
				catch
				{
					serverInstance.Latency = long.MaxValue;
					Logger.Log(this.logFile, string.Format("Ping failed for {0}/{1}", serverInstance.InstanceIp, serverInstance.InstancePort), true);
				}
			}
		}

		// Token: 0x0400001B RID: 27
		private readonly string requestUrl;

		// Token: 0x0400001C RID: 28
		private readonly string logFile;

		// Token: 0x0400001D RID: 29
		private bool gettingInstances = false;

		// Token: 0x0400001E RID: 30
		private bool pickingInstance = false;

		// Token: 0x02000028 RID: 40
		public enum InstanceSelectionType
		{
			// Token: 0x040000A0 RID: 160
			Proximity
		}
	}
}
