using System;
using Endless.Gameplay;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020001EB RID: 491
	public class UIInventorySpawnOptionsView : UIBaseView<InventorySpawnOptions, UIInventorySpawnOptionsView.Styles>, IUIInteractable
	{
		// Token: 0x170000E4 RID: 228
		// (get) Token: 0x0600079D RID: 1949 RVA: 0x00025AA9 File Offset: 0x00023CA9
		// (set) Token: 0x0600079E RID: 1950 RVA: 0x00025AB1 File Offset: 0x00023CB1
		public override UIInventorySpawnOptionsView.Styles Style { get; protected set; }

		// Token: 0x0600079F RID: 1951 RVA: 0x00025ABA File Offset: 0x00023CBA
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.editButton.onClick.AddListener(new UnityAction(this.OnEditButtonPressed));
		}

		// Token: 0x14000007 RID: 7
		// (add) Token: 0x060007A0 RID: 1952 RVA: 0x00025AF0 File Offset: 0x00023CF0
		// (remove) Token: 0x060007A1 RID: 1953 RVA: 0x00025B28 File Offset: 0x00023D28
		public event Action OnEditPressed;

		// Token: 0x060007A2 RID: 1954 RVA: 0x00025B5D File Offset: 0x00023D5D
		public override void View(InventorySpawnOptions model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { model });
			}
		}

		// Token: 0x060007A3 RID: 1955 RVA: 0x00025B7C File Offset: 0x00023D7C
		public override void Clear()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", Array.Empty<object>());
			}
		}

		// Token: 0x060007A4 RID: 1956 RVA: 0x00025B96 File Offset: 0x00023D96
		public void SetInteractable(bool interactable)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetInteractable", new object[] { interactable });
			}
			this.editButton.interactable = interactable;
		}

		// Token: 0x060007A5 RID: 1957 RVA: 0x00025BC6 File Offset: 0x00023DC6
		private void OnEditButtonPressed()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEditButtonPressed", Array.Empty<object>());
			}
			Action onEditPressed = this.OnEditPressed;
			if (onEditPressed == null)
			{
				return;
			}
			onEditPressed();
		}

		// Token: 0x040006D1 RID: 1745
		[SerializeField]
		private UIButton editButton;

		// Token: 0x020001EC RID: 492
		public enum Styles
		{
			// Token: 0x040006D4 RID: 1748
			Default
		}
	}
}
