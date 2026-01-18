using System;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI
{
	// Token: 0x020001F7 RID: 503
	public class UIPropEntryView : UIBaseView<PropEntry, UIPropEntryView.Styles>
	{
		// Token: 0x170000EB RID: 235
		// (get) Token: 0x060007E2 RID: 2018 RVA: 0x00026DB8 File Offset: 0x00024FB8
		// (set) Token: 0x060007E3 RID: 2019 RVA: 0x00026DC0 File Offset: 0x00024FC0
		public override UIPropEntryView.Styles Style { get; protected set; }

		// Token: 0x170000EC RID: 236
		// (get) Token: 0x060007E4 RID: 2020 RVA: 0x00026DC9 File Offset: 0x00024FC9
		// (set) Token: 0x060007E5 RID: 2021 RVA: 0x00026DD0 File Offset: 0x00024FD0
		public static PropEntry UseContext { get; private set; } = new PropEntry
		{
			InstanceId = UIPropEntryView.useContextPropEntryInstanceId,
			Label = "Use Context"
		};

		// Token: 0x060007E6 RID: 2022 RVA: 0x00026DD8 File Offset: 0x00024FD8
		public override void View(PropEntry model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { model });
			}
			this.labelText.text = model.Label;
			if (model.InstanceId == UIPropEntryView.useContextPropEntryInstanceId)
			{
				this.iconImage.sprite = this.useContextIconImage;
				return;
			}
			try
			{
				PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(model.AssetId);
				this.iconImage.sprite = runtimePropInfo.Icon;
			}
			catch (Exception)
			{
				this.labelText.text = "null";
				this.iconImage.sprite = this.nullDisplayIconImage;
			}
		}

		// Token: 0x060007E7 RID: 2023 RVA: 0x00026E98 File Offset: 0x00025098
		public override void Clear()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", Array.Empty<object>());
			}
			this.iconImage.sprite = null;
		}

		// Token: 0x040006FB RID: 1787
		private static readonly SerializableGuid useContextPropEntryInstanceId = SerializableGuid.NewGuid();

		// Token: 0x040006FD RID: 1789
		[SerializeField]
		private Image iconImage;

		// Token: 0x040006FE RID: 1790
		[SerializeField]
		private TextMeshProUGUI labelText;

		// Token: 0x040006FF RID: 1791
		[SerializeField]
		private Sprite nullDisplayIconImage;

		// Token: 0x04000700 RID: 1792
		[SerializeField]
		private Sprite useContextIconImage;

		// Token: 0x020001F8 RID: 504
		public enum Styles
		{
			// Token: 0x04000703 RID: 1795
			Default
		}
	}
}
