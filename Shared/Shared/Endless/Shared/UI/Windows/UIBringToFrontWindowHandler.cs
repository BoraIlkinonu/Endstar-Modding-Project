using System;
using Endless.Shared.Debugging;
using Endless.Shared.Validation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Endless.Shared.UI.Windows
{
	// Token: 0x0200028B RID: 651
	public class UIBringToFrontWindowHandler : Selectable, IValidatable
	{
		// Token: 0x06001053 RID: 4179 RVA: 0x00045836 File Offset: 0x00043A36
		public void Validate()
		{
			if (this.window == null)
			{
				DebugUtility.LogError("The window field is null!", this);
				return;
			}
			if (this.window.Interface == null)
			{
				DebugUtility.LogError("The window's Interface field is null!", this);
			}
		}

		// Token: 0x06001054 RID: 4180 RVA: 0x00045864 File Offset: 0x00043A64
		public override void OnSelect(BaseEventData eventData)
		{
			base.OnSelect(eventData);
			MonoBehaviourSingleton<UINewWindowManager>.Instance.BringToFront(this.window.Interface);
		}

		// Token: 0x04000A5B RID: 2651
		[SerializeField]
		private InterfaceReference<IUIWindow> window;
	}
}
