using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI;

public class UITilesetDetailView : UIGameObject, IUIViewable<Tileset>, IClearable
{
	[Header("UITilesetDetailView")]
	[SerializeField]
	private Image displayIconImage;

	[SerializeField]
	private TextMeshProUGUI displayNameText;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[SerializeField]
	private TextMeshProUGUI versionText;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	public Tileset Model { get; private set; }

	public void View(Tileset model)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "View", model);
		}
		Model = model;
		displayIconImage.sprite = model.DisplayIcon;
		displayNameText.text = model.DisplayName;
		descriptionText.text = model.Description;
		versionText.text = model.Asset.AssetVersion;
	}

	public void Clear()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Clear");
		}
		displayIconImage.sprite = null;
	}
}
