using System;
using System.Collections.Generic;
using Endless.Shared.Debugging;
using Endless.Shared.Validation;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x0200027D RID: 637
	public class UIRectTransformValidator : UIGameObject, IValidatable
	{
		// Token: 0x06000FED RID: 4077 RVA: 0x000443F4 File Offset: 0x000425F4
		[ContextMenu("Validate")]
		public void Validate()
		{
			RectTransformValue rectTransformValue = new RectTransformValue(base.RectTransform);
			List<string> list = new List<string>();
			if (rectTransformValue.AnchorMin != this.rectTransformValue.AnchorMin)
			{
				list.Add("AnchorMin");
			}
			if (rectTransformValue.AnchorMax != this.rectTransformValue.AnchorMax)
			{
				list.Add("AnchorMax");
			}
			if (rectTransformValue.SizeDelta != this.rectTransformValue.SizeDelta)
			{
				list.Add("SizeDelta");
			}
			if (rectTransformValue.Pivot != this.rectTransformValue.Pivot)
			{
				list.Add("Pivot");
			}
			if (list.Count > 0)
			{
				DebugUtility.LogError(string.Concat(new string[]
				{
					base.gameObject.name,
					"'s ",
					StringUtility.CommaSeparate(list),
					" in <b>",
					base.transform.root.name,
					"</b> RectTransform is not its intended value!"
				}), this);
			}
		}

		// Token: 0x04000A27 RID: 2599
		[SerializeField]
		private RectTransformValue rectTransformValue;
	}
}
