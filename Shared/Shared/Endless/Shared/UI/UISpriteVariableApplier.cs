using System;
using Endless.Shared.Debugging;
using Endless.Shared.SoVariables;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x0200025E RID: 606
	public class UISpriteVariableApplier : UIGameObject
	{
		// Token: 0x06000F55 RID: 3925 RVA: 0x000421E9 File Offset: 0x000403E9
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.Apply();
		}

		// Token: 0x06000F56 RID: 3926 RVA: 0x0004220C File Offset: 0x0004040C
		[ContextMenu("Apply")]
		public void Apply()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Apply", Array.Empty<object>());
			}
			if (this.image.sprite == this.spriteVariable.Value)
			{
				return;
			}
			this.image.sprite = this.spriteVariable.Value;
		}

		// Token: 0x040009CB RID: 2507
		[SerializeField]
		private SpriteVariable spriteVariable;

		// Token: 0x040009CC RID: 2508
		[SerializeField]
		private Image image;

		// Token: 0x040009CD RID: 2509
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
