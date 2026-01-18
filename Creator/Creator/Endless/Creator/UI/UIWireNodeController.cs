using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Endless.Creator.UI
{
	// Token: 0x0200030E RID: 782
	public class UIWireNodeController : UIGameObject, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
	{
		// Token: 0x170001F7 RID: 503
		// (get) Token: 0x06000DFF RID: 3583 RVA: 0x000429CC File Offset: 0x00040BCC
		public UIWireNodeView WireNodeView
		{
			get
			{
				return this.wireNodeView;
			}
		}

		// Token: 0x170001F8 RID: 504
		// (get) Token: 0x06000E00 RID: 3584 RVA: 0x000429D4 File Offset: 0x00040BD4
		public UIWiringObjectInspectorView EmitterInspector
		{
			get
			{
				return this.WiringManager.EmitterInspector;
			}
		}

		// Token: 0x170001F9 RID: 505
		// (get) Token: 0x06000E01 RID: 3585 RVA: 0x000429E1 File Offset: 0x00040BE1
		public UIWiringObjectInspectorView ReceiverInspector
		{
			get
			{
				return this.WiringManager.ReceiverInspector;
			}
		}

		// Token: 0x170001FA RID: 506
		// (get) Token: 0x06000E02 RID: 3586 RVA: 0x0004061F File Offset: 0x0003E81F
		private UIWiringManager WiringManager
		{
			get
			{
				return MonoBehaviourSingleton<UIWiringManager>.Instance;
			}
		}

		// Token: 0x170001FB RID: 507
		// (get) Token: 0x06000E03 RID: 3587 RVA: 0x000429EE File Offset: 0x00040BEE
		private UIWireCreatorController WireCreatorController
		{
			get
			{
				return this.WiringManager.WireCreatorController;
			}
		}

		// Token: 0x170001FC RID: 508
		// (get) Token: 0x06000E04 RID: 3588 RVA: 0x000429FB File Offset: 0x00040BFB
		private UIWireEditorController WireEditorController
		{
			get
			{
				return this.WiringManager.WireEditorController;
			}
		}

		// Token: 0x170001FD RID: 509
		// (get) Token: 0x06000E05 RID: 3589 RVA: 0x00042A08 File Offset: 0x00040C08
		private UIWiresView WiresView
		{
			get
			{
				return this.WiringManager.WiresView;
			}
		}

		// Token: 0x170001FE RID: 510
		// (get) Token: 0x06000E06 RID: 3590 RVA: 0x00042A15 File Offset: 0x00040C15
		private UIWireConfirmationModalView WireConfirmationModal
		{
			get
			{
				return this.WiringManager.WireConfirmationModalView;
			}
		}

		// Token: 0x170001FF RID: 511
		// (get) Token: 0x06000E07 RID: 3591 RVA: 0x00042A22 File Offset: 0x00040C22
		private bool IsReceiver
		{
			get
			{
				return this.WireNodeView.ParentWiringObjectInspectorView.IsReceiver;
			}
		}

		// Token: 0x17000200 RID: 512
		// (get) Token: 0x06000E08 RID: 3592 RVA: 0x00042A34 File Offset: 0x00040C34
		private bool CanInteract
		{
			get
			{
				return this.WiringManager.EmitterInspector.IsOpen && this.WiringManager.ReceiverInspector.IsOpen && ((this.WiringManager.WiringState != UIWiringStates.Nothing && this.WiringManager.WiringState != UIWiringStates.CreateNew) || !this.IsReceiver || this.WireCreatorController.EmitterEventIndex != -1) && this.WiringManager.WiringState != UIWiringStates.EditExisting;
			}
		}

		// Token: 0x06000E09 RID: 3593 RVA: 0x00042AAB File Offset: 0x00040CAB
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.dropHandler.DropWithGameObjectUnityEvent.AddListener(new UnityAction<GameObject>(this.OnGameObjectDropped));
		}

		// Token: 0x06000E0A RID: 3594 RVA: 0x00042AE1 File Offset: 0x00040CE1
		public void OnPointerDown(PointerEventData eventData)
		{
			if (!this.CanInteract)
			{
				return;
			}
			if (this.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "OnPointerDown", "eventData", eventData), this);
			}
			this.Select();
		}

		// Token: 0x06000E0B RID: 3595 RVA: 0x00042B18 File Offset: 0x00040D18
		public void OnBeginDrag(PointerEventData eventData)
		{
			if (!this.CanInteract)
			{
				return;
			}
			if (this.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "OnBeginDrag", "eventData", eventData), this);
			}
			PoolManagerT instance = MonoBehaviourSingleton<PoolManagerT>.Instance;
			UIWireView uiwireView = this.wireViewSource;
			Transform transform = this.WiresView.transform;
			this.tempWire = instance.Spawn<UIWireView>(uiwireView, default(Vector3), default(Quaternion), transform);
			this.tempWireLineRenderer = this.tempWire.LineRenderer;
			this.tempWireLineRenderer.material = this.wireTempMaterial;
			this.tempWire.transform.localScale = Vector3.one;
			this.tempWire.RectTransform.SetAnchor(AnchorPresets.StretchAll, 0f, 0f, 0f, 0f);
			Vector2 vector = this.WiresView.transform.InverseTransformPoint(base.transform.position);
			Vector2 vector2 = this.WiresView.transform.TransformPoint(eventData.position);
			Vector2[] points = this.tempWireLineRenderer.Points;
			if (this.IsReceiver)
			{
				points[0] = vector2;
				points[points.Length - 1] = vector;
			}
			else
			{
				points[0] = vector;
				points[points.Length - 1] = vector2;
			}
			this.tempWireLineRenderer.SetPoints(points);
			this.EmitterInspector.ToggleDropHandlers(true, this.wireNodeView);
			this.ReceiverInspector.ToggleDropHandlers(true, this.wireNodeView);
		}

		// Token: 0x06000E0C RID: 3596 RVA: 0x00042C98 File Offset: 0x00040E98
		public void OnDrag(PointerEventData eventData)
		{
			if (!this.CanInteract)
			{
				return;
			}
			if (this.superVerboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "OnDrag", "eventData", eventData), this);
			}
			Vector2 vector = this.WiresView.RectTransform.InverseTransformPoint(eventData.position);
			Vector2[] points = this.tempWireLineRenderer.Points;
			if (this.IsReceiver)
			{
				points[0] = vector;
			}
			else
			{
				points[points.Length - 1] = vector;
			}
			int num = Mathf.RoundToInt(Vector2.Distance(points[0], points[points.Length - 1]) / 20f);
			this.tempWireLineRenderer.SetTiling(num);
			this.tempWireLineRenderer.SetPoints(points);
		}

		// Token: 0x06000E0D RID: 3597 RVA: 0x00042D58 File Offset: 0x00040F58
		public void OnEndDrag(PointerEventData eventData)
		{
			if (!this.CanInteract)
			{
				return;
			}
			if (this.verboseLogging)
			{
				Debug.Log(string.Format("{0} ( {1}: {2} )", "OnEndDrag", "eventData", eventData), this);
			}
			this.Clean();
			if (!this.WireConfirmationModal.IsOpen)
			{
				this.WireCreatorController.Restart(true);
			}
		}

		// Token: 0x06000E0E RID: 3598 RVA: 0x00042DB0 File Offset: 0x00040FB0
		public void OnDespawn()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDespawn", Array.Empty<object>());
			}
			if (!this.tempWire)
			{
				return;
			}
			this.Clean();
		}

		// Token: 0x06000E0F RID: 3599 RVA: 0x00042DE0 File Offset: 0x00040FE0
		private void Clean()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Clean", Array.Empty<object>());
			}
			this.EmitterInspector.ToggleDropHandlers(false, null);
			this.ReceiverInspector.ToggleDropHandlers(false, null);
			this.tempWireLineRenderer.material = this.wireViewSource.LineRenderer.material;
			MonoBehaviourSingleton<PoolManagerT>.Instance.Despawn<UIWireView>(this.tempWire);
			this.tempWire = null;
			this.tempWireLineRenderer = null;
		}

		// Token: 0x06000E10 RID: 3600 RVA: 0x00042E58 File Offset: 0x00041058
		private void OnGameObjectDropped(GameObject droppedGameObject)
		{
			if (!this.CanInteract)
			{
				return;
			}
			if (this.verboseLogging)
			{
				Debug.Log("OnGameObjectDropped ( droppedGameObject: " + droppedGameObject.name + " )", this);
			}
			this.Select();
		}

		// Token: 0x06000E11 RID: 3601 RVA: 0x00042E8C File Offset: 0x0004108C
		private void Select()
		{
			if (!this.CanInteract)
			{
				return;
			}
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Select", Array.Empty<object>());
			}
			this.onSelectTweens.Tween();
			bool flag = true;
			if (this.WiringManager.WiringState == UIWiringStates.Nothing || this.WiringManager.WiringState == UIWiringStates.CreateNew)
			{
				if (this.WiringManager.WiringState == UIWiringStates.Nothing)
				{
					this.WiringManager.SetWiringState(UIWiringStates.CreateNew);
				}
				if (this.IsReceiver)
				{
					this.WireCreatorController.SetReceiverEventIndex(this.wireNodeView.NodeIndex);
				}
				else
				{
					this.WireCreatorController.SetEmitterEventIndex(this.wireNodeView.NodeIndex);
				}
				if (this.WireCreatorController.CanCreateWire)
				{
					flag = false;
					this.WireCreatorController.DisplayWireConfirmation();
				}
			}
			else if (this.WiringManager.WiringState == UIWiringStates.EditExisting)
			{
				if (this.IsReceiver)
				{
					this.WireEditorController.SetReceiverEventIndex(this.wireNodeView.NodeIndex);
				}
				else
				{
					this.WireEditorController.SetEmitterEventIndex(this.wireNodeView.NodeIndex);
				}
				if (this.WireEditorController.CanCreateWire)
				{
					flag = false;
				}
			}
			else
			{
				Debug.LogWarningFormat(this, "UIWireNodeController's Select method does not support a wiring state of '{0}'!", new object[] { this.WiringManager.WiringState });
			}
			if (flag)
			{
				MonoBehaviourSingleton<UIWiringPrompterManager>.Instance.DisplayToolPrompt();
			}
		}

		// Token: 0x04000BF0 RID: 3056
		[SerializeField]
		private UIWireNodeView wireNodeView;

		// Token: 0x04000BF1 RID: 3057
		[SerializeField]
		private DropHandler dropHandler;

		// Token: 0x04000BF2 RID: 3058
		[SerializeField]
		private UIWireView wireViewSource;

		// Token: 0x04000BF3 RID: 3059
		[SerializeField]
		private Material wireTempMaterial;

		// Token: 0x04000BF4 RID: 3060
		[SerializeField]
		private TweenCollection onSelectTweens;

		// Token: 0x04000BF5 RID: 3061
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000BF6 RID: 3062
		[SerializeField]
		private bool superVerboseLogging;

		// Token: 0x04000BF7 RID: 3063
		private UIWireView tempWire;

		// Token: 0x04000BF8 RID: 3064
		private UILineRenderer tempWireLineRenderer;
	}
}
