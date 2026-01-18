using System;
using Endless.Shared.Debugging;

namespace Endless.Shared.UI
{
	// Token: 0x02000124 RID: 292
	public class UIRectTransformDebugger : UIGameObject
	{
		// Token: 0x06000737 RID: 1847 RVA: 0x0001E6F8 File Offset: 0x0001C8F8
		private void OnRectTransformDimensionsChange()
		{
			DebugUtility.LogMethod(this, "OnRectTransformDimensionsChange", Array.Empty<object>());
		}
	}
}
