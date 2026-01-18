using System;
using System.Linq;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;
using Unity.Mathematics;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x0200014C RID: 332
	public class ProjectileEmitter : NpcNetworkComponent
	{
		// Token: 0x060007DE RID: 2014 RVA: 0x00024F60 File Offset: 0x00023160
		public void SpawnProjectile(bool randomizeDeflection = false, float nearDeflectionStrength = 0f, float farDeflectionStrength = 0f)
		{
			global::UnityEngine.Vector3 vector;
			if (!base.NpcEntity.Components.TargeterComponent.Target)
			{
				vector = base.NpcEntity.Components.EmitterPoint.rotation.eulerAngles;
			}
			else
			{
				Collider collider = null;
				float num = float.MaxValue;
				foreach (Collider collider2 in base.NpcEntity.Components.TargeterComponent.Target.HittableColliders)
				{
					global::UnityEngine.Vector3 vector2 = collider2.bounds.center - base.NpcEntity.Components.EmitterPoint.position;
					RaycastHit raycastHit;
					if (Physics.Raycast(base.NpcEntity.Components.EmitterPoint.position, vector2.normalized, out raycastHit, vector2.magnitude, ProjectileManager.AttackableAndWallCollisionMask) && raycastHit.collider == collider2 && raycastHit.distance < num)
					{
						num = raycastHit.distance;
						collider = collider2;
					}
				}
				if (!collider)
				{
					collider = base.NpcEntity.Target.HittableColliders.OrderBy((Collider col) => global::UnityEngine.Vector3.Distance(col.transform.position, base.NpcEntity.Components.EmitterPoint.position)).FirstOrDefault<Collider>();
				}
				if (collider)
				{
					global::UnityEngine.Vector3 vector3 = collider.bounds.center - collider.transform.position;
					float speed = base.NpcEntity.Components.ProjectileShooter.ProjectilePrefab.GetComponent<Projectile>().Speed;
					float num2 = global::UnityEngine.Vector3.Distance(base.NpcEntity.Components.TargeterComponent.Target.Position, base.NpcEntity.Position);
					float num3 = num2 / speed;
					global::UnityEngine.Vector3 vector4 = base.NpcEntity.Components.TargeterComponent.Target.PositionPredictions[collider].GetPredictedPosition(num3 + 0.2f);
					vector4 += vector3;
					if (randomizeDeflection)
					{
						float num4 = math.lerp(nearDeflectionStrength, farDeflectionStrength, num2 / 10f);
						global::UnityEngine.Vector3 vector5 = ProjectileEmitter.GetRandomPointInSphere() * num4;
						vector4 += vector5;
					}
					vector = Quaternion.LookRotation(vector4 - base.NpcEntity.Components.EmitterPoint.position).eulerAngles;
				}
				else
				{
					vector = base.NpcEntity.Components.EmitterPoint.rotation.eulerAngles;
				}
			}
			base.NpcEntity.Components.ProjectileShooter.UpdateBaseTeam(base.NpcEntity.Team);
			base.NpcEntity.Components.ProjectileShooter.ShootProjectileLocal(base.NpcEntity.Components.EmitterPoint.position, vector, NetClock.CurrentFrame, new Action<HealthChangeResult, Context>(this.OnModifiedObjectHealthCallback), base.NpcEntity.WorldObject);
		}

		// Token: 0x060007DF RID: 2015 RVA: 0x00025260 File Offset: 0x00023460
		private static global::UnityEngine.Vector3 GetRandomPointInSphere()
		{
			float num = global::UnityEngine.Random.value - 0.5f;
			float num2 = global::UnityEngine.Random.value - 0.5f;
			float num3 = global::UnityEngine.Random.value - 0.5f;
			float num4 = math.sqrt(num * num + num2 * num2 + num3 * num3);
			float num5 = num / num4;
			num2 /= num4;
			num3 /= num4;
			float value = global::UnityEngine.Random.value;
			return new global::UnityEngine.Vector3(num5 * value, num2 * value, num3 * value);
		}

		// Token: 0x060007E0 RID: 2016 RVA: 0x000252BE File Offset: 0x000234BE
		private void OnModifiedObjectHealthCallback(HealthChangeResult result, Context context)
		{
			NpcEntity npcEntity = base.NpcEntity;
			if (npcEntity == null)
			{
				return;
			}
			npcEntity.RaiseOnModifiedObjectHealthEvent(result, context);
		}

		// Token: 0x060007E3 RID: 2019 RVA: 0x00025304 File Offset: 0x00023504
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x060007E4 RID: 2020 RVA: 0x0002531A File Offset: 0x0002351A
		protected override void __initializeRpcs()
		{
			base.__initializeRpcs();
		}

		// Token: 0x060007E5 RID: 2021 RVA: 0x00025324 File Offset: 0x00023524
		protected internal override string __getTypeName()
		{
			return "ProjectileEmitter";
		}
	}
}
