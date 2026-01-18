using System;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI;

[Serializable]
public struct UIInteractionPrompt
{
	public UIDeviceTypeSpriteDictionary InteractionResultSprite;

	public Color InteractionResultColor;

	public string InteractionResultText;

	public Sprite supplementalInteractionResultSprite;
}
