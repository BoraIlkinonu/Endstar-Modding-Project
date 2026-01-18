using Endless.Creator.DynamicPropCreation;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIPropCreationPromptDataModalView : UIBaseModalView
{
	[Header("UIPropCreationPromptDataModalView")]
	[SerializeField]
	private TextMeshProUGUI text;

	public override void OnDisplay(params object[] modalData)
	{
		base.OnDisplay(modalData);
		PropCreationPromptData propCreationPromptData = (PropCreationPromptData)modalData[0];
		text.text = propCreationPromptData.Message;
	}

	public override void OnBack()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnBack");
		}
		MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack();
	}
}
