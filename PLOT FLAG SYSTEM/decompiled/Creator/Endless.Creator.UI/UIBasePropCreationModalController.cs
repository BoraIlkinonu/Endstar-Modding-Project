using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI;

public abstract class UIBasePropCreationModalController : UIGameObject, IUILoadingSpinnerViewCompatible
{
	[Header("UIBasePropCreationModalController")]
	[SerializeField]
	protected UIBasePropCreationModalView view;

	[SerializeField]
	private UIInputField nameInputField;

	[SerializeField]
	private UIInputField descriptionInputField;

	[SerializeField]
	private UIToggle grantEditRightsToCollaboratorsToggle;

	[SerializeField]
	private UIButton createButton;

	protected PropTool propTool;

	[field: Header("Debugging")]
	[field: SerializeField]
	protected bool VerboseLogging { get; set; }

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	protected string Name => nameInputField.text;

	protected string Description => descriptionInputField.text;

	protected bool GrantEditRightsToCollaborators => grantEditRightsToCollaboratorsToggle.IsOn;

	protected virtual void Start()
	{
		if (VerboseLogging)
		{
			DebugUtility.Log("Start", this);
		}
		createButton.onClick.AddListener(Create);
		propTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<PropTool>();
	}

	protected bool ValidatePropCreation()
	{
		if (VerboseLogging)
		{
			DebugUtility.Log("ValidatePropCreation", this);
		}
		return !nameInputField.IsNullOrEmptyOrWhiteSpace();
	}

	protected abstract void Create();
}
