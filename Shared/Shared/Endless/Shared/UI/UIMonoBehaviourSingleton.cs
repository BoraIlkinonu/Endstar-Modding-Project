using System;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000241 RID: 577
	[RequireComponent(typeof(RectTransform))]
	public abstract class UIMonoBehaviourSingleton<T> : MonoBehaviourSingleton<T> where T : MonoBehaviour
	{
		// Token: 0x170002C2 RID: 706
		// (get) Token: 0x06000EB2 RID: 3762 RVA: 0x0003F76F File Offset: 0x0003D96F
		public RectTransform RectTransform
		{
			get
			{
				if (this.rectTransformGotten)
				{
					return this.rectTransform;
				}
				base.TryGetComponent<RectTransform>(out this.rectTransform);
				this.rectTransformGotten = true;
				return this.rectTransform;
			}
		}

		// Token: 0x0400093E RID: 2366
		private RectTransform rectTransform;

		// Token: 0x0400093F RID: 2367
		private bool rectTransformGotten;
	}
}
