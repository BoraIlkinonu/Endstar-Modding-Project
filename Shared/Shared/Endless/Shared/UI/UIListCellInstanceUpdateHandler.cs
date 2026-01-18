using System;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x0200017D RID: 381
	public class UIListCellInstanceUpdateHandler : UIGameObject
	{
		// Token: 0x06000974 RID: 2420 RVA: 0x0002885E File Offset: 0x00026A5E
		private void Update()
		{
			Action<RectTransform> updateWithRectTransform = UIListCellInstanceUpdateHandler.UpdateWithRectTransform;
			if (updateWithRectTransform == null)
			{
				return;
			}
			updateWithRectTransform(base.RectTransform);
		}

		// Token: 0x040005F6 RID: 1526
		public static Action<RectTransform> UpdateWithRectTransform;
	}
}
