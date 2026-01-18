using System;
using UnityEngine;

namespace Endless.Shared.UI.Shapes
{
	// Token: 0x02000291 RID: 657
	public class UICircle : UIBaseShape
	{
		// Token: 0x17000324 RID: 804
		// (get) Token: 0x06001075 RID: 4213 RVA: 0x00046050 File Offset: 0x00044250
		protected override Vector2[] CoreVertices
		{
			get
			{
				Rect rect = base.rectTransform.rect;
				float num = Mathf.Min(rect.width, rect.height) / 2f;
				Vector2[] array = new Vector2[this.segments];
				float num2 = 360f / (float)this.segments;
				for (int i = 0; i < this.segments; i++)
				{
					float num3 = 0.017453292f * (float)i * num2;
					Vector2 vector = new Vector2(Mathf.Cos(num3), Mathf.Sin(num3)) * num;
					array[i] = vector;
				}
				return array;
			}
		}

		// Token: 0x17000325 RID: 805
		// (get) Token: 0x06001076 RID: 4214 RVA: 0x000050D2 File Offset: 0x000032D2
		protected override bool SetVerticesDirtyOnRectTransformDimensionsChange
		{
			get
			{
				return true;
			}
		}

		// Token: 0x04000A6D RID: 2669
		[Header("Circle")]
		[SerializeField]
		[Range(3f, 100f)]
		private int segments = 50;
	}
}
