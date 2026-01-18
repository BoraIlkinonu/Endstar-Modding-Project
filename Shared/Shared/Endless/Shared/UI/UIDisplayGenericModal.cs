using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Serialization;

namespace Endless.Shared.UI
{
	// Token: 0x020001C6 RID: 454
	public class UIDisplayGenericModal : UIGameObject
	{
		// Token: 0x06000B52 RID: 2898 RVA: 0x00030F6C File Offset: 0x0002F16C
		public void Display()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Display", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.DisplayGenericModal(this.title, this.titleIconSprite, this.body, this.modalManagerStackAction, this.buttons);
		}

		// Token: 0x0400073A RID: 1850
		[SerializeField]
		private string title;

		// Token: 0x0400073B RID: 1851
		[SerializeField]
		private Sprite titleIconSprite;

		// Token: 0x0400073C RID: 1852
		[FormerlySerializedAs("text")]
		[SerializeField]
		private string body;

		// Token: 0x0400073D RID: 1853
		[SerializeField]
		private UIModalManagerStackActions modalManagerStackAction;

		// Token: 0x0400073E RID: 1854
		[SerializeField]
		private UIModalGenericViewAction[] buttons = new UIModalGenericViewAction[0];

		// Token: 0x0400073F RID: 1855
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
