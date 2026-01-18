using System;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI;

public class UIGameLibraryFilterListCellView : UIBaseListCellView<UIGameAssetTypes>
{
	[Header("UIGameLibraryFilterListCellView")]
	[SerializeField]
	private TextMeshProUGUI displayName;

	[SerializeField]
	private Image iconImage;

	[SerializeField]
	private Image[] imagesToColorByType = Array.Empty<Image>();

	[SerializeField]
	private TweenTextColor[] typeTextColorTweens = Array.Empty<TweenTextColor>();

	[SerializeField]
	private TweenGraphicColor[] typeGraphicColorTweens = Array.Empty<TweenGraphicColor>();

	[SerializeField]
	private TweenCollection selectedTweenCollection;

	[SerializeField]
	private TweenCollection notSelectedTweenCollection;

	[SerializeField]
	private UIGameAssetTypeStyleDictionary gameAssetTypeStyleDictionary;

	public override void View(UIBaseListView<UIGameAssetTypes> listView, int dataIndex)
	{
		base.View(listView, dataIndex);
		bool flag = (base.ListModel as UIGameLibraryFilterListModel).DoNotAllowNoneSelection && base.Model == UIGameAssetTypes.None;
		displayName.enabled = !flag;
		UIGameAssetTypeStyle uIGameAssetTypeStyle = gameAssetTypeStyleDictionary[base.Model];
		displayName.text = uIGameAssetTypeStyle.DisplayName;
		iconImage.sprite = uIGameAssetTypeStyle.IconSprite;
		Color color = uIGameAssetTypeStyle.Color;
		Image[] array = imagesToColorByType;
		foreach (Image image in array)
		{
			image.color = new Color(color.r, color.g, color.b, image.color.a);
		}
		TweenTextColor[] array2 = typeTextColorTweens;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].To = color;
		}
		TweenGraphicColor[] array3 = typeGraphicColorTweens;
		for (int i = 0; i < array3.Length; i++)
		{
			array3[i].To = color;
		}
		if (base.IsSelected)
		{
			selectedTweenCollection.Tween();
		}
		else
		{
			notSelectedTweenCollection.Tween();
		}
	}
}
