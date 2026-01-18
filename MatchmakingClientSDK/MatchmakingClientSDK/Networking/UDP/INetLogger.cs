using System;

namespace Networking.UDP
{
	// Token: 0x02000021 RID: 33
	public interface INetLogger
	{
		// Token: 0x0600007A RID: 122
		void WriteNet(NetLogLevel level, string str, params object[] args);
	}
}
