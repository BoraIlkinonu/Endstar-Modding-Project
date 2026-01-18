using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaInterfaces;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200027F RID: 639
	public class PlayerPhysicsTaker : NetworkBehaviour, IPhysicsTaker, IScriptInjector
	{
		// Token: 0x1700028A RID: 650
		// (get) Token: 0x06000DBD RID: 3517 RVA: 0x0004A344 File Offset: 0x00048544
		float IPhysicsTaker.GravityAccelerationRate
		{
			get
			{
				return this.playerController.GravityAccelerationRate;
			}
		}

		// Token: 0x1700028B RID: 651
		// (get) Token: 0x06000DBE RID: 3518 RVA: 0x0004A351 File Offset: 0x00048551
		float IPhysicsTaker.Mass
		{
			get
			{
				return this.playerController.Mass;
			}
		}

		// Token: 0x1700028C RID: 652
		// (get) Token: 0x06000DBF RID: 3519 RVA: 0x0004A35E File Offset: 0x0004855E
		float IPhysicsTaker.Drag
		{
			get
			{
				return this.playerController.Drag;
			}
		}

		// Token: 0x1700028D RID: 653
		// (get) Token: 0x06000DC0 RID: 3520 RVA: 0x0004A36B File Offset: 0x0004856B
		float IPhysicsTaker.AirborneDrag
		{
			get
			{
				return this.playerController.AirborneDrag;
			}
		}

		// Token: 0x1700028E RID: 654
		// (get) Token: 0x06000DC1 RID: 3521 RVA: 0x0004A378 File Offset: 0x00048578
		Vector3 IPhysicsTaker.CurrentVelocity
		{
			get
			{
				return this.playerController.CurrentState.TotalForce;
			}
		}

		// Token: 0x1700028F RID: 655
		// (get) Token: 0x06000DC2 RID: 3522 RVA: 0x0004A38A File Offset: 0x0004858A
		public Vector3 CenterOfMassOffset
		{
			get
			{
				return this.centerOfMassOffset;
			}
		}

		// Token: 0x17000290 RID: 656
		// (get) Token: 0x06000DC3 RID: 3523 RVA: 0x0004A392 File Offset: 0x00048592
		public object LuaObject
		{
			get
			{
				if (this.luaObject == null)
				{
					this.luaObject = new PhysicsComponent(this);
				}
				return this.luaObject;
			}
		}

		// Token: 0x06000DC4 RID: 3524 RVA: 0x0004A3B0 File Offset: 0x000485B0
		public void GetFramePhysics(uint frame, ref NetState currentState)
		{
			PlayerPhysicsTaker.PushState pushState = PlayerPhysicsTaker.PushState.None;
			if (currentState.PhysicsForces == null)
			{
				currentState.PhysicsForces = new List<PhysicsForceInfo>();
			}
			foreach (PhysicsForceInfo physicsForceInfo in this.newPhysicsForces)
			{
				currentState.PhysicsForces.Add(physicsForceInfo);
				currentState.TotalForce += physicsForceInfo.DirectionNormal * physicsForceInfo.Force;
				if (physicsForceInfo.FriendlyForce)
				{
					currentState.JumpFrame = 0;
					currentState.AirborneFrames = 0;
					if (physicsForceInfo.DirectionNormal.y > 0f)
					{
						currentState.Grounded = false;
					}
				}
			}
			this.newPhysicsForces.Clear();
			currentState.HasHostileForce = false;
			Vector3 vector = Vector3.zero;
			for (int i = 0; i < currentState.PhysicsForces.Count; i++)
			{
				if (currentState.PhysicsForces[i].StartFrame <= frame)
				{
					PhysicsForceInfo physicsForceInfo2 = currentState.PhysicsForces[i];
					physicsForceInfo2.Force *= 1f - NetClock.FixedDeltaTime * (currentState.Grounded ? ((IPhysicsTaker)this).Drag : ((IPhysicsTaker)this).AirborneDrag);
					if (physicsForceInfo2.Force > 4f)
					{
						pushState = PlayerPhysicsTaker.PushState.Large;
					}
					else if (pushState == PlayerPhysicsTaker.PushState.None)
					{
						pushState = PlayerPhysicsTaker.PushState.Small;
					}
					if (!physicsForceInfo2.FriendlyForce)
					{
						currentState.HasHostileForce = true;
					}
					vector += currentState.PhysicsForces[i].DirectionNormal * currentState.PhysicsForces[i].Force;
					currentState.PhysicsForces[i] = physicsForceInfo2;
				}
			}
			vector.x = Mathf.Clamp(vector.x, -50f, 50f);
			vector.y = Mathf.Clamp(vector.y, -50f, 50f);
			vector.z = Mathf.Clamp(vector.z, -50f, 50f);
			if (currentState.JumpFrame < 0 && pushState == PlayerPhysicsTaker.PushState.Small)
			{
				currentState.PhysicsForces.Clear();
			}
			else
			{
				currentState.PhysicsForces.RemoveAll((PhysicsForceInfo x) => x.Force < 0.1f);
			}
			if (currentState.Grounded && pushState == PlayerPhysicsTaker.PushState.Small && currentState.HasHostileForce)
			{
				float magnitude = vector.magnitude;
				vector.y = 0f;
				vector = magnitude * vector.normalized;
			}
			if (pushState == PlayerPhysicsTaker.PushState.Large)
			{
				currentState.JumpFrame = 0;
			}
			if (currentState.HasHostileForce && (pushState == PlayerPhysicsTaker.PushState.Large || (pushState == PlayerPhysicsTaker.PushState.Small && currentState.PhysicsPushState != PlayerPhysicsTaker.PushState.Small)))
			{
				currentState.PushFrame = this.forceRecoveryFrames;
			}
			else if (currentState.PushFrame > 0U)
			{
				currentState.PushFrame -= 1U;
			}
			currentState.PhysicsPushState = pushState;
			currentState.PushForceControl = 1f - currentState.PushFrame / this.forceRecoveryFrames;
		}

		// Token: 0x06000DC5 RID: 3525 RVA: 0x0004A6A0 File Offset: 0x000488A0
		public void TakePhysicsForce(float force, Vector3 directionNormal, uint startFrame, ulong source, bool forceFreeFall = false, bool friendlyForce = false, bool applyRandomTorque = false)
		{
			if (!friendlyForce && force <= 4f)
			{
				directionNormal.y = 0f;
				force = Mathf.Clamp(force * directionNormal.magnitude, 1f, 4f);
				directionNormal = directionNormal.normalized;
			}
			else
			{
				float y = directionNormal.y;
				float num = Mathf.Lerp(0.35f, 1f, Mathf.Abs(y));
				if (y < 0f)
				{
					num *= -1f;
				}
				directionNormal.y = num;
				directionNormal = directionNormal.normalized;
				if (base.IsServer && force > 4f)
				{
					this.playerController.CheckAndCancelReload();
				}
			}
			if (friendlyForce)
			{
				bool hasLargerFriendlyForce = false;
				this.newPhysicsForces.RemoveAll(delegate(PhysicsForceInfo x)
				{
					if (!x.FriendlyForce)
					{
						return false;
					}
					if (x.Force >= force)
					{
						hasLargerFriendlyForce = true;
						return false;
					}
					return true;
				});
				if (hasLargerFriendlyForce)
				{
					return;
				}
			}
			PhysicsForceInfo physicsForceInfo = new PhysicsForceInfo
			{
				Force = force,
				DirectionNormal = directionNormal,
				StartFrame = startFrame,
				SourceID = source,
				FriendlyForce = friendlyForce,
				ApplyRandomTorque = applyRandomTorque
			};
			this.newPhysicsForces.Add(physicsForceInfo);
		}

		// Token: 0x06000DC6 RID: 3526 RVA: 0x0004A7DE File Offset: 0x000489DE
		public void EndFrame()
		{
			this.newPhysicsForces.Clear();
		}

		// Token: 0x06000DC8 RID: 3528 RVA: 0x0004A820 File Offset: 0x00048A20
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x06000DC9 RID: 3529 RVA: 0x0000E74E File Offset: 0x0000C94E
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x06000DCA RID: 3530 RVA: 0x0004A836 File Offset: 0x00048A36
		protected internal override string __getTypeName()
		{
			return "PlayerPhysicsTaker";
		}

		// Token: 0x04000C98 RID: 3224
		private const float FORCE_KILLOFF_THRESHOLD = 0.1f;

		// Token: 0x04000C99 RID: 3225
		[SerializeField]
		private PlayerController playerController;

		// Token: 0x04000C9A RID: 3226
		[SerializeField]
		private Vector3 centerOfMassOffset = new Vector3(0f, 0.9f, 0f);

		// Token: 0x04000C9B RID: 3227
		[SerializeField]
		private uint forceRecoveryFrames = 20U;

		// Token: 0x04000C9C RID: 3228
		private List<PhysicsForceInfo> newPhysicsForces = new List<PhysicsForceInfo>();

		// Token: 0x04000C9D RID: 3229
		private object luaObject;

		// Token: 0x02000280 RID: 640
		public enum PushState : byte
		{
			// Token: 0x04000C9F RID: 3231
			None,
			// Token: 0x04000CA0 RID: 3232
			Small,
			// Token: 0x04000CA1 RID: 3233
			Large
		}
	}
}
