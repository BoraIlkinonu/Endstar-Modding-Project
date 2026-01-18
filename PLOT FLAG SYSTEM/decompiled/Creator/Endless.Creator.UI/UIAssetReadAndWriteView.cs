using Endless.Assets;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public abstract class UIAssetReadAndWriteView<T> : UIAssetView<T>, IRoleInteractable where T : Asset
{
	[SerializeField]
	private UIInputField descriptionInputField;

	[SerializeField]
	private UIInputField nameInputField;

	public override void View(T model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "View", "model", model), this);
		}
		nameInputField.text = model.Name;
		descriptionInputField.text = model.Description;
	}

	public override void Clear()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("Clear", this);
		}
		nameInputField.text = string.Empty;
		descriptionInputField.text = string.Empty;
	}

	public virtual void SetLocalUserCanInteract(bool localUserCanInteract)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetLocalUserCanInteract", "localUserCanInteract", localUserCanInteract), this);
		}
		nameInputField.interactable = localUserCanInteract;
		descriptionInputField.interactable = localUserCanInteract;
	}
}
