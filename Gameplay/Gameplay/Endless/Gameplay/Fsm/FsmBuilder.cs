using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Endless.Shared;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay.Fsm
{
	// Token: 0x0200042E RID: 1070
	public static class FsmBuilder
	{
		// Token: 0x06001A7E RID: 6782 RVA: 0x00079910 File Offset: 0x00077B10
		public static void BuildFsm(NpcEntity npc)
		{
			if (NetworkManager.Singleton.IsServer)
			{
				FsmBuilder.BuildServerFsm(npc);
			}
		}

		// Token: 0x06001A7F RID: 6783 RVA: 0x00079924 File Offset: 0x00077B24
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
			List<Transition> list = new List<Transition>();
			list.Add(new Transition(neutralState, (NpcEntity entity) => entity.Components.Parameters.TeleportCompleteTrigger && MonoBehaviourSingleton<Pathfinding>.Instance.IsPositionNavigable(npcEntity.FootPosition), null));
			list.Add(new Transition(freeFallState, (NpcEntity entity) => entity.Components.Parameters.TeleportCompleteTrigger, new Action<NpcEntity>(FsmBuilder.<BuildServerFsm>g__SpawningToFreefallAction|1_0)));
			List<Transition> list2 = list;
			teleportState.Initialize(list2);
			List<Transition> list3 = new List<Transition>();
			list3.Add(new Transition(teleportState, (NpcEntity entity) => entity.Components.Parameters.TeleportTrigger, null));
			list3.Add(new Transition(neutralState, (NpcEntity entity) => entity.FinishedSpawnAnimation && MonoBehaviourSingleton<Pathfinding>.Instance.IsPositionNavigable(npcEntity.FootPosition), null));
			list3.Add(new Transition(freeFallState, (NpcEntity entity) => entity.FinishedSpawnAnimation, new Action<NpcEntity>(FsmBuilder.<BuildServerFsm>g__SpawningToFreefallAction|1_0)));
			List<Transition> list4 = list3;
			spawningState.Initialize(list4);
			List<Transition> list5 = new List<Transition>();
			list5.Add(new Transition(teleportState, (NpcEntity entity) => entity.Components.Parameters.TeleportTrigger, null));
			list5.Add(new Transition(freeFallState, (NpcEntity entity) => entity.Components.Parameters.PhysicsTrigger && !entity.Components.Grounding.IsGrounded, null));
			list5.Add(new Transition(stumbleState, (NpcEntity entity) => entity.Components.Parameters.PhysicsTrigger && entity.Components.Grounding.IsGrounded, null));
			list5.Add(new Transition(downedState, (NpcEntity entity) => entity.Health <= 0, null));
			list5.Add(new Transition(flinchState, (NpcEntity entity) => entity.Components.Parameters.FlinchTrigger, null));
			list5.Add(new Transition(interactionState, (NpcEntity entity) => entity.Components.Parameters.InteractionStartedTrigger, null));
			List<Transition> list6 = list5;
			neutralState.Initialize(list6);
			List<Transition> list7 = new List<Transition>();
			list7.Add(new Transition(teleportState, (NpcEntity entity) => entity.Components.Parameters.TeleportTrigger, null));
			list7.Add(new Transition(freeFallState, (NpcEntity entity) => entity.Components.Parameters.PhysicsTrigger && !entity.Components.Grounding.IsGrounded, null));
			list7.Add(new Transition(stumbleState, (NpcEntity entity) => entity.Components.Parameters.PhysicsTrigger && entity.Components.Grounding.IsGrounded, null));
			list7.Add(new Transition(downedState, (NpcEntity entity) => entity.Health <= 0, null));
			list7.Add(new Transition(flinchState, (NpcEntity entity) => entity.Components.Parameters.FlinchTrigger, null));
			list7.Add(new Transition(neutralState, (NpcEntity entity) => entity.Components.Parameters.InteractionFinishedTrigger, null));
			List<Transition> list8 = list7;
			interactionState.Initialize(list8);
			List<Transition> list9 = new List<Transition>();
			list9.Add(new Transition(teleportState, (NpcEntity entity) => entity.Components.Parameters.TeleportTrigger, null));
			list9.Add(new Transition(freeFallState, (NpcEntity entity) => !entity.Components.Grounding.IsGrounded, null));
			list9.Add(new Transition(downedState, (NpcEntity entity) => entity.Components.PhysicsTaker.CurrentXzPhysics.magnitude < 0.2f && entity.Health <= 0, null));
			list9.Add(new Transition(neutralState, (NpcEntity entity) => entity.Components.PhysicsTaker.CurrentXzPhysics.magnitude < 0.2f, null));
			List<Transition> list10 = list9;
			stumbleState.Initialize(list10);
			List<Transition> list11 = new List<Transition>();
			list11.Add(new Transition(teleportState, (NpcEntity entity) => entity.Components.Parameters.TeleportTrigger, null));
			list11.Add(new Transition(freeFallState, (NpcEntity entity) => entity.Components.Parameters.PhysicsTrigger, null));
			list11.Add(new Transition(groundedPhysicsState, (NpcEntity entity) => entity.Components.Grounding.IsGrounded && entity.Components.Parameters.WillRoll, null));
			list11.Add(new Transition(downedState, (NpcEntity entity) => entity.Components.Grounding.IsGrounded && entity.Health <= 0, null));
			list11.Add(new Transition(standUpState, (NpcEntity entity) => entity.Components.Grounding.IsGrounded && entity.Components.Parameters.WillSplat, null));
			list11.Add(new Transition(landingState, (NpcEntity entity) => entity.Components.Grounding.IsGrounded, null));
			List<Transition> list12 = list11;
			freeFallState.Initialize(list12);
			List<Transition> list13 = new List<Transition>();
			list13.Add(new Transition(teleportState, (NpcEntity entity) => entity.Components.Parameters.TeleportTrigger, null));
			list13.Add(new Transition(freeFallState, (NpcEntity entity) => NetClock.CurrentFrame - entity.Components.Grounding.LastGroundedFrame > 20U, null));
			list13.Add(new Transition(downedState, (NpcEntity entity) => entity.Components.PhysicsTaker.CurrentXzPhysics.magnitude < 2f && entity.Health <= 0, null));
			list13.Add(new Transition(standUpState, (NpcEntity entity) => entity.Components.PhysicsTaker.CurrentXzPhysics.magnitude < 2f, null));
			List<Transition> list14 = list13;
			groundedPhysicsState.Initialize(list14);
			List<Transition> list15 = new List<Transition>();
			list15.Add(new Transition(teleportState, (NpcEntity entity) => entity.Components.Parameters.TeleportTrigger, null));
			list15.Add(new Transition(freeFallState, (NpcEntity entity) => entity.Components.Parameters.PhysicsTrigger && !entity.Components.Grounding.IsGrounded, null));
			list15.Add(new Transition(stumbleState, (NpcEntity entity) => entity.Components.Parameters.PhysicsTrigger && entity.Components.Grounding.IsGrounded, null));
			list15.Add(new Transition(downedState, (NpcEntity entity) => entity.Health <= 0, null));
			list15.Add(new Transition(flinchState, (NpcEntity entity) => entity.Components.Parameters.FlinchTrigger, null));
			list15.Add(new Transition(warpState, (NpcEntity entity) => entity.Components.Parameters.WarpTrigger, null));
			list15.Add(new Transition(warpState, (NpcEntity entity) => entity.Components.Parameters.JumpTrigger, null));
			list15.Add(new Transition(neutralState, (NpcEntity entity) => entity.Components.Parameters.LandingCompleteTrigger, null));
			List<Transition> list16 = list15;
			landingState.Initialize(list16);
			List<Transition> list17 = new List<Transition>();
			list17.Add(new Transition(teleportState, (NpcEntity entity) => entity.Components.Parameters.TeleportTrigger, null));
			list17.Add(new Transition(freeFallState, (NpcEntity entity) => entity.Components.Parameters.PhysicsTrigger && !entity.Components.Grounding.IsGrounded, null));
			list17.Add(new Transition(stumbleState, (NpcEntity entity) => entity.Components.Parameters.PhysicsTrigger && entity.Components.Grounding.IsGrounded, null));
			list17.Add(new Transition(downedState, (NpcEntity entity) => entity.Health <= 0, null));
			list17.Add(new Transition(flinchState, (NpcEntity entity) => entity.Components.Parameters.FlinchTrigger, null));
			list17.Add(new Transition(warpState, (NpcEntity entity) => entity.Components.Parameters.WarpTrigger, null));
			list17.Add(new Transition(warpState, (NpcEntity entity) => entity.Components.Parameters.JumpTrigger, null));
			list17.Add(new Transition(neutralState, (NpcEntity entity) => entity.Components.Parameters.StandUpCompleteTrigger, null));
			List<Transition> list18 = list17;
			standUpState.Initialize(list18);
			List<Transition> list19 = new List<Transition>();
			list19.Add(new Transition(teleportState, (NpcEntity entity) => entity.Components.Parameters.TeleportTrigger, null));
			list19.Add(new Transition(freeFallState, (NpcEntity entity) => entity.Components.Parameters.PhysicsTrigger && !entity.Components.Grounding.IsGrounded, null));
			list19.Add(new Transition(stumbleState, (NpcEntity entity) => entity.Components.Parameters.PhysicsTrigger && entity.Components.Grounding.IsGrounded, null));
			list19.Add(new Transition(downedState, (NpcEntity entity) => entity.Health <= 0, null));
			list19.Add(new Transition(neutralState, (NpcEntity entity) => entity.Components.Parameters.FlinchFinishedTrigger, null));
			List<Transition> list20 = list19;
			flinchState.Initialize(list20);
			List<Transition> list21 = new List<Transition>();
			list21.Add(new Transition(neutralState, (NpcEntity entity) => entity.Components.Parameters.WarpCompleteTrigger, null));
			List<Transition> list22 = list21;
			warpState.Initialize(list22);
			List<Transition> list23 = new List<Transition>();
			list23.Add(new Transition(neutralState, (NpcEntity entity) => entity.Components.Parameters.JumpCompleteTrigger, null));
			list23.Add(new Transition(freeFallState, (NpcEntity entity) => entity.Components.Parameters.PhysicsTrigger && !entity.Components.Grounding.IsGrounded, null));
			list23.Add(new Transition(stumbleState, (NpcEntity entity) => entity.Components.Parameters.PhysicsTrigger && entity.Components.Grounding.IsGrounded, null));
			List<Transition> list24 = list23;
			jumpState.Initialize(list24);
			List<Transition> list25 = new List<Transition>();
			list25.Add(new Transition(neutralState, (NpcEntity entity) => entity.Components.Parameters.ReviveTrigger, null));
			List<Transition> list26 = list25;
			downedState.Initialize(list26);
			spawningState.Enter();
		}

		// Token: 0x06001A80 RID: 6784 RVA: 0x0007A49C File Offset: 0x0007869C
		[CompilerGenerated]
		internal static void <BuildServerFsm>g__SpawningToFreefallAction|1_0(NpcEntity entity)
		{
			uint num;
			entity.WorldObject.TryGetNetworkObjectId(out num);
			entity.Components.PhysicsTaker.TakePhysicsForce(0.2f, Vector3.down, NetClock.CurrentFrame, (ulong)num, true, false, false);
		}
	}
}
