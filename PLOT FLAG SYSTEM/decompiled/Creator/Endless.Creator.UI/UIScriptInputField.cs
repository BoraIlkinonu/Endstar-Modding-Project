using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Endless.Creator.UI;

public class UIScriptInputField : UIInputField
{
	[Header("UIScriptInputField")]
	[SerializeField]
	private UIScriptWindowModel model;

	private bool blockNextIsValidChar;

	public override bool CanSelectNextUiOnTab => false;

	public UnityEvent OnTabInputUnityEvent { get; } = new UnityEvent();

	public UnityEvent OnNewLineInputUnityEvent { get; } = new UnityEvent();

	protected override bool IsValidChar(char character)
	{
		if (blockNextIsValidChar)
		{
			blockNextIsValidChar = false;
			return false;
		}
		bool key = EndlessInput.GetKey(Key.Tab);
		bool key2 = EndlessInput.GetKey(Key.Enter);
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "IsValidChar", character);
			DebugUtility.Log(string.Format("{0}:{1}", "AutoComplete", model.AutoComplete.Count), this);
			DebugUtility.Log(string.Format("{0}:{1}", "isTab", key), this);
			DebugUtility.Log(string.Format("{0}:{1}", "isNewLine", key2), this);
		}
		if (model.AutoComplete.Count == 0)
		{
			return base.IsValidChar(character);
		}
		if (key)
		{
			OnTabInputUnityEvent.Invoke();
			blockNextIsValidChar = true;
			return false;
		}
		if (key2)
		{
			OnNewLineInputUnityEvent.Invoke();
			blockNextIsValidChar = true;
			return false;
		}
		return base.IsValidChar(character);
	}
}
