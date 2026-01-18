using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x0200012E RID: 302
	public class UILevelAssetListView : UIBaseListView<LevelAsset>, IRoleInteractable
	{
		// Token: 0x17000078 RID: 120
		// (get) Token: 0x060004C4 RID: 1220 RVA: 0x0001B48E File Offset: 0x0001968E
		// (set) Token: 0x060004C5 RID: 1221 RVA: 0x0001B496 File Offset: 0x00019696
		public UILevelAssetListView.SelectActions SelectAction { get; private set; }

		// Token: 0x17000079 RID: 121
		// (get) Token: 0x060004C6 RID: 1222 RVA: 0x0001B49F File Offset: 0x0001969F
		// (set) Token: 0x060004C7 RID: 1223 RVA: 0x0001B4A7 File Offset: 0x000196A7
		public bool LocalUserCanInteract { get; private set; }

		// Token: 0x060004C8 RID: 1224 RVA: 0x0001B4B0 File Offset: 0x000196B0
		protected override void Start()
		{
			base.Start();
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.SetCellSourceBasedOnLocalUserCanInteract();
		}

		// Token: 0x060004C9 RID: 1225 RVA: 0x0001B4D6 File Offset: 0x000196D6
		public void SetLocalUserCanInteract(bool localUserCanInteract)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetLocalUserCanInteract", new object[] { localUserCanInteract });
			}
			if (this.LocalUserCanInteract == localUserCanInteract)
			{
				return;
			}
			this.LocalUserCanInteract = localUserCanInteract;
			this.SetCellSourceBasedOnLocalUserCanInteract();
		}

		// Token: 0x060004CA RID: 1226 RVA: 0x0001B511 File Offset: 0x00019711
		private void SetCellSourceBasedOnLocalUserCanInteract()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetCellSourceBasedOnLocalUserCanInteract", Array.Empty<object>());
			}
			base.SetCellSource(this.LocalUserCanInteract ? this.draggableLevelAssetListRowSource : this.staticLevelAssetListRowSource);
		}

		// Token: 0x04000471 RID: 1137
		[Header("UILevelAssetListView")]
		[SerializeField]
		private UILevelAssetListRowView staticLevelAssetListRowSource;

		// Token: 0x04000472 RID: 1138
		[SerializeField]
		private UILevelAssetListRowView draggableLevelAssetListRowSource;

		// Token: 0x0200012F RID: 303
		public enum SelectActions
		{
			// Token: 0x04000475 RID: 1141
			OpenLevelLoaderModal,
			// Token: 0x04000476 RID: 1142
			StartEditMatch
		}
	}
}
