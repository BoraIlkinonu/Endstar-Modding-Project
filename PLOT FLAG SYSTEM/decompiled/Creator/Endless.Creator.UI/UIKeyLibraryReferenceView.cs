using System;
using Endless.Gameplay;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Runtime.Shared;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIKeyLibraryReferenceView : UIBaseInventoryLibraryReferenceView<KeyLibraryReference, UIKeyLibraryReferenceView.Styles>, IUIInteractable
{
	public enum Styles
	{
		Default
	}

	[SerializeField]
	private UIToggle lockedToggle;

	[field: Header("UIKeyLibraryReferenceView")]
	[field: SerializeField]
	public override Styles Style { get; protected set; }

	protected override ReferenceFilter ReferenceFilter => ReferenceFilter.Key;

	public event Action<bool> OnLockedChanged;

	protected override void Start()
	{
		base.Start();
		lockedToggle.OnChange.AddListener(InvokeOnLockedChanged);
	}

	public override void View(KeyLibraryReference model)
	{
		bool state = model != null && !model.IsReferenceEmpty() && !InspectorReferenceUtility.GetId(model).IsEmpty;
		lockedToggle.SetIsOn(state, suppressOnChange: false);
	}

	public override void SetInteractable(bool interactable)
	{
		base.SetInteractable(interactable);
		lockedToggle.SetInteractable(interactable, tweenVisuals: false);
	}

	protected override string GetReferenceName(KeyLibraryReference model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "GetReferenceName", model);
		}
		return GetPropLibraryReferenceName(model);
	}

	private void InvokeOnLockedChanged(bool locked)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "InvokeOnLockedChanged", locked);
		}
		this.OnLockedChanged?.Invoke(locked);
	}
}
