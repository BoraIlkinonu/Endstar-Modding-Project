using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x0200013F RID: 319
	[RequireComponent(typeof(CanvasRenderer))]
	public class UIGradient : MaskableGraphic
	{
		// Token: 0x060007ED RID: 2029 RVA: 0x000217B1 File Offset: 0x0001F9B1
		public void SetDirection(UIGradient.Directions newValue)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetDirection", new object[] { newValue });
			}
			this.direction = newValue;
		}

		// Token: 0x060007EE RID: 2030 RVA: 0x000217DC File Offset: 0x0001F9DC
		public void SetStart(Color newValue)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetStart", new object[] { newValue });
			}
			this.start = newValue;
		}

		// Token: 0x060007EF RID: 2031 RVA: 0x00021807 File Offset: 0x0001FA07
		public void SetEnd(Color newValue)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetEnd", new object[] { newValue });
			}
			this.end = newValue;
		}

		// Token: 0x060007F0 RID: 2032 RVA: 0x00021832 File Offset: 0x0001FA32
		public void SetFlip(bool newValue)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetFlip", new object[] { newValue });
			}
			this.flip = newValue;
		}

		// Token: 0x060007F1 RID: 2033 RVA: 0x00021860 File Offset: 0x0001FA60
		protected override void OnPopulateMesh(VertexHelper vertexHelper)
		{
			if (this.superVerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnPopulateMesh", new object[] { vertexHelper });
			}
			base.OnPopulateMesh(vertexHelper);
			int currentVertCount = vertexHelper.currentVertCount;
			if (currentVertCount == 0)
			{
				return;
			}
			UIVertex uivertex = default(UIVertex);
			for (int i = 0; i < currentVertCount; i++)
			{
				vertexHelper.PopulateUIVertex(ref uivertex, i);
				float num = ((this.direction == UIGradient.Directions.Horizontal) ? uivertex.position.x : (-uivertex.position.y));
				num *= (this.flip ? (-1f) : 1f);
				uivertex.color = Color.Lerp(this.start, this.end, num);
				vertexHelper.SetUIVertex(uivertex, i);
			}
		}

		// Token: 0x040004C3 RID: 1219
		[SerializeField]
		private UIGradient.Directions direction;

		// Token: 0x040004C4 RID: 1220
		[SerializeField]
		private Color start = new Color(1f, 1f, 1f, 1f);

		// Token: 0x040004C5 RID: 1221
		[SerializeField]
		private Color end = new Color(0f, 0f, 0f, 1f);

		// Token: 0x040004C6 RID: 1222
		[SerializeField]
		private bool flip;

		// Token: 0x040004C7 RID: 1223
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040004C8 RID: 1224
		[SerializeField]
		private bool superVerboseLogging;

		// Token: 0x02000140 RID: 320
		public enum Directions
		{
			// Token: 0x040004CA RID: 1226
			Horizontal,
			// Token: 0x040004CB RID: 1227
			Vertical
		}
	}
}
