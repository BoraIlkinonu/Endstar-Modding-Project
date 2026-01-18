using UnityEngine;

namespace Endless.Creator.DynamicPropCreation;

public abstract class PropCreationData : ScriptableObject
{
	[SerializeField]
	private string displayName;

	[SerializeField]
	private Sprite icon;

	public abstract bool IsSubMenu { get; }

	public Sprite Icon => icon;

	public string DisplayName => displayName;
}
