using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003AD RID: 941
	public class UIIconDefinitionView : UIGameObject
	{
		// Token: 0x170004F0 RID: 1264
		// (get) Token: 0x06001801 RID: 6145 RVA: 0x0006F84B File Offset: 0x0006DA4B
		// (set) Token: 0x06001802 RID: 6146 RVA: 0x0006F853 File Offset: 0x0006DA53
		public IconDefinition Model { get; private set; }

		// Token: 0x06001803 RID: 6147 RVA: 0x0006F85C File Offset: 0x0006DA5C
		public void View(IconDefinition model)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { model });
			}
			this.Model = model;
			if (model == null)
			{
				return;
			}
			this.rawImage.texture = model.IconTexture;
		}

		// Token: 0x04001345 RID: 4933
		[SerializeField]
		private RawImage rawImage;

		// Token: 0x04001346 RID: 4934
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
