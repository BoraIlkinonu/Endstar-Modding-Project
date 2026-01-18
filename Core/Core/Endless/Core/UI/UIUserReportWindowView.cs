using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Core.UI
{
	// Token: 0x02000075 RID: 117
	[RequireComponent(typeof(UIDisplayAndHideHandler))]
	public class UIUserReportWindowView : UIMonoBehaviourSingleton<UIUserReportWindowView>, IBackable
	{
		// Token: 0x1700003A RID: 58
		// (get) Token: 0x06000222 RID: 546 RVA: 0x0000C1E0 File Offset: 0x0000A3E0
		public bool IsDisplaying
		{
			get
			{
				return this.displayAndHideHandler.IsDisplaying;
			}
		}

		// Token: 0x06000223 RID: 547 RVA: 0x0000C1ED File Offset: 0x0000A3ED
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			base.TryGetComponent<UIDisplayAndHideHandler>(out this.displayAndHideHandler);
			this.displayAndHideHandler.SetToHideEnd(true);
		}

		// Token: 0x06000224 RID: 548 RVA: 0x0000C220 File Offset: 0x0000A420
		public void OnBack()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnBack", Array.Empty<object>());
			}
			this.Hide();
		}

		// Token: 0x06000225 RID: 549 RVA: 0x0000C240 File Offset: 0x0000A440
		public void Display()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.bugInputField.Clear(true);
			this.reproStepsInputField.Clear(true);
			this.expectedInputField.Clear(true);
			this.feedbackInputField.Clear(true);
			this.displayAndHideHandler.Display();
			MonoBehaviourSingleton<BackManager>.Instance.ClaimContext(this);
			Action onDisplay = UIUserReportWindowView.OnDisplay;
			if (onDisplay == null)
			{
				return;
			}
			onDisplay();
		}

		// Token: 0x06000226 RID: 550 RVA: 0x0000C2BA File Offset: 0x0000A4BA
		public void Hide()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Hide", Array.Empty<object>());
			}
			this.displayAndHideHandler.Hide();
			MonoBehaviourSingleton<BackManager>.Instance.UnclaimContext(this);
			Action onHide = UIUserReportWindowView.OnHide;
			if (onHide == null)
			{
				return;
			}
			onHide();
		}

		// Token: 0x04000197 RID: 407
		public static Action OnDisplay;

		// Token: 0x04000198 RID: 408
		public static Action OnHide;

		// Token: 0x04000199 RID: 409
		[SerializeField]
		private UISpriteAndStringTabGroup categoryTabs;

		// Token: 0x0400019A RID: 410
		[Header("Bug")]
		[SerializeField]
		private UIInputField bugInputField;

		// Token: 0x0400019B RID: 411
		[SerializeField]
		private UIInputField reproStepsInputField;

		// Token: 0x0400019C RID: 412
		[SerializeField]
		private UIInputField expectedInputField;

		// Token: 0x0400019D RID: 413
		[Header("Feedback")]
		[SerializeField]
		private UIInputField feedbackInputField;

		// Token: 0x0400019E RID: 414
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x0400019F RID: 415
		private UIDisplayAndHideHandler displayAndHideHandler;
	}
}
