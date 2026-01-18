using Endless.Gameplay.Scripting;
using UnityEngine;

namespace Endless.Gameplay.LuaInterfaces;

public class PhysicsComponent
{
	private IPhysicsTaker physicsTaker;

	public void AddImpulse(Context instigator, UnityEngine.Vector3 force, bool applyRandomTorque)
	{
		force.x = Mathf.Clamp(force.x, -50f, 50f);
		force.y = Mathf.Clamp(force.y, -50f, 50f);
		force.z = Mathf.Clamp(force.z, -50f, 50f);
		physicsTaker.TakePhysicsForce(force.magnitude, force.normalized, NetClock.CurrentFrame + 2, 0uL, forceFreeFall: false, friendlyForce: false, applyRandomTorque);
	}

	public void AddImpulse(Context instigator, UnityEngine.Vector3 force)
	{
		AddImpulse(instigator, force, applyRandomTorque: false);
	}

	internal PhysicsComponent(IPhysicsTaker physicsTaker)
	{
		this.physicsTaker = physicsTaker;
	}
}
