using System;

namespace Endless.Networking
{
	// Token: 0x0200000F RID: 15
	[Serializable]
	public class ServerInstance
	{
		// Token: 0x06000052 RID: 82 RVA: 0x00003168 File Offset: 0x00001368
		public void Serialize(DataBuffer buffer)
		{
			buffer.WriteString(this.InstanceIp);
			buffer.WriteString(this.InstanceName);
			buffer.WriteInteger(this.InstancePort);
		}

		// Token: 0x06000053 RID: 83 RVA: 0x00003194 File Offset: 0x00001394
		public static ServerInstance Deserialize(DataBuffer buffer)
		{
			return new ServerInstance
			{
				InstanceIp = buffer.ReadString(true),
				InstanceName = buffer.ReadString(true),
				InstancePort = buffer.ReadInteger(true)
			};
		}

		// Token: 0x04000027 RID: 39
		public string InstanceIp;

		// Token: 0x04000028 RID: 40
		public string InstanceName;

		// Token: 0x04000029 RID: 41
		public int InstancePort;

		// Token: 0x0400002A RID: 42
		[NonSerialized]
		public long Latency = long.MaxValue;
	}
}
