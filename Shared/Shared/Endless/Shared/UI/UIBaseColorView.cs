using System;
using Endless.Shared.Debugging;
using Runtime.Shared;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x020001E5 RID: 485
	public abstract class UIBaseColorView : UIBaseView<Color, UIBaseColorView.Styles>, IUIInteractable
	{
		// Token: 0x17000235 RID: 565
		// (get) Token: 0x06000C07 RID: 3079 RVA: 0x00034355 File Offset: 0x00032555
		// (set) Token: 0x06000C08 RID: 3080 RVA: 0x0003435D File Offset: 0x0003255D
		public Color CachedModel { get; private set; }

		// Token: 0x17000236 RID: 566
		// (get) Token: 0x06000C09 RID: 3081 RVA: 0x00034366 File Offset: 0x00032566
		// (set) Token: 0x06000C0A RID: 3082 RVA: 0x0003436E File Offset: 0x0003256E
		public override UIBaseColorView.Styles Style { get; protected set; }

		// Token: 0x06000C0B RID: 3083
		public abstract void SetInteractable(bool interactable);

		// Token: 0x06000C0C RID: 3084 RVA: 0x00034377 File Offset: 0x00032577
		public override void View(Color model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "View", "model", model), this);
			}
			this.CachedModel = model;
			this.ViewColorPreview(model);
		}

		// Token: 0x06000C0D RID: 3085 RVA: 0x000343AF File Offset: 0x000325AF
		public override void Clear()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("Clear", this);
			}
			this.View(Color.white);
		}

		// Token: 0x06000C0E RID: 3086
		protected abstract void ViewColorPreview(Color model);

		// Token: 0x040007C2 RID: 1986
		public Action<Color> OnColorChanged;

		// Token: 0x020001E6 RID: 486
		public enum Styles
		{
			// Token: 0x040007C6 RID: 1990
			Default,
			// Token: 0x040007C7 RID: 1991
			Hdr,
			// Token: 0x040007C8 RID: 1992
			Detail,
			// Token: 0x040007C9 RID: 1993
			DetailHdr
		}
	}
}
