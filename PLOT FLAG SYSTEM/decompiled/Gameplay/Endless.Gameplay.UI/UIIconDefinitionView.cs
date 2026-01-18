using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Gameplay.UI;

public class UIIconDefinitionView : UIGameObject
{
	[SerializeField]
	private RawImage rawImage;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	public IconDefinition Model { get; private set; }

	public void View(IconDefinition model)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "View", model);
		}
		Model = model;
		if (!(model == null))
		{
			rawImage.texture = model.IconTexture;
		}
	}
}
