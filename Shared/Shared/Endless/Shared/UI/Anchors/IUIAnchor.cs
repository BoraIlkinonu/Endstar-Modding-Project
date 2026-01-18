using System;
using UnityEngine;

namespace Endless.Shared.UI.Anchors
{
	// Token: 0x020002A4 RID: 676
	public interface IUIAnchor : IPoolableT
	{
		// Token: 0x17000331 RID: 817
		// (get) Token: 0x060010AC RID: 4268 RVA: 0x000050D2 File Offset: 0x000032D2
		bool IPoolableT.IsUi
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000332 RID: 818
		// (get) Token: 0x060010AD RID: 4269
		// (set) Token: 0x060010AE RID: 4270
		Transform Target { get; set; }

		// Token: 0x060010AF RID: 4271
		void UpdatePosition();

		// Token: 0x060010B0 RID: 4272
		void Close();
	}
}
