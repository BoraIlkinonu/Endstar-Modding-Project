using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI;

public abstract class UIAssetReadAndWriteController : UIGameObject, IUILoadingSpinnerViewCompatible
{
	[field: SerializeField]
	protected UIInputField NameInputField { get; private set; }

	[field: SerializeField]
	protected UIInputField DescriptionInputField { get; private set; }

	[field: Header("Debugging")]
	[field: SerializeField]
	protected bool VerboseLogging { get; set; }

	public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

	public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

	protected virtual void Start()
	{
		if (VerboseLogging)
		{
			DebugUtility.Log("Start", this);
		}
		NameInputField.onSubmit.AddListener(SetName);
		NameInputField.onDeselect.AddListener(SetName);
		DescriptionInputField.onSubmit.AddListener(SetDescription);
		DescriptionInputField.onDeselect.AddListener(SetDescription);
	}

	protected abstract void SetName(string newValue);

	protected abstract void SetDescription(string newValue);
}
