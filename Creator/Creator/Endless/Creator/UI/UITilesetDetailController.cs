using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000255 RID: 597
	public class UITilesetDetailController : UIGameObject, IBackable, IUIDetailControllable
	{
		// Token: 0x1700013A RID: 314
		// (get) Token: 0x060009BF RID: 2495 RVA: 0x0002D36C File Offset: 0x0002B56C
		public UnityEvent OnHide { get; } = new UnityEvent();

		// Token: 0x060009C0 RID: 2496 RVA: 0x0002D374 File Offset: 0x0002B574
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.hideButton.onClick.AddListener(new UnityAction(this.Hide));
			this.paintingTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<PaintingTool>();
		}

		// Token: 0x060009C1 RID: 2497 RVA: 0x0002D3C5 File Offset: 0x0002B5C5
		private void OnEnable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			MonoBehaviourSingleton<BackManager>.Instance.ClaimContext(this);
		}

		// Token: 0x060009C2 RID: 2498 RVA: 0x0002D3EA File Offset: 0x0002B5EA
		private void OnDisable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisable", Array.Empty<object>());
			}
			if (MonoBehaviourSingleton<BackManager>.Instance.HasContext(this))
			{
				MonoBehaviourSingleton<BackManager>.Instance.UnclaimContext(this);
			}
		}

		// Token: 0x060009C3 RID: 2499 RVA: 0x0002D41C File Offset: 0x0002B61C
		public void OnBack()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnBack", Array.Empty<object>());
			}
			MonoBehaviourSingleton<BackManager>.Instance.UnclaimContext(this);
			this.Hide();
		}

		// Token: 0x060009C4 RID: 2500 RVA: 0x0002D448 File Offset: 0x0002B648
		private void Hide()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Hide", Array.Empty<object>());
			}
			this.displayAndHideHandler.Hide();
			if (!MobileUtility.IsMobile)
			{
				this.paintingTool.SetActiveTilesetIndex(PaintingTool.NoSelection);
			}
			this.OnHide.Invoke();
		}

		// Token: 0x040007F9 RID: 2041
		[SerializeField]
		private UITilesetDetailView view;

		// Token: 0x040007FA RID: 2042
		[SerializeField]
		private UIDisplayAndHideHandler displayAndHideHandler;

		// Token: 0x040007FB RID: 2043
		[SerializeField]
		private UIButton hideButton;

		// Token: 0x040007FC RID: 2044
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040007FD RID: 2045
		private PaintingTool paintingTool;
	}
}
