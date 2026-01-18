using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.LuaInterfaces;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000149 RID: 329
	public class PhysicsTaker : NpcComponent, IPhysicsTaker, IScriptInjector
	{
		// Token: 0x17000170 RID: 368
		// (get) Token: 0x060007C3 RID: 1987 RVA: 0x0002495F File Offset: 0x00022B5F
		private float CurrentDrag
		{
			get
			{
				if (!base.NpcEntity.Components.Grounding.IsGrounded)
				{
					return this.AirborneDrag;
				}
				return this.Drag;
			}
		}

		// Token: 0x17000171 RID: 369
		// (get) Token: 0x060007C4 RID: 1988 RVA: 0x00024985 File Offset: 0x00022B85
		public Vector3 CenterOfMassOffset
		{
			get
			{
				return new Vector3(0f, 0.4f, 0f);
			}
		}

		// Token: 0x17000172 RID: 370
		// (get) Token: 0x060007C5 RID: 1989 RVA: 0x0002499B File Offset: 0x00022B9B
		public bool HasPhysicsImpulse
		{
			get
			{
				return this.currentPhysicsForces.Count > 0;
			}
		}

		// Token: 0x17000173 RID: 371
		// (get) Token: 0x060007C6 RID: 1990 RVA: 0x000249AB File Offset: 0x00022BAB
		// (set) Token: 0x060007C7 RID: 1991 RVA: 0x000249B3 File Offset: 0x00022BB3
		public Vector3 CurrentPhysics { get; private set; }

		// Token: 0x17000174 RID: 372
		// (get) Token: 0x060007C8 RID: 1992 RVA: 0x000249BC File Offset: 0x00022BBC
		// (set) Token: 0x060007C9 RID: 1993 RVA: 0x000249C4 File Offset: 0x00022BC4
		public Vector3 CurrentXzPhysics { get; private set; }

		// Token: 0x17000175 RID: 373
		// (get) Token: 0x060007CA RID: 1994 RVA: 0x0001DBBB File Offset: 0x0001BDBB
		public float GravityAccelerationRate
		{
			get
			{
				return -Physics.gravity.y;
			}
		}

		// Token: 0x17000176 RID: 374
		// (get) Token: 0x060007CB RID: 1995 RVA: 0x0001B40D File Offset: 0x0001960D
		public float Mass
		{
			get
			{
				return 1f;
			}
		}

		// Token: 0x17000177 RID: 375
		// (get) Token: 0x060007CC RID: 1996 RVA: 0x000249CD File Offset: 0x00022BCD
		public float Drag
		{
			get
			{
				return this.groundedDrag;
			}
		}

		// Token: 0x17000178 RID: 376
		// (get) Token: 0x060007CD RID: 1997 RVA: 0x000249D5 File Offset: 0x00022BD5
		public float AirborneDrag
		{
			get
			{
				return this.airborneDrag;
			}
		}

		// Token: 0x17000179 RID: 377
		// (get) Token: 0x060007CE RID: 1998 RVA: 0x0001DBB4 File Offset: 0x0001BDB4
		public Vector3 CurrentVelocity
		{
			get
			{
				return Vector3.zero;
			}
		}

		// Token: 0x1700017A RID: 378
		// (get) Token: 0x060007CF RID: 1999 RVA: 0x000249DD File Offset: 0x00022BDD
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

		// Token: 0x060007D0 RID: 2000 RVA: 0x000249FC File Offset: 0x00022BFC
		public void TakePhysicsForce(float force, Vector3 directionNormal, uint startFrame, ulong source, bool forceFreeFall = false, bool friendlyForce = false, bool applyRandomTorque = false)
		{
			if (!base.IsServer || !base.NpcEntity.IsConfigured || base.NpcEntity.PhysicsMode == PhysicsMode.IgnorePhysics)
			{
				return;
			}
			if (force <= 4f)
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
			}
			PhysicsForceInfo physicsForceInfo = new PhysicsForceInfo
			{
				Force = force,
				DirectionNormal = directionNormal,
				StartFrame = startFrame,
				SourceID = source,
				ApplyRandomTorque = applyRandomTorque
			};
			this.AddPhysicsForce(physicsForceInfo);
			if (physicsForceInfo.Force > 4f || forceFreeFall)
			{
				base.NpcEntity.Components.Grounding.ForceUnground();
			}
			base.NpcEntity.Components.Parameters.PhysicsTrigger = true;
		}

		// Token: 0x060007D1 RID: 2001 RVA: 0x00024B1C File Offset: 0x00022D1C
		private void AddPhysicsForce(PhysicsForceInfo info)
		{
			for (int i = 0; i < this.currentPhysicsForces.Count; i++)
			{
				if (this.currentPhysicsForces[i].SourceID == info.SourceID && NetClock.CurrentFrame - this.currentPhysicsForces[i].StartFrame < 3U)
				{
					return;
				}
			}
			this.currentPhysicsForces.Add(info);
		}

		// Token: 0x060007D2 RID: 2002 RVA: 0x00024B7F File Offset: 0x00022D7F
		private void Update()
		{
			this.CurrentPhysics = this.GetUpdatePhysics();
			this.CurrentXzPhysics = new Vector3(this.CurrentPhysics.x, 0f, this.CurrentPhysics.z);
		}

		// Token: 0x060007D3 RID: 2003 RVA: 0x00024BB4 File Offset: 0x00022DB4
		private Vector3 GetUpdatePhysics()
		{
			Vector3 vector = Vector3.zero;
			for (int i = 0; i < this.currentPhysicsForces.Count; i++)
			{
				PhysicsForceInfo physicsForceInfo = this.currentPhysicsForces[i];
				physicsForceInfo.Force *= 1f - Time.deltaTime * this.CurrentDrag;
				this.currentPhysicsForces[i] = physicsForceInfo;
				if (physicsForceInfo.Force > 0.1f)
				{
					vector += physicsForceInfo.DirectionNormal * physicsForceInfo.Force;
				}
				else
				{
					this.staleForces.Add(physicsForceInfo);
				}
			}
			foreach (PhysicsForceInfo physicsForceInfo2 in this.staleForces)
			{
				this.currentPhysicsForces.Remove(physicsForceInfo2);
			}
			this.staleForces.Clear();
			vector.x = Mathf.Clamp(vector.x, -50f, 50f);
			vector.y = Mathf.Clamp(vector.y, -50f, 50f);
			vector.z = Mathf.Clamp(vector.z, -50f, 50f);
			return vector;
		}

		// Token: 0x060007D4 RID: 2004 RVA: 0x00024CF8 File Offset: 0x00022EF8
		public void OnCollisionEnter(Collision other)
		{
			PhysicsCubeController component = other.gameObject.GetComponent<PhysicsCubeController>();
			if (component && Vector3.Dot(other.contacts[0].normal, Vector3.up) < 0.5f)
			{
				float num = Mathf.Max(3f, other.relativeVelocity.magnitude);
				this.TakePhysicsForce(num, other.relativeVelocity.normalized, NetClock.CurrentFrame, component.NetworkObjectId, false, false, false);
			}
		}

		// Token: 0x04000634 RID: 1588
		[SerializeField]
		private float groundedDrag;

		// Token: 0x04000635 RID: 1589
		[SerializeField]
		private float airborneDrag;

		// Token: 0x04000636 RID: 1590
		private readonly List<PhysicsForceInfo> currentPhysicsForces = new List<PhysicsForceInfo>();

		// Token: 0x04000637 RID: 1591
		private readonly List<PhysicsForceInfo> staleForces = new List<PhysicsForceInfo>();

		// Token: 0x0400063A RID: 1594
		private object luaObject;
	}
}
