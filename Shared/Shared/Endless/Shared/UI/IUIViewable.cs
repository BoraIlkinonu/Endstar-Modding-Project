using System;

namespace Endless.Shared.UI
{
	// Token: 0x0200014A RID: 330
	public interface IUIViewable<in T> : IClearable
	{
		// Token: 0x06000815 RID: 2069
		void View(T model);
	}
}
