using System;
using Endless.Shared;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x0200030B RID: 779
	[RequireComponent(typeof(UIWiringObjectInspectorView))]
	public class UIWiringObjectInspectorController : UIGameObject
	{
		// Token: 0x170001ED RID: 493
		// (get) Token: 0x06000DDA RID: 3546 RVA: 0x00042069 File Offset: 0x00040269
		private UIWiringObjectInspectorView WiringObjectInspectorView
		{
			get
			{
				if (!this.wiringObjectInspectorView)
				{
					base.TryGetComponent<UIWiringObjectInspectorView>(out this.wiringObjectInspectorView);
				}
				return this.wiringObjectInspectorView;
			}
		}

		// Token: 0x170001EE RID: 494
		// (get) Token: 0x06000DDB RID: 3547 RVA: 0x0004061F File Offset: 0x0003E81F
		private UIWiringManager WiringManager
		{
			get
			{
				return MonoBehaviourSingleton<UIWiringManager>.Instance;
			}
		}

		// Token: 0x06000DDC RID: 3548 RVA: 0x0004208B File Offset: 0x0004028B
		public void Close()
		{
			if (this.verboseLogging)
			{
				Debug.Log("Close", this);
			}
			this.WiringManager.HideWiringInspector(this.WiringObjectInspectorView, true);
		}

		// Token: 0x04000BD3 RID: 3027
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000BD4 RID: 3028
		private UIWiringObjectInspectorView wiringObjectInspectorView;
	}
}
