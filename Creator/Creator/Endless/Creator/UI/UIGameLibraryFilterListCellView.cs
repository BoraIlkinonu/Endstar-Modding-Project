using System;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI
{
	// Token: 0x02000113 RID: 275
	public class UIGameLibraryFilterListCellView : UIBaseListCellView<UIGameAssetTypes>
	{
		// Token: 0x06000460 RID: 1120 RVA: 0x0001A028 File Offset: 0x00018228
		public override void View(UIBaseListView<UIGameAssetTypes> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			bool flag = (base.ListModel as UIGameLibraryFilterListModel).DoNotAllowNoneSelection && base.Model == UIGameAssetTypes.None;
			this.displayName.enabled = !flag;
			UIGameAssetTypeStyle uigameAssetTypeStyle = this.gameAssetTypeStyleDictionary[base.Model];
			this.displayName.text = uigameAssetTypeStyle.DisplayName;
			this.iconImage.sprite = uigameAssetTypeStyle.IconSprite;
			Color color = uigameAssetTypeStyle.Color;
			foreach (Image image in this.imagesToColorByType)
			{
				image.color = new Color(color.r, color.g, color.b, image.color.a);
			}
			TweenTextColor[] array2 = this.typeTextColorTweens;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].To = color;
			}
			TweenGraphicColor[] array3 = this.typeGraphicColorTweens;
			for (int i = 0; i < array3.Length; i++)
			{
				array3[i].To = color;
			}
			if (base.IsSelected)
			{
				this.selectedTweenCollection.Tween();
				return;
			}
			this.notSelectedTweenCollection.Tween();
		}

		// Token: 0x0400043A RID: 1082
		[Header("UIGameLibraryFilterListCellView")]
		[SerializeField]
		private TextMeshProUGUI displayName;

		// Token: 0x0400043B RID: 1083
		[SerializeField]
		private Image iconImage;

		// Token: 0x0400043C RID: 1084
		[SerializeField]
		private Image[] imagesToColorByType = Array.Empty<Image>();

		// Token: 0x0400043D RID: 1085
		[SerializeField]
		private TweenTextColor[] typeTextColorTweens = Array.Empty<TweenTextColor>();

		// Token: 0x0400043E RID: 1086
		[SerializeField]
		private TweenGraphicColor[] typeGraphicColorTweens = Array.Empty<TweenGraphicColor>();

		// Token: 0x0400043F RID: 1087
		[SerializeField]
		private TweenCollection selectedTweenCollection;

		// Token: 0x04000440 RID: 1088
		[SerializeField]
		private TweenCollection notSelectedTweenCollection;

		// Token: 0x04000441 RID: 1089
		[SerializeField]
		private UIGameAssetTypeStyleDictionary gameAssetTypeStyleDictionary;
	}
}
