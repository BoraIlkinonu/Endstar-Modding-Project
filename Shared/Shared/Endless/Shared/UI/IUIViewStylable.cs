using System;

namespace Endless.Shared.UI
{
	// Token: 0x0200023F RID: 575
	public interface IUIViewStylable<out TViewStyle> where TViewStyle : Enum
	{
		// Token: 0x170002BC RID: 700
		// (get) Token: 0x06000E9F RID: 3743
		TViewStyle Style { get; }
	}
}
