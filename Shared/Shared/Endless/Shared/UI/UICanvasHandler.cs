using System;
using Endless.Shared.Debugging;
using Endless.Shared.Validation;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x02000112 RID: 274
	public class UICanvasHandler : UIGameObject, IValidatable
	{
		// Token: 0x17000114 RID: 276
		// (get) Token: 0x06000697 RID: 1687 RVA: 0x0001C15C File Offset: 0x0001A35C
		public bool Enabled
		{
			get
			{
				for (int i = 0; i < this.canvases.Length; i++)
				{
					if (this.canvases[i].enabled)
					{
						return true;
					}
				}
				for (int j = 0; j < this.graphicRaycasters.Length; j++)
				{
					if (this.graphicRaycasters[j].enabled)
					{
						return true;
					}
				}
				return false;
			}
		}

		// Token: 0x06000698 RID: 1688 RVA: 0x0001C1B2 File Offset: 0x0001A3B2
		[ContextMenu("Validate")]
		public void Validate()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Validate", Array.Empty<object>());
			}
			DebugUtility.DebugHasNullItem<Canvas>(this.canvases, "canvases", this);
			DebugUtility.DebugHasNullItem<GraphicRaycaster>(this.graphicRaycasters, "graphicRaycasters", this);
		}

		// Token: 0x06000699 RID: 1689 RVA: 0x0001C1F0 File Offset: 0x0001A3F0
		public void Enable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Enable", Array.Empty<object>());
			}
			this.Set(true);
		}

		// Token: 0x0600069A RID: 1690 RVA: 0x0001C211 File Offset: 0x0001A411
		public void Disable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Disable", Array.Empty<object>());
			}
			this.Set(false);
		}

		// Token: 0x0600069B RID: 1691 RVA: 0x0001C232 File Offset: 0x0001A432
		public void Toggle()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Toggle", Array.Empty<object>());
			}
			this.Set(!this.Enabled);
		}

		// Token: 0x0600069C RID: 1692 RVA: 0x0001C25C File Offset: 0x0001A45C
		public void Set(bool enabled)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Set", new object[] { enabled });
			}
			foreach (Canvas canvas in this.canvases)
			{
				if (canvas.enabled != enabled)
				{
					canvas.enabled = enabled;
				}
			}
			foreach (GraphicRaycaster graphicRaycaster in this.graphicRaycasters)
			{
				if (graphicRaycaster.enabled != enabled)
				{
					graphicRaycaster.enabled = enabled;
				}
			}
		}

		// Token: 0x0600069D RID: 1693 RVA: 0x0001C2E0 File Offset: 0x0001A4E0
		public void SetCanvasSortingOrder(int sortingOrder)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetCanvasSortingOrder", new object[] { sortingOrder });
			}
			foreach (Canvas canvas in this.canvases)
			{
				canvas.overrideSorting = true;
				canvas.sortingOrder = sortingOrder;
			}
		}

		// Token: 0x040003D3 RID: 979
		[SerializeField]
		private Canvas[] canvases = Array.Empty<Canvas>();

		// Token: 0x040003D4 RID: 980
		[SerializeField]
		private GraphicRaycaster[] graphicRaycasters = Array.Empty<GraphicRaycaster>();

		// Token: 0x040003D5 RID: 981
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
