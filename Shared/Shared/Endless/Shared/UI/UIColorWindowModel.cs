using System;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x0200027E RID: 638
	public class UIColorWindowModel
	{
		// Token: 0x17000301 RID: 769
		// (get) Token: 0x06000FEF RID: 4079 RVA: 0x000444F9 File Offset: 0x000426F9
		// (set) Token: 0x06000FF0 RID: 4080 RVA: 0x00044501 File Offset: 0x00042701
		public Color Color { get; private set; }

		// Token: 0x17000302 RID: 770
		// (get) Token: 0x06000FF1 RID: 4081 RVA: 0x0004450A File Offset: 0x0004270A
		// (set) Token: 0x06000FF2 RID: 4082 RVA: 0x00044512 File Offset: 0x00042712
		public UIBaseColorView.Styles Style { get; private set; }

		// Token: 0x17000303 RID: 771
		// (get) Token: 0x06000FF3 RID: 4083 RVA: 0x0004451B File Offset: 0x0004271B
		// (set) Token: 0x06000FF4 RID: 4084 RVA: 0x00044523 File Offset: 0x00042723
		public Action<Color> OnConfirm { get; private set; }

		// Token: 0x17000304 RID: 772
		// (get) Token: 0x06000FF5 RID: 4085 RVA: 0x0004452C File Offset: 0x0004272C
		// (set) Token: 0x06000FF6 RID: 4086 RVA: 0x00044534 File Offset: 0x00042734
		public bool Interactable { get; private set; }

		// Token: 0x06000FF7 RID: 4087 RVA: 0x0004453D File Offset: 0x0004273D
		public UIColorWindowModel(Color color, UIBaseColorView.Styles style, Action<Color> onConfirm, bool interactable)
		{
			this.Color = color;
			this.Style = style;
			this.OnConfirm = onConfirm;
			this.Interactable = interactable;
		}
	}
}
