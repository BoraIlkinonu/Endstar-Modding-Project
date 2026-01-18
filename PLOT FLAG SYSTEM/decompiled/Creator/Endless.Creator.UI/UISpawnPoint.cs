using Endless.Shared.DataTypes;

namespace Endless.Creator.UI;

public struct UISpawnPoint
{
	public SerializableGuid Id;

	public readonly string DisplayName;

	public UISpawnPoint(SerializableGuid id, string displayName)
	{
		Id = id;
		DisplayName = displayName;
	}
}
