using System;
using UnityEngine;

namespace Endless.Shared.UI.Shapes
{
	// Token: 0x0200028E RID: 654
	public abstract class UIBaseRoundedCornerShape : UIBaseShape
	{
		// Token: 0x17000320 RID: 800
		// (get) Token: 0x0600106B RID: 4203 RVA: 0x00045CC1 File Offset: 0x00043EC1
		protected override Vector2[] FinalVertices
		{
			get
			{
				return UIShapeUtility.GetVerticesForShapeWithRoundedCorners(this.CoreVertices, this.cornerRadius, this.cornerSegments);
			}
		}

		// Token: 0x04000A63 RID: 2659
		[Header("Corner")]
		[Min(0f)]
		[Tooltip("How much to round the corners.")]
		[SerializeField]
		private float cornerRadius;

		// Token: 0x04000A64 RID: 2660
		[Range(0f, 10f)]
		[Tooltip("How smooth the rounded corner should be (more segments = smoother arc).")]
		[SerializeField]
		private int cornerSegments;
	}
}
