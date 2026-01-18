using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Gameplay.LuaInterfaces;
using UnityEngine;

namespace Endless.Gameplay;

public class PhysicsTaker : NpcComponent, IPhysicsTaker, IScriptInjector
{
	[SerializeField]
	private float groundedDrag;

	[SerializeField]
	private float airborneDrag;

	private readonly List<PhysicsForceInfo> currentPhysicsForces = new List<PhysicsForceInfo>();

	private readonly List<PhysicsForceInfo> staleForces = new List<PhysicsForceInfo>();

	private object luaObject;

	private float CurrentDrag
	{
		get
		{
			if (!base.NpcEntity.Components.Grounding.IsGrounded)
			{
				return AirborneDrag;
			}
			return Drag;
		}
	}

	public Vector3 CenterOfMassOffset => new Vector3(0f, 0.4f, 0f);

	public bool HasPhysicsImpulse => currentPhysicsForces.Count > 0;

	public Vector3 CurrentPhysics { get; private set; }

	public Vector3 CurrentXzPhysics { get; private set; }

	public float GravityAccelerationRate => 0f - Physics.gravity.y;

	public float Mass => 1f;

	public float Drag => groundedDrag;

	public float AirborneDrag => airborneDrag;

	public Vector3 CurrentVelocity => Vector3.zero;

	public object LuaObject
	{
		get
		{
			if (luaObject == null)
			{
				luaObject = new PhysicsComponent(this);
			}
			return luaObject;
		}
	}

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
		PhysicsForceInfo info = new PhysicsForceInfo
		{
			Force = force,
			DirectionNormal = directionNormal,
			StartFrame = startFrame,
			SourceID = source,
			ApplyRandomTorque = applyRandomTorque
		};
		AddPhysicsForce(info);
		if (info.Force > 4f || forceFreeFall)
		{
			base.NpcEntity.Components.Grounding.ForceUnground();
		}
		base.NpcEntity.Components.Parameters.PhysicsTrigger = true;
	}

	private void AddPhysicsForce(PhysicsForceInfo info)
	{
		for (int i = 0; i < currentPhysicsForces.Count; i++)
		{
			if (currentPhysicsForces[i].SourceID == info.SourceID && NetClock.CurrentFrame - currentPhysicsForces[i].StartFrame < 3)
			{
				return;
			}
		}
		currentPhysicsForces.Add(info);
	}

	private void Update()
	{
		CurrentPhysics = GetUpdatePhysics();
		CurrentXzPhysics = new Vector3(CurrentPhysics.x, 0f, CurrentPhysics.z);
	}

	private Vector3 GetUpdatePhysics()
	{
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < currentPhysicsForces.Count; i++)
		{
			PhysicsForceInfo physicsForceInfo = currentPhysicsForces[i];
			physicsForceInfo.Force *= 1f - Time.deltaTime * CurrentDrag;
			currentPhysicsForces[i] = physicsForceInfo;
			if (physicsForceInfo.Force > 0.1f)
			{
				zero += physicsForceInfo.DirectionNormal * physicsForceInfo.Force;
			}
			else
			{
				staleForces.Add(physicsForceInfo);
			}
		}
		foreach (PhysicsForceInfo staleForce in staleForces)
		{
			currentPhysicsForces.Remove(staleForce);
		}
		staleForces.Clear();
		zero.x = Mathf.Clamp(zero.x, -50f, 50f);
		zero.y = Mathf.Clamp(zero.y, -50f, 50f);
		zero.z = Mathf.Clamp(zero.z, -50f, 50f);
		return zero;
	}

	public void OnCollisionEnter(Collision other)
	{
		PhysicsCubeController component = other.gameObject.GetComponent<PhysicsCubeController>();
		if ((bool)component && Vector3.Dot(other.contacts[0].normal, Vector3.up) < 0.5f)
		{
			float force = Mathf.Max(3f, other.relativeVelocity.magnitude);
			TakePhysicsForce(force, other.relativeVelocity.normalized, NetClock.CurrentFrame, component.NetworkObjectId);
		}
	}
}
