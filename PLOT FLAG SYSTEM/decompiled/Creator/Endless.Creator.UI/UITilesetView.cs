using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI;

public class UITilesetView : UIBaseView<Tileset, UITilesetView.Styles>
{
	public enum Styles
	{
		Default
	}

	[SerializeField]
	private Image displayIconImage;

	[SerializeField]
	private TextMeshProUGUI displayNameText;

	[field: Header("UITilesetView")]
	[field: SerializeField]
	public override Styles Style { get; protected set; }

	public override void View(Tileset model)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "View", model);
		}
		displayIconImage.sprite = model.DisplayIcon;
		displayNameText.text = model.DisplayName;
	}

	public override void Clear()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Clear");
		}
		displayIconImage.sprite = null;
	}
}
