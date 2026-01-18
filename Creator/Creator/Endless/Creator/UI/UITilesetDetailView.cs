using System;
using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI
{
	// Token: 0x02000256 RID: 598
	public class UITilesetDetailView : UIGameObject, IUIViewable<Tileset>, IClearable
	{
		// Token: 0x1700013B RID: 315
		// (get) Token: 0x060009C6 RID: 2502 RVA: 0x0002D4AD File Offset: 0x0002B6AD
		// (set) Token: 0x060009C7 RID: 2503 RVA: 0x0002D4B5 File Offset: 0x0002B6B5
		public Tileset Model { get; private set; }

		// Token: 0x060009C8 RID: 2504 RVA: 0x0002D4C0 File Offset: 0x0002B6C0
		public void View(Tileset model)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { model });
			}
			this.Model = model;
			this.displayIconImage.sprite = model.DisplayIcon;
			this.displayNameText.text = model.DisplayName;
			this.descriptionText.text = model.Description;
			this.versionText.text = model.Asset.AssetVersion;
		}

		// Token: 0x060009C9 RID: 2505 RVA: 0x0002D53A File Offset: 0x0002B73A
		public void Clear()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", Array.Empty<object>());
			}
			this.displayIconImage.sprite = null;
		}

		// Token: 0x040007FF RID: 2047
		[Header("UITilesetDetailView")]
		[SerializeField]
		private Image displayIconImage;

		// Token: 0x04000800 RID: 2048
		[SerializeField]
		private TextMeshProUGUI displayNameText;

		// Token: 0x04000801 RID: 2049
		[SerializeField]
		private TextMeshProUGUI descriptionText;

		// Token: 0x04000802 RID: 2050
		[SerializeField]
		private TextMeshProUGUI versionText;

		// Token: 0x04000803 RID: 2051
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
