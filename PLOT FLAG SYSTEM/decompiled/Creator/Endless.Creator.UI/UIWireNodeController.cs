using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Endless.Creator.UI;

public class UIWireNodeController : UIGameObject, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
{
	[SerializeField]
	private UIWireNodeView wireNodeView;

	[SerializeField]
	private DropHandler dropHandler;

	[SerializeField]
	private UIWireView wireViewSource;

	[SerializeField]
	private Material wireTempMaterial;

	[SerializeField]
	private TweenCollection onSelectTweens;

	[SerializeField]
	private bool verboseLogging;

	[SerializeField]
	private bool superVerboseLogging;

	private UIWireView tempWire;

	private UILineRenderer tempWireLineRenderer;

	public UIWireNodeView WireNodeView => wireNodeView;

	public UIWiringObjectInspectorView EmitterInspector => WiringManager.EmitterInspector;

	public UIWiringObjectInspectorView ReceiverInspector => WiringManager.ReceiverInspector;

	private UIWiringManager WiringManager => MonoBehaviourSingleton<UIWiringManager>.Instance;

	private UIWireCreatorController WireCreatorController => WiringManager.WireCreatorController;

	private UIWireEditorController WireEditorController => WiringManager.WireEditorController;

	private UIWiresView WiresView => WiringManager.WiresView;

	private UIWireConfirmationModalView WireConfirmationModal => WiringManager.WireConfirmationModalView;

	private bool IsReceiver => WireNodeView.ParentWiringObjectInspectorView.IsReceiver;

	private bool CanInteract
	{
		get
		{
			if (!WiringManager.EmitterInspector.IsOpen || !WiringManager.ReceiverInspector.IsOpen)
			{
				return false;
			}
			if ((WiringManager.WiringState == UIWiringStates.Nothing || WiringManager.WiringState == UIWiringStates.CreateNew) && IsReceiver && WireCreatorController.EmitterEventIndex == -1)
			{
				return false;
			}
			if (WiringManager.WiringState == UIWiringStates.EditExisting)
			{
				return false;
			}
			return true;
		}
	}

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		dropHandler.DropWithGameObjectUnityEvent.AddListener(OnGameObjectDropped);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (CanInteract)
		{
			if (verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "OnPointerDown", "eventData", eventData), this);
			}
			Select();
		}
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (CanInteract)
		{
			if (verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "OnBeginDrag", "eventData", eventData), this);
			}
			PoolManagerT instance = MonoBehaviourSingleton<PoolManagerT>.Instance;
			UIWireView prefab = wireViewSource;
			Transform parent = WiresView.transform;
			tempWire = instance.Spawn(prefab, default(Vector3), default(Quaternion), parent);
			tempWireLineRenderer = tempWire.LineRenderer;
			tempWireLineRenderer.material = wireTempMaterial;
			tempWire.transform.localScale = Vector3.one;
			tempWire.RectTransform.SetAnchor(AnchorPresets.StretchAll);
			Vector2 vector = WiresView.transform.InverseTransformPoint(base.transform.position);
			Vector2 vector2 = WiresView.transform.TransformPoint(eventData.position);
			Vector2[] points = tempWireLineRenderer.Points;
			if (IsReceiver)
			{
				points[0] = vector2;
				points[^1] = vector;
			}
			else
			{
				points[0] = vector;
				points[^1] = vector2;
			}
			tempWireLineRenderer.SetPoints(points);
			EmitterInspector.ToggleDropHandlers(state: true, wireNodeView);
			ReceiverInspector.ToggleDropHandlers(state: true, wireNodeView);
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (CanInteract)
		{
			if (superVerboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "OnDrag", "eventData", eventData), this);
			}
			Vector2 vector = WiresView.RectTransform.InverseTransformPoint(eventData.position);
			Vector2[] points = tempWireLineRenderer.Points;
			if (IsReceiver)
			{
				points[0] = vector;
			}
			else
			{
				points[^1] = vector;
			}
			int tiling = Mathf.RoundToInt(Vector2.Distance(points[0], points[^1]) / 20f);
			tempWireLineRenderer.SetTiling(tiling);
			tempWireLineRenderer.SetPoints(points);
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (CanInteract)
		{
			if (verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "OnEndDrag", "eventData", eventData), this);
			}
			Clean();
			if (!WireConfirmationModal.IsOpen)
			{
				WireCreatorController.Restart();
			}
		}
	}

	public void OnDespawn()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "OnDespawn");
		}
		if ((bool)tempWire)
		{
			Clean();
		}
	}

	private void Clean()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Clean");
		}
		EmitterInspector.ToggleDropHandlers(state: false, null);
		ReceiverInspector.ToggleDropHandlers(state: false, null);
		tempWireLineRenderer.material = wireViewSource.LineRenderer.material;
		MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn(tempWire);
		tempWire = null;
		tempWireLineRenderer = null;
	}

	private void OnGameObjectDropped(GameObject droppedGameObject)
	{
		if (CanInteract)
		{
			if (verboseLogging)
			{
				Debug.Log("OnGameObjectDropped ( droppedGameObject: " + droppedGameObject.name + " )", this);
			}
			Select();
		}
	}

	private void Select()
	{
		if (!CanInteract)
		{
			return;
		}
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Select");
		}
		onSelectTweens.Tween();
		bool flag = true;
		if (WiringManager.WiringState == UIWiringStates.Nothing || WiringManager.WiringState == UIWiringStates.CreateNew)
		{
			if (WiringManager.WiringState == UIWiringStates.Nothing)
			{
				WiringManager.SetWiringState(UIWiringStates.CreateNew);
			}
			if (IsReceiver)
			{
				WireCreatorController.SetReceiverEventIndex(wireNodeView.NodeIndex);
			}
			else
			{
				WireCreatorController.SetEmitterEventIndex(wireNodeView.NodeIndex);
			}
			if (WireCreatorController.CanCreateWire)
			{
				flag = false;
				WireCreatorController.DisplayWireConfirmation();
			}
		}
		else if (WiringManager.WiringState == UIWiringStates.EditExisting)
		{
			if (IsReceiver)
			{
				WireEditorController.SetReceiverEventIndex(wireNodeView.NodeIndex);
			}
			else
			{
				WireEditorController.SetEmitterEventIndex(wireNodeView.NodeIndex);
			}
			if (WireEditorController.CanCreateWire)
			{
				flag = false;
			}
		}
		else
		{
			Debug.LogWarningFormat(this, "UIWireNodeController's Select method does not support a wiring state of '{0}'!", WiringManager.WiringState);
		}
		if (flag)
		{
			MonoBehaviourSingleton<UIWiringPrompterManager>.Instance.DisplayToolPrompt();
		}
	}
}
