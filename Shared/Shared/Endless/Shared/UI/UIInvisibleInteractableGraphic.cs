using System;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x0200014B RID: 331
	[RequireComponent(typeof(CanvasRenderer))]
	public class UIInvisibleInteractableGraphic : Graphic
	{
		// Token: 0x06000816 RID: 2070 RVA: 0x000050D2 File Offset: 0x000032D2
		public override bool Raycast(Vector2 sp, Camera eventCamera)
		{
			return true;
		}

		// Token: 0x06000817 RID: 2071 RVA: 0x00021FCE File Offset: 0x000201CE
		protected override void OnPopulateMesh(VertexHelper vertexHelper)
		{
			vertexHelper.Clear();
		}
	}
}
