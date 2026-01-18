using System;
using Endless.Gameplay.LuaInterfaces;
using UnityEngine;

namespace Endless.Gameplay;

public interface IPhysicsTaker : IScriptInjector
{
	const float SMALL_PUSH_THRESHOLD = 4f;

	Vector3 CenterOfMassOffset { get; }

	float GravityAccelerationRate { get; }

	float Mass { get; }

	float Drag { get; }

	float AirborneDrag { get; }

	Vector3 CurrentVelocity { get; }

	float BlastForceMultiplier => 1f;

	new object LuaObject { get; }

	Type IScriptInjector.LuaObjectType => typeof(PhysicsComponent);

	void TakePhysicsForce(float force, Vector3 directionNormal, uint startFrame, ulong source, bool forceFreeFall = false, bool friendlyForce = false, bool applyRandomTorque = false);
}
