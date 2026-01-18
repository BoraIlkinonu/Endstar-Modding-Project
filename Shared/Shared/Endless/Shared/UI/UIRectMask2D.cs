using System;
using Endless.Shared.Debugging;
using Endless.Shared.Validation;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x020001C5 RID: 453
	public class UIRectMask2D : RectMask2D, IValidatable
	{
		// Token: 0x06000B50 RID: 2896 RVA: 0x00030F00 File Offset: 0x0002F100
		[ContextMenu("Validate")]
		public void Validate()
		{
			foreach (MaskableGraphic maskableGraphic in base.GetComponentsInChildren<MaskableGraphic>())
			{
				if (!maskableGraphic.maskable)
				{
					DebugUtility.LogWarning(maskableGraphic.name + " under " + base.transform.root.name + " is within a RectMask2D but has maskable set to false!", base.transform.root);
				}
			}
		}
	}
}
