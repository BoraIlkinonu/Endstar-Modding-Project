using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.UI
{
	// Token: 0x0200041B RID: 1051
	public class UIIconDefinitionSelectorWindowController : UIDraggableWindowController
	{
		// Token: 0x06001A1F RID: 6687 RVA: 0x00078614 File Offset: 0x00076814
		protected override void Start()
		{
			base.Start();
			this.selector.OnSelectedUnityEvent.AddListener(new UnityAction<IconDefinition>(this.SetSelection));
		}

		// Token: 0x06001A20 RID: 6688 RVA: 0x00078638 File Offset: 0x00076838
		private void SetSelection(IconDefinition item)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetSelection", new object[] { item });
			}
			this.view.OnSelect(item);
			this.Close();
		}

		// Token: 0x040014E2 RID: 5346
		[Header("UIIconDefinitionSelectorWindowController")]
		[SerializeField]
		private UIIconDefinitionSelectorWindowView view;

		// Token: 0x040014E3 RID: 5347
		[SerializeField]
		private UIIconDefinitionSelector selector;
	}
}
