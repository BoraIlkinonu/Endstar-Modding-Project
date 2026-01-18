using System.Collections.Generic;
using Endless.Shared;

namespace Endless.Gameplay;

public class NetworkedWorldObjectMap : MonoBehaviourSingleton<NetworkedWorldObjectMap>
{
	public Dictionary<uint, WorldObject> ObjectMap { get; } = new Dictionary<uint, WorldObject>();
}
