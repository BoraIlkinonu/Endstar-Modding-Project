using System;
using Endless.Gameplay;
using Endless.Shared;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Creator.LevelEditing.Runtime
{
	// Token: 0x02000369 RID: 873
	public class LevelEditorTool : EndlessTool
	{
		// Token: 0x1700027C RID: 636
		// (get) Token: 0x06001092 RID: 4242 RVA: 0x0004F8DE File Offset: 0x0004DADE
		public override ToolType ToolType
		{
			get
			{
				return ToolType.LevelEditor;
			}
		}

		// Token: 0x1700027D RID: 637
		// (get) Token: 0x06001093 RID: 4243 RVA: 0x0001BF89 File Offset: 0x0001A189
		public override bool PerformsLineCast
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06001094 RID: 4244 RVA: 0x0004F8E2 File Offset: 0x0004DAE2
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

		// Token: 0x06001095 RID: 4245 RVA: 0x0004F91D File Offset: 0x0004DB1D
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

		// Token: 0x06001097 RID: 4247 RVA: 0x0004F958 File Offset: 0x0004DB58
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x06001098 RID: 4248 RVA: 0x00049EBA File Offset: 0x000480BA
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x06001099 RID: 4249 RVA: 0x0004F96E File Offset: 0x0004DB6E
		protected internal override string __getTypeName()
		{
			return "LevelEditorTool";
		}

		// Token: 0x04000DB4 RID: 3508
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
