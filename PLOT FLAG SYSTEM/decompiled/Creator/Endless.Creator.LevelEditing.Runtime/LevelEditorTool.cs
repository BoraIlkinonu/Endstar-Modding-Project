using Endless.Gameplay;
using Endless.Shared;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Creator.LevelEditing.Runtime;

public class LevelEditorTool : EndlessTool
{
	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	public override ToolType ToolType => ToolType.LevelEditor;

	public override bool PerformsLineCast => false;

	public override void HandleDeselected()
	{
		base.HandleDeselected();
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "HandleDeselected");
		}
		PlayerReferenceManager.LocalInstance.PlayerNetworkController.ToggleInput(state: true);
		MonoBehaviourSingleton<CameraController>.Instance.ToggleInput(state: true);
	}

	public override void HandleSelected()
	{
		base.HandleSelected();
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "HandleSelected");
		}
		PlayerReferenceManager.LocalInstance.PlayerNetworkController.ToggleInput(state: false);
		MonoBehaviourSingleton<CameraController>.Instance.ToggleInput(state: false);
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected internal override string __getTypeName()
	{
		return "LevelEditorTool";
	}
}
