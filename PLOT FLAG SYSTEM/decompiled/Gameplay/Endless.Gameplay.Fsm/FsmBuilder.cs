using System.Collections.Generic;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay.Fsm;

public static class FsmBuilder
{
	public static void BuildFsm(NpcEntity npc)
	{
		if (NetworkManager.Singleton.IsServer)
		{
			BuildServerFsm(npc);
		}
	}

	private static void BuildServerFsm(NpcEntity npcEntity)
	{
		SpawningState spawningState = new SpawningState(npcEntity);
		NeutralState neutralState = new NeutralState(npcEntity);
		InteractionState interactionState = new InteractionState(npcEntity);
		FlinchState flinchState = new FlinchState(npcEntity);
		FreeFallState freeFallState = new FreeFallState(npcEntity);
		JumpState jumpState = new JumpState(npcEntity);
		WarpState warpState = new WarpState(npcEntity);
		GroundedPhysicsState groundedPhysicsState = new GroundedPhysicsState(npcEntity);
		StandUpState standUpState = new StandUpState(npcEntity);
		StumbleState stumbleState = new StumbleState(npcEntity);
		DownedState downedState = new DownedState(npcEntity);
		LandingState landingState = new LandingState(npcEntity);
		TeleportState teleportState = new TeleportState(npcEntity);
		List<Transition> transitions = new List<Transition>
		{
			new Transition(neutralState, (NpcEntity entity) => entity.Components.Parameters.TeleportCompleteTrigger && MonoBehaviourSingleton<Pathfinding>.Instance.IsPositionNavigable(npcEntity.FootPosition)),
			new Transition(freeFallState, (NpcEntity entity) => entity.Components.Parameters.TeleportCompleteTrigger, SpawningToFreefallAction)
		};
		teleportState.Initialize(transitions);
		List<Transition> transitions2 = new List<Transition>
		{
			new Transition(teleportState, (NpcEntity entity) => entity.Components.Parameters.TeleportTrigger),
			new Transition(neutralState, (NpcEntity entity) => entity.FinishedSpawnAnimation && MonoBehaviourSingleton<Pathfinding>.Instance.IsPositionNavigable(npcEntity.FootPosition)),
			new Transition(freeFallState, (NpcEntity entity) => entity.FinishedSpawnAnimation, SpawningToFreefallAction)
		};
		spawningState.Initialize(transitions2);
		List<Transition> transitions3 = new List<Transition>
		{
			new Transition(teleportState, (NpcEntity entity) => entity.Components.Parameters.TeleportTrigger),
			new Transition(freeFallState, (NpcEntity entity) => entity.Components.Parameters.PhysicsTrigger && !entity.Components.Grounding.IsGrounded),
			new Transition(stumbleState, (NpcEntity entity) => entity.Components.Parameters.PhysicsTrigger && entity.Components.Grounding.IsGrounded),
			new Transition(downedState, (NpcEntity entity) => entity.Health <= 0),
			new Transition(flinchState, (NpcEntity entity) => entity.Components.Parameters.FlinchTrigger),
			new Transition(interactionState, (NpcEntity entity) => entity.Components.Parameters.InteractionStartedTrigger)
		};
		neutralState.Initialize(transitions3);
		List<Transition> transitions4 = new List<Transition>
		{
			new Transition(teleportState, (NpcEntity entity) => entity.Components.Parameters.TeleportTrigger),
			new Transition(freeFallState, (NpcEntity entity) => entity.Components.Parameters.PhysicsTrigger && !entity.Components.Grounding.IsGrounded),
			new Transition(stumbleState, (NpcEntity entity) => entity.Components.Parameters.PhysicsTrigger && entity.Components.Grounding.IsGrounded),
			new Transition(downedState, (NpcEntity entity) => entity.Health <= 0),
			new Transition(flinchState, (NpcEntity entity) => entity.Components.Parameters.FlinchTrigger),
			new Transition(neutralState, (NpcEntity entity) => entity.Components.Parameters.InteractionFinishedTrigger)
		};
		interactionState.Initialize(transitions4);
		List<Transition> transitions5 = new List<Transition>
		{
			new Transition(teleportState, (NpcEntity entity) => entity.Components.Parameters.TeleportTrigger),
			new Transition(freeFallState, (NpcEntity entity) => !entity.Components.Grounding.IsGrounded),
			new Transition(downedState, (NpcEntity entity) => entity.Components.PhysicsTaker.CurrentXzPhysics.magnitude < 0.2f && entity.Health <= 0),
			new Transition(neutralState, (NpcEntity entity) => entity.Components.PhysicsTaker.CurrentXzPhysics.magnitude < 0.2f)
		};
		stumbleState.Initialize(transitions5);
		List<Transition> transitions6 = new List<Transition>
		{
			new Transition(teleportState, (NpcEntity entity) => entity.Components.Parameters.TeleportTrigger),
			new Transition(freeFallState, (NpcEntity entity) => entity.Components.Parameters.PhysicsTrigger),
			new Transition(groundedPhysicsState, (NpcEntity entity) => entity.Components.Grounding.IsGrounded && entity.Components.Parameters.WillRoll),
			new Transition(downedState, (NpcEntity entity) => entity.Components.Grounding.IsGrounded && entity.Health <= 0),
			new Transition(standUpState, (NpcEntity entity) => entity.Components.Grounding.IsGrounded && entity.Components.Parameters.WillSplat),
			new Transition(landingState, (NpcEntity entity) => entity.Components.Grounding.IsGrounded)
		};
		freeFallState.Initialize(transitions6);
		List<Transition> transitions7 = new List<Transition>
		{
			new Transition(teleportState, (NpcEntity entity) => entity.Components.Parameters.TeleportTrigger),
			new Transition(freeFallState, (NpcEntity entity) => NetClock.CurrentFrame - entity.Components.Grounding.LastGroundedFrame > 20),
			new Transition(downedState, (NpcEntity entity) => entity.Components.PhysicsTaker.CurrentXzPhysics.magnitude < 2f && entity.Health <= 0),
			new Transition(standUpState, (NpcEntity entity) => entity.Components.PhysicsTaker.CurrentXzPhysics.magnitude < 2f)
		};
		groundedPhysicsState.Initialize(transitions7);
		List<Transition> transitions8 = new List<Transition>
		{
			new Transition(teleportState, (NpcEntity entity) => entity.Components.Parameters.TeleportTrigger),
			new Transition(freeFallState, (NpcEntity entity) => entity.Components.Parameters.PhysicsTrigger && !entity.Components.Grounding.IsGrounded),
			new Transition(stumbleState, (NpcEntity entity) => entity.Components.Parameters.PhysicsTrigger && entity.Components.Grounding.IsGrounded),
			new Transition(downedState, (NpcEntity entity) => entity.Health <= 0),
			new Transition(flinchState, (NpcEntity entity) => entity.Components.Parameters.FlinchTrigger),
			new Transition(warpState, (NpcEntity entity) => entity.Components.Parameters.WarpTrigger),
			new Transition(warpState, (NpcEntity entity) => entity.Components.Parameters.JumpTrigger),
			new Transition(neutralState, (NpcEntity entity) => entity.Components.Parameters.LandingCompleteTrigger)
		};
		landingState.Initialize(transitions8);
		List<Transition> transitions9 = new List<Transition>
		{
			new Transition(teleportState, (NpcEntity entity) => entity.Components.Parameters.TeleportTrigger),
			new Transition(freeFallState, (NpcEntity entity) => entity.Components.Parameters.PhysicsTrigger && !entity.Components.Grounding.IsGrounded),
			new Transition(stumbleState, (NpcEntity entity) => entity.Components.Parameters.PhysicsTrigger && entity.Components.Grounding.IsGrounded),
			new Transition(downedState, (NpcEntity entity) => entity.Health <= 0),
			new Transition(flinchState, (NpcEntity entity) => entity.Components.Parameters.FlinchTrigger),
			new Transition(warpState, (NpcEntity entity) => entity.Components.Parameters.WarpTrigger),
			new Transition(warpState, (NpcEntity entity) => entity.Components.Parameters.JumpTrigger),
			new Transition(neutralState, (NpcEntity entity) => entity.Components.Parameters.StandUpCompleteTrigger)
		};
		standUpState.Initialize(transitions9);
		List<Transition> transitions10 = new List<Transition>
		{
			new Transition(teleportState, (NpcEntity entity) => entity.Components.Parameters.TeleportTrigger),
			new Transition(freeFallState, (NpcEntity entity) => entity.Components.Parameters.PhysicsTrigger && !entity.Components.Grounding.IsGrounded),
			new Transition(stumbleState, (NpcEntity entity) => entity.Components.Parameters.PhysicsTrigger && entity.Components.Grounding.IsGrounded),
			new Transition(downedState, (NpcEntity entity) => entity.Health <= 0),
			new Transition(neutralState, (NpcEntity entity) => entity.Components.Parameters.FlinchFinishedTrigger)
		};
		flinchState.Initialize(transitions10);
		List<Transition> transitions11 = new List<Transition>
		{
			new Transition(neutralState, (NpcEntity entity) => entity.Components.Parameters.WarpCompleteTrigger)
		};
		warpState.Initialize(transitions11);
		List<Transition> transitions12 = new List<Transition>
		{
			new Transition(neutralState, (NpcEntity entity) => entity.Components.Parameters.JumpCompleteTrigger),
			new Transition(freeFallState, (NpcEntity entity) => entity.Components.Parameters.PhysicsTrigger && !entity.Components.Grounding.IsGrounded),
			new Transition(stumbleState, (NpcEntity entity) => entity.Components.Parameters.PhysicsTrigger && entity.Components.Grounding.IsGrounded)
		};
		jumpState.Initialize(transitions12);
		List<Transition> transitions13 = new List<Transition>
		{
			new Transition(neutralState, (NpcEntity entity) => entity.Components.Parameters.ReviveTrigger)
		};
		downedState.Initialize(transitions13);
		spawningState.Enter();
		static void SpawningToFreefallAction(NpcEntity entity)
		{
			entity.WorldObject.TryGetNetworkObjectId(out var networkObjectId);
			entity.Components.PhysicsTaker.TakePhysicsForce(0.2f, Vector3.down, NetClock.CurrentFrame, networkObjectId, forceFreeFall: true);
		}
	}
}
