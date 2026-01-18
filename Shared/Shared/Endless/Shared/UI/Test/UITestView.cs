using System;
using Endless.Shared.Debugging;
using TMPro;
using UnityEngine;

namespace Endless.Shared.UI.Test
{
	// Token: 0x020002A2 RID: 674
	public class UITestView : UIGameObject
	{
		// Token: 0x17000330 RID: 816
		// (get) Token: 0x060010A7 RID: 4263 RVA: 0x00046EAE File Offset: 0x000450AE
		// (set) Token: 0x060010A8 RID: 4264 RVA: 0x00046EB6 File Offset: 0x000450B6
		public int Model { get; private set; }

		// Token: 0x060010A9 RID: 4265 RVA: 0x00046EBF File Offset: 0x000450BF
		public void View(int model)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { model });
			}
			this.Model = model;
			this.text.text = model.ToString();
		}

		// Token: 0x04000A84 RID: 2692
		[SerializeField]
		private TextMeshProUGUI text;

		// Token: 0x04000A85 RID: 2693
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
