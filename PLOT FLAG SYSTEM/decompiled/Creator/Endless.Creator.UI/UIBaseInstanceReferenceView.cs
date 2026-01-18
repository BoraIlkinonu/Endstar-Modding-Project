using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public abstract class UIBaseInstanceReferenceView<TModel, TViewStyle> : UIInspectorPropReferenceView<TModel, TViewStyle>, IInstanceReferenceViewable where TModel : InstanceReference where TViewStyle : Enum
{
	[Header("UIBaseInstanceReferenceView")]
	[SerializeField]
	private UIButton eyeDropperButton;

	[SerializeField]
	private UIButton highlightReferenceButton;

	[SerializeField]
	private UIPinAnchor pinAnchorSource;

	private InspectorTool inspectorTool;

	private UIPinAnchor pin;

	private bool isEmpty;

	private bool subscribedToOnItemEyeDropped;

	public event Action<SerializableGuid> OnInstanceEyeDropped;

	protected override void Start()
	{
		base.Start();
		inspectorTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<InspectorTool>();
		eyeDropperButton.onClick.AddListener(SetStateToEyeDropper);
		highlightReferenceButton.onClick.AddListener(HighlightReference);
		inspectorTool.OnStateChanged.AddListener(OnInspectorStateChanged);
	}

	public override void View(TModel model)
	{
		base.View(model);
		isEmpty = GetReferenceIsEmpty(model);
		eyeDropperButton.gameObject.SetActive(isEmpty);
		highlightReferenceButton.interactable = !isEmpty;
		Layout();
		ClosePin();
		if (!isEmpty && !model.useContext)
		{
			GameObject instanceObject = InspectorReferenceUtility.GetInstanceObject(model);
			RectTransform anchorContainer = MonoBehaviourSingleton<UICreatorReferenceManager>.Instance.AnchorContainer;
			SerializableGuid instanceDefinitionId = InspectorReferenceUtility.GetInstanceDefinitionId(model);
			if (!MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.TryGetRuntimePropInfo(instanceDefinitionId, out var metadata))
			{
				DebugUtility.LogException(new NullReferenceException("endlessDefinition is null!"), this);
				return;
			}
			Vector3 pinOffset = GetPinOffset(metadata);
			UIPinAnchor prefab = pinAnchorSource;
			Transform target = instanceObject.transform;
			Vector3? offset = pinOffset;
			pin = UIPinAnchor.CreateInstance(prefab, target, anchorContainer, null, offset);
		}
	}

	public override void Clear()
	{
		base.Clear();
		ClosePin();
		UnsubscribeToOnItemEyeDropped();
	}

	public override void SetInteractable(bool interactable)
	{
		base.SetInteractable(interactable);
		eyeDropperButton.interactable = interactable;
		highlightReferenceButton.interactable = interactable;
	}

	public override void SetPermission(Permissions permission)
	{
		base.SetPermission(permission);
		eyeDropperButton.gameObject.SetActive(isEmpty && permission == Permissions.ReadWrite);
	}

	protected override string GetReferenceName(TModel model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "GetReferenceName", "model", model), this);
		}
		return model.GetReferenceName();
	}

	protected override bool GetReferenceIsEmpty(TModel model)
	{
		if (base.GetReferenceIsEmpty(model))
		{
			return !model.useContext;
		}
		return false;
	}

	protected virtual Vector3 GetPinOffset(PropLibrary.RuntimePropInfo endlessDefinition)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("GetPinOffset ( endlessDefinition: " + endlessDefinition.PropData.Name + " )", this);
		}
		return Vector3.up * endlessDefinition.PropData.GetBoundingSize().y;
	}

	private void SetStateToEyeDropper()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("SetStateToEyeDropper", this);
		}
		inspectorTool.SetStateToEyeDropper(ReferenceFilter);
		if (!subscribedToOnItemEyeDropped)
		{
			inspectorTool.OnItemEyeDropped.AddListener(OnItemEyeDropped);
			subscribedToOnItemEyeDropped = true;
		}
	}

	private void OnItemEyeDropped(SerializableGuid assetId, SerializableGuid instanceId, PropEntry propEntry)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4}, {5}: {6} )", "OnItemEyeDropped", "assetId", assetId, "instanceId", instanceId, "propEntry", propEntry), this);
		}
		if (subscribedToOnItemEyeDropped)
		{
			this.OnInstanceEyeDropped?.Invoke(instanceId);
			UnsubscribeToOnItemEyeDropped();
		}
	}

	private void OnInspectorStateChanged(InspectorTool.States newState)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "OnInspectorStateChanged", "newState", newState), this);
		}
		if (newState == InspectorTool.States.Inspect && subscribedToOnItemEyeDropped)
		{
			UnsubscribeToOnItemEyeDropped();
		}
	}

	private void HighlightReference()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("HighlightReference", this);
		}
		pin?.Highlight();
	}

	private void UnsubscribeToOnItemEyeDropped()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("UnsubscribeToOnItemEyeDropped", this);
		}
		if (subscribedToOnItemEyeDropped)
		{
			inspectorTool.OnItemEyeDropped.RemoveListener(OnItemEyeDropped);
			subscribedToOnItemEyeDropped = false;
		}
	}

	private void ClosePin()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("ClosePin", this);
		}
		if ((bool)pin)
		{
			pin.Close();
			pin = null;
		}
	}
}
