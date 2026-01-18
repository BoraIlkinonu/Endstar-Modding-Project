using System;
using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay.LuaInterfaces
{
	// Token: 0x0200045D RID: 1117
	public class PhysicsComponent
	{
		// Token: 0x06001BF2 RID: 7154 RVA: 0x0007CF90 File Offset: 0x0007B190
		public void AddImpulse(Context instigator, global::UnityEngine.Vector3 force, bool applyRandomTorque)
		{
			force.x = Mathf.Clamp(force.x, -50f, 50f);
			force.y = Mathf.Clamp(force.y, -50f, 50f);
			force.z = Mathf.Clamp(force.z, -50f, 50f);
			this.physicsTaker.TakePhysicsForce(force.magnitude, force.normalized, NetClock.CurrentFrame + 2U, 0UL, false, false, applyRandomTorque);
		}

		// Token: 0x06001BF3 RID: 7155 RVA: 0x0007D016 File Offset: 0x0007B216
		public void AddImpulse(Context instigator, global::UnityEngine.Vector3 force)
		{
			this.AddImpulse(instigator, force, false);
		}

		// Token: 0x06001BF4 RID: 7156 RVA: 0x0007D021 File Offset: 0x0007B221
		internal PhysicsComponent(IPhysicsTaker physicsTaker)
		{
			this.physicsTaker = physicsTaker;
		}

		// Token: 0x040015BF RID: 5567
		private IPhysicsTaker physicsTaker;
	}
}
