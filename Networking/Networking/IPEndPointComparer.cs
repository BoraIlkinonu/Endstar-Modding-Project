using System;
using System.Collections.Generic;
using System.Net;

namespace Endless.Networking
{
	// Token: 0x0200000B RID: 11
	internal class IPEndPointComparer : IComparer<IPEndPoint>
	{
		// Token: 0x06000040 RID: 64 RVA: 0x00002FF0 File Offset: 0x000011F0
		public int Compare(IPEndPoint x, IPEndPoint y)
		{
			int num = string.Compare(x.Address.ToString(), y.Address.ToString(), StringComparison.Ordinal);
			bool flag = num != 0;
			int num2;
			if (flag)
			{
				num2 = num;
			}
			else
			{
				num2 = y.Port - x.Port;
			}
			return num2;
		}

		// Token: 0x0400001F RID: 31
		public static readonly IPEndPointComparer Instance = new IPEndPointComparer();
	}
}
