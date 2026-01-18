using System;
using System.Collections.Generic;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000300 RID: 768
	public class UIWiringManager : MonoBehaviourSingleton<UIWiringManager>
	{
		// Token: 0x170001C3 RID: 451
		// (get) Token: 0x06000D4D RID: 3405 RVA: 0x0003FE55 File Offset: 0x0003E055
		public UIWireCreatorController WireCreatorController
		{
			get
			{
				return this.wireCreatorController;
			}
		}

		// Token: 0x170001C4 RID: 452
		// (get) Token: 0x06000D4E RID: 3406 RVA: 0x0003FE5D File Offset: 0x0003E05D
		public UIWireEditorController WireEditorController
		{
			get
			{
				return this.wireEditorController;
			}
		}

		// Token: 0x170001C5 RID: 453
		// (get) Token: 0x06000D4F RID: 3407 RVA: 0x0003FE65 File Offset: 0x0003E065
		public UIWiringObjectInspectorView EmitterInspector
		{
			get
			{
				return this.emitterInspector;
			}
		}

		// Token: 0x170001C6 RID: 454
		// (get) Token: 0x06000D50 RID: 3408 RVA: 0x0003FE6D File Offset: 0x0003E06D
		public UIWiringObjectInspectorView ReceiverInspector
		{
			get
			{
				return this.receiverInspector;
			}
		}

		// Token: 0x170001C7 RID: 455
		// (get) Token: 0x06000D51 RID: 3409 RVA: 0x0003FE75 File Offset: 0x0003E075
		public UIWiresView WiresView
		{
			get
			{
				return this.wiresView;
			}
		}

		// Token: 0x170001C8 RID: 456
		// (get) Token: 0x06000D52 RID: 3410 RVA: 0x0003FE7D File Offset: 0x0003E07D
		public UIWiringRerouteView WiringRerouteView
		{
			get
			{
				return this.wiringRerouteView;
			}
		}

		// Token: 0x170001C9 RID: 457
		// (get) Token: 0x06000D53 RID: 3411 RVA: 0x0003FE85 File Offset: 0x0003E085
		public UIWireConfirmationModalView WireConfirmationModalView
		{
			get
			{
				return this.wireConfirmationModalView;
			}
		}

		// Token: 0x170001CA RID: 458
		// (get) Token: 0x06000D54 RID: 3412 RVA: 0x0003FE8D File Offset: 0x0003E08D
		public UIWireEditorModalView WireEditorModal
		{
			get
			{
				return this.wireEditorModal;
			}
		}

		// Token: 0x170001CB RID: 459
		// (get) Token: 0x06000D55 RID: 3413 RVA: 0x0003FE95 File Offset: 0x0003E095
		// (set) Token: 0x06000D56 RID: 3414 RVA: 0x0003FE9D File Offset: 0x0003E09D
		public UIWiringStates WiringState { get; private set; }

		// Token: 0x170001CC RID: 460
		// (get) Token: 0x06000D57 RID: 3415 RVA: 0x0003FEA6 File Offset: 0x0003E0A6
		// (set) Token: 0x06000D58 RID: 3416 RVA: 0x0003FEAE File Offset: 0x0003E0AE
		public WiringTool WiringTool { get; private set; }

		// Token: 0x170001CD RID: 461
		// (get) Token: 0x06000D59 RID: 3417 RVA: 0x0003FEB7 File Offset: 0x0003E0B7
		public UnityEvent OnObjectSelected { get; } = new UnityEvent();

		// Token: 0x06000D5A RID: 3418 RVA: 0x0003FEC0 File Offset: 0x0003E0C0
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.WiringTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<WiringTool>();
			this.canvas.SetActive(false);
			this.WiringTool.OnEventObjectSelected.AddListener(new UnityAction<Transform, SerializableGuid, List<UIEndlessEventList>>(this.OnEmitterObjectSelected));
			this.WiringTool.OnReceiverObjectSelected.AddListener(new UnityAction<Transform, SerializableGuid, List<UIEndlessEventList>>(this.OnReceiverObjectSelected));
			this.WiringTool.OnWireConfirmed.AddListener(new UnityAction(this.OnWireConfirmed));
			this.WiringTool.OnWireRemoved.AddListener(new UnityAction(this.OnWireRemoved));
			MonoBehaviourSingleton<ToolManager>.Instance.OnToolChange.AddListener(new UnityAction<EndlessTool>(this.OnToolChange));
			NetworkBehaviourSingleton<CreatorManager>.Instance.LocalClientRoleChanged.AddListener(new UnityAction<Roles>(this.OnLocalClientRoleChanged));
		}

		// Token: 0x06000D5B RID: 3419 RVA: 0x0003FFA8 File Offset: 0x0003E1A8
		public void HideWiringInspector(UIWiringObjectInspectorView wiringInspectorView, bool displayToolPrompt)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "HideWiringInspector", new object[]
				{
					wiringInspectorView.gameObject.name,
					displayToolPrompt
				});
			}
			if (this.WiringTool.ToolState == WiringTool.WiringToolState.Rerouting)
			{
				this.WiringTool.SetToolState(WiringTool.WiringToolState.Wiring);
			}
			bool flag = wiringInspectorView == this.emitterInspector;
			if (flag && this.WiringTool.ToolState == WiringTool.WiringToolState.Rerouting)
			{
				this.WiringTool.SetToolState(WiringTool.WiringToolState.Wiring);
			}
			this.wireCreatorController.Restart(!flag);
			this.wireEditorController.Restart(!flag);
			this.wiresView.DespawnAllWires();
			if (wiringInspectorView.IsOpen)
			{
				wiringInspectorView.Hide();
			}
			if (flag)
			{
				this.CloseAndResetEverything();
			}
			else
			{
				this.WiringTool.ReceiverSelectionCancelled(false);
			}
			if (displayToolPrompt)
			{
				MonoBehaviourSingleton<UIWiringPrompterManager>.Instance.DisplayToolPrompt();
			}
		}

		// Token: 0x06000D5C RID: 3420 RVA: 0x00040084 File Offset: 0x0003E284
		public void SetWiringState(UIWiringStates newWiringState)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SetWiringState", new object[] { newWiringState });
			}
			if (this.WiringState == newWiringState)
			{
				return;
			}
			this.WiringState = newWiringState;
			this.OnWiringStateChanged.Invoke(this.WiringState);
		}

		// Token: 0x06000D5D RID: 3421 RVA: 0x000400D8 File Offset: 0x0003E2D8
		private void OnEmitterObjectSelected(Transform targetTransform, SerializableGuid targetId, List<UIEndlessEventList> nodeEvents)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEmitterObjectSelected", new object[] { targetTransform.name, targetId, nodeEvents.Count });
			}
			RectTransform leftWiringInspectorViewContainer = this.wiringInspectorPositioner.LeftWiringInspectorViewContainer;
			this.emitterInspector.Display(targetTransform, targetId, nodeEvents, true, false, leftWiringInspectorViewContainer);
			this.canvas.SetActive(true);
			MonoBehaviourSingleton<UIWiringPrompterManager>.Instance.DisplayToolPrompt();
			this.OnObjectSelected.Invoke();
		}

		// Token: 0x06000D5E RID: 3422 RVA: 0x0004015C File Offset: 0x0003E35C
		private void OnReceiverObjectSelected(Transform targetTransform, SerializableGuid targetId, List<UIEndlessEventList> nodeEvents)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnReceiverObjectSelected", new object[] { targetTransform.name, targetId, nodeEvents.Count });
			}
			RectTransform reftWiringInspectorViewContainer = this.wiringInspectorPositioner.ReftWiringInspectorViewContainer;
			this.receiverInspector.Display(targetTransform, targetId, nodeEvents, false, true, reftWiringInspectorViewContainer);
			this.wiresView.DespawnAllWires();
			this.wiresView.Display();
			MonoBehaviourSingleton<UIWiringPrompterManager>.Instance.DisplayToolPrompt();
			this.OnObjectSelected.Invoke();
		}

		// Token: 0x06000D5F RID: 3423 RVA: 0x000401EC File Offset: 0x0003E3EC
		private void CloseAndResetEverything()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CloseAndResetEverything", Array.Empty<object>());
			}
			this.wiringRerouteView.HideRerouteSwitch();
			this.wireCreatorController.Restart(false);
			this.wireEditorController.Restart(false);
			this.wiresView.DespawnAllWires();
			if (this.emitterInspector.IsOpen)
			{
				this.emitterInspector.Hide();
			}
			if (this.receiverInspector.IsOpen)
			{
				this.receiverInspector.Hide();
			}
			this.canvas.SetActive(false);
			this.WiringTool.ReceiverSelectionCancelled(true);
			this.WiringTool.EmitterSelectionCancelled();
			this.wiringInspectorPositioner.enabled = false;
			this.SetWiringState(UIWiringStates.Nothing);
		}

		// Token: 0x06000D60 RID: 3424 RVA: 0x000402A8 File Offset: 0x0003E4A8
		private void OnWireConfirmed()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnWireConfirmed", Array.Empty<object>());
			}
			this.wiringRerouteView.HideRerouteSwitch();
			this.wireCreatorController.Restart(false);
			this.wireEditorController.Restart(false);
			this.wiresView.DespawnAllWires();
			this.wiresView.Display();
			this.emitterInspector.UpdateWireCounts();
			this.ReceiverInspector.UpdateWireCounts();
			MonoBehaviourSingleton<UIWiringPrompterManager>.Instance.DisplayToolPrompt();
		}

		// Token: 0x06000D61 RID: 3425 RVA: 0x00040326 File Offset: 0x0003E526
		private void OnWireRemoved()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnWireRemoved", Array.Empty<object>());
			}
			this.emitterInspector.UpdateWireCounts();
			this.ReceiverInspector.UpdateWireCounts();
		}

		// Token: 0x06000D62 RID: 3426 RVA: 0x00040358 File Offset: 0x0003E558
		private void OnToolChange(EndlessTool newTool)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnToolChange", new object[] { (newTool != null) ? newTool.GetType().Name : "null" });
			}
			bool flag = newTool is WiringTool;
			if (flag)
			{
				MonoBehaviourSingleton<UIWiringPrompterManager>.Instance.DisplayToolPrompt();
			}
			else if (this.wiringToolActive)
			{
				this.HideWiringInspector(this.emitterInspector, false);
			}
			this.wiringToolActive = flag;
		}

		// Token: 0x06000D63 RID: 3427 RVA: 0x000403D1 File Offset: 0x0003E5D1
		private void OnLocalClientRoleChanged(Roles localClientLevelRole)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnLocalClientRoleChanged", new object[] { localClientLevelRole });
			}
			if (localClientLevelRole.IsLessThan(Roles.Editor))
			{
				this.HideWiringInspector(this.emitterInspector, false);
			}
		}

		// Token: 0x04000B74 RID: 2932
		public UnityEvent<UIWiringStates> OnWiringStateChanged = new UnityEvent<UIWiringStates>();

		// Token: 0x04000B75 RID: 2933
		[SerializeField]
		private GameObject canvas;

		// Token: 0x04000B76 RID: 2934
		[SerializeField]
		private UIWiringInspectorPositioner wiringInspectorPositioner;

		// Token: 0x04000B77 RID: 2935
		[SerializeField]
		private UIWireCreatorController wireCreatorController;

		// Token: 0x04000B78 RID: 2936
		[SerializeField]
		private UIWireEditorController wireEditorController;

		// Token: 0x04000B79 RID: 2937
		[SerializeField]
		private UIWiringObjectInspectorView emitterInspector;

		// Token: 0x04000B7A RID: 2938
		[SerializeField]
		private UIWiringObjectInspectorView receiverInspector;

		// Token: 0x04000B7B RID: 2939
		[SerializeField]
		private UIWiresView wiresView;

		// Token: 0x04000B7C RID: 2940
		[SerializeField]
		private UIWiringRerouteView wiringRerouteView;

		// Token: 0x04000B7D RID: 2941
		[SerializeField]
		private UIWireConfirmationModalView wireConfirmationModalView;

		// Token: 0x04000B7E RID: 2942
		[SerializeField]
		private UIWireEditorModalView wireEditorModal;

		// Token: 0x04000B7F RID: 2943
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000B80 RID: 2944
		private bool wiringToolActive;
	}
}
