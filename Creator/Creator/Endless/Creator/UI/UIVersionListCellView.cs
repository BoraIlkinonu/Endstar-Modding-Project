using System;
using System.Text.RegularExpressions;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000199 RID: 409
	public class UIVersionListCellView : UIBaseListCellView<string>, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x1700009B RID: 155
		// (get) Token: 0x060005F9 RID: 1529 RVA: 0x0001E935 File Offset: 0x0001CB35
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x1700009C RID: 156
		// (get) Token: 0x060005FA RID: 1530 RVA: 0x0001E93D File Offset: 0x0001CB3D
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x060005FB RID: 1531 RVA: 0x0001E948 File Offset: 0x0001CB48
		public override void View(UIBaseListView<string> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			this.revertButton.interactable = base.ListModel.AddButtonIsInteractable;
			string text = base.Model;
			Match match = this.everythingAfterLastPeriodRegexMatch.Match(text);
			if (!match.Success)
			{
				DebugUtility.LogWarning("Could not extract version from " + text, this);
				return;
			}
			text = match.Value;
			this.versionText.text = text;
			this.OnLoadingStarted.Invoke();
			if (base.IsSelected)
			{
				this.selectedTweens.SetToEnd();
				return;
			}
			this.notSelectedTweens.SetToEnd();
		}

		// Token: 0x04000532 RID: 1330
		[Header("UIVersionListCellView")]
		[SerializeField]
		private TextMeshProUGUI versionText;

		// Token: 0x04000533 RID: 1331
		[SerializeField]
		private UIButton revertButton;

		// Token: 0x04000534 RID: 1332
		[SerializeField]
		private TweenCollection selectedTweens;

		// Token: 0x04000535 RID: 1333
		[SerializeField]
		private TweenCollection notSelectedTweens;

		// Token: 0x04000536 RID: 1334
		private readonly Regex everythingAfterLastPeriodRegexMatch = new Regex("([^\\\\.]+$)");
	}
}
