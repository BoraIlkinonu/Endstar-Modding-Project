using System;
using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI
{
	// Token: 0x02000258 RID: 600
	public class UITilesetView : UIBaseView<Tileset, UITilesetView.Styles>
	{
		// Token: 0x1700013C RID: 316
		// (get) Token: 0x060009CC RID: 2508 RVA: 0x0002D568 File Offset: 0x0002B768
		// (set) Token: 0x060009CD RID: 2509 RVA: 0x0002D570 File Offset: 0x0002B770
		public override UITilesetView.Styles Style { get; protected set; }

		// Token: 0x060009CE RID: 2510 RVA: 0x0002D57C File Offset: 0x0002B77C
		public override void View(Tileset model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { model });
			}
			this.displayIconImage.sprite = model.DisplayIcon;
			this.displayNameText.text = model.DisplayName;
		}

		// Token: 0x060009CF RID: 2511 RVA: 0x0002D5C8 File Offset: 0x0002B7C8
		public override void Clear()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", Array.Empty<object>());
			}
			this.displayIconImage.sprite = null;
		}

		// Token: 0x04000806 RID: 2054
		[SerializeField]
		private Image displayIconImage;

		// Token: 0x04000807 RID: 2055
		[SerializeField]
		private TextMeshProUGUI displayNameText;

		// Token: 0x02000259 RID: 601
		public enum Styles
		{
			// Token: 0x04000809 RID: 2057
			Default
		}
	}
}
