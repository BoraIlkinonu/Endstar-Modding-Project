using Endless.Assets;
using Endless.Shared.Debugging;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIAssetReadOnlyView<T> : UIAssetView<T> where T : Asset
{
	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[SerializeField]
	private TextMeshProUGUI nameText;

	public override void View(T model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "View", "model", model), this);
		}
		nameText.text = model.Name;
		descriptionText.text = model.Description;
	}

	public override void Clear()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("Clear", this);
		}
		nameText.text = string.Empty;
		descriptionText.text = string.Empty;
	}
}
