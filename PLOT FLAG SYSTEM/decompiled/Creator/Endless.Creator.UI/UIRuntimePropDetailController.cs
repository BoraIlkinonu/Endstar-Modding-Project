using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI;

public class UIRuntimePropDetailController : UIGameObject, IBackable, IUIDetailControllable
{
	[SerializeField]
	private UIRuntimePropInfoDetailView view;

	[SerializeField]
	private UIDisplayAndHideHandler displayAndHideHandler;

	[SerializeField]
	private UIButton hideButton;

	[SerializeField]
	private UIButton editScriptButton;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private PropTool propTool;

	public UnityEvent OnHide { get; } = new UnityEvent();

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		hideButton.onClick.AddListener(Hide);
		propTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<PropTool>();
		editScriptButton.onClick.AddListener(EditScript);
	}

	private void OnEnable()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnEnable");
		}
		MonoBehaviourSingleton<BackManager>.Instance.ClaimContext(this);
	}

	private void OnDisable()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDisable");
		}
		if (MonoBehaviourSingleton<BackManager>.Instance.HasContext(this))
		{
			MonoBehaviourSingleton<BackManager>.Instance.UnclaimContext(this);
		}
	}

	public void OnBack()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnBack");
		}
		MonoBehaviourSingleton<BackManager>.Instance.UnclaimContext(this);
		Hide();
	}

	private void Hide()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Hide");
		}
		displayAndHideHandler.Hide();
		if (!MobileUtility.IsMobile)
		{
			propTool.UpdateSelectedAssetId(SerializableGuid.Empty);
		}
		OnHide.Invoke();
	}

	private void EditScript()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "EditScript");
		}
		bool readOnly = view.Mode == UIRuntimePropInfoDetailView.Modes.Read;
		propTool.EditScript(readOnly);
	}
}
