using System;
using Endless.Assets;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020000A4 RID: 164
	public abstract class UIAssetView<T> : UIGameObject where T : Asset
	{
		// Token: 0x17000032 RID: 50
		// (get) Token: 0x06000298 RID: 664 RVA: 0x00011A82 File Offset: 0x0000FC82
		// (set) Token: 0x06000299 RID: 665 RVA: 0x00011A8A File Offset: 0x0000FC8A
		private protected UIAssetModelHandler<T> ModelHandler { protected get; private set; }

		// Token: 0x17000033 RID: 51
		// (get) Token: 0x0600029A RID: 666 RVA: 0x00011A93 File Offset: 0x0000FC93
		// (set) Token: 0x0600029B RID: 667 RVA: 0x00011A9B File Offset: 0x0000FC9B
		protected bool VerboseLogging { get; set; }

		// Token: 0x0600029C RID: 668 RVA: 0x00011AA4 File Offset: 0x0000FCA4
		protected void Start()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Start", this);
			}
			this.ModelHandler.OnSet.AddListener(new UnityAction<T>(this.View));
			if (this.ModelHandler.Model != null)
			{
				this.View(this.ModelHandler.Model);
			}
		}

		// Token: 0x0600029D RID: 669 RVA: 0x00011B04 File Offset: 0x0000FD04
		private void OnDisable()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("OnDisable", this);
			}
			this.Clear();
		}

		// Token: 0x0600029E RID: 670
		public abstract void View(T model);

		// Token: 0x0600029F RID: 671
		public abstract void Clear();
	}
}
