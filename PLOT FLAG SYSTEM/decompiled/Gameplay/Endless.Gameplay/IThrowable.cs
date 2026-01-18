using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public interface IThrowable
{
	void InitiateThrow(float force, Vector3 forwardDirectionNormal, uint currentFrame, NetworkObject thrower, Item sourceItem);
}
