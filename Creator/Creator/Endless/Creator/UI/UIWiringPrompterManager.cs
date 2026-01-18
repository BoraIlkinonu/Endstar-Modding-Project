using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x02000301 RID: 769
	public class UIWiringPrompterManager : UIMonoBehaviourSingleton<UIWiringPrompterManager>
	{
		// Token: 0x170001CE RID: 462
		// (get) Token: 0x06000D65 RID: 3429 RVA: 0x0004042A File Offset: 0x0003E62A
		private int EmitterIndex
		{
			get
			{
				if (this.wiringManager.WiringState != UIWiringStates.CreateNew)
				{
					return this.wireEditorController.EmitterEventIndex;
				}
				return this.wireCreatorController.EmitterEventIndex;
			}
		}

		// Token: 0x170001CF RID: 463
		// (get) Token: 0x06000D66 RID: 3430 RVA: 0x00040451 File Offset: 0x0003E651
		private int ReceiverIndex
		{
			get
			{
				if (this.wiringManager.WiringState != UIWiringStates.CreateNew)
				{
					return this.wireEditorController.ReceiverEventIndex;
				}
				return this.wireCreatorController.ReceiverEventIndex;
			}
		}

		// Token: 0x06000D67 RID: 3431 RVA: 0x00040478 File Offset: 0x0003E678
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.wiringManager = MonoBehaviourSingleton<UIWiringManager>.Instance;
			this.toolPrompter = MonoBehaviourSingleton<UIToolPrompterManager>.Instance;
			this.wireCreatorController = this.wiringManager.WireCreatorController;
			this.wireEditorController = this.wiringManager.WireEditorController;
		}

		// Token: 0x06000D68 RID: 3432 RVA: 0x000404D8 File Offset: 0x0003E6D8
		public void DisplayToolPrompt()
		{
			if (this.verboseLogging)
			{
				Debug.Log("DisplayToolPrompt", this);
			}
			string prompt = this.GetPrompt();
			this.toolPrompter.Display(prompt, false);
		}

		// Token: 0x06000D69 RID: 3433 RVA: 0x0004050C File Offset: 0x0003E70C
		private string GetPrompt()
		{
			if (!this.emitterWiringInspectorView.TargetTransform)
			{
				return this.emitterObjectRequiredPrompt;
			}
			if (!this.receiverWiringInspectorView.TargetTransform)
			{
				return this.receiverObjectRequiredPrompt;
			}
			if (this.EmitterIndex <= -1 && this.ReceiverIndex <= -1)
			{
				if (this.wiresView.SpawnedWiresCount == 0)
				{
					return this.noInProgressOrExistingWiresPrompt;
				}
				return this.noInProgressWiresPrompt;
			}
			else
			{
				if (this.EmitterIndex <= -1)
				{
					return this.emitterEventRequiredPrompt;
				}
				if (this.ReceiverIndex <= -1)
				{
					return this.receiverEventRequiredPrompt;
				}
				Debug.LogWarning("UIWiringPrompter does not know what prompt to display! ", this);
				return string.Empty;
			}
		}

		// Token: 0x04000B84 RID: 2948
		[SerializeField]
		[TextArea]
		private string emitterObjectRequiredPrompt = "Select an object to emit an event";

		// Token: 0x04000B85 RID: 2949
		[SerializeField]
		[TextArea]
		private string receiverObjectRequiredPrompt = "Select an object to receive an event";

		// Token: 0x04000B86 RID: 2950
		[SerializeField]
		[TextArea]
		private string emitterEventRequiredPrompt = "Select an Emitter event";

		// Token: 0x04000B87 RID: 2951
		[SerializeField]
		[TextArea]
		private string receiverEventRequiredPrompt = "Select a Receiver event";

		// Token: 0x04000B88 RID: 2952
		[SerializeField]
		[TextArea]
		private string noInProgressOrExistingWiresPrompt = "Select an Event Node to begin a wire";

		// Token: 0x04000B89 RID: 2953
		[SerializeField]
		[TextArea]
		private string noInProgressWiresPrompt = "Select an Event Node to begin\na wire or an existing wire to edit";

		// Token: 0x04000B8A RID: 2954
		[SerializeField]
		private UIWiresView wiresView;

		// Token: 0x04000B8B RID: 2955
		[SerializeField]
		private UIWiringObjectInspectorView emitterWiringInspectorView;

		// Token: 0x04000B8C RID: 2956
		[SerializeField]
		private UIWiringObjectInspectorView receiverWiringInspectorView;

		// Token: 0x04000B8D RID: 2957
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000B8E RID: 2958
		private UIWiringManager wiringManager;

		// Token: 0x04000B8F RID: 2959
		private UIToolPrompterManager toolPrompter;

		// Token: 0x04000B90 RID: 2960
		private UIWireCreatorController wireCreatorController;

		// Token: 0x04000B91 RID: 2961
		private UIWireEditorController wireEditorController;
	}
}
