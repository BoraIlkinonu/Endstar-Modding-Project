using System.Collections.Generic;
using Endless.Gameplay.LuaInterfaces;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay;

public class PlayerPhysicsTaker : NetworkBehaviour, IPhysicsTaker, IScriptInjector
{
	public enum PushState : byte
	{
		None,
		Small,
		Large
	}

	private const float FORCE_KILLOFF_THRESHOLD = 0.1f;

	[SerializeField]
	private PlayerController playerController;

	[SerializeField]
	private Vector3 centerOfMassOffset = new Vector3(0f, 0.9f, 0f);

	[SerializeField]
	private uint forceRecoveryFrames = 20u;

	private List<PhysicsForceInfo> newPhysicsForces = new List<PhysicsForceInfo>();

	private object luaObject;

	float IPhysicsTaker.GravityAccelerationRate => playerController.GravityAccelerationRate;

	float IPhysicsTaker.Mass => playerController.Mass;

	float IPhysicsTaker.Drag => playerController.Drag;

	float IPhysicsTaker.AirborneDrag => playerController.AirborneDrag;

	Vector3 IPhysicsTaker.CurrentVelocity => playerController.CurrentState.TotalForce;

	public Vector3 CenterOfMassOffset => centerOfMassOffset;

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

	public void GetFramePhysics(uint frame, ref NetState currentState)
	{
		PushState pushState = PushState.None;
		if (currentState.PhysicsForces == null)
		{
			currentState.PhysicsForces = new List<PhysicsForceInfo>();
		}
		foreach (PhysicsForceInfo newPhysicsForce in newPhysicsForces)
		{
			currentState.PhysicsForces.Add(newPhysicsForce);
			currentState.TotalForce += newPhysicsForce.DirectionNormal * newPhysicsForce.Force;
			if (newPhysicsForce.FriendlyForce)
			{
				currentState.JumpFrame = 0;
				currentState.AirborneFrames = 0;
				if (newPhysicsForce.DirectionNormal.y > 0f)
				{
					currentState.Grounded = false;
				}
			}
		}
		newPhysicsForces.Clear();
		currentState.HasHostileForce = false;
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < currentState.PhysicsForces.Count; i++)
		{
			if (currentState.PhysicsForces[i].StartFrame <= frame)
			{
				PhysicsForceInfo value = currentState.PhysicsForces[i];
				value.Force *= 1f - NetClock.FixedDeltaTime * (currentState.Grounded ? ((IPhysicsTaker)this).Drag : ((IPhysicsTaker)this).AirborneDrag);
				if (value.Force > 4f)
				{
					pushState = PushState.Large;
				}
				else if (pushState == PushState.None)
				{
					pushState = PushState.Small;
				}
				if (!value.FriendlyForce)
				{
					currentState.HasHostileForce = true;
				}
				zero += currentState.PhysicsForces[i].DirectionNormal * currentState.PhysicsForces[i].Force;
				currentState.PhysicsForces[i] = value;
			}
		}
		zero.x = Mathf.Clamp(zero.x, -50f, 50f);
		zero.y = Mathf.Clamp(zero.y, -50f, 50f);
		zero.z = Mathf.Clamp(zero.z, -50f, 50f);
		if (currentState.JumpFrame < 0 && pushState == PushState.Small)
		{
			currentState.PhysicsForces.Clear();
		}
		else
		{
			currentState.PhysicsForces.RemoveAll((PhysicsForceInfo x) => x.Force < 0.1f);
		}
		if (currentState.Grounded && pushState == PushState.Small && currentState.HasHostileForce)
		{
			float magnitude = zero.magnitude;
			zero.y = 0f;
			zero = magnitude * zero.normalized;
		}
		if (pushState == PushState.Large)
		{
			currentState.JumpFrame = 0;
		}
		if (currentState.HasHostileForce && (pushState == PushState.Large || (pushState == PushState.Small && currentState.PhysicsPushState != PushState.Small)))
		{
			currentState.PushFrame = forceRecoveryFrames;
		}
		else if (currentState.PushFrame != 0)
		{
			currentState.PushFrame--;
		}
		currentState.PhysicsPushState = pushState;
		currentState.PushForceControl = 1f - (float)currentState.PushFrame / (float)forceRecoveryFrames;
	}

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
				playerController.CheckAndCancelReload();
			}
		}
		if (friendlyForce)
		{
			bool hasLargerFriendlyForce = false;
			newPhysicsForces.RemoveAll(delegate(PhysicsForceInfo x)
			{
				if (x.FriendlyForce)
				{
					if (x.Force >= force)
					{
						hasLargerFriendlyForce = true;
						return false;
					}
					return true;
				}
				return false;
			});
			if (hasLargerFriendlyForce)
			{
				return;
			}
		}
		PhysicsForceInfo item = new PhysicsForceInfo
		{
			Force = force,
			DirectionNormal = directionNormal,
			StartFrame = startFrame,
			SourceID = source,
			FriendlyForce = friendlyForce,
			ApplyRandomTorque = applyRandomTorque
		};
		newPhysicsForces.Add(item);
	}

	public void EndFrame()
	{
		newPhysicsForces.Clear();
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
		return "PlayerPhysicsTaker";
	}
}
