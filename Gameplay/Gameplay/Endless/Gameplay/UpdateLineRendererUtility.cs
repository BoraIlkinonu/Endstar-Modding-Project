using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000377 RID: 887
	public class UpdateLineRendererUtility : MonoBehaviour
	{
		// Token: 0x060016B9 RID: 5817 RVA: 0x0006AA1B File Offset: 0x00068C1B
		private void Update()
		{
			this.UpdateLineRenderers();
		}

		// Token: 0x060016BA RID: 5818 RVA: 0x0006AA24 File Offset: 0x00068C24
		public void UpdateLineRenderers()
		{
			for (int i = 0; i < this.transformTargets.Length; i++)
			{
				Transform transform = this.transformTargets[i];
				if (!(transform == null))
				{
					this.lineRenderer.SetPosition(i, transform.position);
				}
			}
		}

		// Token: 0x04001245 RID: 4677
		[SerializeField]
		private Transform[] transformTargets;

		// Token: 0x04001246 RID: 4678
		[SerializeField]
		private LineRenderer lineRenderer;
	}
}
