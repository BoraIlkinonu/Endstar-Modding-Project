using System;
using System.Collections.Generic;
using Endless.Gameplay.LuaEnums;
using Endless.Shared.DataTypes;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay;

public class PathFollower
{
	private Motion motion;

	private Coroutine motionRoutine;

	private readonly int mask = LayerMask.GetMask("Default");

	private readonly NpcEntity entity;

	private readonly NavMeshAgent agent;

	private readonly MonoBehaviorProxy proxy;

	public Transform LookRotationOverride;

	public bool IsJumping => motion is Jump;

	public NavPath Path { get; private set; }

	public event Action<bool> OnPathFinished;

	public PathFollower(NpcEntity entity, IndividualStateUpdater stateUpdater, NavMeshAgent agent, DynamicAttributes dynamicAttributes, MonoBehaviorProxy proxy)
	{
		this.entity = entity;
		this.agent = agent;
		this.proxy = proxy;
		stateUpdater.OnWriteState += HandleOnWriteState;
		stateUpdater.OnUpdateState += HandleOnUpdateState;
		dynamicAttributes.OnMovementModeChanged += HandleOnMovementModeChanged;
		HandleOnMovementModeChanged();
		this.agent.avoidancePriority = UnityEngine.Random.Range(0, 100);
	}

	public void SetPath(NavPath path)
	{
		if (motion is Jump)
		{
			bool flag = false;
			foreach (NavPath.Segment navigationSegment in path.NavigationSegments)
			{
				ConnectionKind connectionKind = navigationSegment.ConnectionKind;
				if (connectionKind == ConnectionKind.Jump || connectionKind == ConnectionKind.Dropdown)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				while (path.NavigationSegments.Count > 0)
				{
					if (path.NavigationSegments.Dequeue().ConnectionKind == ConnectionKind.Jump)
					{
						path.NavigationSegments.Dequeue();
						break;
					}
				}
			}
			Path = path;
		}
		else
		{
			StopMotion();
			Path = path;
			BeginNextSegment();
		}
	}

	public void StopPath(bool forceStop = false)
	{
		if (motion != null && (!(motion is Jump) || forceStop))
		{
			StopMotion();
		}
		Path = null;
		LookRotationOverride = null;
		if (entity.Components.Agent.isOnNavMesh)
		{
			entity.Components.Agent.ResetPath();
		}
	}

	public bool IsRepathNecessary(HashSet<SerializableGuid> updatedProps)
	{
		if (motion is Threshold threshold)
		{
			return !updatedProps.Contains(threshold.Door.WorldObject.InstanceId);
		}
		return true;
	}

	private void HandleOnMovementModeChanged()
	{
		float speed = entity.MovementMode switch
		{
			MovementMode.Walk => entity.Settings.WalkSpeed, 
			MovementMode.Run => entity.Settings.RunSpeed, 
			MovementMode.Sprint => entity.Settings.SprintSpeed, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		agent.speed = speed;
	}

	private void HandleOnWriteState(ref NpcState state)
	{
		state.Position = entity.transform.position;
		motion?.WriteState(ref state);
	}

	private void HandleOnUpdateState(uint frame)
	{
		if (motion != null)
		{
			if (motion.HasFailed())
			{
				this.OnPathFinished?.Invoke(obj: false);
				StopMotion();
			}
			else if (motion.IsComplete())
			{
				StopMotion();
				BeginNextSegment();
			}
		}
	}

	private void BeginNextSegment()
	{
		if (Path == null)
		{
			return;
		}
		if (Path.NavigationSegments.Count > 0)
		{
			NavPath.Segment segment = Path.NavigationSegments.Dequeue();
			motion = Motion.Factory.Build(entity, segment, LookRotationOverride);
			if (motion.CanRun())
			{
				motionRoutine = proxy.StartMonoBehaviorRoutine(motion.Execute());
				return;
			}
			this.OnPathFinished?.Invoke(obj: false);
			Path = null;
			LookRotationOverride = null;
			motion = null;
		}
		else
		{
			this.OnPathFinished?.Invoke(obj: true);
			Path = null;
			LookRotationOverride = null;
		}
	}

	private void StopMotion()
	{
		motion?.Stop();
		if (motionRoutine != null)
		{
			proxy.StopMonoBehaviorRoutine(motionRoutine);
		}
		motionRoutine = null;
		motion = null;
	}

	public void Tick()
	{
		if (!IsJumping && entity.CurrentState.State == NpcEnum.FsmState.Neutral && !Physics.CheckSphere(entity.FootPosition, entity.Components.Agent.radius / 2f, mask))
		{
			entity.Components.PhysicsTaker.TakePhysicsForce(0f, Vector3.down, NetClock.CurrentFrame, entity.NetworkObjectId, forceFreeFall: true);
		}
	}

	private void DisplayDebugPath(NavPath path)
	{
		while (path.NavigationSegments.Count > 0)
		{
			NavPath.Segment segment = path.NavigationSegments.Dequeue();
			Color color = (Motion.Factory.Build(entity, segment).CanRun() ? Color.cyan : Color.red);
			Debug.DrawLine(segment.StartPosition, segment.EndPosition, color, 15f);
		}
	}
}
