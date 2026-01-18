using System;
using Networking.UDP.Utils;

namespace Networking.UDP
{
	// Token: 0x02000012 RID: 18
	public interface INtpEventListener
	{
		// Token: 0x0600002A RID: 42
		void OnNtpResponse(NtpPacket packet);
	}
}
