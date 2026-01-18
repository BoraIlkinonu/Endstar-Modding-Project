using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x020001CD RID: 461
	[RequireComponent(typeof(UIButton))]
	public class UIDisplayModalButton : UIGameObject
	{
		// Token: 0x06000B7A RID: 2938 RVA: 0x0003168C File Offset: 0x0002F88C
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			UIButton uibutton;
			base.TryGetComponent<UIButton>(out uibutton);
			uibutton.onClick.AddListener(new UnityAction(this.Display));
		}

		// Token: 0x06000B7B RID: 2939 RVA: 0x000316D1 File Offset: 0x0002F8D1
		private void Display()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Display", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.Display(this.modalSource, this.stackAction, Array.Empty<object>());
		}

		// Token: 0x04000758 RID: 1880
		[SerializeField]
		private UIBaseModalView modalSource;

		// Token: 0x04000759 RID: 1881
		[SerializeField]
		private UIModalManagerStackActions stackAction;

		// Token: 0x0400075A RID: 1882
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
