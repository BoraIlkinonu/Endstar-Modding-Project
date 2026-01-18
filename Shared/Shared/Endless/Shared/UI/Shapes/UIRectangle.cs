using System;
using UnityEngine;

namespace Endless.Shared.UI.Shapes
{
	// Token: 0x02000294 RID: 660
	public class UIRectangle : UIBaseRoundedCornerShape
	{
		// Token: 0x1700032A RID: 810
		// (get) Token: 0x0600107F RID: 4223 RVA: 0x0004628C File Offset: 0x0004448C
		protected override Vector2[] CoreVertices
		{
			get
			{
				Vector3[] array = new Vector3[4];
				base.rectTransform.GetWorldCorners(array);
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = base.rectTransform.InverseTransformPoint(array[i]);
				}
				return new Vector2[]
				{
					new Vector2(array[0].x, array[0].y),
					new Vector2(array[1].x, array[1].y),
					new Vector2(array[2].x, array[2].y),
					new Vector2(array[3].x, array[3].y)
				};
			}
		}

		// Token: 0x1700032B RID: 811
		// (get) Token: 0x06001080 RID: 4224 RVA: 0x000050D2 File Offset: 0x000032D2
		protected override bool SetVerticesDirtyOnRectTransformDimensionsChange
		{
			get
			{
				return true;
			}
		}
	}
}
