using Endless.Creator.DynamicPropCreation;
using Endless.Shared.DataTypes;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI;

public class UIAbstractPropCreationModalView : UIBasePropCreationModalView
{
	[Header("UIAbstractPropCreationModalView")]
	[SerializeField]
	private Image iconDisplay;

	private SerializableGuid iconId;

	public AbstractPropCreationScreenData AbstractPropCreationScreenData { get; private set; }

	public Texture2D FinalTexture { get; private set; }

	public SerializableGuid IconId
	{
		get
		{
			return iconId;
		}
		set
		{
			iconId = value;
			FinalTexture = AbstractPropIconUtility.MergeIcon(AbstractPropCreationScreenData.Icon.texture, IconId);
			Rect rect = new Rect(0f, 0f, FinalTexture.width, FinalTexture.height);
			iconDisplay.sprite = Sprite.Create(FinalTexture, rect, new Vector2(0.5f, 0.5f), 100f);
		}
	}

	public override void OnDisplay(params object[] modalData)
	{
		base.OnDisplay(modalData);
		AbstractPropCreationScreenData = (AbstractPropCreationScreenData)modalData[0];
		IconId = AbstractPropCreationScreenData.DefaultIconGuid;
	}
}
