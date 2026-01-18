using System;
using Endless.Shared.Debugging;
using Endless.Shared.SoVariables;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x0200025D RID: 605
	public class UIColorVariableApplier : UIGameObject
	{
		// Token: 0x06000F52 RID: 3922 RVA: 0x0004216E File Offset: 0x0004036E
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.Apply();
		}

		// Token: 0x06000F53 RID: 3923 RVA: 0x00042190 File Offset: 0x00040390
		[ContextMenu("Apply")]
		public void Apply()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Apply", Array.Empty<object>());
			}
			if (this.graphic.color == this.colorVariable.Value)
			{
				return;
			}
			this.graphic.color = this.colorVariable.Value;
		}

		// Token: 0x040009C8 RID: 2504
		[SerializeField]
		private ColorVariable colorVariable;

		// Token: 0x040009C9 RID: 2505
		[SerializeField]
		private Graphic graphic;

		// Token: 0x040009CA RID: 2506
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
