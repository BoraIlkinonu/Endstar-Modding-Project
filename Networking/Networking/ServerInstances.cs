using System;

namespace Endless.Networking
{
	// Token: 0x0200000E RID: 14
	[Serializable]
	public class ServerInstances
	{
		// Token: 0x0600004F RID: 79 RVA: 0x000030C4 File Offset: 0x000012C4
		public void Serialize(DataBuffer buffer)
		{
			ServerInstance[] instances = this.Instances;
			int num = ((instances != null) ? instances.Length : 0);
			buffer.WriteInteger(num);
			for (int i = 0; i < num; i++)
			{
				this.Instances[i].Serialize(buffer);
			}
		}

		// Token: 0x06000050 RID: 80 RVA: 0x0000310C File Offset: 0x0000130C
		public static ServerInstances Deserialize(DataBuffer buffer)
		{
			int num = buffer.ReadInteger(true);
			ServerInstances serverInstances = new ServerInstances
			{
				Instances = new ServerInstance[num]
			};
			for (int i = 0; i < num; i++)
			{
				serverInstances.Instances[i] = ServerInstance.Deserialize(buffer);
			}
			return serverInstances;
		}

		// Token: 0x04000026 RID: 38
		public ServerInstance[] Instances;
	}
}
