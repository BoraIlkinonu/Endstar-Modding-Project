using System;

namespace Runtime.Shared.Matchmaking
{
	// Token: 0x02000011 RID: 17
	public interface IMatchAllocator
	{
		// Token: 0x14000002 RID: 2
		// (add) Token: 0x06000097 RID: 151
		// (remove) Token: 0x06000098 RID: 152
		event Action OnMatchAllocated;

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x06000099 RID: 153
		object LastAllocation { get; }

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x0600009A RID: 154
		string PublicIp { get; }

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x0600009B RID: 155
		string LocalIp { get; }

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x0600009C RID: 156
		string Name { get; }

		// Token: 0x1700001C RID: 28
		// (get) Token: 0x0600009D RID: 157
		int Port { get; }

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x0600009E RID: 158
		string Key { get; }

		// Token: 0x0600009F RID: 159
		void Allocate();

		// Token: 0x060000A0 RID: 160
		void Reset();
	}
}
