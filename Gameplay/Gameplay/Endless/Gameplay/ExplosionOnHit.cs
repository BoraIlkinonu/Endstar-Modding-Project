using System;
using Endless.Gameplay.LuaEnums;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x02000283 RID: 643
	public class ExplosionOnHit : OnHitModule
	{
		// Token: 0x06000DD0 RID: 3536 RVA: 0x0004A87C File Offset: 0x00048A7C
		private void OnValidate()
		{
			if (this.layerMask == 0)
			{
				this.layerMask = (1 << LayerMask.NameToLayer("HittableColliders")) | (1 << LayerMask.NameToLayer("NetworkPhysics")) | (1 << LayerMask.NameToLayer("ExplosionsOnly"));
			}
			Projectile component = base.GetComponent<Projectile>();
			if (component && !component.OnHitModules.Contains(this))
			{
				component.OnHitModules.Add(this);
			}
		}

		// Token: 0x06000DD1 RID: 3537 RVA: 0x0004A8FC File Offset: 0x00048AFC
		public override void Hit(uint frame, WorldObject shooter, WorldObject hitObject, Vector3 hitPosition, Vector3 travelDirection)
		{
			int num = Physics.OverlapSphereNonAlloc(hitPosition, this.maxDistance, GameplayManager.TemporaryColliderArray, this.layerMask);
			GameplayManager.TemporaryHittableComponentsHashset.Clear();
			GameplayManager.TemporaryPhysicsTakerHashset.Clear();
			IPhysicsTaker physicsTaker = ((shooter != null) ? shooter.GetComponentInChildren(typeof(IPhysicsTaker)) : null) as IPhysicsTaker;
			IPhysicsTaker physicsTaker2 = ((hitObject != null) ? hitObject.GetComponentInChildren(typeof(IPhysicsTaker)) : null) as IPhysicsTaker;
			for (int i = 0; i < num; i++)
			{
				HittableComponent hittableFromMap = MonoBehaviourSingleton<HittableMap>.Instance.GetHittableFromMap(GameplayManager.TemporaryColliderArray[i]);
				IPhysicsTaker physicsTaker3;
				if (hittableFromMap != null)
				{
					physicsTaker3 = hittableFromMap.WorldObject.GetComponentInChildren<IPhysicsTaker>();
				}
				else
				{
					physicsTaker3 = GameplayManager.TemporaryColliderArray[i].GetComponent<IPhysicsTaker>();
				}
				TeamComponent teamComponent;
				if ((physicsTaker == null || this.explosionHitsShooter || physicsTaker != physicsTaker3) && (physicsTaker2 == null || this.explosionHitsInitialTarget || physicsTaker2 != physicsTaker3) && (this.explosionHitsTeam || !(shooter != hittableFromMap.WorldObject) || !shooter.TryGetUserComponent<TeamComponent>(out teamComponent) || teamComponent.Team.IsHostileTo(hittableFromMap.Team)))
				{
					bool flag = physicsTaker3 != null;
					bool flag2 = hittableFromMap != null;
					if (flag || flag2)
					{
						Vector3 vector = GameplayManager.TemporaryColliderArray[i].transform.position;
						if (flag)
						{
							vector += physicsTaker3.CenterOfMassOffset;
						}
						float num2 = Vector3.Distance(hitPosition, vector);
						float num3 = Mathf.Max(0f, Mathf.InverseLerp(this.minDistance, this.maxDistance, num2));
						if (flag2 && !GameplayManager.TemporaryHittableComponentsHashset.Contains(hittableFromMap))
						{
							GameplayManager.TemporaryHittableComponentsHashset.Add(hittableFromMap);
							if (NetworkManager.Singleton.IsServer)
							{
								HealthModificationArgs healthModificationArgs = new HealthModificationArgs(Mathf.RoundToInt(Mathf.Lerp(this.minDamage, this.maxDamage, num3)) * -1, shooter, DamageType.Normal, HealthChangeType.Damage);
								hittableFromMap.ModifyHealth(healthModificationArgs);
							}
							else if (NetClock.CurrentFrame == frame)
							{
								NetworkObject componentInParent = GameplayManager.TemporaryColliderArray[i].GetComponentInParent<NetworkObject>();
								if (componentInParent != null && componentInParent.IsOwner)
								{
									HealthModificationArgs healthModificationArgs2 = new HealthModificationArgs(Mathf.RoundToInt(Mathf.Lerp(this.minDamage, this.maxDamage, num3)) * -1, shooter, DamageType.Normal, HealthChangeType.Damage);
									hittableFromMap.ModifyHealth(healthModificationArgs2);
								}
							}
						}
						if (flag && !GameplayManager.TemporaryPhysicsTakerHashset.Contains(physicsTaker3))
						{
							GameplayManager.TemporaryPhysicsTakerHashset.Add(physicsTaker3);
							float num4 = Mathf.Lerp(this.minForce, this.maxForce, num3);
							physicsTaker3.TakePhysicsForce(num4, (GameplayManager.TemporaryColliderArray[i].transform.position + physicsTaker3.CenterOfMassOffset - hitPosition).normalized, frame + 1U, (shooter.NetworkObject != null) ? shooter.NetworkObject.NetworkObjectId : 0UL, false, false, false);
						}
					}
				}
			}
		}

		// Token: 0x04000CA6 RID: 3238
		[SerializeField]
		private LayerMask layerMask;

		// Token: 0x04000CA7 RID: 3239
		[Header("Hit Options")]
		[SerializeField]
		private bool explosionHitsTeam;

		// Token: 0x04000CA8 RID: 3240
		[SerializeField]
		private bool explosionHitsShooter;

		// Token: 0x04000CA9 RID: 3241
		[SerializeField]
		private bool explosionHitsInitialTarget;

		// Token: 0x04000CAA RID: 3242
		[Header("Blast Distance")]
		[SerializeField]
		[Tooltip("Total radius of explosion//Distance of MIN force")]
		private float maxDistance = 4f;

		// Token: 0x04000CAB RID: 3243
		[SerializeField]
		[Tooltip("Distance of MAX force, anything in min distance get max force and max damage.")]
		private float minDistance;

		// Token: 0x04000CAC RID: 3244
		[Header("Blast Force")]
		[SerializeField]
		private float maxForce = 5f;

		// Token: 0x04000CAD RID: 3245
		[SerializeField]
		private float minForce = 1f;

		// Token: 0x04000CAE RID: 3246
		[Header("Blast Damage")]
		[SerializeField]
		private float maxDamage = 1f;

		// Token: 0x04000CAF RID: 3247
		[SerializeField]
		private float minDamage = 1f;
	}
}
