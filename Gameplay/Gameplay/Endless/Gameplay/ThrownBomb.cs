using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020002CE RID: 718
	public class ThrownBomb : EndlessNetworkBehaviour, IThrowable, IGameEndSubscriber, NetClock.ISimulateFrameEnvironmentSubscriber
	{
		// Token: 0x17000334 RID: 820
		// (get) Token: 0x06001045 RID: 4165 RVA: 0x00052473 File Offset: 0x00050673
		public NetworkObject OwnerEntity
		{
			get
			{
				return this.ownerEntity;
			}
		}

		// Token: 0x17000335 RID: 821
		// (get) Token: 0x06001046 RID: 4166 RVA: 0x0005247B File Offset: 0x0005067B
		// (set) Token: 0x06001047 RID: 4167 RVA: 0x00052483 File Offset: 0x00050683
		public WorldObject WorldObject { get; private set; }

		// Token: 0x06001048 RID: 4168 RVA: 0x0005248C File Offset: 0x0005068C
		public void InitiateThrow(float force, Vector3 directionNormal, uint currentFrame, NetworkObject thrower, Item sourceItem)
		{
			this.startFrame.Value = currentFrame + 5U;
			this.explosionFrame.Value = this.startFrame.Value + this.frameDelay;
			this.networkRigidbodyController.TakePhysicsForce(force, directionNormal, currentFrame + 1U, thrower.NetworkObjectId, false, false, false);
			this.ownerEntity = sourceItem.NetworkObject;
			ThrownBombItem thrownBombItem = (ThrownBombItem)sourceItem;
			this.thrower = thrower;
			if (thrownBombItem)
			{
				this.maxDamage.Value = thrownBombItem.DamageAtCenter;
				this.minDamage.Value = thrownBombItem.DamageAtEdge;
				this.maxDistance.Value = thrownBombItem.TotalBlastRadius;
				this.minDistance.Value = thrownBombItem.CenterRadius;
				this.maxForce.Value = thrownBombItem.CenterBlastForce;
				this.minForce.Value = thrownBombItem.EdgeBlastForce;
			}
		}

		// Token: 0x06001049 RID: 4169 RVA: 0x00052569 File Offset: 0x00050769
		public override void OnNetworkSpawn()
		{
			NetClock.Register(this);
			base.OnNetworkSpawn();
		}

		// Token: 0x0600104A RID: 4170 RVA: 0x00052577 File Offset: 0x00050777
		public override void OnNetworkDespawn()
		{
			NetClock.Unregister(this);
			base.OnNetworkDespawn();
		}

		// Token: 0x0600104B RID: 4171 RVA: 0x00052588 File Offset: 0x00050788
		private void Detonate(uint frame)
		{
			this.visible = false;
			int num = Physics.OverlapSphereNonAlloc(base.transform.position, this.maxDistance.Value, ThrownBomb.colliderCache, this.layerMask);
			this.hitPhysicsTakers.Clear();
			this.hitHittableComponents.Clear();
			for (int i = 0; i < num; i++)
			{
				HittableComponent hittableFromMap = MonoBehaviourSingleton<HittableMap>.Instance.GetHittableFromMap(ThrownBomb.colliderCache[i]);
				IPhysicsTaker physicsTaker;
				if (hittableFromMap != null)
				{
					physicsTaker = hittableFromMap.WorldObject.GetComponentInChildren<IPhysicsTaker>();
				}
				else
				{
					physicsTaker = ThrownBomb.colliderCache[i].GetComponent<IPhysicsTaker>();
				}
				if (!(this.networkRigidbodyController == (global::UnityEngine.Object)physicsTaker))
				{
					bool flag = physicsTaker != null;
					bool flag2 = hittableFromMap != null;
					if (flag || flag2)
					{
						Vector3 vector = ThrownBomb.colliderCache[i].transform.position;
						if (flag)
						{
							vector += physicsTaker.CenterOfMassOffset;
						}
						float num2 = Vector3.Distance(base.transform.position, vector);
						float num3 = Mathf.Max(0f, Mathf.InverseLerp(this.minDistance.Value, this.maxDistance.Value, num2));
						float num4 = this.dropoffCurve.Evaluate(num3);
						if (flag2 && !this.hitHittableComponents.Contains(hittableFromMap))
						{
							this.hitHittableComponents.Add(hittableFromMap);
							if (base.IsServer)
							{
								HealthModificationArgs healthModificationArgs = new HealthModificationArgs(Mathf.RoundToInt(Mathf.Lerp((float)this.minDamage.Value, (float)this.maxDamage.Value, num4)) * -1, this.thrower ? this.thrower.GetComponent<WorldObject>() : null, DamageType.Normal, HealthChangeType.Damage);
								hittableFromMap.ModifyHealth(healthModificationArgs);
							}
							else if (NetClock.CurrentFrame == frame)
							{
								NetworkObject componentInParent = ThrownBomb.colliderCache[i].GetComponentInParent<NetworkObject>();
								if (componentInParent != null && componentInParent.IsOwner)
								{
									HealthModificationArgs healthModificationArgs2 = new HealthModificationArgs(Mathf.RoundToInt(Mathf.Lerp((float)this.minDamage.Value, (float)this.maxDamage.Value, num4)) * -1, this.thrower ? this.thrower.GetComponent<WorldObject>() : null, DamageType.Normal, HealthChangeType.Damage);
									hittableFromMap.ModifyHealth(healthModificationArgs2);
								}
							}
						}
						if (flag && !this.hitPhysicsTakers.Contains(physicsTaker))
						{
							this.hitPhysicsTakers.Add(physicsTaker);
							float num5 = Mathf.Lerp(this.minForce.Value, this.maxForce.Value, num4);
							physicsTaker.TakePhysicsForce(num5, (ThrownBomb.colliderCache[i].transform.position + physicsTaker.CenterOfMassOffset - base.transform.position).normalized, frame + 1U, base.NetworkObject.NetworkObjectId, false, false, false);
						}
					}
				}
			}
			this.networkRigidbodyController.AppearanceController.SendMessage("SetDetonateFrame", frame);
		}

		// Token: 0x0600104C RID: 4172 RVA: 0x00052874 File Offset: 0x00050A74
		private void OnDrawGizmosSelected()
		{
			DebugExtension.DrawCircle(base.transform.position, Vector3.up, Color.red, this.minDistance.Value);
			float num = Mathf.InverseLerp(this.minForce.Value, this.maxForce.Value, 4f);
			float num2 = this.dropoffCurve.Evaluate(num);
			DebugExtension.DrawCircle(base.transform.position, Vector3.up, Color.yellow, Mathf.Lerp(this.minDistance.Value, this.maxDistance.Value, num2));
			DebugExtension.DrawCircle(base.transform.position, Vector3.up, Color.white, this.maxDistance.Value);
		}

		// Token: 0x0600104D RID: 4173 RVA: 0x0005292F File Offset: 0x00050B2F
		public void EndlessGameEnd()
		{
			if (base.IsServer)
			{
				base.NetworkObject.Despawn(true);
				global::UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		// Token: 0x0600104E RID: 4174 RVA: 0x00052950 File Offset: 0x00050B50
		public void SimulateFrameEnvironment(uint frame)
		{
			if (!this.visible && frame >= this.startFrame.Value && frame < this.explosionFrame.Value)
			{
				this.visible = true;
			}
			if (frame == this.explosionFrame.Value)
			{
				this.Detonate(this.explosionFrame.Value);
			}
			if (base.IsServer && frame == this.explosionFrame.Value + 5U)
			{
				global::UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		// Token: 0x06001051 RID: 4177 RVA: 0x00052A8C File Offset: 0x00050C8C
		protected override void __initializeVariables()
		{
			bool flag = this.maxDamage == null;
			if (flag)
			{
				throw new Exception("ThrownBomb.maxDamage cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.maxDamage.Initialize(this);
			base.__nameNetworkVariable(this.maxDamage, "maxDamage");
			this.NetworkVariableFields.Add(this.maxDamage);
			flag = this.minDamage == null;
			if (flag)
			{
				throw new Exception("ThrownBomb.minDamage cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.minDamage.Initialize(this);
			base.__nameNetworkVariable(this.minDamage, "minDamage");
			this.NetworkVariableFields.Add(this.minDamage);
			flag = this.maxDistance == null;
			if (flag)
			{
				throw new Exception("ThrownBomb.maxDistance cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.maxDistance.Initialize(this);
			base.__nameNetworkVariable(this.maxDistance, "maxDistance");
			this.NetworkVariableFields.Add(this.maxDistance);
			flag = this.minDistance == null;
			if (flag)
			{
				throw new Exception("ThrownBomb.minDistance cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.minDistance.Initialize(this);
			base.__nameNetworkVariable(this.minDistance, "minDistance");
			this.NetworkVariableFields.Add(this.minDistance);
			flag = this.maxForce == null;
			if (flag)
			{
				throw new Exception("ThrownBomb.maxForce cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.maxForce.Initialize(this);
			base.__nameNetworkVariable(this.maxForce, "maxForce");
			this.NetworkVariableFields.Add(this.maxForce);
			flag = this.minForce == null;
			if (flag)
			{
				throw new Exception("ThrownBomb.minForce cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.minForce.Initialize(this);
			base.__nameNetworkVariable(this.minForce, "minForce");
			this.NetworkVariableFields.Add(this.minForce);
			flag = this.startFrame == null;
			if (flag)
			{
				throw new Exception("ThrownBomb.startFrame cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.startFrame.Initialize(this);
			base.__nameNetworkVariable(this.startFrame, "startFrame");
			this.NetworkVariableFields.Add(this.startFrame);
			flag = this.explosionFrame == null;
			if (flag)
			{
				throw new Exception("ThrownBomb.explosionFrame cannot be null. All NetworkVariableBase instances must be initialized.");
			}
			this.explosionFrame.Initialize(this);
			base.__nameNetworkVariable(this.explosionFrame, "explosionFrame");
			this.NetworkVariableFields.Add(this.explosionFrame);
			base.__initializeVariables();
		}

		// Token: 0x06001052 RID: 4178 RVA: 0x0001E813 File Offset: 0x0001CA13
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x06001053 RID: 4179 RVA: 0x00052D0A File Offset: 0x00050F0A
		protected internal override string __getTypeName()
		{
			return "ThrownBomb";
		}

		// Token: 0x04000DF0 RID: 3568
		private const uint BOMB_START_DELAY_FRAMES = 5U;

		// Token: 0x04000DF1 RID: 3569
		private const uint SERVER_DESTROY_FRAME_DELAY = 5U;

		// Token: 0x04000DF2 RID: 3570
		[SerializeField]
		private LayerMask layerMask;

		// Token: 0x04000DF3 RID: 3571
		[SerializeField]
		[Min(1f)]
		private uint frameDelay = 1U;

		// Token: 0x04000DF4 RID: 3572
		private NetworkVariable<int> maxDamage = new NetworkVariable<int>(2, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04000DF5 RID: 3573
		private NetworkVariable<int> minDamage = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04000DF6 RID: 3574
		private NetworkVariable<float> maxDistance = new NetworkVariable<float>(4f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04000DF7 RID: 3575
		private NetworkVariable<float> minDistance = new NetworkVariable<float>(2f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04000DF8 RID: 3576
		private NetworkVariable<float> maxForce = new NetworkVariable<float>(12f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04000DF9 RID: 3577
		private NetworkVariable<float> minForce = new NetworkVariable<float>(3f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04000DFA RID: 3578
		[Header("Dropoff")]
		[Tooltip("Should start at 1, and end at 0 over a period of 1")]
		[SerializeField]
		private AnimationCurve dropoffCurve;

		// Token: 0x04000DFB RID: 3579
		[Header("References")]
		[SerializeField]
		private NetworkRigidbodyController networkRigidbodyController;

		// Token: 0x04000DFC RID: 3580
		private NetworkVariable<uint> startFrame = new NetworkVariable<uint>(0U, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04000DFD RID: 3581
		private NetworkVariable<uint> explosionFrame = new NetworkVariable<uint>(0U, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

		// Token: 0x04000DFE RID: 3582
		private NetworkObject ownerEntity;

		// Token: 0x04000E00 RID: 3584
		private static Collider[] colliderCache = new Collider[60];

		// Token: 0x04000E01 RID: 3585
		private readonly HashSet<IPhysicsTaker> hitPhysicsTakers = new HashSet<IPhysicsTaker>();

		// Token: 0x04000E02 RID: 3586
		private readonly HashSet<HittableComponent> hitHittableComponents = new HashSet<HittableComponent>();

		// Token: 0x04000E03 RID: 3587
		private bool visible;

		// Token: 0x04000E04 RID: 3588
		private NetworkObject thrower;
	}
}
