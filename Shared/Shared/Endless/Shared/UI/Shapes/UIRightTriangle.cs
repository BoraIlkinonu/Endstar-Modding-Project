using System;
using UnityEngine;

namespace Endless.Shared.UI.Shapes
{
	// Token: 0x02000295 RID: 661
	public class UIRightTriangle : UIBaseRoundedCornerShape
	{
		// Token: 0x1700032C RID: 812
		// (get) Token: 0x06001082 RID: 4226 RVA: 0x00046369 File Offset: 0x00044569
		// (set) Token: 0x06001083 RID: 4227 RVA: 0x00046371 File Offset: 0x00044571
		public UIRightTriangle.RightAnglePivots RightAnglePivot { get; private set; }

		// Token: 0x1700032D RID: 813
		// (get) Token: 0x06001084 RID: 4228 RVA: 0x0004637C File Offset: 0x0004457C
		protected override Vector2[] CoreVertices
		{
			get
			{
				Vector3[] array = new Vector3[4];
				base.rectTransform.GetWorldCorners(array);
				Vector3 vector = array[0];
				Vector3 vector2 = array[1];
				Vector3 vector3 = array[2];
				Vector3 vector4 = array[3];
				Vector2 vector5 = Vector2.zero;
				Vector2 vector6 = Vector2.zero;
				Vector2 vector7 = Vector2.zero;
				switch (this.RightAnglePivot)
				{
				case UIRightTriangle.RightAnglePivots.TopLeft:
					vector5 = base.rectTransform.InverseTransformPoint(vector2);
					vector6 = base.rectTransform.InverseTransformPoint(vector3);
					vector7 = base.rectTransform.InverseTransformPoint(vector);
					break;
				case UIRightTriangle.RightAnglePivots.TopRight:
					vector5 = base.rectTransform.InverseTransformPoint(vector3);
					vector6 = base.rectTransform.InverseTransformPoint(vector4);
					vector7 = base.rectTransform.InverseTransformPoint(vector2);
					break;
				case UIRightTriangle.RightAnglePivots.BottomLeft:
					vector5 = base.rectTransform.InverseTransformPoint(vector);
					vector6 = base.rectTransform.InverseTransformPoint(vector2);
					vector7 = base.rectTransform.InverseTransformPoint(vector4);
					break;
				case UIRightTriangle.RightAnglePivots.BottomRight:
					vector5 = base.rectTransform.InverseTransformPoint(vector4);
					vector6 = base.rectTransform.InverseTransformPoint(vector);
					vector7 = base.rectTransform.InverseTransformPoint(vector3);
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
				return new Vector2[] { vector5, vector6, vector7 };
			}
		}

		// Token: 0x1700032E RID: 814
		// (get) Token: 0x06001085 RID: 4229 RVA: 0x000050D2 File Offset: 0x000032D2
		protected override bool SetVerticesDirtyOnRectTransformDimensionsChange
		{
			get
			{
				return true;
			}
		}

		// Token: 0x02000296 RID: 662
		public enum RightAnglePivots
		{
			// Token: 0x04000A71 RID: 2673
			TopLeft,
			// Token: 0x04000A72 RID: 2674
			TopRight,
			// Token: 0x04000A73 RID: 2675
			BottomLeft,
			// Token: 0x04000A74 RID: 2676
			BottomRight
		}
	}
}
