using System;
using UnityEngine;

namespace Endless.Shared.UI.Shapes
{
	// Token: 0x02000293 RID: 659
	public class UIPolygon : UIBaseRoundedCornerShape
	{
		// Token: 0x17000328 RID: 808
		// (get) Token: 0x0600107B RID: 4219 RVA: 0x0004619B File Offset: 0x0004439B
		protected override Vector2[] CoreVertices
		{
			get
			{
				return this.GetScaledVertices();
			}
		}

		// Token: 0x17000329 RID: 809
		// (get) Token: 0x0600107C RID: 4220 RVA: 0x000050D2 File Offset: 0x000032D2
		protected override bool SetVerticesDirtyOnRectTransformDimensionsChange
		{
			get
			{
				return true;
			}
		}

		// Token: 0x0600107D RID: 4221 RVA: 0x000461A4 File Offset: 0x000443A4
		private Vector2[] GetScaledVertices()
		{
			Rect rect = base.rectTransform.rect;
			float width = rect.width;
			float height = rect.height;
			Vector2[] array = new Vector2[this.baseVertices.Length];
			for (int i = 0; i < this.baseVertices.Length; i++)
			{
				array[i] = new Vector2(this.baseVertices[i].x * width, this.baseVertices[i].y * height);
			}
			return array;
		}

		// Token: 0x04000A6E RID: 2670
		[SerializeField]
		private Vector2[] baseVertices = new Vector2[]
		{
			new Vector2(0f, 0.5f),
			new Vector2(-0.5f, -0.5f),
			new Vector2(0.5f, -0.5f)
		};
	}
}
