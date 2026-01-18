using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Gameplay.UI
{
	// Token: 0x02000396 RID: 918
	public class UISimpleText : UIBaseAnchor
	{
		// Token: 0x0600175F RID: 5983 RVA: 0x0006CF9C File Offset: 0x0006B19C
		public static UISimpleText CreateInstance(UISimpleText prefab, Transform target, RectTransform container, string text, Vector3? offset = null)
		{
			UISimpleText uisimpleText = UIBaseAnchor.CreateAndInitialize<UISimpleText>(prefab, target, container, offset);
			uisimpleText.SetText(text);
			return uisimpleText;
		}

		// Token: 0x06001760 RID: 5984 RVA: 0x0006CFAF File Offset: 0x0006B1AF
		public void SetText(string text)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetText", new object[] { text });
			}
			this.simpleText.text = text;
		}

		// Token: 0x06001761 RID: 5985 RVA: 0x0006CFDA File Offset: 0x0006B1DA
		public void UpdateText(string text)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdateText", new object[] { text });
			}
			this.SetText(text);
		}

		// Token: 0x040012CC RID: 4812
		[SerializeField]
		private TextMeshProUGUI simpleText;
	}
}
