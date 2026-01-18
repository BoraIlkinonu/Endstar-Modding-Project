using Endless.Shared;
using UnityEngine;
using UnityEngine.AI;

namespace Endless.Gameplay;

public class GroundingComponent
{
	[ShowOnly]
	public bool IsGrounded = true;

	[ShowOnly]
	public uint LastGroundedFrame;

	private readonly NavMeshAgent agent;

	private const uint ungroundFrames = 5u;

	private uint forcedUngroundFrame;

	public bool IsForcedUnground => NetClock.CurrentFrame <= forcedUngroundFrame + 5;

	public GroundingComponent(NavMeshAgent agent, IndividualStateUpdater stateUpdater)
	{
		this.agent = agent;
		stateUpdater.OnCheckGroundState += HandleOnUpdateState;
		LastGroundedFrame = NetClock.CurrentFrame;
	}

	public void ForceUnground()
	{
		forcedUngroundFrame = NetClock.CurrentFrame;
		IsGrounded = false;
	}

	private void HandleOnUpdateState()
	{
		bool num = NetClock.CurrentFrame <= forcedUngroundFrame + 5;
		Vector3 sourcePosition = agent.transform.position - new Vector3(0f, agent.baseOffset, 0f);
		if (num)
		{
			IsGrounded = false;
			return;
		}
		IsGrounded = agent.isOnNavMesh || NavMesh.SamplePosition(sourcePosition, out var _, 0.1f, -1);
		if (IsGrounded)
		{
			LastGroundedFrame = NetClock.CurrentFrame;
		}
	}
}
