using System;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x0200023E RID: 574
	public interface IUIViewable : IClearable
	{
		// Token: 0x170002B9 RID: 697
		// (get) Token: 0x06000E97 RID: 3735
		Enum StyleEnum { get; }

		// Token: 0x170002BA RID: 698
		// (get) Token: 0x06000E98 RID: 3736
		UILayoutElement LayoutElement { get; }

		// Token: 0x170002BB RID: 699
		// (get) Token: 0x06000E99 RID: 3737 RVA: 0x0003F516 File Offset: 0x0003D716
		ILayoutElement ILayoutElement
		{
			get
			{
				return this.LayoutElement;
			}
		}

		// Token: 0x06000E9A RID: 3738
		float GetPreferredHeight(object model);

		// Token: 0x06000E9B RID: 3739
		float GetPreferredWidth(object model);

		// Token: 0x06000E9C RID: 3740
		void SetMaskable(bool maskable);

		// Token: 0x06000E9D RID: 3741
		void SetDefaultLayoutElementValueToCurrentValue();

		// Token: 0x06000E9E RID: 3742
		void ApplyDefaultLayoutElementValue();
	}
}
