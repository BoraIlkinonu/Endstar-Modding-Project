using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x0200012B RID: 299
	public class UILevelAssetListController : UIBaseLocalFilterableListController<LevelAsset>
	{
		// Token: 0x060004AE RID: 1198 RVA: 0x0001AF55 File Offset: 0x00019155
		protected override void Start()
		{
			base.Start();
			this.showArchivedToggled.OnChange.AddListener(new UnityAction<bool>(this.OnShowArchivedToggledChanged));
		}

		// Token: 0x060004AF RID: 1199 RVA: 0x0001AF7C File Offset: 0x0001917C
		protected override bool IncludeInFilteredResults(LevelAsset item)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "IncludeInFilteredResults", new object[] { item.Name });
			}
			if (item == null)
			{
				DebugUtility.LogError("LevelAsset was null!", this);
				return false;
			}
			string text = item.Name;
			if (!base.CaseSensitive)
			{
				text = text.ToLower();
			}
			string text2 = base.StringFilter;
			if (!base.CaseSensitive)
			{
				text2 = text2.ToLower();
			}
			return text.Contains(text2);
		}

		// Token: 0x060004B0 RID: 1200 RVA: 0x0001AFEE File Offset: 0x000191EE
		private void OnShowArchivedToggledChanged(bool showArchived)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnShowArchivedToggledChanged", new object[] { showArchived });
			}
			throw new NotImplementedException();
		}

		// Token: 0x04000466 RID: 1126
		[Header("UILevelAssetListController")]
		[SerializeField]
		private UIToggle showArchivedToggled;
	}
}
