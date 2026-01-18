using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x02000302 RID: 770
	[RequireComponent(typeof(UIWireView))]
	[RequireComponent(typeof(UILineRenderer))]
	public class UIWireController : UIGameObject
	{
		// Token: 0x170001D0 RID: 464
		// (get) Token: 0x06000D6B RID: 3435 RVA: 0x000405FD File Offset: 0x0003E7FD
		private UIWireView WireView
		{
			get
			{
				if (!this.wireView)
				{
					base.TryGetComponent<UIWireView>(out this.wireView);
				}
				return this.wireView;
			}
		}

		// Token: 0x170001D1 RID: 465
		// (get) Token: 0x06000D6C RID: 3436 RVA: 0x0004061F File Offset: 0x0003E81F
		private UIWiringManager WiringManager
		{
			get
			{
				return MonoBehaviourSingleton<UIWiringManager>.Instance;
			}
		}

		// Token: 0x06000D6D RID: 3437 RVA: 0x00040626 File Offset: 0x0003E826
		public void OnSelect()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSelect", Array.Empty<object>());
			}
			this.WiringManager.WireEditorController.EditWire(this.WireView);
		}

		// Token: 0x04000B92 RID: 2962
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000B93 RID: 2963
		private UIWireView wireView;
	}
}
