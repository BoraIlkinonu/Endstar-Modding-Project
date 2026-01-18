using System;

namespace Endless.Shared
{
	// Token: 0x02000083 RID: 131
	public struct PoolPrewarmRequest
	{
		// Token: 0x060003D3 RID: 979 RVA: 0x00010ECA File Offset: 0x0000F0CA
		public PoolPrewarmRequest(IPoolable source, int count)
		{
			this.Source = source;
			this.Count = count;
		}

		// Token: 0x040001D7 RID: 471
		public readonly IPoolable Source;

		// Token: 0x040001D8 RID: 472
		public readonly int Count;
	}
}
