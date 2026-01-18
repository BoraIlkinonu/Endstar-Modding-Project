using System;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000275 RID: 629
	[RequireComponent(typeof(RectTransform))]
	public abstract class UIGameObject : MonoBehaviour
	{
		// Token: 0x170002FB RID: 763
		// (get) Token: 0x06000FCE RID: 4046 RVA: 0x00043CC5 File Offset: 0x00041EC5
		public RectTransform RectTransform
		{
			get
			{
				if (!this.rectTransform)
				{
					base.TryGetComponent<RectTransform>(out this.rectTransform);
				}
				return this.rectTransform;
			}
		}

		// Token: 0x04000A18 RID: 2584
		private RectTransform rectTransform;
	}
}
