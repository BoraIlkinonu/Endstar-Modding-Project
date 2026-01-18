using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020001BC RID: 444
	public class UILevelLoaderModalView : UIEscapableModalView
	{
		// Token: 0x170000B7 RID: 183
		// (get) Token: 0x06000699 RID: 1689 RVA: 0x00021EE7 File Offset: 0x000200E7
		// (set) Token: 0x0600069A RID: 1690 RVA: 0x00021EEF File Offset: 0x000200EF
		public SerializableGuid LevelId { get; private set; } = SerializableGuid.Empty;

		// Token: 0x0600069B RID: 1691 RVA: 0x00021EF8 File Offset: 0x000200F8
		public override void OnDisplay(params object[] modalData)
		{
			base.OnDisplay(modalData);
			if (MatchmakingClientController.Instance.ActiveGameId == SerializableGuid.Empty)
			{
				DebugUtility.LogError(this, "OnSpawn", "MatchmakingClientController's ActiveGameId has no value!", Array.Empty<object>());
				return;
			}
			if (MatchmakingClientController.Instance.LocalMatch == null)
			{
				DebugUtility.LogError(this, "OnSpawn", "MatchmakingClientController's LocalMatch has no value!", Array.Empty<object>());
				return;
			}
			LevelAsset levelAsset = (LevelAsset)modalData[0];
			this.levelNameText.text = levelAsset.Name;
			this.levelDescriptionText.text = levelAsset.Description;
			this.LevelId = levelAsset.AssetID;
			if (this.levelDescriptionText.text.IsNullOrEmptyOrWhiteSpace())
			{
				this.levelDescriptionText.text = "<color=#B5B5B5>(There is none)</color>";
			}
		}

		// Token: 0x040005E9 RID: 1513
		[SerializeField]
		private TextMeshProUGUI levelNameText;

		// Token: 0x040005EA RID: 1514
		[SerializeField]
		private TextMeshProUGUI levelDescriptionText;
	}
}
