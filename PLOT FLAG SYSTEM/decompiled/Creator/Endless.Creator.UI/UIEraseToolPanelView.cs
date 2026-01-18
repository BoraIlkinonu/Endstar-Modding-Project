using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using Endless.Shared.Validation;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIEraseToolPanelView : UIDockableToolPanelView<EraseTool>, IValidatable
{
	[Header("UIEraseToolPanelView")]
	[SerializeField]
	private TextMeshProUGUI nothingToEraseTextObject;

	[SerializeField]
	private string nothingToEraseText;

	[SerializeField]
	private UIToggle terrain;

	[SerializeField]
	private UIToggle unwiredProps;

	[SerializeField]
	private UIToggle wiredProps;

	[SerializeField]
	private TweenCollection nothingToEraseDisplayTweens;

	[SerializeField]
	private TweenCollection nothingToEraseHideTweens;

	protected override void Start()
	{
		base.Start();
		Tool.OnFunctionChange.AddListener(OnFunctionChange);
		OnFunctionChange(Tool.CurrentFunction);
		nothingToEraseTextObject.text = string.Empty;
		nothingToEraseHideTweens.SetToEnd();
		nothingToEraseHideTweens.OnAllTweenCompleted.AddListener(SetNothingToEraseTextToNothing);
	}

	[ContextMenu("Validate")]
	public void Validate()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Validate");
		}
		nothingToEraseHideTweens.ValidateForNumberOfTweens();
	}

	protected override void OnToolChange(EndlessTool activeTool)
	{
		base.OnToolChange(activeTool);
		if (!(activeTool == null) && activeTool.GetType() == Tool.GetType())
		{
			OnFunctionChange(Tool.CurrentFunction);
		}
	}

	private void OnFunctionChange(EraseToolFunction function)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnFunctionChange", function);
		}
		terrain.SetIsOn(function.HasFlag(EraseToolFunction.Terrain), suppressOnChange: true);
		unwiredProps.SetIsOn(function.HasFlag(EraseToolFunction.UnwiredProps), suppressOnChange: true);
		wiredProps.SetIsOn(function.HasFlag(EraseToolFunction.WiredProps), suppressOnChange: true);
		if (!terrain.IsOn && !unwiredProps.IsOn && !wiredProps.IsOn)
		{
			nothingToEraseHideTweens.Cancel();
			nothingToEraseTextObject.text = nothingToEraseText;
			nothingToEraseDisplayTweens.Tween();
		}
		else
		{
			nothingToEraseDisplayTweens.Cancel();
			nothingToEraseHideTweens.Tween();
		}
	}

	private void SetNothingToEraseTextToNothing()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetNothingToEraseTextToNothing");
		}
		nothingToEraseTextObject.text = string.Empty;
	}
}
