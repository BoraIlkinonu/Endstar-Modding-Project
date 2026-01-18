using System;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020000EA RID: 234
	public interface IThrowable
	{
		// Token: 0x06000548 RID: 1352
		void InitiateThrow(float force, Vector3 forwardDirectionNormal, uint currentFrame, NetworkObject thrower, Item sourceItem);
	}
}
