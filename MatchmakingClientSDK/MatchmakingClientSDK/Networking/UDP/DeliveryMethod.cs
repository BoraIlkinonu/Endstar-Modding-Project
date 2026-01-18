using System;

namespace Networking.UDP
{
	// Token: 0x0200001C RID: 28
	public enum DeliveryMethod : byte
	{
		// Token: 0x04000055 RID: 85
		Unreliable = 4,
		// Token: 0x04000056 RID: 86
		ReliableUnordered = 0,
		// Token: 0x04000057 RID: 87
		Sequenced,
		// Token: 0x04000058 RID: 88
		ReliableOrdered,
		// Token: 0x04000059 RID: 89
		ReliableSequenced
	}
}
