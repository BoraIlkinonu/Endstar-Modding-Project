using Endless.Gameplay.LuaEnums;
using UnityEngine;

namespace Endless.Gameplay.Fsm;

public class TeleportState : FsmState
{
	private uint teleportEndFrame;

	private Vector3 teleportPosition;

	private float teleportRotation;

	private bool teleporting;

	private PhysicsMode oldPhysicsMode;

	private TeleportType teleportType;

	public override NpcEnum.FsmState State => NpcEnum.FsmState.Teleport;

	public TeleportState(NpcEntity entity)
		: base(entity)
	{
	}

	public override void Enter()
	{
		base.Enter();
		base.Components.IndividualStateUpdater.OnWriteState += HandleOnWriteState;
		base.Components.HittableComponent.IsDamageable = false;
		base.Entity.BasePhysicsMode = PhysicsMode.IgnorePhysics;
		if (!base.Entity.NpcBlackboard.TryGet<int>(NpcBlackboard.Key.TeleportType, out var value) || !base.Entity.NpcBlackboard.TryGet<Vector3>(NpcBlackboard.Key.TeleportPosition, out teleportPosition) || !base.Entity.NpcBlackboard.TryGet<float>(NpcBlackboard.Key.TeleportRotation, out teleportRotation))
		{
			Debug.LogError("Incomplete teleport data");
			base.Components.Parameters.TeleportCompleteTrigger = true;
		}
		else
		{
			teleportType = (TeleportType)value;
			TeleportInfo teleportInfo = RuntimeDatabase.GetTeleportInfo(teleportType);
			teleportEndFrame = NetClock.CurrentFrame + teleportInfo.FramesToTeleport;
		}
	}

	private void HandleOnWriteState(ref NpcState npcState)
	{
		if (!teleporting)
		{
			RuntimeDatabase.GetTeleportInfo(teleportType).TeleportStart(base.Entity.WorldObject.EndlessVisuals, base.Entity.Components.Animator, base.Entity.Position);
			base.Entity.StartTeleportClientRpc(teleportType);
			teleporting = true;
		}
		if (NetClock.CurrentFrame == teleportEndFrame)
		{
			base.Entity.Position = teleportPosition;
			base.Entity.Rotation = teleportRotation;
			RuntimeDatabase.GetTeleportInfo(teleportType).TeleportEnd(base.Entity.WorldObject.EndlessVisuals, base.Entity.Components.Animator, base.Entity.Position);
			base.Entity.EndTeleportClientRpc(teleportType);
			base.Components.Parameters.TeleportCompleteTrigger = true;
		}
	}

	protected override void Exit()
	{
		base.Exit();
		base.Components.IndividualStateUpdater.OnWriteState -= HandleOnWriteState;
		base.Components.HittableComponent.IsDamageable = true;
		teleporting = false;
		base.Entity.BasePhysicsMode = oldPhysicsMode;
	}
}
