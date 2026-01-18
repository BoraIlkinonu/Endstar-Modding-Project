using System.Linq;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.Scripting;
using Unity.Mathematics;
using UnityEngine;

namespace Endless.Gameplay;

public class ProjectileEmitter : NpcNetworkComponent
{
	public void SpawnProjectile(bool randomizeDeflection = false, float nearDeflectionStrength = 0f, float farDeflectionStrength = 0f)
	{
		UnityEngine.Vector3 eulerAngles;
		if (!base.NpcEntity.Components.TargeterComponent.Target)
		{
			eulerAngles = base.NpcEntity.Components.EmitterPoint.rotation.eulerAngles;
		}
		else
		{
			Collider collider = null;
			float num = float.MaxValue;
			foreach (Collider hittableCollider in base.NpcEntity.Components.TargeterComponent.Target.HittableColliders)
			{
				UnityEngine.Vector3 vector = hittableCollider.bounds.center - base.NpcEntity.Components.EmitterPoint.position;
				if (Physics.Raycast(base.NpcEntity.Components.EmitterPoint.position, vector.normalized, out var hitInfo, vector.magnitude, ProjectileManager.AttackableAndWallCollisionMask) && hitInfo.collider == hittableCollider && hitInfo.distance < num)
				{
					num = hitInfo.distance;
					collider = hittableCollider;
				}
			}
			if (!collider)
			{
				collider = base.NpcEntity.Target.HittableColliders.OrderBy((Collider col) => UnityEngine.Vector3.Distance(col.transform.position, base.NpcEntity.Components.EmitterPoint.position)).FirstOrDefault();
			}
			if ((bool)collider)
			{
				UnityEngine.Vector3 vector2 = collider.bounds.center - collider.transform.position;
				float speed = base.NpcEntity.Components.ProjectileShooter.ProjectilePrefab.GetComponent<Projectile>().Speed;
				float num2 = UnityEngine.Vector3.Distance(base.NpcEntity.Components.TargeterComponent.Target.Position, base.NpcEntity.Position);
				float num3 = num2 / speed;
				UnityEngine.Vector3 predictedPosition = base.NpcEntity.Components.TargeterComponent.Target.PositionPredictions[collider].GetPredictedPosition(num3 + 0.2f);
				predictedPosition += vector2;
				if (randomizeDeflection)
				{
					float num4 = math.lerp(nearDeflectionStrength, farDeflectionStrength, num2 / 10f);
					UnityEngine.Vector3 vector3 = GetRandomPointInSphere() * num4;
					predictedPosition += vector3;
				}
				eulerAngles = Quaternion.LookRotation(predictedPosition - base.NpcEntity.Components.EmitterPoint.position).eulerAngles;
			}
			else
			{
				eulerAngles = base.NpcEntity.Components.EmitterPoint.rotation.eulerAngles;
			}
		}
		base.NpcEntity.Components.ProjectileShooter.UpdateBaseTeam(base.NpcEntity.Team);
		base.NpcEntity.Components.ProjectileShooter.ShootProjectileLocal(base.NpcEntity.Components.EmitterPoint.position, eulerAngles, NetClock.CurrentFrame, OnModifiedObjectHealthCallback, base.NpcEntity.WorldObject);
	}

	private static UnityEngine.Vector3 GetRandomPointInSphere()
	{
		float num = UnityEngine.Random.value - 0.5f;
		float num2 = UnityEngine.Random.value - 0.5f;
		float num3 = UnityEngine.Random.value - 0.5f;
		float num4 = math.sqrt(num * num + num2 * num2 + num3 * num3);
		float num5 = num / num4;
		num2 /= num4;
		num3 /= num4;
		float value = UnityEngine.Random.value;
		return new UnityEngine.Vector3(num5 * value, num2 * value, num3 * value);
	}

	private void OnModifiedObjectHealthCallback(HealthChangeResult result, Context context)
	{
		base.NpcEntity?.RaiseOnModifiedObjectHealthEvent(result, context);
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected internal override string __getTypeName()
	{
		return "ProjectileEmitter";
	}
}
