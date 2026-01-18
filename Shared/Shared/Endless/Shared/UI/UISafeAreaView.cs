using System;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x0200024E RID: 590
	public class UISafeAreaView : UIGameObject
	{
		// Token: 0x06000EF6 RID: 3830 RVA: 0x000406F4 File Offset: 0x0003E8F4
		public void FitToSafeArea()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "FitToSafeArea", Array.Empty<object>());
			}
			Rect safeArea = Screen.safeArea;
			Vector2 position = safeArea.position;
			Vector2 vector = safeArea.position + safeArea.size;
			Vector2 pivot = base.RectTransform.pivot;
			position.x /= (float)Screen.width;
			position.y /= (float)Screen.height;
			vector.x /= (float)Screen.width;
			vector.y /= (float)Screen.height;
			base.RectTransform.anchorMin = position;
			base.RectTransform.anchorMax = vector;
			base.RectTransform.pivot = pivot;
			base.RectTransform.offsetMin = (base.RectTransform.offsetMax = Vector2.zero);
		}

		// Token: 0x04000970 RID: 2416
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
