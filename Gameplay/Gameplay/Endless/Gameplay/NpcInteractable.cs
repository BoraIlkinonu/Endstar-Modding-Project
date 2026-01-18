using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020000DD RID: 221
	public class NpcInteractable : InteractableBase, IStartSubscriber
	{
		// Token: 0x170000BF RID: 191
		// (get) Token: 0x060004AC RID: 1196 RVA: 0x000183F5 File Offset: 0x000165F5
		public List<InteractorBase> ActiveInteractors { get; } = new List<InteractorBase>();

		// Token: 0x170000C0 RID: 192
		// (get) Token: 0x060004AD RID: 1197 RVA: 0x000183FD File Offset: 0x000165FD
		private IInteractionBehavior InteractionBehavior
		{
			get
			{
				return this.npcEntity.InteractionBehavior;
			}
		}

		// Token: 0x170000C1 RID: 193
		// (get) Token: 0x060004AE RID: 1198 RVA: 0x0001840A File Offset: 0x0001660A
		// (set) Token: 0x060004AF RID: 1199 RVA: 0x00018412 File Offset: 0x00016612
		private bool IsInteractable
		{
			get
			{
				return this.isInteractable;
			}
			set
			{
				this.isInteractable = value;
				this.UpdateInteractableState();
			}
		}

		// Token: 0x060004B0 RID: 1200 RVA: 0x00018421 File Offset: 0x00016621
		public override void InteractionStopped(InteractorBase interactor)
		{
			if (this.ActiveInteractors.Remove(interactor))
			{
				IInteractionBehavior interactionBehavior = this.InteractionBehavior;
				if (interactionBehavior == null)
				{
					return;
				}
				interactionBehavior.InteractionStopped(interactor.ContextObject, this.npcEntity.Context);
			}
		}

		// Token: 0x060004B1 RID: 1201 RVA: 0x00018452 File Offset: 0x00016652
		public override void SetAllInteractablesEnabled(bool interactable)
		{
			base.SetAllInteractablesEnabled(interactable);
			this.IsInteractable = interactable;
		}

		// Token: 0x060004B2 RID: 1202 RVA: 0x00018464 File Offset: 0x00016664
		private void UpdateInteractableState()
		{
			bool flag = this.IsInteractable && this.npcEntity.CombatState == NpcEnum.CombatState.None && !this.npcEntity.IsDowned;
			this.SetColliderEnabledState_ClientRpc(flag);
		}

		// Token: 0x060004B3 RID: 1203 RVA: 0x000184A0 File Offset: 0x000166A0
		[ClientRpc]
		private void SetColliderEnabledState_ClientRpc(bool newState)
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsServer || networkManager.IsHost))
			{
				ClientRpcParams clientRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendClientRpc(1453106355U, clientRpcParams, RpcDelivery.Reliable);
				fastBufferWriter.WriteValueSafe<bool>(in newState, default(FastBufferWriter.ForPrimitives));
				base.__endSendClientRpc(ref fastBufferWriter, 1453106355U, clientRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsClient && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			foreach (Collider collider in this.colliders)
			{
				collider.enabled = newState;
			}
		}

		// Token: 0x060004B4 RID: 1204 RVA: 0x000185D4 File Offset: 0x000167D4
		protected override bool AttemptInteract_ServerLogic(InteractorBase interactor, int colliderIndex)
		{
			if (this.npcEntity.CombatState != NpcEnum.CombatState.None)
			{
				return false;
			}
			IInteractionBehavior interactionBehavior = this.InteractionBehavior;
			return interactionBehavior != null && interactionBehavior.AttemptInteractServerLogic(interactor.ContextObject, this.npcEntity.Context, colliderIndex);
		}

		// Token: 0x060004B5 RID: 1205 RVA: 0x00018608 File Offset: 0x00016808
		protected override void InteractServerLogic(InteractorBase interactor, int colliderIndex)
		{
			if (!this.ActiveInteractors.Contains(interactor))
			{
				this.ActiveInteractors.Add(interactor);
			}
			IInteractionBehavior interactionBehavior = this.InteractionBehavior;
			if (interactionBehavior == null)
			{
				return;
			}
			interactionBehavior.OnInteracted(interactor.ContextObject, this.npcEntity.Context, colliderIndex);
		}

		// Token: 0x060004B6 RID: 1206 RVA: 0x00018648 File Offset: 0x00016848
		[ServerRpc(RequireOwnership = false)]
		private void RequestInteractableState_ServerRpc()
		{
			NetworkManager networkManager = base.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute && (networkManager.IsClient || networkManager.IsHost))
			{
				ServerRpcParams serverRpcParams;
				FastBufferWriter fastBufferWriter = base.__beginSendServerRpc(2240966889U, serverRpcParams, RpcDelivery.Reliable);
				base.__endSendServerRpc(ref fastBufferWriter, 2240966889U, serverRpcParams, RpcDelivery.Reliable);
			}
			if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute || (!networkManager.IsServer && !networkManager.IsHost))
			{
				return;
			}
			this.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
			if (this.interactableColliders.Count > 0)
			{
				this.SetColliderEnabledState_ClientRpc(this.colliders[0].enabled);
			}
		}

		// Token: 0x060004B7 RID: 1207 RVA: 0x00018740 File Offset: 0x00016940
		private void OnDisable()
		{
			this.npcEntity.OnCombatStateChanged -= this.NpcEntityOnOnCombatStateChanged;
		}

		// Token: 0x060004B8 RID: 1208 RVA: 0x00018759 File Offset: 0x00016959
		public void EndlessStart()
		{
			if (!NetworkManager.Singleton.IsServer)
			{
				this.RequestInteractableState_ServerRpc();
			}
			this.npcEntity.OnCombatStateChanged += this.NpcEntityOnOnCombatStateChanged;
		}

		// Token: 0x060004B9 RID: 1209 RVA: 0x00018784 File Offset: 0x00016984
		private void NpcEntityOnOnCombatStateChanged()
		{
			this.UpdateInteractableState();
		}

		// Token: 0x060004BB RID: 1211 RVA: 0x000187A0 File Offset: 0x000169A0
		protected override void __initializeVariables()
		{
			base.__initializeVariables();
		}

		// Token: 0x060004BC RID: 1212 RVA: 0x000187B8 File Offset: 0x000169B8
		protected override void __initializeRpcs()
		{
			base.__registerRpc(1453106355U, new NetworkBehaviour.RpcReceiveHandler(NpcInteractable.__rpc_handler_1453106355), "SetColliderEnabledState_ClientRpc");
			base.__registerRpc(2240966889U, new NetworkBehaviour.RpcReceiveHandler(NpcInteractable.__rpc_handler_2240966889), "RequestInteractableState_ServerRpc");
			base.__initializeRpcs();
		}

		// Token: 0x060004BD RID: 1213 RVA: 0x00018808 File Offset: 0x00016A08
		private static void __rpc_handler_1453106355(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			bool flag;
			reader.ReadValueSafe<bool>(out flag, default(FastBufferWriter.ForPrimitives));
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((NpcInteractable)target).SetColliderEnabledState_ClientRpc(flag);
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060004BE RID: 1214 RVA: 0x00018878 File Offset: 0x00016A78
		private static void __rpc_handler_2240966889(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
		{
			NetworkManager networkManager = target.NetworkManager;
			if (networkManager == null || !networkManager.IsListening)
			{
				return;
			}
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Execute;
			((NpcInteractable)target).RequestInteractableState_ServerRpc();
			target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Send;
		}

		// Token: 0x060004BF RID: 1215 RVA: 0x000188C9 File Offset: 0x00016AC9
		protected internal override string __getTypeName()
		{
			return "NpcInteractable";
		}

		// Token: 0x040003D5 RID: 981
		[SerializeField]
		private NpcEntity npcEntity;

		// Token: 0x040003D6 RID: 982
		[SerializeField]
		private List<Collider> colliders;

		// Token: 0x040003D7 RID: 983
		private bool isInteractable;
	}
}
