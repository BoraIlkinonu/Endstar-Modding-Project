using System.Text.RegularExpressions;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI;

public class UIVersionListCellView : UIBaseListCellView<string>, IUILoadingSpinnerViewCompatible
{
	[Header("UIVersionListCellView")]
	[SerializeField]
	private TextMeshProUGUI versionText;

	[SerializeField]
	private UIButton revertButton;

	[SerializeField]
	private TweenCollection selectedTweens;

	[SerializeField]
	private TweenCollection notSelectedTweens;

	private readonly Regex everythingAfterLastPeriodRegexMatch = new Regex("([^\\\\.]+$)");

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	public override void View(UIBaseListView<string> listView, int dataIndex)
	{
		base.View(listView, dataIndex);
		revertButton.interactable = base.ListModel.AddButtonIsInteractable;
		string model = base.Model;
		Match match = everythingAfterLastPeriodRegexMatch.Match(model);
		if (match.Success)
		{
			model = match.Value;
			versionText.text = model;
			OnLoadingStarted.Invoke();
			if (base.IsSelected)
			{
				selectedTweens.SetToEnd();
			}
			else
			{
				notSelectedTweens.SetToEnd();
			}
		}
		else
		{
			DebugUtility.LogWarning("Could not extract version from " + model, this);
		}
	}
}
