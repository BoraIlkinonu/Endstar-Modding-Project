using System;
using Endless.Gameplay.LuaInterfaces;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020000E8 RID: 232
	public interface IPhysicsTaker : IScriptInjector
	{
		// Token: 0x170000DA RID: 218
		// (get) Token: 0x0600053D RID: 1341
		Vector3 CenterOfMassOffset { get; }

		// Token: 0x0600053E RID: 1342
		void TakePhysicsForce(float force, Vector3 directionNormal, uint startFrame, ulong source, bool forceFreeFall = false, bool friendlyForce = false, bool applyRandomTorque = false);

		// Token: 0x170000DB RID: 219
		// (get) Token: 0x0600053F RID: 1343
		float GravityAccelerationRate { get; }

		// Token: 0x170000DC RID: 220
		// (get) Token: 0x06000540 RID: 1344
		float Mass { get; }

		// Token: 0x170000DD RID: 221
		// (get) Token: 0x06000541 RID: 1345
		float Drag { get; }

		// Token: 0x170000DE RID: 222
		// (get) Token: 0x06000542 RID: 1346
		float AirborneDrag { get; }

		// Token: 0x170000DF RID: 223
		// (get) Token: 0x06000543 RID: 1347
		Vector3 CurrentVelocity { get; }

		// Token: 0x170000E0 RID: 224
		// (get) Token: 0x06000544 RID: 1348 RVA: 0x0001B40D File Offset: 0x0001960D
		float BlastForceMultiplier
		{
			get
			{
				return 1f;
			}
		}

		// Token: 0x170000E1 RID: 225
		// (get) Token: 0x06000545 RID: 1349
		object LuaObject { get; }

		// Token: 0x170000E2 RID: 226
		// (get) Token: 0x06000546 RID: 1350 RVA: 0x0001B414 File Offset: 0x00019614
		Type IScriptInjector.LuaObjectType
		{
			get
			{
				return typeof(PhysicsComponent);
			}
		}

		// Token: 0x0400040C RID: 1036
		public const float SMALL_PUSH_THRESHOLD = 4f;
	}
}
