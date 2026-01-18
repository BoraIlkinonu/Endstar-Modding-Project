using System;
using Endless.Gameplay;
using Endless.Shared;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Creator.LevelEditing.Runtime
{
	// Token: 0x02000355 RID: 853
	public class GameEditorTool : EndlessTool
	{
		// Token: 0x17000277 RID: 631
		// (get) Token: 0x06001019 RID: 4121 RVA: 0x0002B55C File Offset: 0x0002975C
		public override ToolType ToolType
		{
			get
			{
				return ToolType.GameEditor;
			}
		}

		// Token: 0x17000278 RID: 632
		// (get) Token: 0x0600101A RID: 4122 RVA: 0x0001BF89 File Offset: 0x0001A189
		public override bool PerformsLineCast
		{
			get
			{
				return false;
			}
		}

		// Token: 0x0600101B RID: 4123 RVA: 0x0004B6E6 File Offset: 0x000498E6
		public override void HandleDeselected()
		{
			base.HandleDeselected();
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "HandleDeselected", Array.Empty<object>());
			}
			PlayerReferenceManager.LocalInstance.PlayerNetworkController.ToggleInput(true);
			MonoBehaviourSingleton<CameraController>.Instance.ToggleInput(true);
		}

		// Token: 0x0600101C RID: 4124 RVA: 0x0004B721 File Offset: 0x00049921
		public override void HandleSelected()
		{
			base.HandleSelected();
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "HandleSelected", Array.Empty<object>());
			}
			PlayerReferenceManager.LocalInstance.PlayerNetworkController.ToggleInput(false);
			MonoBehaviourSingleton<CameraController>.Instance.ToggleInput(false);
		}

		// Token: 0x0600101E RID: 4126 RVA: 0x0004B75C File Offset: 0x0004995C
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x0600101F RID: 4127 RVA: 0x00049EBA File Offset: 0x000480BA
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x06001020 RID: 4128 RVA: 0x0004B772 File Offset: 0x00049972
		protected internal override string __getTypeName()
		{
			return "GameEditorTool";
		}

		// Token: 0x04000D43 RID: 3395
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
