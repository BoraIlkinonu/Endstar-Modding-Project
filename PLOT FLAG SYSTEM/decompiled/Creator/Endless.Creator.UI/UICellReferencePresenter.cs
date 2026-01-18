using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UICellReferencePresenter : UIBasePresenter<CellReference>
{
	[Header("UICellReferencePresenter")]
	[SerializeField]
	private UIPinAnchor pinAnchorSource;

	private InspectorTool inspectorTool;

	private bool isDisplayingHighlightAndPin;

	private bool listeningToInspectorEvents;

	private UIPinAnchor pin;

	private Transform pinAnchor;

	private bool pinAnchorCreated;

	private bool HasCellValue => base.Model?.HasValue ?? false;

	private bool RotationHasValue
	{
		get
		{
			if (HasCellValue)
			{
				return base.Model.RotationHasValue;
			}
			return false;
		}
	}

	protected override void Start()
	{
		base.Start();
		inspectorTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<InspectorTool>();
		UICellReferenceView obj = base.View.Interface as UICellReferenceView;
		obj.OnClear += Clear;
		obj.OnHighlight += Highlight;
		obj.OnEyeDrop += SetInspectorToEyeDropperState;
		obj.OnPositionChanged += SetPosition;
		obj.OnRotationChanged += SetRotation;
		obj.OnRotationClear += ClearRotation;
	}

	private void OnDisable()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDisable");
		}
		if (listeningToInspectorEvents)
		{
			SetInspectorEventListeners(state: false);
		}
	}

	public override void SetModel(CellReference model, bool triggerOnModelChanged)
	{
		if (base.Model != model && HasCellValue && isDisplayingHighlightAndPin)
		{
			CloseHighlightAndPin();
		}
		base.SetModel(model, triggerOnModelChanged);
		if (HasCellValue)
		{
			DisplayHighlightAndPin();
		}
		else
		{
			CloseHighlightAndPin();
		}
	}

	public override void Clear()
	{
		base.Clear();
		if (isDisplayingHighlightAndPin)
		{
			CloseHighlightAndPin();
		}
		CellReference model = ReferenceFactory.CreateCellReference();
		SetModel(model, triggerOnModelChanged: true);
	}

	private void Highlight()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Highlight");
		}
		if ((bool)pin)
		{
			pin.Highlight();
		}
	}

	private void SetRotation(float rotation)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetRotation", rotation);
		}
		Vector3 cellPosition = base.Model.GetCellPosition();
		CellReferenceUtility.SetCell(base.Model, cellPosition, rotation);
		SetModel(base.Model, triggerOnModelChanged: true);
	}

	private void ClearRotation()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ClearRotation");
		}
		Vector3 cellPosition = base.Model.GetCellPosition();
		CellReferenceUtility.SetCell(base.Model, cellPosition, null);
		SetModel(base.Model, triggerOnModelChanged: true);
	}

	private void SetInspectorToEyeDropperState()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetInspectorToEyeDropperState");
		}
		inspectorTool?.SetStateToEyeDropper(ReferenceFilter.None);
		SetInspectorEventListeners(state: true);
	}

	private void SetPosition(Vector3 position)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetPosition", position);
		}
		if (!(base.Model.GetCellPosition() == position))
		{
			CellReferenceUtility.SetCell(base.Model, position, base.Model.RotationHasValue ? new float?(base.Model.GetRotation()) : ((float?)null));
			SetModel(base.Model, triggerOnModelChanged: true);
		}
	}

	private void SetInspectorEventListeners(bool state)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "SetInspectorEventListeners", state);
		}
		if (listeningToInspectorEvents != state)
		{
			if (state)
			{
				inspectorTool.OnCellEyedropped.AddListener(OnCellEyeDropped);
				inspectorTool.OnStateChanged.AddListener(OnInspectorToolStateChanged);
			}
			else
			{
				inspectorTool.OnCellEyedropped.RemoveListener(OnCellEyeDropped);
				inspectorTool.OnStateChanged.RemoveListener(OnInspectorToolStateChanged);
			}
			listeningToInspectorEvents = state;
		}
	}

	private void OnCellEyeDropped(Vector3Int coordinate, float? yawRotation)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnCellEyeDropped", coordinate, yawRotation);
		}
		if (isDisplayingHighlightAndPin)
		{
			CloseHighlightAndPin();
		}
		CellReferenceUtility.SetCell(base.Model, coordinate, yawRotation);
		SetInspectorEventListeners(state: false);
		SetModel(base.Model, triggerOnModelChanged: true);
	}

	private void OnInspectorToolStateChanged(InspectorTool.States state)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnInspectorToolStateChanged", state);
		}
		if (state == InspectorTool.States.Inspect && listeningToInspectorEvents)
		{
			SetInspectorEventListeners(state: false);
		}
	}

	private void DisplayHighlightAndPin()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethodWithAppension(this, "DisplayHighlightAndPin", string.Format("{0}: {1}", "isDisplayingHighlightAndPin", isDisplayingHighlightAndPin));
		}
		ViewMarker();
		if (isDisplayingHighlightAndPin)
		{
			pinAnchor.position = base.Model.GetCellPosition();
			return;
		}
		if (!pinAnchorCreated)
		{
			CreatePinAnchor();
		}
		pinAnchor.position = base.Model.GetCellPosition();
		RectTransform anchorContainer = MonoBehaviourSingleton<UICreatorReferenceManager>.Instance.AnchorContainer;
		Vector3 up = Vector3.up;
		if ((bool)pin)
		{
			pin.SetTarget(pinAnchor);
			pin.SetOffset(up);
		}
		else
		{
			UIPinAnchor prefab = pinAnchorSource;
			Transform target = pinAnchor;
			Vector3? offset = up;
			pin = UIPinAnchor.CreateInstance(prefab, target, anchorContainer, null, offset);
			pin.OnClosed += OnPinClosed;
		}
		isDisplayingHighlightAndPin = true;
	}

	private void ViewMarker()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ViewMarker");
		}
		MonoBehaviourSingleton<MarkerManager>.Instance.ClearMarkersOfType(MarkerType.CellHighlight);
		Vector3Int cellPositionAsVector3Int = base.Model.GetCellPositionAsVector3Int();
		MonoBehaviourSingleton<MarkerManager>.Instance.MarkCoordinate(cellPositionAsVector3Int, MarkerType.CellHighlight, RotationHasValue ? new float?(base.Model.GetRotation()) : ((float?)null));
	}

	private void CreatePinAnchor()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "CreatePinAnchor");
		}
		if (!pinAnchorCreated)
		{
			GameObject gameObject = new GameObject("UICellReference Pin Anchor");
			pinAnchor = gameObject.transform;
			pinAnchor.SetParent(MonoBehaviourSingleton<UICreatorReferenceManager>.Instance.AnchorContainer);
			pinAnchorCreated = true;
		}
	}

	private void CloseHighlightAndPin()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "CloseHighlightAndPin");
		}
		if (isDisplayingHighlightAndPin)
		{
			MonoBehaviourSingleton<MarkerManager>.Instance.ReleaseAllMarkers();
			if ((bool)pin)
			{
				pin.Close();
				pin = null;
			}
			isDisplayingHighlightAndPin = false;
		}
	}

	private void OnPinClosed()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnPinClosed");
		}
		if (!(pin == null))
		{
			pin.OnClosed -= OnPinClosed;
			pin = null;
		}
	}
}
