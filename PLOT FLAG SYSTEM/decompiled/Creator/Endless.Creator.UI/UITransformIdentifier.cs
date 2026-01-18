using Endless.Props;

namespace Endless.Creator.UI;

public struct UITransformIdentifier
{
	public readonly TransformIdentifier TransformIdentifier;

	public readonly string DisplayName;

	public UITransformIdentifier(TransformIdentifier transformIdentifier, string displayName)
	{
		TransformIdentifier = transformIdentifier;
		DisplayName = displayName;
	}
}
