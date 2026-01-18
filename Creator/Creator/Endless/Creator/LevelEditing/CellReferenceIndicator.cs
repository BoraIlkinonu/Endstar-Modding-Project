using System;
using UnityEngine;

namespace Endless.Creator.LevelEditing
{
	// Token: 0x02000343 RID: 835
	public class CellReferenceIndicator : MonoBehaviour
	{
		// Token: 0x06000F77 RID: 3959 RVA: 0x000478DD File Offset: 0x00045ADD
		public void SetRotationArrowEnabled(bool arrowEnabled)
		{
			this.arrow.SetActive(arrowEnabled);
		}

		// Token: 0x04000CDF RID: 3295
		[SerializeField]
		private GameObject arrow;
	}
}
