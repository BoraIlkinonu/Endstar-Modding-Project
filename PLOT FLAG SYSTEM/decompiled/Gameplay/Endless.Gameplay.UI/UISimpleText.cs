using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Gameplay.UI;

public class UISimpleText : UIBaseAnchor
{
	[SerializeField]
	private TextMeshProUGUI simpleText;

	public static UISimpleText CreateInstance(UISimpleText prefab, Transform target, RectTransform container, string text, Vector3? offset = null)
	{
		UISimpleText uISimpleText = UIBaseAnchor.CreateAndInitialize(prefab, target, container, offset);
		uISimpleText.SetText(text);
		return uISimpleText;
	}

	public void SetText(string text)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetText", text);
		}
		simpleText.text = text;
	}

	public void UpdateText(string text)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "UpdateText", text);
		}
		SetText(text);
	}
}
