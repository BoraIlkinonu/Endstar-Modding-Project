using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x02000127 RID: 295
	public class UIDeviceTypeSpriteHandler : UIGameObject
	{
		// Token: 0x0600073B RID: 1851 RVA: 0x0001E74C File Offset: 0x0001C94C
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.image.sprite = this.dictionary.GetActiveDeviceTypeValue();
		}

		// Token: 0x04000438 RID: 1080
		[SerializeField]
		private UIDeviceTypeSpriteDictionary dictionary;

		// Token: 0x04000439 RID: 1081
		[SerializeField]
		private Image image;

		// Token: 0x0400043A RID: 1082
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
