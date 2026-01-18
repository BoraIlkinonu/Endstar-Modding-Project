using System;
using System.Net;
using System.Net.Sockets;

namespace Networking.UDP.Utils
{
	// Token: 0x02000044 RID: 68
	internal sealed class NtpRequest
	{
		// Token: 0x0600027D RID: 637 RVA: 0x0000BB85 File Offset: 0x00009D85
		public NtpRequest(IPEndPoint endPoint)
		{
			this._ntpEndPoint = endPoint;
		}

		// Token: 0x17000046 RID: 70
		// (get) Token: 0x0600027E RID: 638 RVA: 0x0000BB9F File Offset: 0x00009D9F
		public bool NeedToKill
		{
			get
			{
				return this._killTime >= 10000f;
			}
		}

		// Token: 0x0600027F RID: 639 RVA: 0x0000BBB4 File Offset: 0x00009DB4
		public bool Send(Socket socket, float time)
		{
			this._resendTime += time;
			this._killTime += time;
			if (this._resendTime < 1000f)
			{
				return false;
			}
			NtpPacket ntpPacket = new NtpPacket();
			bool flag;
			try
			{
				flag = socket.SendTo(ntpPacket.Bytes, 0, ntpPacket.Bytes.Length, SocketFlags.None, this._ntpEndPoint) == ntpPacket.Bytes.Length;
			}
			catch
			{
				flag = false;
			}
			return flag;
		}

		// Token: 0x04000185 RID: 389
		private const int ResendTimer = 1000;

		// Token: 0x04000186 RID: 390
		private const int KillTimer = 10000;

		// Token: 0x04000187 RID: 391
		public const int DefaultPort = 123;

		// Token: 0x04000188 RID: 392
		private readonly IPEndPoint _ntpEndPoint;

		// Token: 0x04000189 RID: 393
		private float _resendTime = 1000f;

		// Token: 0x0400018A RID: 394
		private float _killTime;
	}
}
