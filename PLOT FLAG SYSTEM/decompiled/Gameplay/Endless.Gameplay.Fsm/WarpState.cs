using System.Collections.Generic;
using Endless.Shared;
using UnityEngine;

namespace Endless.Gameplay.Fsm;

public class WarpState : FsmState
{
	private Vector3 warpPosition;

	private uint warpCompleteFrame;

	private readonly Collider[] colliders = new Collider[5];

	public override NpcEnum.FsmState State => NpcEnum.FsmState.Warp;

	public WarpState(NpcEntity entity)
		: base(entity)
	{
	}

	public override void Enter()
	{
		base.Enter();
		List<Vector3> list = MonoBehaviourSingleton<Pathfinding>.Instance.FindNavigationPositionsInRange(base.Entity.transform.position, 5f);
		foreach (Vector3 item in list)
		{
			Vector3 position = item + Vector3.up;
			if (Physics.OverlapSphereNonAlloc(position, 0.1f, colliders, base.Entity.Settings.CharacterCollisionMask) <= 0)
			{
				warpPosition = position;
				break;
			}
		}
		if (warpPosition.Equals(Vector3.zero))
		{
			warpPosition = list[0];
		}
		warpCompleteFrame = NetClock.CurrentFrame + 8;
		base.Entity.Components.IndividualStateUpdater.OnTickAi += HandleOnTickAi;
	}

	protected override void Exit()
	{
		base.Exit();
		base.Entity.Components.IndividualStateUpdater.OnTickAi -= HandleOnTickAi;
	}

	private void HandleOnTickAi()
	{
		if (NetClock.CurrentFrame >= warpCompleteFrame)
		{
			base.Entity.Position = warpPosition + Vector3.up * 0.5f;
			base.Entity.Components.Agent.Warp(warpPosition);
			base.Components.Parameters.WarpCompleteTrigger = true;
		}
	}
}
