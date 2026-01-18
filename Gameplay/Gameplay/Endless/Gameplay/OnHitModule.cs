using System;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000286 RID: 646
	public abstract class OnHitModule : MonoBehaviour
	{
		// Token: 0x06000DED RID: 3565
		public abstract void Hit(uint frame, WorldObject shooter, WorldObject hitObject, Vector3 hitPosition, Vector3 travelDirection);
	}
}
