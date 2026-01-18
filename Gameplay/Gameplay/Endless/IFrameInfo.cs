using System;

namespace Endless
{
	// Token: 0x02000037 RID: 55
	public interface IFrameInfo
	{
		// Token: 0x1700002D RID: 45
		// (get) Token: 0x060000F5 RID: 245
		// (set) Token: 0x060000F6 RID: 246
		uint NetFrame { get; set; }

		// Token: 0x060000F7 RID: 247
		void Clear();

		// Token: 0x060000F8 RID: 248
		void Initialize();
	}
}
