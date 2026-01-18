using System;
using UnityEngine;

namespace Endless.Shared.UI.Shapes
{
	// Token: 0x02000292 RID: 658
	public class UIIsoscelesTriangle : UIBaseRoundedCornerShape
	{
		// Token: 0x17000326 RID: 806
		// (get) Token: 0x06001078 RID: 4216 RVA: 0x000460F8 File Offset: 0x000442F8
		protected override Vector2[] CoreVertices
		{
			get
			{
				float width = base.rectTransform.rect.width;
				float height = base.rectTransform.rect.height;
				Vector2 vector = new Vector2(-width / 2f, -height / 2f);
				Vector2 vector2 = new Vector2(width / 2f, -height / 2f);
				Vector2 vector3 = new Vector3(0f, height / 2f);
				return new Vector2[] { vector, vector2, vector3 };
			}
		}

		// Token: 0x17000327 RID: 807
		// (get) Token: 0x06001079 RID: 4217 RVA: 0x000050D2 File Offset: 0x000032D2
		protected override bool SetVerticesDirtyOnRectTransformDimensionsChange
		{
			get
			{
				return true;
			}
		}
	}
}
