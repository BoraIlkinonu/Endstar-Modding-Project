using Endless.Shared.DataTypes;
using UnityEngine;

namespace Endless.Gameplay;

public class IconDefinition : ScriptableObject
{
	[SerializeField]
	private SerializableGuid iconId;

	[SerializeField]
	private Texture2D iconTexture;

	public SerializableGuid IconId => iconId;

	public Texture2D IconTexture => iconTexture;
}
