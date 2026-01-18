using System;

namespace Endless.Shared.UI.Windows
{
	// Token: 0x0200028A RID: 650
	public interface IUIWindow : IPoolableT
	{
		// Token: 0x1700031D RID: 797
		// (get) Token: 0x06001051 RID: 4177 RVA: 0x000050D2 File Offset: 0x000032D2
		bool IPoolableT.IsUi
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06001052 RID: 4178
		void Close();
	}
}
