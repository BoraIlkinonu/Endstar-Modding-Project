using System;

namespace Endless.Shared.UI
{
	// Token: 0x0200023D RID: 573
	public interface IUITypedViewable<in TModel> : IUIViewable, IClearable
	{
		// Token: 0x06000E96 RID: 3734
		void View(TModel model);
	}
}
