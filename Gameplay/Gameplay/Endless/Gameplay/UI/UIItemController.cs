using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003B4 RID: 948
	public class UIItemController : UIGameObject
	{
		// Token: 0x170004FE RID: 1278
		// (get) Token: 0x06001839 RID: 6201 RVA: 0x00070A5D File Offset: 0x0006EC5D
		public UnityEvent OnSelectUnityEvent { get; } = new UnityEvent();

		// Token: 0x0600183A RID: 6202 RVA: 0x00070A65 File Offset: 0x0006EC65
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.button.PointerUpUnityEvent.AddListener(new UnityAction(this.OnPointerUp));
		}

		// Token: 0x0600183B RID: 6203 RVA: 0x00070A9B File Offset: 0x0006EC9B
		private void OnPointerUp()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnPointerUp", Array.Empty<object>());
			}
			if (this.view.IsEmpty || this.view.HasDragInstance)
			{
				return;
			}
			this.OnSelectUnityEvent.Invoke();
		}

		// Token: 0x04001373 RID: 4979
		[SerializeField]
		private UIItemView view;

		// Token: 0x04001374 RID: 4980
		[SerializeField]
		private UIButton button;

		// Token: 0x04001375 RID: 4981
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
