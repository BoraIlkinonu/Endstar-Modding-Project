using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020002A9 RID: 681
	public abstract class UIItemSelectionToolPanelController<T, TItemType> : UIBaseToolPanelController<T> where T : EndlessTool
	{
		// Token: 0x17000171 RID: 369
		// (get) Token: 0x06000B66 RID: 2918 RVA: 0x00035D3E File Offset: 0x00033F3E
		// (set) Token: 0x06000B67 RID: 2919 RVA: 0x00035D46 File Offset: 0x00033F46
		private protected UIItemSelectionToolPanelView<T, TItemType> ItemSelectionToolPanelView { protected get; private set; }

		// Token: 0x06000B68 RID: 2920 RVA: 0x00035D50 File Offset: 0x00033F50
		protected override void Start()
		{
			base.Start();
			this.ItemSelectionToolPanelView = (UIItemSelectionToolPanelView<T, TItemType>)this.View;
			this.deselectButton.onClick.AddListener(new UnityAction(this.Deselect));
			this.infoButton.onClick.AddListener(new UnityAction(this.ViewInfo));
		}

		// Token: 0x06000B69 RID: 2921
		public abstract void Deselect();

		// Token: 0x06000B6A RID: 2922 RVA: 0x00035DAD File Offset: 0x00033FAD
		private void ViewInfo()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("ViewInfo", this);
			}
			this.ItemSelectionToolPanelView.Undock();
		}

		// Token: 0x040009A8 RID: 2472
		[Header("UIItemSelectionToolPanelController")]
		[SerializeField]
		private UIButton deselectButton;

		// Token: 0x040009A9 RID: 2473
		[SerializeField]
		private UIButton infoButton;
	}
}
