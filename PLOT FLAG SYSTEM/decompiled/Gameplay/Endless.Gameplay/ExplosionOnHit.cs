using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class ExplosionOnHit : OnHitModule
{
	[SerializeField]
	private LayerMask layerMask;

	[Header("Hit Options")]
	[SerializeField]
	private bool explosionHitsTeam;

	[SerializeField]
	private bool explosionHitsShooter;

	[SerializeField]
	private bool explosionHitsInitialTarget;

	[Header("Blast Distance")]
	[SerializeField]
	[Tooltip("Total radius of explosion//Distance of MIN force")]
	private float maxDistance = 4f;

	[SerializeField]
	[Tooltip("Distance of MAX force, anything in min distance get max force and max damage.")]
	private float minDistance;

	[Header("Blast Force")]
	[SerializeField]
	private float maxForce = 5f;

	[SerializeField]
	private float minForce = 1f;

	[Header("Blast Damage")]
	[SerializeField]
	private float maxDamage = 1f;

	[SerializeField]
	private float minDamage = 1f;

	private void OnValidate()
	{
		if ((int)layerMask == 0)
		{
			layerMask = (1 << LayerMask.NameToLayer("HittableColliders")) | (1 << LayerMask.NameToLayer("NetworkPhysics")) | (1 << LayerMask.NameToLayer("ExplosionsOnly"));
		}
		Projectile component = GetComponent<Projectile>();
		if ((bool)component && !component.OnHitModules.Contains(this))
		{
			component.OnHitModules.Add(this);
		}
	}

	public override void Hit(uint frame, WorldObject shooter, WorldObject hitObject, Vector3 hitPosition, Vector3 travelDirection)
	{
		int num = Physics.OverlapSphereNonAlloc(hitPosition, maxDistance, GameplayManager.TemporaryColliderArray, layerMask);
		GameplayManager.TemporaryHittableComponentsHashset.Clear();
		GameplayManager.TemporaryPhysicsTakerHashset.Clear();
		IPhysicsTaker physicsTaker = shooter?.GetComponentInChildren(typeof(IPhysicsTaker)) as IPhysicsTaker;
		IPhysicsTaker physicsTaker2 = hitObject?.GetComponentInChildren(typeof(IPhysicsTaker)) as IPhysicsTaker;
		for (int i = 0; i < num; i++)
		{
			HittableComponent hittableFromMap = MonoBehaviourSingleton<HittableMap>.Instance.GetHittableFromMap(GameplayManager.TemporaryColliderArray[i]);
			IPhysicsTaker physicsTaker3 = null;
			physicsTaker3 = ((!(hittableFromMap != null)) ? GameplayManager.TemporaryColliderArray[i].GetComponent<IPhysicsTaker>() : hittableFromMap.WorldObject.GetComponentInChildren<IPhysicsTaker>());
			if ((physicsTaker != null && !explosionHitsShooter && physicsTaker == physicsTaker3) || (physicsTaker2 != null && !explosionHitsInitialTarget && physicsTaker2 == physicsTaker3) || (!explosionHitsTeam && shooter != hittableFromMap.WorldObject && shooter.TryGetUserComponent<TeamComponent>(out var component) && !component.Team.IsHostileTo(hittableFromMap.Team)))
			{
				continue;
			}
			bool flag = physicsTaker3 != null;
			bool flag2 = hittableFromMap != null;
			if (!(flag || flag2))
			{
				continue;
			}
			Vector3 position = GameplayManager.TemporaryColliderArray[i].transform.position;
			if (flag)
			{
				position += physicsTaker3.CenterOfMassOffset;
			}
			float value = Vector3.Distance(hitPosition, position);
			float t = Mathf.Max(0f, Mathf.InverseLerp(minDistance, maxDistance, value));
			if (flag2 && !GameplayManager.TemporaryHittableComponentsHashset.Contains(hittableFromMap))
			{
				GameplayManager.TemporaryHittableComponentsHashset.Add(hittableFromMap);
				if (NetworkManager.Singleton.IsServer)
				{
					HealthModificationArgs modificationArgs = new HealthModificationArgs(Mathf.RoundToInt(Mathf.Lerp(minDamage, maxDamage, t)) * -1, shooter);
					hittableFromMap.ModifyHealth(modificationArgs);
				}
				else if (NetClock.CurrentFrame == frame)
				{
					NetworkObject componentInParent = GameplayManager.TemporaryColliderArray[i].GetComponentInParent<NetworkObject>();
					if (componentInParent != null && componentInParent.IsOwner)
					{
						HealthModificationArgs modificationArgs2 = new HealthModificationArgs(Mathf.RoundToInt(Mathf.Lerp(minDamage, maxDamage, t)) * -1, shooter);
						hittableFromMap.ModifyHealth(modificationArgs2);
					}
				}
			}
			if (flag && !GameplayManager.TemporaryPhysicsTakerHashset.Contains(physicsTaker3))
			{
				GameplayManager.TemporaryPhysicsTakerHashset.Add(physicsTaker3);
				float force = Mathf.Lerp(minForce, maxForce, t);
				physicsTaker3.TakePhysicsForce(force, (GameplayManager.TemporaryColliderArray[i].transform.position + physicsTaker3.CenterOfMassOffset - hitPosition).normalized, frame + 1, (shooter.NetworkObject != null) ? shooter.NetworkObject.NetworkObjectId : 0);
			}
		}
	}
}
