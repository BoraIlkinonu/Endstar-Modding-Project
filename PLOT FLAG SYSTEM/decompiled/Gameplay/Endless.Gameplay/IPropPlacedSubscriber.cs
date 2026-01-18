using Endless.Shared.DataTypes;

namespace Endless.Gameplay;

public interface IPropPlacedSubscriber
{
	void PropPlaced(SerializableGuid instanceId, bool isCopy);
}
