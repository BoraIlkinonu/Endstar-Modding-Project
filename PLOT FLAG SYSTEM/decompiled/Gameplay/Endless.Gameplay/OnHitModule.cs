using UnityEngine;

namespace Endless.Gameplay;

public abstract class OnHitModule : MonoBehaviour
{
	public abstract void Hit(uint frame, WorldObject shooter, WorldObject hitObject, Vector3 hitPosition, Vector3 travelDirection);
}
