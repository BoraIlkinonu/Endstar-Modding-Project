using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Props.Scripting;
using Endless.Shared;
using Endless.Shared.Debugging;

namespace Endless.Creator.UI
{
	// Token: 0x02000307 RID: 775
	public class UIWireCreatorController : UIBaseWireController
	{
		// Token: 0x06000DB3 RID: 3507 RVA: 0x000415E9 File Offset: 0x0003F7E9
		public override void Restart(bool displayToolPrompt = true)
		{
			base.Restart(displayToolPrompt);
			if (this.WireConfirmationModal.IsOpen)
			{
				this.WireConfirmationModal.Hide();
			}
		}

		// Token: 0x06000DB4 RID: 3508 RVA: 0x0004160C File Offset: 0x0003F80C
		public void DisplayWireConfirmation()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "DisplayWireConfirmation", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIToolPrompterManager>.Instance.Hide();
			string memberName = base.EmitterInspector.SpawnedNodes[base.EmitterEventIndex].MemberName;
			string memberName2 = base.ReceiverInspector.SpawnedNodes[base.ReceiverEventIndex].MemberName;
			EndlessEventInfo endlessEventInfo = base.ReceiverInspector.NodeEvents[base.ReceiverEventIndex];
			string assemblyQualifiedTypeName = base.EmitterInspector.SpawnedNodes[base.EmitterEventIndex].AssemblyQualifiedTypeName;
			string assemblyQualifiedTypeName2 = base.ReceiverInspector.SpawnedNodes[base.ReceiverEventIndex].AssemblyQualifiedTypeName;
			UIWireView uiwireView = base.WiresView.DisplayTempWire(assemblyQualifiedTypeName, memberName, assemblyQualifiedTypeName2, memberName2);
			if (base.WiringTool.ToolState == WiringTool.WiringToolState.Wiring)
			{
				this.WireConfirmationModal.Display(uiwireView, endlessEventInfo);
			}
			base.WiresView.ToggleDarkMode(true, uiwireView);
		}

		// Token: 0x06000DB5 RID: 3509 RVA: 0x000416FE File Offset: 0x0003F8FE
		protected override void ResetEmitterEventIndex(bool displayToolPrompt = true)
		{
			base.ResetEmitterEventIndex(displayToolPrompt);
			if (this.WireConfirmationModal.IsOpen)
			{
				this.WireConfirmationModal.Hide();
			}
		}

		// Token: 0x06000DB6 RID: 3510 RVA: 0x0004171F File Offset: 0x0003F91F
		protected override void ResetReceiverEventIndex(bool displayToolPrompt = true)
		{
			base.ResetReceiverEventIndex(displayToolPrompt);
			if (this.WireConfirmationModal.IsOpen)
			{
				this.WireConfirmationModal.Hide();
			}
		}
	}
}
