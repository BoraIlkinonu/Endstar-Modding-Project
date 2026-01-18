using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000306 RID: 774
	public abstract class UIBaseWireController : UIGameObject
	{
		// Token: 0x170001DF RID: 479
		// (get) Token: 0x06000D9C RID: 3484 RVA: 0x00041004 File Offset: 0x0003F204
		// (set) Token: 0x06000D9D RID: 3485 RVA: 0x0004100C File Offset: 0x0003F20C
		protected bool VerboseLogging { get; set; }

		// Token: 0x170001E0 RID: 480
		// (get) Token: 0x06000D9E RID: 3486 RVA: 0x00041015 File Offset: 0x0003F215
		// (set) Token: 0x06000D9F RID: 3487 RVA: 0x0004101D File Offset: 0x0003F21D
		public int EmitterEventIndex { get; private set; } = -1;

		// Token: 0x170001E1 RID: 481
		// (get) Token: 0x06000DA0 RID: 3488 RVA: 0x00041026 File Offset: 0x0003F226
		// (set) Token: 0x06000DA1 RID: 3489 RVA: 0x0004102E File Offset: 0x0003F22E
		public int ReceiverEventIndex { get; private set; } = -1;

		// Token: 0x170001E2 RID: 482
		// (get) Token: 0x06000DA2 RID: 3490 RVA: 0x00041038 File Offset: 0x0003F238
		public bool CanCreateWire
		{
			get
			{
				bool flag = this.EmitterEventIndex > -1 && this.ReceiverEventIndex > -1;
				if (flag)
				{
					SerializableGuid targetId = this.EmitterInspector.TargetId;
					string memberName = this.EmitterInspector.NodeEvents[this.EmitterEventIndex].MemberName;
					SerializableGuid targetId2 = this.ReceiverInspector.TargetId;
					string memberName2 = this.ReceiverInspector.NodeEvents[this.ReceiverEventIndex].MemberName;
					if (WiringUtilities.GetWiringId(targetId, memberName, targetId2, memberName2) != SerializableGuid.Empty)
					{
						this.WiresView.DespawnTempWire();
						this.WiresView.ToggleDarkMode(false, null);
						this.ResetEmitterEventIndex(true);
						this.ResetReceiverEventIndex(true);
						return false;
					}
				}
				return flag;
			}
		}

		// Token: 0x170001E3 RID: 483
		// (get) Token: 0x06000DA3 RID: 3491 RVA: 0x000410ED File Offset: 0x0003F2ED
		// (set) Token: 0x06000DA4 RID: 3492 RVA: 0x000410F5 File Offset: 0x0003F2F5
		private protected WiringTool WiringTool { protected get; private set; }

		// Token: 0x170001E4 RID: 484
		// (get) Token: 0x06000DA5 RID: 3493 RVA: 0x0004061F File Offset: 0x0003E81F
		protected UIWiringManager WiringManager
		{
			get
			{
				return MonoBehaviourSingleton<UIWiringManager>.Instance;
			}
		}

		// Token: 0x170001E5 RID: 485
		// (get) Token: 0x06000DA6 RID: 3494 RVA: 0x000410FE File Offset: 0x0003F2FE
		protected UIWiringObjectInspectorView EmitterInspector
		{
			get
			{
				return this.WiringManager.EmitterInspector;
			}
		}

		// Token: 0x170001E6 RID: 486
		// (get) Token: 0x06000DA7 RID: 3495 RVA: 0x0004110B File Offset: 0x0003F30B
		protected UIWiringObjectInspectorView ReceiverInspector
		{
			get
			{
				return this.WiringManager.ReceiverInspector;
			}
		}

		// Token: 0x170001E7 RID: 487
		// (get) Token: 0x06000DA8 RID: 3496 RVA: 0x00041118 File Offset: 0x0003F318
		protected UIWiresView WiresView
		{
			get
			{
				return this.WiringManager.WiresView;
			}
		}

		// Token: 0x06000DA9 RID: 3497 RVA: 0x00041128 File Offset: 0x0003F328
		private void Start()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("Start", this);
			}
			this.WiringTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<WiringTool>();
			this.EmitterInspector.OnDisplay.AddListener(new UnityAction(this.OnEmitterInspectorDisplayed));
			this.ReceiverInspector.OnDisplay.AddListener(new UnityAction(this.OnReceiverInspectorDisplayed));
		}

		// Token: 0x06000DAA RID: 3498 RVA: 0x00041190 File Offset: 0x0003F390
		public void SetEmitterEventIndex(int newValue)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetEmitterEventIndex", "newValue", newValue), this);
			}
			if (this.EmitterEventIndex == newValue || newValue == -1)
			{
				this.ResetEmitterEventIndex(true);
				return;
			}
			int emitterEventIndex = this.EmitterEventIndex;
			if (emitterEventIndex > -1)
			{
				this.EmitterInspector.ToggleNodeSelectedVisuals(emitterEventIndex, false);
			}
			this.EmitterEventIndex = newValue;
			this.EmitterInspector.ToggleNodeSelectedVisuals(newValue, true);
		}

		// Token: 0x06000DAB RID: 3499 RVA: 0x00041208 File Offset: 0x0003F408
		public void SetReceiverEventIndex(int newValue)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "SetReceiverEventIndex", "newValue", newValue), this);
			}
			if (this.ReceiverEventIndex == newValue || newValue == -1)
			{
				this.ResetReceiverEventIndex(true);
				return;
			}
			int receiverEventIndex = this.ReceiverEventIndex;
			if (receiverEventIndex > -1)
			{
				this.ReceiverInspector.ToggleNodeSelectedVisuals(receiverEventIndex, false);
			}
			this.ReceiverEventIndex = newValue;
			this.ReceiverInspector.ToggleNodeSelectedVisuals(newValue, true);
		}

		// Token: 0x06000DAC RID: 3500 RVA: 0x00041280 File Offset: 0x0003F480
		public virtual void CreateWire(string[] storedParameterValues, WireColor wireColor)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2}, {3}: {4} )", new object[] { "CreateWire", "storedParameterValues", storedParameterValues.Length, "wireColor", wireColor }), this);
				for (int i = 0; i < storedParameterValues.Length; i++)
				{
					DebugUtility.Log(string.Format("{0}[{1}]: {2}", "storedParameterValues", i, storedParameterValues[i]), this);
				}
			}
			if (this.EmitterEventIndex < 0 || this.EmitterEventIndex >= this.EmitterInspector.SpawnedNodes.Count)
			{
				DebugUtility.LogError(string.Format("{0} {1} is out of range for {2}.{3} with a Count of {4}", new object[]
				{
					"EmitterEventIndex",
					this.EmitterEventIndex,
					"EmitterInspector",
					"SpawnedNodes",
					this.EmitterInspector.SpawnedNodes.Count
				}), this);
				return;
			}
			if (this.ReceiverEventIndex < 0 || this.ReceiverEventIndex >= this.ReceiverInspector.SpawnedNodes.Count)
			{
				DebugUtility.LogError(string.Format("{0} {1} is out of range for {2}.{3} with a Count of {4}", new object[]
				{
					"ReceiverEventIndex",
					this.ReceiverEventIndex,
					"ReceiverInspector",
					"SpawnedNodes",
					this.ReceiverInspector.SpawnedNodes.Count
				}), this);
				return;
			}
			UIWireNodeView uiwireNodeView = this.EmitterInspector.SpawnedNodes[this.EmitterEventIndex];
			UIWireNodeView uiwireNodeView2 = this.ReceiverInspector.SpawnedNodes[this.ReceiverEventIndex];
			this.WiringTool.EventConfirmed(uiwireNodeView.NodeEvent, uiwireNodeView.AssemblyQualifiedTypeName, uiwireNodeView2.NodeEvent, uiwireNodeView2.AssemblyQualifiedTypeName, storedParameterValues, wireColor);
			this.WiresView.DespawnAllWires();
			this.WiresView.Display();
		}

		// Token: 0x06000DAD RID: 3501 RVA: 0x00041458 File Offset: 0x0003F658
		public virtual void Restart(bool displayToolPrompt = true)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "Restart", "displayToolPrompt", displayToolPrompt), this);
			}
			this.ResetEmitterEventIndex(displayToolPrompt);
			this.ResetReceiverEventIndex(displayToolPrompt);
			this.WiresView.DespawnTempWire();
			this.WiresView.ToggleDarkMode(false, null);
			this.WiringManager.SetWiringState(UIWiringStates.Nothing);
			if (displayToolPrompt)
			{
				MonoBehaviourSingleton<UIWiringPrompterManager>.Instance.DisplayToolPrompt();
			}
		}

		// Token: 0x06000DAE RID: 3502 RVA: 0x000414CC File Offset: 0x0003F6CC
		protected virtual void ResetEmitterEventIndex(bool displayToolPrompt = true)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "ResetEmitterEventIndex", "displayToolPrompt", displayToolPrompt), this);
			}
			if (this.EmitterEventIndex > -1)
			{
				this.EmitterInspector.ToggleNodeSelectedVisuals(this.EmitterEventIndex, false);
			}
			this.EmitterEventIndex = -1;
			if (displayToolPrompt)
			{
				MonoBehaviourSingleton<UIWiringPrompterManager>.Instance.DisplayToolPrompt();
			}
		}

		// Token: 0x06000DAF RID: 3503 RVA: 0x00041530 File Offset: 0x0003F730
		protected virtual void ResetReceiverEventIndex(bool displayToolPrompt = true)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "ResetReceiverEventIndex", "displayToolPrompt", displayToolPrompt), this);
			}
			if (this.ReceiverEventIndex > -1)
			{
				this.ReceiverInspector.ToggleNodeSelectedVisuals(this.ReceiverEventIndex, false);
			}
			this.ReceiverEventIndex = -1;
			if (displayToolPrompt)
			{
				MonoBehaviourSingleton<UIWiringPrompterManager>.Instance.DisplayToolPrompt();
			}
		}

		// Token: 0x06000DB0 RID: 3504 RVA: 0x00041594 File Offset: 0x0003F794
		private void OnEmitterInspectorDisplayed()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("OnEmitterInspectorDisplayed", this);
			}
			this.ResetReceiverEventIndex(true);
			this.ResetEmitterEventIndex(true);
		}

		// Token: 0x06000DB1 RID: 3505 RVA: 0x000415B7 File Offset: 0x0003F7B7
		private void OnReceiverInspectorDisplayed()
		{
			if (this.VerboseLogging)
			{
				DebugUtility.Log("OnReceiverInspectorDisplayed", this);
			}
			this.ResetReceiverEventIndex(true);
		}

		// Token: 0x04000BB9 RID: 3001
		[SerializeField]
		protected UIWireConfirmationModalView WireConfirmationModal;
	}
}
